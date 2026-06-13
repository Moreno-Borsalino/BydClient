using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BydClient.Config;
using BydClient.Models;
using BydClient.Transport;

namespace BydClient.Api;

public static class ChargingApi
{
    private const string Endpoint = "/control/smartCharge/homePage";

    /// <summary>
    /// Fetch smart charging status (SOC, charge state, time-to-full).
    /// </summary>
    public static async Task<ChargingStatus> FetchChargingStatusAsync(
        BydConfig config,
        Session session,
        ITransport transport,
        string vin, CancellationToken cancellationToken = default)
    {
        // Equivalent to Common::buildInnerBase(...)
        var inner = Common.BuildInnerBase(config, null, vin);

        // Equivalent to TokenJson::postTokenJson(...)
        var decoded = await TokenJson.PostTokenJsonAsync(
            Endpoint,
            config,
            session,
            transport,
            inner,
            null,
            vin
        );

        return new ChargingStatus(
            decoded as Dictionary<string, object?> ?? new Dictionary<string, object?>()
        );
    }
}