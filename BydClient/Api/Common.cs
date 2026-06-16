using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using BydClient.Config;

namespace BydClient.Api;

public static class Common
{
    /// <summary>
    /// Build common inner payload fields used by most BYD endpoints.
    /// </summary>
    public static Dictionary<string, string?> BuildInnerBase(
        BydConfig config,
        long? nowMs = null,
        string? vin = null,
        string? requestSerial = null)
    {
        nowMs ??= DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var inner = new Dictionary<string, string?>
        {
            ["deviceType"] = config.Device?.DeviceType,
            ["imeiMD5"] = config.Device?.ImeiMd5,
            ["networkType"] = config.Device?.NetworkType,
            ["random"] = GenerateRandomHex(16),
            ["timeStamp"] = nowMs.Value.ToString(),
            ["version"] = config.AppInnerVersion
        };

        if (!string.IsNullOrEmpty(vin))
        {
            inner["vin"] = vin;
        }

        if (!string.IsNullOrEmpty(requestSerial))
        {
            inner["requestSerial"] = requestSerial;
        }

        return inner;
    }

    /// <summary>
    /// Generate uppercase random hex string from secure random bytes.
    /// Equivalent to strtoupper(bin2hex(random_bytes(n))).
    /// </summary>
    private static string GenerateRandomHex(int byteCount)
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(byteCount);
        return Convert.ToHexString(bytes); // uppercase by default
    }
}