using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace BydClient.Models;

public enum StearingWheelHeat : int
{
    ON = -1,  // Confirmed live: -1 means ON
    OFF = 1
}

// The Extension Method (Replaces toCommandLevel)
// How to use it in your code:
// StearingWheelHeat status = StearingWheelHeat.ON;
// int commandValue = status.ToCommandLevel(); // Returns 1

public static class StearingWheelHeatExtensions
{
    /// <summary>
    /// Return the command level value for seat-climate commands.
    /// Command scale: 1 = on, 3 = off.
    /// </summary>
    public static int ToCommandLevel(this StearingWheelHeat heat)
    {
        return heat == StearingWheelHeat.ON ? 1 : 3;
    }
}