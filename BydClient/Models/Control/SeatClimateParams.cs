using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace BydClient.Models.Control;

/// <summary>
/// Seat climate parameters.
/// </summary>
public class SeatClimateParams : BaseModel, IControlParams
{
    public int? SeatHeating { get; private set; }
    public int? SeatVentilation { get; private set; }
    public bool? SteeringWheelHeating { get; private set; }

    public SeatClimateParams() { }

    public SeatClimateParams(IDictionary<string, object?> data) : base(data)
    {
        if(data == null) throw new ArgumentNullException(nameof(data));
        Populate(data);
    }

    protected override void Populate(IDictionary<string, object?> data)
    {
        SeatHeating = ToNullableInt(data["seatHeating"]);
        SeatVentilation = ToNullableInt(data["seatVentilation"]);
        SteeringWheelHeating = ToNullableBool(data["steeringWheelHeating"]);
    }

    /// <summary>
    /// Convert to control parameters map (string values).
    /// Mirrors PHP: numeric values for seat levels and '1'/'0' for booleans.
    /// </summary>
    public IDictionary<string, string?> ToControlParamsMap()
    {
        var map = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        if(SeatHeating.HasValue)
            map["seatHeating"] = SeatHeating.Value.ToString(CultureInfo.InvariantCulture);

        if(SeatVentilation.HasValue)
            map["seatVentilation"] = SeatVentilation.Value.ToString(CultureInfo.InvariantCulture);

        if(SteeringWheelHeating.HasValue)
            map["steeringWheelHeating"] = SteeringWheelHeating.Value ? "1" : "0";

        return map;
    }

    // Helper converters
    private static int? ToNullableInt(object? v)
    {
        if(v == null) return null;
        if(v is int i) return i;
        if(int.TryParse(v.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)) return parsed;
        return null;
    }

    private static bool? ToNullableBool(object? v)
    {
        if(v == null) return null;
        if(v is bool b) return b;
        var s = v.ToString();
        if(string.IsNullOrEmpty(s)) return null;
        if(bool.TryParse(s, out var parsedBool)) return parsedBool;
        // handle numeric-like booleans (0/1)
        if(int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n))
            return n != 0;
        return null;
    }

    // Factory helper
    public static SeatClimateParams FromDictionary(IDictionary<string, object?> data)
    {
        return new SeatClimateParams(data);
    }
}
