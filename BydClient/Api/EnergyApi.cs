using BydClient.Config;
using System;
using System.Collections.Generic;
using System.Text;
using BydClient.Models;
using BydClient.Transport;

namespace BydClient.Api;

public sealed class EnergyApi
{
    private const string ENDPOINT = "/vehicleInfo/vehicle/getEnergyConsumption";

    /// <summary>
    /// Fetch energy consumption data for a vehicle.
    /// </summary>
    public static async Task<EnergyConsumption> FetchEnergyConsumptionAsync(
        BydConfig config,
        Session session,
        ITransport transport,
        string vin,
        CancellationToken cancellationToken = default
    )
    {
        Dictionary<string, string?> inner = Common.BuildInnerBase(config, null, vin);
        inner["energyType"] = "0";  // Defaults to ``EnergyType.EV`` to preserve the old hard-coded
                                    // behaviour for any direct caller.
        inner["tboxVersion"] = config.GetTboxVersion();

        object? decoded = await TokenJson.PostTokenJsonAsync(
            ENDPOINT,
            config,
            session,
            transport,
            inner,
            null,
            vin
        );

        return new EnergyConsumption(decoded is Dictionary<string, object?> dict ? dict : new Dictionary<string, object?>());
    }
}

