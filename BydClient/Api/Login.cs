using BydClient.Config;
using BydClient.Crypto;
using BydClient.Exceptions;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BydClient.Models;

namespace BydClient.Api;

/// <summary>
/// Login endpoint handler.
/// </summary>
public static class Login
{
    private static Dictionary<string, string> CommonOuterFields(BydConfig config)
    {
        var device = config.Device;

        return new Dictionary<string, string>
        {
            ["ostype"] = device.OsType,
            ["imei"] = device.Imei,
            ["mac"] = device.Mac,
            ["model"] = device.Model,
            ["sdk"] = device.Sdk,
            ["mod"] = device.Mod
        };
    }

    public static Dictionary<string, object> BuildLoginRequest(BydConfig config, long nowMs)
    {
        // random hex 16 bytes uppercase
        var randomBytes = RandomNumberGenerator.GetBytes(16);
        var randomHex = Convert.ToHexString(randomBytes).ToUpperInvariant();

        var reqTimestamp = nowMs.ToString();
        var serviceTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

        var device = config.Device;

        var inner = new Dictionary<string, string?>
        {
            ["appInnerVersion"] = config.AppInnerVersion,
            ["appVersion"] = config.AppVersion,
            ["deviceName"] = device.MobileBrand + device.MobileModel,
            ["deviceType"] = device.DeviceType,
            ["imeiMD5"] = device.ImeiMd5,
            ["isAuto"] = config.IsAuto,
            ["mobileBrand"] = device.MobileBrand,
            ["mobileModel"] = device.MobileModel,
            ["networkType"] = device.NetworkType,
            ["osType"] = device.OsType,
            ["osVersion"] = device.OsVersion,
            ["random"] = randomHex,
            ["softType"] = config.SoftType,
            ["timeStamp"] = reqTimestamp,
            ["timeZone"] = config.TimeZone
        };

        var jsonOptions = new JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = false
        };

        string innerJson = JsonSerializer.Serialize(inner, jsonOptions);

        // AES encryption to hex string using password-derived key
        string encryData = BydClient.Crypto.Aes.AesEncryptHex(innerJson, Hashing.PwdLoginKey(config.Password));

        string passwordMd5 = Hashing.Md5Hex(config.Password);

        // Build sign fields
        var signFields = new Dictionary<string, string?>(inner)
        {
            ["countryCode"] = config.CountryCode,
            ["functionType"] = "pwdLogin",
            ["identifier"] = config.Username,
            ["identifierType"] = "0",
            ["language"] = config.Language,
            ["reqTimestamp"] = reqTimestamp
        };

        string signString = Signing.BuildSignString(signFields, passwordMd5);
        string sign = Hashing.Sha1Mixed(signString);

        // Outer payload
        var outer = new Dictionary<string, object>
        {
            ["countryCode"] = config.CountryCode,
            ["encryData"] = encryData,
            ["functionType"] = "pwdLogin",
            ["identifier"] = config.Username,
            ["identifierType"] = "0",
            ["imeiMD5"] = device.ImeiMd5,
            ["isAuto"] = config.IsAuto,
            ["language"] = config.Language,
            ["reqTimestamp"] = reqTimestamp,
            ["sign"] = sign,
            ["signKey"] = config.Password,
            ["serviceTime"] = serviceTime
        };

        // merge common outer fields
        foreach(var kv in CommonOuterFields(config))
        {
            outer[kv.Key] = kv.Value;
        }

        // compute checkcode
        outer["checkcode"] = Hashing.ComputeCheckcode(outer);

        return outer;
    }

    /// <summary>
    /// Parse login response and extract the auth token.
    /// </summary>
    /// <param name="outerResponse">Response payload parsed as dictionary</param>
    /// <param name="password">User password (used to derive AES key)</param>
    /// <returns>AuthToken</returns>
    /// <exception cref="BydAuthenticationException"></exception>
    public static AuthToken ParseLoginResponse(IDictionary<string, object> outerResponse, string password)
    {
        if(outerResponse == null) throw new ArgumentNullException(nameof(outerResponse));

        // Check code == "0"
        outerResponse.TryGetValue("code", out var codeObj);
        var codeStr = codeObj?.ToString() ?? string.Empty;
        if(codeStr != "0")
        {
            outerResponse.TryGetValue("message", out var msgObj);
            var msg = msgObj?.ToString() ?? string.Empty;
            throw new BydAuthenticationException(
                $"Login failed: code={codeStr} message={msg}",
                codeStr != string.Empty ? 0 : int.Parse(codeStr),
                "/app/account/login"
            );
        }

        // Get respondData
        outerResponse.TryGetValue("respondData", out var respondDataObj);
        if(respondDataObj == null)
        {
            throw new BydAuthenticationException(
                "Login response missing respondData",
                0,
                "/app/account/login"
            );
        }

        // Decrypt respondData (expects AesUtil.AesDecryptUtf8 to return plaintext JSON)
        string respondDataCipher = respondDataObj.ToString() ?? string.Empty;
        string plaintext;
        try
        {
            plaintext = BydClient.Crypto.Aes.AesDecryptUtf8(respondDataCipher, Hashing.PwdLoginKey(password));
        }
        catch(Exception ex)
        {
            throw new BydAuthenticationException(
                "Failed to decrypt login response",
                0,
                "/app/account/login"
            );
        }

        // Parse JSON into JsonDocument for robust checks
        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(plaintext);
        }
        catch(JsonException)
        {
            throw new BydAuthenticationException(
                "Failed to decode login response",
                0,
                "/app/account/login"
            );
        }

        if(!doc.RootElement.TryGetProperty("token", out var tokenElement) || tokenElement.ValueKind != JsonValueKind.Object)
        {
            throw new BydAuthenticationException(
                "Login response missing token fields",
                0,
                "/app/account/login"
            );
        }

        // Ensure required token fields exist
        if(!tokenElement.TryGetProperty("userId", out var userIdEl) ||
           !tokenElement.TryGetProperty("signToken", out var signTokenEl) ||
           !tokenElement.TryGetProperty("encryToken", out var encryTokenEl))
        {
            throw new BydAuthenticationException(
                "Login response missing token fields",
                0,
                "/app/account/login"
            );
        }

        var userId = userIdEl.GetString() ?? string.Empty;
        var signToken = signTokenEl.GetString() ?? string.Empty;
        var encryToken = encryTokenEl.GetString() ?? string.Empty;

        if(string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(signToken) || string.IsNullOrEmpty(encryToken))
        {
            throw new BydAuthenticationException(
                "Login response missing token fields",
                0,
                "/app/account/login"
            );
        }

        return new AuthToken
        {
            UserId = userId,
            SignToken = signToken,
            EncryToken = encryToken
        };

        // Extract token fields - adapt keys to actual API response
        //outerResponse.TryGetValue("token", out var tokenObj);
        //outerResponse.TryGetValue("tokenType", out var tokenTypeObj);
        //outerResponse.TryGetValue("expireIn", out var expireInObj);

        //var token = tokenObj?.ToString();
        //var tokenType = tokenTypeObj?.ToString();
        //long.TryParse(expireInObj?.ToString(), out var expiresIn);

        //if(string.IsNullOrEmpty(token))
        //{
        //    throw new BydAuthenticationException("Auth token missing in response");
        //}

        //return new AuthToken
        //{
        //    Token = token,
        //    TokenType = tokenType ?? "Bearer",
        //    ExpiresIn = expiresIn
        //};
    }
}

