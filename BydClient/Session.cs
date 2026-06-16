using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace BydClient;

public class Session
{
    private readonly DateTime _createdAt;
    private string _contentKeyCache = string.Empty;
    private string _signKeyCache = string.Empty;

    public string UserId { get; }
    public string SignToken { get; }
    public string EncryToken { get; }
    public double Ttl { get; } // in seconds

    public Session(string userId, string signToken, string encryToken, double ttl = 43200, DateTime? createdAt = null)
    {
        UserId = userId;
        SignToken = signToken;
        EncryToken = encryToken;
        Ttl = ttl;
        _createdAt = createdAt ?? DateTime.UtcNow;
    }

    /// <summary>
    /// AES key for encrypting/decrypting inner payload data.
    /// Derived as MD5(encry_token) in uppercase hex.
    /// </summary>
    public string ContentKey()
    {
        if (string.IsNullOrEmpty(_contentKeyCache))
        {
            _contentKeyCache = Md5Hex(EncryToken);
        }

        return _contentKeyCache;
    }

    /// <summary>
    /// Key used in request signature computation.
    /// Derived as MD5(sign_token) in uppercase hex.
    /// </summary>
    public string SignKey()
    {
        if (string.IsNullOrEmpty(_signKeyCache))
        {
            _signKeyCache = Md5Hex(SignToken);
        }

        return _signKeyCache;
    }

    /// <summary>
    /// Whether the session has exceeded its TTL.
    /// </summary>
    public bool IsExpired()
    {
        return (DateTime.UtcNow - _createdAt).TotalSeconds >= Ttl;
    }

    /// <summary>
    /// Seconds since the session was created.
    /// </summary>
    public double Age()
    {
        return (DateTime.UtcNow - _createdAt).TotalSeconds;
    }

    // --- Private helper ---
    private static string Md5Hex(string input)
    {
        using var md5 = MD5.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = md5.ComputeHash(bytes);

        var sb = new StringBuilder();
        foreach (var b in hash)
            sb.Append(b.ToString("X2")); // Uppercase hex

        return sb.ToString();
    }
}
