using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace BydClient.Models;

/// <summary>
/// Push notification state.
/// </summary>
public sealed class PushNotificationState : BaseModel
{
    public bool Enabled { get; private set; } = false;
    public string? DeviceId { get; private set; }
    public string? DeviceToken { get; private set; }
    public DateTimeOffset? LastUpdate { get; private set; }

    public PushNotificationState() { }
    public PushNotificationState(IDictionary<string, object?> data) : base(data) => Populate(data);

    protected override void Populate(IDictionary<string, object?> data)
    {
        if(data == null) throw new ArgumentNullException(nameof(data));

        // enabled may be boolean or numeric/string; handle common cases
        var enabledObj = data["enabled"];
        if(enabledObj is bool b) Enabled = b;
        else if(enabledObj != null && int.TryParse(enabledObj.ToString(), out var bi)) Enabled = bi != 0;
        else Enabled = false;

        DeviceId = data["deviceId"]?.ToString();
        DeviceToken = data["deviceToken"]?.ToString();

        if(data.TryGetValue("lastUpdate", out var ts) && ts != null)
        {
            LastUpdate = ParseTimestamp(ts);
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

    // Factory helper
    public static PushNotificationState FromDictionary(IDictionary<string, object?> data)
    {
        var inst = new PushNotificationState();
        inst.Populate(data);
        return inst;
    }
}
