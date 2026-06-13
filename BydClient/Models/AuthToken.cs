using System;
using System.Collections.Generic;
using System.Text;

namespace BydClient.Models;

/// <summary>
/// Token returned after successful login.
/// </summary>
public sealed class AuthToken : BaseModel
{
    public string UserId { get; set; } = string.Empty;
    public string SignToken { get; set; } = string.Empty;
    public string EncryToken { get; set; } = string.Empty;

    public AuthToken()
    {
        
    }

    /// <summary>
    /// Construct from a dictionary (equivalent to PHP array<string,mixed>).
    /// </summary>
    public AuthToken(IDictionary<string, object?> data) : base(data)
    {
        if(data == null) throw new ArgumentNullException(nameof(data));

        UserId = data["userId"]?.ToString() ?? string.Empty;
        SignToken = data["signToken"]?.ToString() ?? string.Empty;
        EncryToken = data["encryToken"]?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Factory helper if you prefer to pass a plain Dictionary.
    /// </summary>
    public static AuthToken FromDictionary(IDictionary<string, object?> data) => new AuthToken(data);
}

