using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace BydClient.Models;

/// <summary>
/// A vehicle associated with the user's account.
/// </summary>
public class Vehicle : BaseModel
{
    // Properties replacing standard boilerplate getters
    public string Vin { get; private set; } = string.Empty;
    public string ModelName { get; private set; } = string.Empty;
    public string BrandName { get; private set; } = string.Empty;
    public string EnergyType { get; private set; } = string.Empty;
    public string AutoAlias { get; private set; } = string.Empty;
    public string AutoPlate { get; private set; } = string.Empty;
    public string PicMainUrl { get; private set; } = string.Empty;
    public string PicSetUrl { get; private set; } = string.Empty;
    public string OutModelType { get; private set; } = string.Empty;
    public float? TotalMileage { get; private set; }
    public int? ModelId { get; private set; }
    public int? CarType { get; private set; }
    public bool DefaultCar { get; private set; }
    public int? EmpowerType { get; private set; }
    public int? PermissionStatus { get; private set; }
    public string TboxVersion { get; private set; } = string.Empty;
    public string VehicleState { get; private set; } = string.Empty;
    public DateTime? AutoBoughtTime { get; private set; }
    public DateTime? YunActiveTime { get; private set; }
    public int? EmpowerId { get; private set; }
    public List<EmpowerRange> RangeDetailList { get; private set; } = [];
    public EmpowerRange[] GetRangeDetailList() => RangeDetailList.ToArray();

    public Vehicle() { }
    public Vehicle(IDictionary<string, object?> data) : base(data) => Populate(data);

    protected override void Populate(IDictionary<string, object?> data)
    {
        if(data == null) throw new ArgumentNullException(nameof(data));

        Vin = data["vin"]?.ToString() ?? string.Empty;
        ModelName = data["modelName"]?.ToString() ?? string.Empty;
        BrandName = data["brandName"]?.ToString() ?? string.Empty;
        EnergyType = data["energyType"]?.ToString() ?? string.Empty;
        AutoAlias = data["autoAlias"]?.ToString() ?? string.Empty;
        AutoPlate = data["autoPlate"]?.ToString() ?? string.Empty;
        PicMainUrl = data["picMainUrl"]?.ToString() ?? string.Empty;
        PicSetUrl = data["picSetUrl"]?.ToString() ?? string.Empty;
        OutModelType = data["outModelType"]?.ToString() ?? string.Empty;

        TotalMileage = ToNullableFloat(data["totalMileage"]);
        ModelId = ToNullableInt(data["modelId"]);
        CarType = ToNullableInt(data["carType"]);
        DefaultCar = data["defaultCar"] is bool b ? b : (data["defaultCar"]?.ToString() == "1");
        EmpowerType = ToNullableInt(data["empowerType"]);
        PermissionStatus = ToNullableInt(data["permissionStatus"]);
        TboxVersion = data["tboxVersion"]?.ToString() ?? string.Empty;
        VehicleState = data["vehicleState"]?.ToString() ?? string.Empty;

        if(data.TryGetValue("autoBoughtTime", out var abt) && abt != null)
            AutoBoughtTime = ParseTimestamp(abt)?.DateTime;

        if(data.TryGetValue("yunActiveTime", out var yat) && yat != null)
            YunActiveTime = ParseTimestamp(yat)?.DateTime;

        EmpowerId = ToNullableInt(data["empowerId"]);

        // rangeDetailList
        RangeDetailList.Clear();
        if(data.TryGetValue("rangeDetailList", out var rdl) && rdl is IEnumerable<object> rdlEnum)
        {
            foreach(var item in rdlEnum)
            {
                if(item is IDictionary<string, object?> itemDict)
                    RangeDetailList.Add(new EmpowerRange(itemDict));
            }
        }

        // Handle cfPic if main URLs are missing
        if((string.IsNullOrEmpty(PicMainUrl) || string.IsNullOrEmpty(PicSetUrl))
            && data.TryGetValue("cfPic", out var cfPicObj)
            && cfPicObj is IDictionary<string, object?> cfPic)
        {
            PicMainUrl = !string.IsNullOrEmpty(PicMainUrl)
                ? PicMainUrl
                : (cfPic["picMainUrl"]?.ToString()
                   ?? cfPic["pic_main_url"]?.ToString()
                   ?? string.Empty);

            PicSetUrl = !string.IsNullOrEmpty(PicSetUrl)
                ? PicSetUrl
                : (cfPic["picSetUrl"]?.ToString()
                   ?? cfPic["pic_set_url"]?.ToString()
                   ?? string.Empty);
        }
    }

    private static DateTimeOffset? ParseTimestamp(object timestamp)
    {
        if(timestamp == null) return null;
        if(!long.TryParse(timestamp.ToString(), out var ts)) return null;

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

    public bool IsShared() => EmpowerType.HasValue && EmpowerType.Value < 0;

    // Helper converters
    private static int? ToNullableInt(object? v)
    {
        if(v == null) return null;
        return int.TryParse(v.ToString(), out var i) ? i : null;
    }

    private static float? ToNullableFloat(object? v)
    {
        if(v == null) return null;
        return float.TryParse(v.ToString(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var f) ? f : null;
    }

    private static string GetStringProperty(JsonElement element, string name)
    {
        return element.TryGetProperty(name, out var prop) && prop.ValueKind == JsonValueKind.String
            ? prop.GetString() ?? string.Empty
            : string.Empty;
    }
}

