using System;
using System.Collections.Generic;
using System.Text;

namespace BydClient.Crypto;

/// <summary>
/// PKCS#7 padding utilities for the Bangcle white-box AES layer.
/// Ports addPkcs7 and stripPkcs7 from bangcle.js.
/// </summary>
public static class Pkcs7
{
    /// <summary>
    /// Adds PKCS#7 padding to a byte array.
    /// If the data length is already a multiple of blockSize, a full block of padding is appended.
    /// </summary>
    /// <param name="data">Data to pad.</param>
    /// <param name="blockSize">Block size in bytes (default 16).</param>
    /// <returns>Padded data whose length is a multiple of blockSize.</returns>
    public static byte[] AddPkcs7(byte[] data, int blockSize = 16)
    {
        if(data == null) throw new ArgumentNullException(nameof(data));
        if(blockSize <= 0 || blockSize > 255) throw new ArgumentOutOfRangeException(nameof(blockSize));

        int dataLength = data.Length;
        int remainder = dataLength % blockSize;
        int padLen = remainder == 0 ? blockSize : blockSize - remainder;

        byte[] result = new byte[dataLength + padLen];
        Array.Copy(data, result, dataLength);

        // Fill padding bytes with the padding length value
        for(int i = 0; i < padLen; i++)
        {
            result[dataLength + i] = (byte)padLen;
        }

        return result;
    }

    /// <summary>
    /// Strips PKCS#7 padding from a byte array.
    /// Returns the original data if padding is invalid.
    /// </summary>
    /// <param name="data">Potentially padded data.</param>
    /// <returns>Unpadded data, or the original data if padding is invalid.</returns>
    public static byte[] StripPkcs7(byte[] data)
    {
        if(data == null || data.Length == 0) return data!;

        int dataLength = data.Length;
        int pad = data[dataLength - 1];

        if(pad == 0 || pad > 16) return data;
        if(dataLength < pad) return data;

        // Validate all padding bytes
        for(int i = dataLength - pad; i < dataLength; i++)
        {
            if(data[i] != (byte)pad) return data;
        }

        byte[] result = new byte[dataLength - pad];
        Array.Copy(data, result, dataLength - pad);
        return result;
    }

    /// <summary>
    /// String overload that converts to/from UTF-8 before applying PKCS#7 padding.
    /// </summary>
    public static string AddPkcs7(string data, int blockSize = 16) =>
        Encoding.UTF8.GetString(AddPkcs7(Encoding.UTF8.GetBytes(data), blockSize));

    /// <summary>
    /// String overload that converts to/from UTF-8 before stripping PKCS#7 padding.
    /// </summary>
    public static string StripPkcs7(string data) =>
        Encoding.UTF8.GetString(StripPkcs7(Encoding.UTF8.GetBytes(data)));
}
