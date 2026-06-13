using System;
using System.Collections.Generic;
using System.Text;

namespace BydClient.Models;

using System;
using System.Collections.Generic;

public sealed class VerifyControlPasswordResponse : BaseModel
{
    public string Vin { get; private set; } = string.Empty;
    public bool? Success { get; private set; }
    public string? Message { get; private set; }

    public VerifyControlPasswordResponse(IDictionary<string, object?> data) : base(data) { }

    protected override void Populate(IDictionary<string, object?> data)
    {
        if(data == null) throw new ArgumentNullException(nameof(data));

        if(data.TryGetValue("vin", out var vinVal) && vinVal != null)
            Vin = vinVal.ToString() ?? string.Empty;
        else
            Vin = string.Empty;

        if(data.TryGetValue("success", out var successVal) && successVal != null)
        {
            if(successVal is bool b) Success = b;
            else if(bool.TryParse(successVal.ToString(), out var parsedBool)) Success = parsedBool;
            else Success = null;
        }
        else
        {
            Success = null;
        }

        if(data.TryGetValue("message", out var msgVal) && msgVal != null)
            Message = msgVal.ToString();
        else
            Message = null;
    }

    // Optional: helper factory to create from a dictionary
    public static VerifyControlPasswordResponse FromDictionary(IDictionary<string, object?> data)
    {
        var inst = new VerifyControlPasswordResponse(data);
        //inst.Populate(data);
        return inst;
    }
}


