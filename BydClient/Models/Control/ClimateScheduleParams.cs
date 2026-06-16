using System;
using System.Collections.Generic;
using System.Text;

namespace BydClient.Models.Control;

// Stubs for dependencies mentioned in the PHP code
// In your actual project, these might be defined elsewhere.

public class ClimateScheduleParams : BaseModel, IControlParams
{
    public int? StartHour { get; private set; } = null;
    public int? StartMinute { get; private set; } = null;
    public int? EndHour { get; private set; } = null;
    public int? EndMinute { get; private set; } = null;
    public int? Temperature { get; private set; } = null;
    public int? SeatHeating { get; private set; } = null;
    public int? SeatVentilation { get; private set; } = null;
    public bool? SteeringWheelHeating { get; private set; } = null;

    /**
     * @param Dictionary<string, object> $data
     */
    protected override void Populate(IDictionary<string, object?> data)
    {
        if(data == null)
        {
            return;
        }

        if(data.TryGetValue("startHour", out var valStartHour))
            this.StartHour = valStartHour != null ? Convert.ToInt32(valStartHour) : null;

        if(data.TryGetValue("startMinute", out var valStartMinute))
            this.StartMinute = valStartMinute != null ? Convert.ToInt32(valStartMinute) : null;

        if(data.TryGetValue("endHour", out var valEndHour))
            this.EndHour = valEndHour != null ? Convert.ToInt32(valEndHour) : null;

        if(data.TryGetValue("endMinute", out var valEndMinute))
            this.EndMinute = valEndMinute != null ? Convert.ToInt32(valEndMinute) : null;

        if(data.TryGetValue("temperature", out var valTemperature))
            this.Temperature = valTemperature != null ? Convert.ToInt32(valTemperature) : null;

        if(data.TryGetValue("seatHeating", out var valSeatHeating))
            this.SeatHeating = valSeatHeating != null ? Convert.ToInt32(valSeatHeating) : null;

        if(data.TryGetValue("seatVentilation", out var valSeatVentilation))
            this.SeatVentilation = valSeatVentilation != null ? Convert.ToInt32(valSeatVentilation) : null;

        if(data.TryGetValue("steeringWheelHeating", out var valSteeringWheelHeating))
            this.SteeringWheelHeating = valSteeringWheelHeating != null ? Convert.ToBoolean(valSteeringWheelHeating) : null;
    }

    public IDictionary<string, string?> ToControlParamsMap()
    {
        var param = new Dictionary<string, string?>();

        if(this.StartHour != null)
        {
            param.Add("startHour", this.StartHour.ToString());
        }

        if(this.StartMinute != null)
        {
            param.Add("startMinute", this.StartMinute.ToString());
        }

        if(this.EndHour != null)
        {
            param.Add("endHour", this.EndHour.ToString());
        }

        if(this.EndMinute != null)
        {
            param.Add("endMinute", this.EndMinute.ToString());
        }

        if(this.Temperature != null)
        {
            param.Add("temperature", this.Temperature.ToString());
        }

        if(this.SeatHeating != null)
        {
            param.Add("seatHeating", this.SeatHeating.ToString());
        }

        if(this.SeatVentilation != null)
        {
            param.Add("seatVentilation", this.SeatVentilation.ToString());
        }

        if(this.SteeringWheelHeating != null)
        {
            param.Add("steeringWheelHeating", this.SteeringWheelHeating.Value ? "1" : "0");
        }

        return param;
    }
}
