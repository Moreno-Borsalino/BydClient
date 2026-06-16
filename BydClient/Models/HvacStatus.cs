using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace BydClient.Models;

public enum HvacOverallStatus : int
{
    UNKNOWN = -1,
    ON = 1,
    OFF = 2
}

public enum HvacWindMode : int
{
    UNKNOWN = -1,
    OFF = 0,
    FACE = 1,
    FACE_FOOT = 2,
    FOOT = 3,
    FOOT_DEFROST = 4,
    DEFROST = 5
}

public enum HvacWindPosition : int
{
    UNKNOWN = -1,
    OFF = 0,
    POSITION_1 = 1,
    POSITION_2 = 2,
    POSITION_3 = 3,
    POSITION_4 = 4,
    POSITION_5 = 5,
    POSITION_6 = 6,
    POSITION_7 = 7
}

public enum AcSwitch : int
{
    UNKNOWN = -1,
    OFF = 0,
    ON = 1,
    HEAT = 2
}

public enum AirCirculationMode : int
{
    UNKNOWN = -1,
    UNAVAILABLE = 0,
    EXTERNAL = 1,
    INTERNAL = 2
}

public enum AirConditioningMode : int
{
    UNKNOWN = -1,
    OFF = 0,
    AUTO = 1,
    MANUAL = 2
}

/// <summary>
/// HVAC / climate status from the BYD API (/control/getStatusNow).
/// </summary>
public class HvacStatus : BaseModel
{

    public HvacOverallStatus Status { get; private set; } = HvacOverallStatus.UNKNOWN;

    public AcSwitch AcSwitch { get; private set; } = AcSwitch.UNKNOWN;

    public AirConditioningMode AirConditioningMode { get; private set; } = AirConditioningMode.UNKNOWN;

    public AirCirculationMode CycleChoice { get; private set; } = AirCirculationMode.UNKNOWN;

    public HvacWindMode WindMode { get; private set; } = HvacWindMode.UNKNOWN;

    public HvacWindPosition WindPosition { get; private set; } = HvacWindPosition.UNKNOWN;

    // Temperatures
    public float? TempInCar { get; private set; } = null;

    public float? TempOutCar { get; private set; } = null;

    public int? MainSettingTemp { get; private set; } = null;

    public float? MainSettingTempNew { get; private set; } = null;

    public int? CopilotSettingTemp { get; private set; } = null;

    public float? CopilotSettingTempNew { get; private set; } = null;

    // Seat heating/ventilation
    public SeatHeatVentState MainSeatHeatState { get; private set; } = SeatHeatVentState.UNKNOWN;

    public SeatHeatVentState MainSeatVentilationState { get; private set; } = SeatHeatVentState.UNKNOWN;

    public SeatHeatVentState CopilotSeatHeatState { get; private set; } = SeatHeatVentState.UNKNOWN;

    public SeatHeatVentState CopilotSeatVentilationState { get; private set; } = SeatHeatVentState.UNKNOWN;

    public SeatHeatVentState LrSeatHeatState { get; private set; } = SeatHeatVentState.UNKNOWN;

    public SeatHeatVentState LrSeatVentilationState { get; private set; } = SeatHeatVentState.UNKNOWN;

    public SeatHeatVentState RrSeatHeatState { get; private set; } = SeatHeatVentState.UNKNOWN;

    public SeatHeatVentState RrSeatVentilationState { get; private set; } = SeatHeatVentState.UNKNOWN;

    public StearingWheelHeat SteeringWheelHeatState { get; private set; } = StearingWheelHeat.OFF;

    // Air quality
    public float? Pm { get; private set; } = null;

    public float? Pm25StateOutCar { get; private set; } = null;

    // Misc
    public int? FrontDefrostStatus { get; private set; } = null;

    public int? ElectricDefrostStatus { get; private set; } = null;

    public int? WiperHeatStatus { get; private set; } = null;

    public int? RefrigeratorState { get; private set; } = null;

    public int? RefrigeratorDoorState { get; private set; } = null;

    public int? RapidIncreaseTempState { get; private set; } = null;

    public int? RapidDecreaseTempState { get; private set; } = null;

    public int? WhetherSupportAdjustTemp { get; private set; } = null;

    public HvacStatus(Dictionary<string, object?> hvacData): base(hvacData)
    {
        
    }

    /// <param name="data">Dictionary of string keys and mixed values</param>
    protected override void Populate(IDictionary<string, object?> data)
    {
        this.Status = data.ContainsKey("status") && Enum.IsDefined(typeof(HvacOverallStatus), Convert.ToInt32(data["status"]))
            ? (HvacOverallStatus)Convert.ToInt32(data["status"])
            : HvacOverallStatus.UNKNOWN;

        this.AcSwitch = data.ContainsKey("acSwitch") && Enum.IsDefined(typeof(AcSwitch), Convert.ToInt32(data["acSwitch"]))
            ? (AcSwitch)Convert.ToInt32(data["acSwitch"])
            : AcSwitch.UNKNOWN;

        this.AirConditioningMode = data.ContainsKey("airConditioningMode") && Enum.IsDefined(typeof(AirConditioningMode), Convert.ToInt32(data["airConditioningMode"]))
            ? (AirConditioningMode)Convert.ToInt32(data["airConditioningMode"])
            : AirConditioningMode.UNKNOWN;

        this.CycleChoice = data.ContainsKey("cycleChoice") && Enum.IsDefined(typeof(AirCirculationMode), Convert.ToInt32(data["cycleChoice"]))
            ? (AirCirculationMode)Convert.ToInt32(data["cycleChoice"])
            : AirCirculationMode.UNKNOWN;

        this.WindMode = data.ContainsKey("windMode") && Enum.IsDefined(typeof(HvacWindMode), Convert.ToInt32(data["windMode"]))
            ? (HvacWindMode)Convert.ToInt32(data["windMode"])
            : HvacWindMode.UNKNOWN;

        this.WindPosition = data.ContainsKey("windPosition") && Enum.IsDefined(typeof(HvacWindPosition), Convert.ToInt32(data["windPosition"]))
            ? (HvacWindPosition)Convert.ToInt32(data["windPosition"])
            : HvacWindPosition.UNKNOWN;

        // Temperatures — tempInCar uses -129 as "no data" sentinel
        float? rawTempInCar = data.ContainsKey("tempInCar") ? (float?)Convert.ToSingle(data["tempInCar"]) : null;
        this.TempInCar = (rawTempInCar != null && rawTempInCar <= -100.0f) ? null : rawTempInCar;

        this.TempOutCar = data.ContainsKey("tempOutCar") ? (float?)Convert.ToSingle(data["tempOutCar"]) : null;
        this.MainSettingTemp = data.ContainsKey("mainSettingTemp") ? (int?)Convert.ToInt32(data["mainSettingTemp"]) : null;
        this.MainSettingTempNew = data.ContainsKey("mainSettingTempNew") ? (float?)Convert.ToSingle(data["mainSettingTempNew"]) : null;
        this.CopilotSettingTemp = data.ContainsKey("copilotSettingTemp") ? (int?)Convert.ToInt32(data["copilotSettingTemp"]) : null;
        this.CopilotSettingTempNew = data.ContainsKey("copilotSettingTempNew") ? (float?)Convert.ToSingle(data["copilotSettingTempNew"]) : null;

        // Seat heating/ventilation
        this.MainSeatHeatState = data.ContainsKey("mainSeatHeatState") && Enum.IsDefined(typeof(SeatHeatVentState), Convert.ToInt32(data["mainSeatHeatState"]))
            ? (SeatHeatVentState)Convert.ToInt32(data["mainSeatHeatState"])
            : SeatHeatVentState.UNKNOWN;

        this.MainSeatVentilationState = data.ContainsKey("mainSeatVentilationState") && Enum.IsDefined(typeof(SeatHeatVentState), Convert.ToInt32(data["mainSeatVentilationState"]))
            ? (SeatHeatVentState)Convert.ToInt32(data["mainSeatVentilationState"])
            : SeatHeatVentState.UNKNOWN;

        this.CopilotSeatHeatState = data.ContainsKey("copilotSeatHeatState") && Enum.IsDefined(typeof(SeatHeatVentState), Convert.ToInt32(data["copilotSeatHeatState"]))
            ? (SeatHeatVentState)Convert.ToInt32(data["copilotSeatHeatState"])
            : SeatHeatVentState.UNKNOWN;

        this.CopilotSeatVentilationState = data.ContainsKey("copilotSeatVentilationState") && Enum.IsDefined(typeof(SeatHeatVentState), Convert.ToInt32(data["copilotSeatVentilationState"]))
            ? (SeatHeatVentState)Convert.ToInt32(data["copilotSeatVentilationState"])
            : SeatHeatVentState.UNKNOWN;

        this.LrSeatHeatState = data.ContainsKey("lrSeatHeatState") && Enum.IsDefined(typeof(SeatHeatVentState), Convert.ToInt32(data["lrSeatHeatState"]))
            ? (SeatHeatVentState)Convert.ToInt32(data["lrSeatHeatState"])
            : SeatHeatVentState.UNKNOWN;

        this.LrSeatVentilationState = data.ContainsKey("lrSeatVentilationState") && Enum.IsDefined(typeof(SeatHeatVentState), Convert.ToInt32(data["lrSeatVentilationState"]))
            ? (SeatHeatVentState)Convert.ToInt32(data["lrSeatVentilationState"])
            : SeatHeatVentState.UNKNOWN;

        this.RrSeatHeatState = data.ContainsKey("rrSeatHeatState") && Enum.IsDefined(typeof(SeatHeatVentState), Convert.ToInt32(data["rrSeatHeatState"]))
            ? (SeatHeatVentState)Convert.ToInt32(data["rrSeatHeatState"])
            : SeatHeatVentState.UNKNOWN;

        this.RrSeatVentilationState = data.ContainsKey("rrSeatVentilationState") && Enum.IsDefined(typeof(SeatHeatVentState), Convert.ToInt32(data["rrSeatVentilationState"]))
            ? (SeatHeatVentState)Convert.ToInt32(data["rrSeatVentilationState"])
            : SeatHeatVentState.UNKNOWN;

        this.SteeringWheelHeatState = data.ContainsKey("steeringWheelHeatState") && Enum.IsDefined(typeof(StearingWheelHeat), Convert.ToInt32(data["steeringWheelHeatState"]))
            ? (StearingWheelHeat)Convert.ToInt32(data["steeringWheelHeatState"])
            : StearingWheelHeat.OFF;

        // Air quality
        this.Pm = data.ContainsKey("pm") ? (float?)Convert.ToSingle(data["pm"]) : null;
        this.Pm25StateOutCar = data.ContainsKey("pm25StateOutCar") ? (float?)Convert.ToSingle(data["pm25StateOutCar"]) : null;

        // Misc
        this.FrontDefrostStatus = data.ContainsKey("frontDefrostStatus") ? (int?)Convert.ToInt32(data["frontDefrostStatus"]) : null;
        this.ElectricDefrostStatus = data.ContainsKey("electricDefrostStatus") ? (int?)Convert.ToInt32(data["electricDefrostStatus"]) : null;
        this.WiperHeatStatus = data.ContainsKey("wiperHeatStatus") ? (int?)Convert.ToInt32(data["wiperHeatStatus"]) : null;
        this.RefrigeratorState = data.ContainsKey("refrigeratorState") ? (int?)Convert.ToInt32(data["refrigeratorState"]) : null;
        this.RefrigeratorDoorState = data.ContainsKey("refrigeratorDoorState") ? (int?)Convert.ToInt32(data["refrigeratorDoorState"]) : null;
        this.RapidIncreaseTempState = data.ContainsKey("rapidIncreaseTempState") ? (int?)Convert.ToInt32(data["rapidIncreaseTempState"]) : null;
        this.RapidDecreaseTempState = data.ContainsKey("rapidDecreaseTempState") ? (int?)Convert.ToInt32(data["rapidDecreaseTempState"]) : null;
        this.WhetherSupportAdjustTemp = data.ContainsKey("whetherSupportAdjustTemp") ? (int?)Convert.ToInt32(data["whetherSupportAdjustTemp"]) : null;
    }

    // Getters

    public bool IsOn()
    {
        return this.Status == HvacOverallStatus.ON;
    }

    public bool IsAcActive()
    {
        return this.AcSwitch == AcSwitch.ON;
    }
}

