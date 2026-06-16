using System;
using System.Collections.Generic;
using System.Text;

namespace BydClient.Models.Control;

public sealed class BatteryHeatParams : BaseModel, IControlParams
{
    public bool? Enabled { get; set; }

    /// <summary>
    /// @param array&lt;string, mixed&gt; $data
    /// </summary>
    protected override void Populate(IDictionary<string, object?> data)
    {
        if(data.TryGetValue("enabled", out var value) && value != null)
        {
            Enabled = Convert.ToBoolean(value);
        }
        else
        {
            Enabled = null;
        }
    }

    /// <summary>
    /// @return array&lt;string, string&gt;
    /// </summary>
    public IDictionary<string, string?> ToControlParamsMap()
    {
        var paramsland = new Dictionary<string, string?>();

        if(Enabled != null)
        {
            paramsland["batteryHeating"] = Enabled.Value ? "1" : "0";
        }

        return paramsland;
    }
}

