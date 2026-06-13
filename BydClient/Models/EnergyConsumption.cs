using System;
using System.Collections.Generic;
using System.Text;

namespace BydClient.Models;

public sealed class EnergyConsumption : BaseModel
{
    public float? TotalMileage { get; private set; }
    public float? TotalEnergy { get; private set; }
    public float? RecentAverageEnergy { get; private set; }
    public float? Recent50kmEnergy { get; private set; }
    public float? DrivingEnergy { get; private set; }
    public float? ChargingEnergy { get; private set; }
    public float? ElectricMileage { get; private set; }
    public float? FuelMileage { get; private set; }
    public float? TotalMileageOfElectric { get; private set; }
    public float? TotalMileageOfFuel { get; private set; }
    public float? TotalEnergyOfElectric { get; private set; }
    public float? TotalEnergyOfFuel { get; private set; }
    public float? Co2Emission { get; private set; }
    public float? Co2Saved { get; private set; }
    public DateTimeOffset? StartTime { get; private set; }
    public DateTimeOffset? EndTime { get; private set; }

    public EnergyConsumption(IDictionary<string, object?> data) : base(data) { }

    protected override void Populate(IDictionary<string, object?> data)
    {
        if(data == null) throw new ArgumentNullException(nameof(data));

        TotalMileage = ToNullableFloat(data["totalMileage"]);
        TotalEnergy = ToNullableFloat(data["totalEnergy"]);
        RecentAverageEnergy = ToNullableFloat(data["recentAverageEnergy"]);
        Recent50kmEnergy = ToNullableFloat(data["recent50kmEnergy"]);
        DrivingEnergy = ToNullableFloat(data["drivingEnergy"]);
        ChargingEnergy = ToNullableFloat(data["chargingEnergy"]);
        ElectricMileage = ToNullableFloat(data["electricMileage"]);
        FuelMileage = ToNullableFloat(data["fuelMileage"]);
        TotalMileageOfElectric = ToNullableFloat(data["totalMileageOfElectric"]);
        TotalMileageOfFuel = ToNullableFloat(data["totalMileageOfFuel"]);
        TotalEnergyOfElectric = ToNullableFloat(data["totalEnergyOfElectric"]);
        TotalEnergyOfFuel = ToNullableFloat(data["totalEnergyOfFuel"]);
        Co2Emission = ToNullableFloat(data["co2Emission"]);
        Co2Saved = ToNullableFloat(data["co2Saved"]);

        if(data.TryGetValue("startTime", out var st) && st != null)
            StartTime = ParseTimestamp(st);

        if(data.TryGetValue("endTime", out var et) && et != null)
            EndTime = ParseTimestamp(et);
    }

    private static DateTimeOffset? ParseTimestamp(object timestamp)
    {
        if(timestamp == null) return null;
        if(!long.TryParse(timestamp.ToString(), out var ts)) return null;

        // Se il valore sembra essere in millisecondi (>= 1_000_000_000_000), convertirlo in secondi
        if(ts >= 1_000_000_000_000L) ts /= 1000L;

        try
        {
            return DateTimeOffset.FromUnixTimeSeconds(ts);
        }
        catch
        {
            return null;
        }
    }

    // Helper converters
    private static float? ToNullableFloat(object? v)
    {
        if(v == null) return null;
        return float.TryParse(v.ToString(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var f) ? f : null;
    }

    // Factory helper per comodità
    public static EnergyConsumption FromDictionary(IDictionary<string, object?> data)
    {
        var inst = new EnergyConsumption(data);
        //inst.Populate(data);
        return inst;
    }
}
