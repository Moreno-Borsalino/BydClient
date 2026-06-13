using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using BydClient.Config;
using BydClient.Transport;

namespace BydClient.Api;

    public static class GpsApi
    {
        /// <summary>
        /// Check if GPS data has meaningful content.
        /// Mirrors PHP: returns false for empty payloads or payloads that only contain requestSerial.
        /// </summary>
        public static bool IsGpsInfoReady(IDictionary<string, object?> gpsInfo)
        {
            if(gpsInfo == null || gpsInfo.Count == 0) return false;
            if(gpsInfo.Count == 1 && gpsInfo.ContainsKey("requestSerial")) return false;
            return true;
        }

        /// <summary>
        /// Fetch a single GPS endpoint, returning (gpsInfo, nextSerial).
        /// gpsInfo is normalized to IDictionary<string, object?>; nextSerial is response requestSerial or the provided requestSerial.
        /// </summary>
        public static async Task<(IDictionary<string, object?> gpsInfo, string? nextSerial)> FetchGpsEndpointAsync(
            string endpoint,
            BydConfig config,
            Session session,
            ITransport transport,
            string vin,
            string? requestSerial = null,
            CancellationToken cancellationToken = default)
        {
            if(string.IsNullOrEmpty(endpoint)) throw new ArgumentException("endpoint is required", nameof(endpoint));
            if(config == null) throw new ArgumentNullException(nameof(config));
            if(session == null) throw new ArgumentNullException(nameof(session));
            if(transport == null) throw new ArgumentNullException(nameof(transport));
            if(string.IsNullOrEmpty(vin)) throw new ArgumentException("vin is required", nameof(vin));

            // Build inner payload. Adjust signature of Common.BuildInnerBase if needed.
            var inner = Common.BuildInnerBase(config, null, vin, requestSerial);

            // Post and decode. TokenJson.PostTokenJson should return a JsonElement, IDictionary<string,object?>, or similar.
            var decoded = await TokenJson.PostTokenJsonAsync(endpoint, config, session, transport, inner, null, vin);

            // If decoded is not an object/dictionary, return empty gpsInfo and original requestSerial
            IDictionary<string, object?> gpsInfoDict = decoded switch
            {
                IDictionary<string, object?> dict => dict,
                //JsonElement je when je.ValueKind == JsonValueKind.Object => JsonElementToDictionary(je),
                _ => new Dictionary<string, object?>()
            };

            // Determine nextSerial: prefer requestSerial field from response, fallback to provided requestSerial
            string? nextSerial = null;
            if(gpsInfoDict.TryGetValue("requestSerial", out var rsObj) && rsObj is string rsStr)
            {
                nextSerial = rsStr;
            }
            else
            {
                nextSerial = requestSerial;
            }

            return (gpsInfoDict, nextSerial);
        }

        // Helper: convert JsonElement object to dictionary recursively
        private static IDictionary<string, object?> JsonElementToDictionary(JsonElement element)
        {
            var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach(var prop in element.EnumerateObject())
            {
                dict[prop.Name] = JsonElementToObject(prop.Value);
            }
            return dict;
        }

        private static object? JsonElementToObject(JsonElement el)
        {
            switch(el.ValueKind)
            {
                case JsonValueKind.Object:
                    return JsonElementToDictionary(el);
                case JsonValueKind.Array:
                    var list = new List<object?>();
                    foreach(var item in el.EnumerateArray())
                        list.Add(JsonElementToObject(item));
                    return list;
                case JsonValueKind.String:
                    return el.GetString();
                case JsonValueKind.Number:
                    if(el.TryGetInt64(out var l)) return l;
                    if(el.TryGetDouble(out var d)) return d;
                    return el.GetDecimal();
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                default:
                    return null;
            }
        }
    }
