using System;
using System.Collections.Generic;
using System.Text;

namespace BydClient.Models.Control;

/// <summary>
/// Climate start parameters.
/// </summary>
public class ClimateStartParams : IControlParams
{
    public int? Temperature { get; private set; }
    public bool AcOn { get; private set; } = false;
    public bool Heating { get; private set; } = false;
    public bool Defrost { get; private set; } = false;
    public bool FrontDefrost { get; private set; } = false;
    public bool RearDefrost { get; private set; } = false;

    /// <summary>
    /// Convert to control parameters map.
    /// </summary>
    public IDictionary<string, string?> ToControlParamsMap()
    {
        var paramsMap = new Dictionary<string, string?>();

        if(Temperature.HasValue)
        {
            paramsMap["temperature"] = Temperature.Value.ToString();
        }

        paramsMap["acOn"] = AcOn ? "1" : "0";
        paramsMap["heating"] = Heating ? "1" : "0";
        paramsMap["defrost"] = Defrost ? "1" : "0";
        paramsMap["frontDefrost"] = FrontDefrost ? "1" : "0";
        paramsMap["rearDefrost"] = RearDefrost ? "1" : "0";

        return paramsMap;
    }
}
