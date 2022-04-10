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
                if (MainScripts.GotoNextSystem(false))
                {
                    Console.WriteLine("route completed");
                    Environment.Exit(10);
                }
                

                //(int XGate, int YGate) = MainScripts.GetCoordsNextSystem();
                //if (XGate == 1)
                //{
                //    continue;
                //}
                //if (YGate == 0)
                //{
                //    Console.WriteLine("no route, please set dest");
                //    return;
                //}
                //Emulators.ClickLB(XGate, YGate);
                //System.Threading.Thread.Sleep(500);
                //Emulators.ClickLB(2200, 100); //3 button
                //Checkers.WatchState();
                //System.Threading.Thread.Sleep(1000 * 10);
            }
        }
    }
}
