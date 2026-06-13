using System;
using System.Collections.Generic;
using System.Text;

namespace BydClient.Models;

public enum SeatHeatVentState : int
{
    UNKNOWN = -1,
    NODATA = 0,
    OFF = 1,
    LOW = 2,
    HIGH = 3
}

// The Extension Method (Replaces toCommandLevel)
// How to use it in your code:
// SeatHeatVentState myState = SeatHeatVentState.HIGH;
// int command = myState.ToCommandLevel(); // Returns 1
public static class SeatHeatVentStateExtensions
{
    /// <summary>
    /// Return the command level value for seat-climate control commands.
    /// The BYD API uses an inverted scale:
    /// Status HIGH(3) -> Command 1 (most powerful)
    /// Status LOW(2)  -> Command 2
    /// Status OFF(1)  -> Command 3 (off)
    /// Status NO_DATA(0) / UNKNOWN(-1) -> Command 0 (no action)
    /// </summary>
    public static int ToCommandLevel(this SeatHeatVentState state) => state switch
    {
        SeatHeatVentState.HIGH => 1,
        SeatHeatVentState.LOW => 2,
        SeatHeatVentState.OFF => 3,
        _ => 0 // Replaces PHP 'default'
    };
}

