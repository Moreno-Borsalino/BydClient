using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace BydClient.Models;

/// <summary>
/// GPS information for a vehicle.
/// </summary>
public class GpsInfo : BaseModel
{
    public float? Latitude { get; private set; }
    public float? Longitude { get; private set; }
    public float? Altitude { get; private set; }
    public float? Speed { get; private set; }
    public float? Heading { get; private set; }
    public float? Direction { get; private set; }
    public DateTimeOffset? Timestamp { get; private set; }
    public string? PositionType { get; private set; }

    public GpsInfo() { }
    public GpsInfo(IDictionary<string, object?> data) : base(data) => Populate(data);

    protected override void Populate(IDictionary<string, object?> data)
    {
        if(data == null) throw new ArgumentNullException(nameof(data));

        // Expect nested "data" object like PHP: $data['data'][...]
        object? nested = data["data"];
        if(nested is IDictionary<string, object?> d)
        {
            Latitude = ToNullableFloat(d["latitude"]);
            Longitude = ToNullableFloat(d["longitude"]);
            Altitude = ToNullableFloat(d["altitude"]);
            Speed = ToNullableFloat(d["speed"]);
            Heading = ToNullableFloat(d["heading"]);
            Direction = ToNullableFloat(d["direction"]);

            if(d.TryGetValue("gpsTimeStamp", out var ts) && ts != null)
                Timestamp = ParseTimestamp(ts);

            PositionType = d["positionType"]?.ToString();
        }
        else
        {
            // Defensive: if top-level fields are provided directly
            Latitude = ToNullableFloat(data["latitude"]);
            Longitude = ToNullableFloat(data["longitude"]);
            Altitude = ToNullableFloat(data["altitude"]);
            Speed = ToNullableFloat(data["speed"]);
            Heading = ToNullableFloat(data["heading"]);
            Direction = ToNullableFloat(data["direction"]);

            if(data.TryGetValue("gpsTimeStamp", out var ts) && ts != null)
                Timestamp = ParseTimestamp(ts);

            PositionType = data["positionType"]?.ToString();
        }
    }

    private static DateTimeOffset? ParseTimestamp(object timestamp)
    {
        if(timestamp == null) return null;
        if(!long.TryParse(timestamp.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var ts)) return null;

        // If value looks like milliseconds (>= 1_000_000_000_000), convert to seconds
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

    private static float? ToNullableFloat(object? v)
    {
        if(v == null) return null;
        return float.TryParse(v.ToString(), NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var f) ? f : null;
    }

    /// <summary>
    /// Check if GPS data has meaningful content.
    /// Mirrors PHP: returns false for empty payloads or payloads that only contain requestSerial.
    /// </summary>
    public static bool IsGpsInfoReady(IDictionary<string, object?> gpsInfo)
    {
        if(gpsInfo == null || gpsInfo.Count == 0) return false;

        // If the only key is "requestSerial", treat as not ready
        if(gpsInfo.Count == 1 && gpsInfo.ContainsKey("requestSerial")) return false;

        return true;
    }

    // Optional factory
    public static GpsInfo FromDictionary(IDictionary<string, object?> data)
    {
        var inst = new GpsInfo();
        inst.Populate(data);
        return inst;
    }
}
