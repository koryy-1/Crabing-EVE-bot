using System;
using System.Collections.Generic;
using System.Text;

namespace EVE_Bot.Controllers
{
    static public class ThreadManager
    {
        volatile static public bool AllowCheckRedMarker = true;

        volatile static public bool AllowShipControl = true;

        volatile static public bool AllowShieldHPControl = false;

        volatile static public bool AllowPVEMode = false;

        volatile static public bool PVEModeRunning = false;

        volatile static public bool AllowDroneControl = false;

        volatile static public bool AllowDroneRescoop = false;

        volatile static public bool AllowEnemyWatcher = false;

        volatile static public bool EnemiesInGrid = false;

        volatile static public bool AllowToAttack = false;

        volatile static public bool AllowDocking = true;
    }
}
