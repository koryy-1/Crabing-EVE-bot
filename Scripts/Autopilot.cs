using EVE_Bot.Controllers;
using EVE_Bot.Searchers;
using System;
using System.Collections.Generic;
using System.Text;

namespace EVE_Bot.Scripts
{
    static public class Autopilot
    {
        static public void Start()
        {
            for (int i = 0; i < 100; i++)
            {
                if (i % 10 == 0)
                    MainScripts.CheckForConnectionLost();
                (int XGate, int YGate) = MainScripts.GoToNextSystem();
                if (XGate == 1)
                {
                    continue;
                }
                if (XGate == 0 && YGate == 0)
                {
                    Console.WriteLine("no route");
                    return; // lay route
                }
                Emulators.ClickLB(XGate, YGate);
                System.Threading.Thread.Sleep(500);
                Emulators.ClickLB(2200, 100); //3 button
                Checkers.WatchState();
                System.Threading.Thread.Sleep(1000 * 10);
            }
        }
    }
}
