using BydClient.Config;
using BydClient.Transport;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using BydClient.Models;

namespace BydClient.Api;

public static class HvacApi
{
    private const string Endpoint = "/control/getStatusNow";

    /// <summary>
    /// Fetch current HVAC/climate control status for a vehicle.
    /// </summary>
    public static async Task<HvacStatus> FetchHvacStatusAsync(
        BydConfig config,
        Session session,
        ITransport transport, // Standard C# interface naming convention
        string vin,
        CancellationToken cancellationToken = default)
    {
        var inner = Common.BuildInnerBase(config, null, vin);

        // Assumed to return a JsonElement, Dictionary, or dynamic object representing the JSON
        var decoded = await TokenJson.PostTokenJsonAsync(
            Endpoint,
            config,
            session,
            transport,
            inner,
            null,
            vin
        );

        // Safe extraction handling both a nested 'statusNow' object or the root object
        //JsonElement hvacData = default;

        //if(decoded.ValueKind == JsonValueKind.Object)
        //{
        //    if(decoded.TryGetProperty("statusNow", out var statusNowProperty))
        //    {
        //        hvacData = statusNowProperty;
        //    }
        //    else
        //    {
        //        hvacData = decoded;
        //    }
        //}

        //if (decoded != null)
        //{
        //    hvacData = decoded.ContainsKey("statusNow") ? decoded["statusNow"] : decoded;
        //}

        var hvacData = decoded != null ?
            (decoded.TryGetValue("statusNow", out var statusNowProperty)?
            new Dictionary<string, object?>(){ { "statusNow", statusNowProperty } } :
            decoded as Dictionary<string, object?> ) : new Dictionary<string, object?>();

        return new HvacStatus(hvacData);
    }
}