using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using BydClient.Config;
using BydClient.Models;
using BydClient.Transport;

namespace BydClient.Api;

    public static class VehicleApi
    {
        /// <summary>
        /// Fetch all vehicles associated with the authenticated user.
        /// </summary>
        public static async Task<List<Vehicle>> FetchVehicleListAsync(BydConfig config, Session session, ITransport transport, CancellationToken cancellationToken = default)
        {
            if(config == null) throw new ArgumentNullException(nameof(config));
            if(session == null) throw new ArgumentNullException(nameof(session));
            if(transport == null) throw new ArgumentNullException(nameof(transport));

            // Build inner payload (assumes Common.BuildInnerBase returns IDictionary<string, object?> or JsonElement)
            var inner = Common.BuildInnerBase(config);

            // Post and get decoded JSON. TokenJson.PostTokenJson should return a JsonElement or object representing the decoded JSON.
            // Adjust TokenJson.PostTokenJson signature in your codebase if needed.
            var decodedObj = await TokenJson.PostTokenJsonAsync("/app/account/getAllListByUserId", config, session, transport, inner);

            var vehicles = new List<Vehicle>();

            // Handle different possible return types from TokenJson.PostTokenJson
            //if(decodedObj is JsonElement je)
            //{
            //    if(je.ValueKind == JsonValueKind.Array)
            //    {
            //        foreach(var item in je.EnumerateArray())
            //        {
            //            if(item.ValueKind == JsonValueKind.Object)
            //            {
            //                var dict = JsonElementToDictionary(item);
            //                vehicles.Add(new Vehicle(dict));
            //            }
            //        }
            //    }
            //}
            //else if(decodedObj is IEnumerable<object> objEnum)
            //{
            //    foreach(var item in objEnum)
            //    {
            //        if(item is IDictionary<string, object?> dict)
            //        {
            //            vehicles.Add(new Vehicle(dict));
            //        }
            //        else if(item is JsonElement itemJe && itemJe.ValueKind == JsonValueKind.Object)
            //        {
            //            vehicles.Add(new Vehicle(JsonElementToDictionary(itemJe)));
            //        }
            //    }
            //}
            //else if(decodedObj is IDictionary<string, object?> dictSingle)
            if(decodedObj is IDictionary<string, object?> dictSingle)
            {
                // If API returns a single object or keyed collection, try to extract array-like values
                if(dictSingle.TryGetValue("items", out var itemsObj) && itemsObj is IEnumerable<object> itemsEnum)
                {
                    foreach(var it in itemsEnum)
                    {
                        if(it is IDictionary<string, object?> d) vehicles.Add(new Vehicle(d));
                        else if(it is JsonElement itJe && itJe.ValueKind == JsonValueKind.Object) vehicles.Add(new Vehicle(JsonElementToDictionary(itJe)));
                    }
                }
            }

            return vehicles;
        }

        /// <summary>
        /// Convert a JsonElement (object) into a dictionary of string -> object? suitable for model constructors.
        /// Nested objects/arrays are converted to dictionaries/lists recursively.
        /// </summary>
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
