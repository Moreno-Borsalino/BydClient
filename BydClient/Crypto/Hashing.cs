using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace BydClient.Crypto;

/// <summary>
/// Hashing utilities.
/// </summary>
public static class Hashing
{
    /// <summary>
    /// Compute MD5 of a UTF-8 string, returning uppercase hex.
    /// Mirrors JS: crypto.createHash('md5').update(value, 'utf8').digest('hex').toUpperCase()
    /// </summary>
    public static string Md5Hex(string input)
    {
        using var md5 = MD5.Create();
        byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
    }

    /// <summary>
    /// Derive the login AES key from a plaintext password.
    /// Mirrors JS: md5Hex(md5Hex(password))
    /// </summary>
    public static string PwdLoginKey(string password)
    {
        return Md5Hex(Md5Hex(password));
    }

    /// <summary>
    /// Compute SHA1 with alternating-case hex and zero filtering.
    /// Algorithm (from client.js lines 58-76):
    ///   1. SHA1 digest of UTF-8 encoded value -> 20 bytes
    ///   2. For each byte at index i, format as 2-char hex:
    ///      - Even i: uppercase
    ///      - Odd i: lowercase
    ///   3. Concatenate into a 40-char string
    ///   4. Filter: drop any '0' character that falls at an even position
    /// </summary>
    public static string Sha1Mixed(string input)
    {
        using var sha1 = SHA1.Create();
        byte[] digest = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
        var sb = new StringBuilder(digest.Length * 2);

        for(int i = 0; i < digest.Length; i++)
        {
            string hex = digest[i].ToString("x2"); // lowercase base
            sb.Append(i % 2 == 0 ? hex.ToUpperInvariant() : hex);
        }

        string mixed = sb.ToString();
        var filteredSb = new StringBuilder(mixed.Length);

        for(int j = 0; j < mixed.Length; j++)
        {
            if(mixed[j] == '0' && j % 2 == 0)
                continue;

            filteredSb.Append(mixed[j]);
        }

        return filteredSb.ToString();
    }

    /// <summary>
    /// Compute checkcode: MD5 of compact JSON with chunk reordering.
    /// The MD5 hex digest is reordered as: [24:32] + [8:16] + [16:24] + [0:8]
    /// </summary>
    public static string ComputeCheckcode(IDictionary<string, object> payload)
    {
        // Sort keys lexicographically to mirror PHP's ksort()
        var sortedDict = new SortedDictionary<string, object>(payload, StringComparer.Ordinal);

        // Serialize to compact JSON without escaping slashes or unicode
        var jsonOptions = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = false
        };
        string jsonStr = JsonSerializer.Serialize(sortedDict, jsonOptions);

        using var md5 = MD5.Create();
        byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(jsonStr));
        string md5Hex = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

        // Reorder chunks: [24:32] + [8:16] + [16:24] + [0:8]
        return md5Hex.Substring(24, 8) + md5Hex.Substring(8, 8) + md5Hex.Substring(16, 8) + md5Hex.Substring(0, 8);
    }
}
