using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;
using BydClient.Exceptions;

namespace BydClient.Crypto;

/// <summary>
/// White-box AES block cipher and CBC mode for Bangcle envelopes.
/// Faithful port of bangcle.js: decryptBlockAuth, encryptBlockAuth,
/// decryptCbc, encryptCbc.
///
/// The cipher uses pre-computed lookup tables extracted from
/// libencrypt.so rather than a standard AES key schedule.
/// </summary>
public static class BangcleBlock
{
    /// <summary>
    /// Transpose 4x4 block into working state layout (col*8+row).
    /// </summary>
    private static void PrepareAesMatrix(ReadOnlySpan<byte> inputBlock, Span<byte> output)
    {
        for(int col = 0; col < 4; col++)
        {
            for(int row = 0; row < 4; row++)
            {
                output[col * 8 + row] = inputBlock[col + row * 4];
            }
        }
    }

    /// <summary>
    /// Decrypt a single 16-byte block using white-box AES tables.
    /// </summary>
    /// <param name="tables">Pre-loaded lookup tables.</param>
    /// <param name="block">16-byte ciphertext block.</param>
    /// <param name="roundStart">Starting round (default 1 = full decryption).</param>
    /// <returns>16-byte decrypted block.</returns>
    /// <exception cref="BangcleException"></exception>
    public static byte[] DecryptBlockAuth(Dictionary<string, byte[]> tables, byte[] block, int roundStart = 1)
    {
        byte[] state = new byte[32];
        byte[] temp64 = new byte[64];
        byte[] tmp32 = new byte[32];
        byte[] output = new byte[16];

        PrepareAesMatrix(block, state);
        int param3 = roundStart;

        for(int rnd = 9; rnd > Math.Max(0, param3 - 1); rnd--)
        {
            int l_var20 = rnd;
            int l_var21 = l_var20 * 4;
            int permPtr = 0;

            for(int i = 0; i < 4; i++)
            {
                byte b_var3 = tables["perm_decrypt"][permPtr];
                int l_var16 = i * 8;
                int baseIdx = i * 16;

                for(int j = 0; j < 4; j++)
                {
                    byte u_var7 = (byte)((b_var3 + j) & 3);
                    byte byteVal = state[l_var16 + u_var7];
                    int idx = byteVal + (i + (l_var21 + u_var7) * 4) * 256;

                    // Extract 4 bytes as little-endian integer from inv_round table
                    uint value = BinaryPrimitives.ReadUInt32LittleEndian(tables["inv_round"].AsSpan(idx * 4, 4));
                    BinaryPrimitives.WriteUInt32LittleEndian(temp64.AsSpan(baseIdx + (j * 4)), value);
                }

                permPtr += 2;
            }

            int i_var15 = 1;
            for(int l_var21_xor = 0; l_var21_xor < 4; l_var21_xor++)
            {
                int pb_var18_offset = l_var21_xor;

                for(int l_var9_xor = 0; l_var9_xor < 4; l_var9_xor++)
                {
                    uint local10 = temp64[pb_var18_offset];
                    byte u_var6 = (byte)(local10 & 0xF);
                    byte u_var26 = (byte)(local10 & 0xF0);

                    uint local_f0 = temp64[pb_var18_offset + 0x10];
                    uint local_f1 = temp64[pb_var18_offset + 0x20];
                    uint local_f2 = temp64[pb_var18_offset + 0x30];

                    int l_var2 = l_var9_xor * 0x18 + l_var20 * 0x60;
                    int i_var25 = i_var15;

                    for(int l_var16 = 0; l_var16 < 3; l_var16++)
                    {
                        uint b_var3_inner = l_var16 switch
                        {
                            0 => local_f0,
                            1 => local_f1,
                            _ => local_f2
                        };

                        byte u_var1 = (byte)((b_var3_inner << 4) & 0xFF);
                        byte u_var27 = (byte)(u_var6 | u_var1);
                        u_var26 = (byte)(((u_var26 >> 4) | ((b_var3_inner >> 4) << 4)) & 0xFF);

                        int idx1 = (l_var2 + (i_var25 - 1)) * 0x100 + u_var27;
                        u_var6 = (byte)(tables["inv_xor"][idx1] & 0xF);

                        int idx2 = (l_var2 + i_var25) * 0x100 + u_var26;
                        byte b_var3_new = tables["inv_xor"][idx2];
                        u_var26 = (byte)((b_var3_new & 0xF) << 4);
                        i_var25 += 2;
                    }

                    state[l_var9_xor + l_var21_xor * 8] = (byte)((u_var26 | u_var6) & 0xFF);
                    pb_var18_offset += 4;
                }

                i_var15 += 6;
            }
        }

        if(param3 == 1)
        {
            Array.Copy(state, tmp32, 32);
            byte u_var8 = 1;
            byte u_var10 = 3;
            byte u_var12 = 2;

            for(int row = 0; row < 4; row++)
            {
                int idx0 = tmp32[row] + row * 0x400;
                state[row] = tables["inv_first"][idx0];

                byte row1 = (byte)(u_var10 & 3);
                int idx1 = tmp32[8 + row1] + row1 * 0x400 + 0x100;
                state[8 + row] = tables["inv_first"][idx1];

                byte row2 = (byte)(u_var12 & 3);
                int idx2 = tmp32[0x10 + row2] + row2 * 0x400 + 0x200;
                state[0x10 + row] = tables["inv_first"][idx2];

                byte row3 = (byte)(u_var8 & 3);
                int idx3 = tmp32[0x18 + row3] + row3 * 0x400 + 0x300;
                state[0x18 + row] = tables["inv_first"][idx3];

                u_var8++;
                u_var10++;
                u_var12++;
            }
        }

        for(int col = 0; col < 4; col++)
        {
            for(int row = 0; row < 4; row++)
            {
                output[col + row * 4] = state[col * 8 + row];
            }
        }

        return output;
    }

    /// <summary>
    /// Encrypt a single 16-byte block using white-box AES tables.
    /// </summary>
    /// <param name="tables">Pre-loaded lookup tables.</param>
    /// <param name="block">16-byte plaintext block.</param>
    /// <param name="roundEnd">Ending round (default 10 = full encryption).</param>
    /// <returns>16-byte encrypted block.</returns>
    /// <exception cref="BangcleException"></exception>
    public static byte[] EncryptBlockAuth(Dictionary<string, byte[]> tables, byte[] block, int roundEnd = 10)
    {
        byte[] state = new byte[32];
        byte[] temp64 = new byte[64];
        byte[] tmp32 = new byte[32];
        byte[] output = new byte[16];

        PrepareAesMatrix(block, state);
        int param3 = roundEnd;

        int rounds = Math.Min(9, Math.Max(0, param3));
        for(int rnd = 0; rnd < rounds; rnd++)
        {
            int l_var21 = rnd * 4;
            int permPtr = 0;

            for(int i = 0; i < 4; i++)
            {
                byte b_var4 = tables["perm_encrypt"][permPtr];
                int l_var16 = i * 8;
                int baseIdx = i * 16;

                for(int j = 0; j < 4; j++)
                {
                    byte u_var8 = (byte)((b_var4 + j) & 3);
                    byte byteVal = state[l_var16 + u_var8];
                    int idx = byteVal + (i + (l_var21 + u_var8) * 4) * 256;

                    // Extract 4 bytes as little-endian integer from round table
                    var value = BinaryPrimitives.ReadUInt32LittleEndian(tables["round"].AsSpan(idx * 4, 4));
                    BinaryPrimitives.WriteUInt32LittleEndian(temp64.AsSpan(baseIdx + (j * 4)), value);
                }

                permPtr += 2;
            }

            int i_var16 = 1;
            for(int l_var22 = 0; l_var22 < 4; l_var22++)
            {
                int pb_var19_offset = l_var22;

                for(int l_var10 = 0; l_var10 < 4; l_var10++)
                {
                    uint local10 = temp64[pb_var19_offset];
                    byte u_var7 = (byte)(local10 & 0xF);
                    byte u_var26 = (byte)(local10 & 0xF0);

                    uint local_f0 = temp64[pb_var19_offset + 0x10];
                    uint local_f1 = temp64[pb_var19_offset + 0x20];
                    uint local_f2 = temp64[pb_var19_offset + 0x30];

                    int l_var2 = l_var10 * 0x18 + rnd * 0x60;
                    int i_var25 = i_var16;

                    for(int l_var17 = 0; l_var17 < 3; l_var17++)
                    {
                        uint b_var4_inner = l_var17 switch
                        {
                            0 => local_f0,
                            1 => local_f1,
                            _ => local_f2
                        };

                        byte u_var1 = (byte)((b_var4_inner << 4) & 0xFF);
                        byte u_var27 = (byte)(u_var7 | u_var1);
                        u_var26 = (byte)(((u_var26 >> 4) | ((b_var4_inner >> 4) << 4)) & 0xFF);

                        int idx1 = (l_var2 + (i_var25 - 1)) * 0x100 + u_var27;
                        u_var7 = (byte)(tables["xor"][idx1] & 0xF);

                        int idx2 = (l_var2 + i_var25) * 0x100 + u_var26;
                        byte b_var4_new = tables["xor"][idx2];
                        u_var26 = (byte)((b_var4_new & 0xF) << 4);
                        i_var25 += 2;
                    }

                    state[l_var10 + l_var22 * 8] = (byte)((u_var26 | u_var7) & 0xFF);
                    pb_var19_offset += 4;
                }

                i_var16 += 6;
            }
        }

        if(param3 == 10)
        {
            Array.Copy(state, tmp32, 32);
            byte u_var13 = 3;
            byte u_var9 = 2;
            byte u_var11 = 1;
            byte u_var8_enc = 0;

            for(int row = 0; row < 4; row++)
            {
                byte row0 = (byte)((u_var8_enc + row) & 3);
                state[row] = tables["final"][tmp32[row0] + row0 * 0x400];

                byte row1 = (byte)((u_var11 + row) & 3);
                state[8 + row] = tables["final"][tmp32[8 + row1] + row1 * 0x400 + 0x100];

                byte row2 = (byte)((u_var9 + row) & 3);
                state[0x10 + row] = tables["final"][tmp32[0x10 + row2] + row2 * 0x400 + 0x200];

                byte row3 = (byte)((u_var13 + row) & 3);
                state[0x18 + row] = tables["final"][tmp32[0x18 + row3] + row3 * 0x400 + 0x300];
            }
        }

        for(int col = 0; col < 4; col++)
        {
            for(int row = 0; row < 4; row++)
            {
                output[col + row * 4] = state[col * 8 + row];
            }
        }

        return output;
    }

    /// <summary>
    /// Decrypt data using white-box AES in CBC mode.
    /// </summary>
    /// <param name="tables">Pre-loaded lookup tables.</param>
    /// <param name="data">Ciphertext (must be a multiple of 16 bytes).</param>
    /// <param name="iv">16-byte initialization vector.</param>
    /// <returns>Decrypted plaintext.</returns>
    /// <exception cref="BangcleException"></exception>
    public static byte[] DecryptCbc(Dictionary<string, byte[]> tables, byte[] data, byte[] iv)
    {
        if(data.Length % 16 != 0)
            throw new BangcleException($"Ciphertext length {data.Length} is not a multiple of 16");
        if(iv.Length != 16)
            throw new BangcleException($"IV must be 16 bytes, got {iv.Length}");

        byte[] result = new byte[data.Length];
        byte[] prev = new byte[16];
        Array.Copy(iv, prev, 16);

        for(int offset = 0; offset < data.Length; offset += 16)
        {
            byte[] block = new byte[16];
            Array.Copy(data, offset, block, 0, 16);

            byte[] decrypted = DecryptBlockAuth(tables, block, 1);

            for(int i = 0; i < 16; i++)
            {
                decrypted[i] ^= prev[i];
            }

            Array.Copy(decrypted, 0, result, offset, 16);
            Array.Copy(block, prev, 16);
        }

        return result;
    }

    /// <summary>
    /// Encrypt data using white-box AES in CBC mode.
    /// </summary>
    /// <param name="tables">Pre-loaded lookup tables.</param>
    /// <param name="data">Plaintext (must be a multiple of 16 bytes).</param>
    /// <param name="iv">16-byte initialization vector.</param>
    /// <returns>Ciphertext.</returns>
    /// <exception cref="BangcleException"></exception>
    public static byte[] EncryptCbc(Dictionary<string, byte[]> tables, byte[] data, byte[] iv)
    {
        if(data.Length % 16 != 0)
            throw new BangcleException($"Plaintext length {data.Length} is not a multiple of 16");
        if(iv.Length != 16)
            throw new BangcleException($"IV must be 16 bytes, got {iv.Length}");

        byte[] result = new byte[data.Length];
        byte[] prev = new byte[16];
        Array.Copy(iv, prev, 16);

        for(int offset = 0; offset < data.Length; offset += 16)
        {
            byte[] block = new byte[16];
            Array.Copy(data, offset, block, 0, 16);

            for(int i = 0; i < 16; i++)
            {
                block[i] ^= prev[i];
            }

            byte[] encrypted = EncryptBlockAuth(tables, block, 10);
            Array.Copy(encrypted, 0, result, offset, 16);
            Array.Copy(encrypted, prev, 16);
        }

        return result;
    }
}