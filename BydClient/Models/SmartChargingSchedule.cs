using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace BydClient.Models;

/// <summary>
/// Smart charging schedule.
/// </summary>
public class SmartChargingSchedule : BaseModel
{
    public int? TargetSoc { get; private set; }
    public int? StartHour { get; private set; }
    public int? StartMinute { get; private set; }
    public int? EndHour { get; private set; }
    public int? EndMinute { get; private set; }
    public bool Enabled { get; private set; } = false;
    public bool IsEnabled() => Enabled;

    public SmartChargingSchedule()
    {
    }

    public SmartChargingSchedule(IDictionary<string, object?> data) : base(data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        Populate(data);
    }

    protected override void Populate(IDictionary<string, object?> data)
    {
        TargetSoc = ToNullableInt(data["targetSoc"]);
        StartHour = ToNullableInt(data["startHour"]);
        StartMinute = ToNullableInt(data["startMinute"]);
        EndHour = ToNullableInt(data["endHour"]);
        EndMinute = ToNullableInt(data["endMinute"]);

        var enabledObj = data["enabled"];
        if (enabledObj is bool b) Enabled = b;
        else if (enabledObj != null && int.TryParse(enabledObj.ToString(), NumberStyles.Integer,
                     CultureInfo.InvariantCulture, out var bi))
            Enabled = bi != 0;
        else
            Enabled = false;
    }

    // Fluent setters
    public SmartChargingSchedule SetTargetSoc(int? targetSoc)
    {
        TargetSoc = targetSoc;
        return this;
    }

    public SmartChargingSchedule SetStartHour(int? startHour)
    {
        StartHour = startHour;
        return this;
    }

    public SmartChargingSchedule SetStartMinute(int? startMinute)
    {
        StartMinute = startMinute;
        return this;
    }

    public SmartChargingSchedule SetEndHour(int? endHour)
    {
        EndHour = endHour;
        return this;
    }

    public SmartChargingSchedule SetEndMinute(int? endMinute)
    {
        EndMinute = endMinute;
        return this;
    }

    public SmartChargingSchedule SetEnabled(bool enabled)
    {
        Enabled = enabled;
        return this;
    }

    // Factory helper
    public static SmartChargingSchedule FromDictionary(IDictionary<string, object?> data)
    {
        return new SmartChargingSchedule(data);
    }

    // Helper converters
    private static int? ToNullableInt(object? v)
    {
        if (v == null) return null;
        if (v is int i) return i;
        if (int.TryParse(v.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
            return parsed;
        return null;
    }
}
