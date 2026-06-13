using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using BydClient.Config;
using BydClient.Models;
using BydClient.Transport;

namespace BydClient.Api;

public static class SmartChargingApi
{
    private const string TOGGLE_ENDPOINT = "/control/smartCharge/changeChargeStatue";
    private const string SAVE_ENDPOINT = "/control/smartCharge/saveOrUpdate";

    /// <summary>
    /// Toggle smart charging on or off.
    /// </summary>
    public static async Task<CommandAck> ToggleSmartChargingAsync(
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
        inner["smartChargeSwitch"] = enable ? "1" : "0";

        var decoded = await TokenJson.PostTokenJsonAsync(
            TOGGLE_ENDPOINT,
            config,
            session,
            transport,
            inner,
            null,
            vin
        );

        var raw = NormalizeToDictionary(decoded);

        var data = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["vin"] = vin,
            ["raw"] = raw
        };
        foreach(var kv in raw) data[kv.Key] = kv.Value;

        return new CommandAck(data);
    }

    /// <summary>
    /// Save a smart charging schedule.
    /// </summary>
    public static async Task<CommandAck> SaveChargingScheduleAsync(
        BydConfig config,
        Session session,
        ITransport transport,
        string vin,
        int targetSoc,
        int startHour,
        int startMinute,
        int endHour,
        int endMinute,
        CancellationToken cancellationToken = default)
    {
        if(config == null) throw new ArgumentNullException(nameof(config));
        if(session == null) throw new ArgumentNullException(nameof(session));
        if(transport == null) throw new ArgumentNullException(nameof(transport));
        if(string.IsNullOrEmpty(vin)) throw new ArgumentException("vin is required", nameof(vin));

        var inner = Common.BuildInnerBase(config, null, vin);

        // Merge schedule parameters as strings to match PHP behavior
        inner["endHour"] = endHour.ToString();
        inner["endMinute"] = endMinute.ToString();
        inner["startHour"] = startHour.ToString();
        inner["startMinute"] = startMinute.ToString();
        inner["targetSoc"] = targetSoc.ToString();

        var decoded = await TokenJson.PostTokenJsonAsync(
            SAVE_ENDPOINT,
            config,
            session,
            transport,
            inner,
            null,
            vin
        );

        var raw = NormalizeToDictionary(decoded);

        var data = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["vin"] = vin,
            ["raw"] = raw
        };
        foreach(var kv in raw) data[kv.Key] = kv.Value;

        return new CommandAck(data);
    }

    // Helper: normalize dynamic decoded response into IDictionary<string, object?>
    private static IDictionary<string, object?> NormalizeToDictionary(object? decoded)
    {
        if(decoded is IDictionary<string, object?> dict) return dict;

        if(decoded is JsonElement je && je.ValueKind == JsonValueKind.Object)
        {
            return JsonElementToDictionary(je);
        }

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
            default:
                return null;
        }
    }
}
