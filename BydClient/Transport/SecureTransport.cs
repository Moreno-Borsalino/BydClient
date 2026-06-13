using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BydClient.Config;
using BydClient.Crypto;
using BydClient.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace BydClient.Transport;

/// <summary>
/// HTTP transport that handles Bangcle envelope encoding.
/// </summary>
public class SecureTransport : ITransport
{
    private readonly BydConfig _config;
    private readonly BangcleCodec _codec;
    private readonly HttpClient _httpClient;
    private readonly ILogger<SecureTransport> _logger;

    public SecureTransport(
        BydConfig config,
        BangcleCodec codec,
        HttpClient httpClient,
        ILogger<SecureTransport>? logger = null)
    {
        _config = config;
        _codec = codec;
        _httpClient = httpClient;
        _logger = logger ?? NullLogger<SecureTransport>.Instance;
    }

    /// <summary>
    /// Send a signed request through the Bangcle envelope layer.
    /// </summary>
    public async Task<Dictionary<string, object>> PostSecureAsync(
        string endpoint,
        Dictionary<string, object> outerPayload, CancellationToken cancellationToken = default)
    {
        string jsonPayload;

        // JSON encode outer payload
        try
        {
            jsonPayload = JsonSerializer.Serialize(outerPayload);
        }
        catch (Exception ex)
        {
            throw new BydTransportException(
                "Failed to encode payload to JSON",
                0,
                endpoint,
                ex);
        }

        // Bangcle encode
        string encoded = _codec.EncodeEnvelope(jsonPayload);

        string url =
            $"{_config.BaseUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";

        var bodyObject = new Dictionary<string, string>
        {
            ["request"] = encoded
        };

        string body = JsonSerializer.Serialize(bodyObject);

        _logger.LogDebug("HTTP POST {Url}", url);

        HttpResponseMessage response;
        string text;

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url);

            request.Headers.TryAddWithoutValidation("accept-encoding", "identity");
            request.Headers.TryAddWithoutValidation(
                "user-agent",
                "BYD/3.2.2 (iPhone; iOS 15.0; Scale/3.00)");

            request.Content = new StringContent(
                body,
                Encoding.UTF8,
                "application/json");

            response = await _httpClient.SendAsync(request);
            text = await response.Content.ReadAsStringAsync();

            if ((int)response.StatusCode != 200)
            {
                throw new BydTransportException(
                    $"HTTP {(int)response.StatusCode} from {endpoint}: " +
                    Truncate(text, 200),
                    (int)response.StatusCode,
                    endpoint);
            }
        }
        catch (HttpRequestException ex)
        {
            throw new BydTransportException(
                $"Request to {endpoint} failed: {ex.Message}",
                0,
                endpoint,
                ex);
        }

        // Parse outer response JSON
        Dictionary<string, JsonElement>? bodyJson;

        try
        {
            bodyJson = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(text);
        }
        catch
        {
            bodyJson = null;
        }

        if (bodyJson == null)
        {
            throw new BydTransportException(
                $"Invalid JSON from {endpoint}: {Truncate(text, 200)}",
                0,
                endpoint);
        }

        if (!bodyJson.TryGetValue("response", out var responseElement) ||
            responseElement.ValueKind != JsonValueKind.String)
        {
            throw new BydTransportException(
                $"Missing or invalid \"response\" field from {endpoint}",
                0,
                endpoint);
        }

        string responseStr = responseElement.GetString()?.Trim() ?? "";

        if (string.IsNullOrEmpty(responseStr))
        {
            throw new BydTransportException(
                $"Empty response payload from {endpoint}",
                0,
                endpoint);
        }

        // Decode Bangcle envelope
        string decodedText = _codec.DecodeEnvelope(responseStr);

        string decodedString = decodedText.Trim();

        // Handle stray F prefix bug
        if (decodedString.StartsWith("F{") || decodedString.StartsWith("F["))
        {
            decodedString = decodedString.Substring(1);
        }

        Dictionary<string, object>? result;

        try
        {
            result = JsonSerializer.Deserialize<Dictionary<string, object>>(
                decodedString,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
        catch
        {
            result = null;
        }

        if (result == null)
        {
            throw new BydTransportException(
                $"Bangcle response from {endpoint} is not JSON: " +
                Truncate(decodedString, 64),
                0,
                endpoint);
        }

        return result;
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value.Length <= maxLength
            ? value
            : value.Substring(0, maxLength);
    }
}