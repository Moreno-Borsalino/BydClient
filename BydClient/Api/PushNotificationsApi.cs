using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using BydClient.Config;
using BydClient.Models;
using BydClient.Transport;

namespace BydClient.Api;

    public static class PushNotificationsApi
    {
        private const string GET_ENDPOINT = "/app/push/getPushSwitchState";
        private const string SET_ENDPOINT = "/app/push/setPushSwitchState";

        /// <summary>
        /// Fetch the current push notification state for a vehicle.
        /// </summary>
        public static async Task<PushNotificationState> FetchPushStateAsync(
            BydConfig config,
            Session session,
            ITransport transport,
            string vin,
            CancellationToken cancellationToken = default)
        {
            if(config == null) throw new ArgumentNullException(nameof(config));
            if(session == null) throw new ArgumentNullException(nameof(session));
            if(transport == null) throw new ArgumentNullException(nameof(transport));
            if(string.IsNullOrEmpty(vin)) throw new ArgumentException("vin is required", nameof(vin));

            var inner = Common.BuildInnerBase(config, null, vin);

            var decoded = await TokenJson.PostTokenJsonAsync(
                GET_ENDPOINT,
                config,
                session,
                transport,
                inner,
                null,
                vin
            );

            var raw = NormalizeToDictionary(decoded);
            // Merge vin into response data (vin takes precedence if missing)
            var data = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
            {
                ["vin"] = vin
            };
            foreach(var kv in raw) data[kv.Key] = kv.Value;

            return new PushNotificationState(data);
        }

        /// <summary>
        /// Set the push notification state for a vehicle.
        /// </summary>
        public static async Task<CommandAck> SetPushStateAsync(
            BydConfig config,
            Session session,
            ITransport transport,
            string vin,
            bool enable,
            CancellationToken cancellationToken = default)
        {
            if(config == null) throw new ArgumentNullException(nameof(config));
            if(session == null) throw new ArgumentNullException(nameof(session));
            if(transport == null) throw new ArgumentNullException(nameof(transport));
            if(string.IsNullOrEmpty(vin)) throw new ArgumentException("vin is required", nameof(vin));

            var inner = Common.BuildInnerBase(config, null, vin);
            inner["pushSwitch"] = enable ? "1" : "0";

            var decoded = await TokenJson.PostTokenJsonAsync(
                SET_ENDPOINT,
                config,
                session,
                transport,
                inner,
                null,
                vin
            );

            var raw = NormalizeToDictionary(decoded);

            // Merge vin and raw into data dictionary; include raw under "raw" key as in PHP
            var data = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
            {
                ["vin"] = vin,
                ["raw"] = raw
            };
            foreach(var kv in raw) data[kv.Key] = kv.Value;

            return new CommandAck(data);
        }

        /// <summary>
        /// Normalize dynamic decoded response into IDictionary<string, object?>.
        /// Accepts IDictionary, JsonElement (object/array), or other types.
        /// </summary>
        private static IDictionary<string, object?> NormalizeToDictionary(object? decoded)
        {
            if(decoded is IDictionary<string, object?> dict) return dict;

            if(decoded is JsonElement je && je.ValueKind == JsonValueKind.Object)
            {
                return JsonElementToDictionary(je);
            }

            // If decoded is null or not an object, return empty dictionary
            return new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        }

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
