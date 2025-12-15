using Combat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CombatTime
{
    public static float TimeScale { get; private set; } = 1f;

    public static void SetSpeed(CombatSpeedPb speed)
    {
        TimeScale = speed switch
        {
            CombatSpeedPb.CombatSpeedX1 => 1f,
            CombatSpeedPb.CombatSpeedX15 => 1.5f,
            CombatSpeedPb.CombatSpeedX2 => 2f,
            _ => 1f
        };
    }
}