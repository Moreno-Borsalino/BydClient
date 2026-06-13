using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using BydClient.Config;
using BydClient.Transport;

namespace BydClient.Api;

    public static class RealtimeApi
    {
        /// <summary>
        /// Fetch a single realtime endpoint, returning (vehicleInfo, nextSerial).
        /// </summary>
        public static async Task<(IDictionary<string, object?> vehicleInfo, string? nextSerial)> FetchRealtimeEndpointAsync(
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

            // now in milliseconds
            long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // Build inner payload. Adjust signature of Common.BuildInnerBase if needed.
            var inner = Common.BuildInnerBase(config, nowMs, vin, requestSerial);

            // Ensure required fields
            inner["energyType"] = "0";
            inner["tboxVersion"] = config.GetTboxVersion();

            // Post and decode. TokenJson.PostTokenJson should return a JsonElement, IDictionary<string,object?>, or similar.
            var decoded = await TokenJson.PostTokenJsonAsync(endpoint, config, session, transport, inner, nowMs, vin);

            // Normalize decoded into a dictionary
            IDictionary<string, object?> vehicleInfoDict = decoded switch
            {
                IDictionary<string, object?> dict => dict,
                //JsonElement je when je.ValueKind == JsonValueKind.Object => JsonElementToDictionary(je),
                //JsonElement je when je.ValueKind == JsonValueKind.Array => new Dictionary<string, object?> { ["items"] = JsonElementToObject(je) },
                _ => new Dictionary<string, object?>()
            };

            // Determine next serial: prefer requestSerial field from response, fallback to provided requestSerial
            string? nextSerial = null;
            if(vehicleInfoDict.TryGetValue("requestSerial", out var rsObj) && rsObj != null)
            {
                nextSerial = rsObj.ToString();
            }
            else
            {
                nextSerial = requestSerial;
            }

            return (vehicleInfoDict, nextSerial);
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
