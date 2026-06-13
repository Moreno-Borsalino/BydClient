using System;
using System.Collections.Generic;
using System.Text;

namespace BydClient.Crypto;

/// <summary>
/// Request signing for BYD API.
/// </summary>
public static class Signing
{
    /// <summary>
    /// Build the sign string by sorting fields and appending password.
    /// </summary>
    /// <param name="fields">Dictionary of fields to sign.</param>
    /// <param name="password">Password to append.</param>
    /// <returns>The formatted sign string.</returns>
    public static string BuildSignString(Dictionary<string, string?> fields, string password)
    {
        if(fields == null) throw new ArgumentNullException(nameof(fields));
        if(password == null) throw new ArgumentNullException(nameof(password));

        // Sort keys alphabetically (case-sensitive, byte-wise comparison matching PHP's ksort)
        var sortedFields = new SortedDictionary<string, string?>(fields, StringComparer.Ordinal);

        var sb = new StringBuilder();
        bool first = true;

        foreach(var kvp in sortedFields)
        {
            if(!first) sb.Append('&');

            sb.Append(kvp.Key);
            sb.Append('=');
            sb.Append(kvp.Value ?? "null");

            first = false;
        }

        sb.Append("&password=");
        sb.Append(password);

        return sb.ToString();
    }
}
