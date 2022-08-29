using EVE_Bot.Models;
using EVE_Bot.Parsers;
using EVE_Bot.Scripts;
using EVE_Bot.Searchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace EVE_Bot.Controllers
{
    static public class ShipController
    {
        static ModulesInfo ModulesInfo = new ModulesInfo();

        static public Thread NavigationControlSystem = new Thread(() =>
        {
            Thread ShipNavigator = new Thread(Wrapper);
            while (true)
            {
                if (ThreadManager.AllowNavigationControl && ThreadManager.AllowShipControl)
                {
                    if (!ShipNavigator.IsAlive)
                    {
                        ShipNavigator = new Thread(Wrapper);
                        ShipNavigator.Start();
                        //Console.WriteLine("starting thread NavigationMonitor");
                    }
                }
                else
                {
                    if (ShipNavigator.IsAlive)
                    {
                        //Console.WriteLine("stoping thread NavigationMonitor");
                        ShipNavigator.Interrupt();
                        ShipNavigator.Join();
                        Emulators.AllowControlEmulator = true;
                    }
                }

                Thread.Sleep(ThreadManager.MultiplierSleep * 1000);
            }
        });

        static void NavigationMonitor()
        {
            //Jumping
            //Click target
            //Approaching
            //Orbiting
            //Warping
            //Aligning
            var CurrentState = "Ship Stopping";
            while (true)
            {
                //var ShipState = HI.GetShipState(HI.GetHudContainer());
                //if (ShipState == null)
                //{
                //    if (CurrentState != "Ship Stopping")
                //    {
                //        Emulators.AllowControlEmulator = false;
                //        General.ModuleActivityManager(ModulesInfo.MWD, false, PrivilegeControl: true);
                //        Emulators.AllowControlEmulator = true;
                //        General.SetSpeed(0);
                //        var CurrentShipState = HI.GetShipState(HI.GetHudContainer());
                //        if (CurrentShipState != null)
                //        {
                //            CurrentState = CurrentShipState.CurrentState;
                //        }
                //    }
                //}

                if (ThreadManager.ShipShieldIsLow)
                {
                    if (!Checkers.CheckState("Aligning"))
                    {
                        var Stargate = OV.GetInfo().Find(item => item.Type.Contains("Stargate"));
                        General.GotoInActiveItem(Stargate.Name, "AlignTo");
                        CurrentState = "";
                    }
                }

                else if (ThreadManager.CloseDistanceToEnemy)
                {
                    var EnemyInfo = OV.GetInfo()
                        .OrderBy(item => item.Distance.value).ToList()
                        .Find(item => OV.GetColorInfo(item.Colors) is "red");

                    if (!Checkers.CheckState("Orbiting", EnemyInfo.Name))
                    {
                        General.Orbiting(EnemyInfo.Name);

                        Emulators.AllowControlEmulator = false;
                        General.ModuleActivityManager(ModulesInfo.MWD, true, PrivilegeControl: true);
                        Emulators.AllowControlEmulator = true;
                        CurrentState = "";
                    }
                }

                else if (ThreadManager.ItemInSpace != "" && (ThreadManager.FlightManeuver != "" || ThreadManager.ExpectedState != ""))
                {
                    Console.WriteLine(ThreadManager.ExpectedState);
                    if (!Checkers.CheckState(ThreadManager.ExpectedState, ThreadManager.ItemInSpace))
                    {
                        General.GotoInActiveItem(ThreadManager.ItemInSpace, ThreadManager.FlightManeuver);

                        Emulators.AllowControlEmulator = false;
                        General.ModuleActivityManager(ModulesInfo.MWD, true, PrivilegeControl: true);
                        Emulators.AllowControlEmulator = true;
                        CurrentState = "";
                    }
                }
                else if (ThreadManager.ItemInSpace == "" && ThreadManager.FlightManeuver == "")
                {
                    if (CurrentState != "Ship Stopping")
                    {
                        Emulators.AllowControlEmulator = false;
                        General.ModuleActivityManager(ModulesInfo.MWD, false, PrivilegeControl: true);
                        Emulators.AllowControlEmulator = true;
                        General.SetSpeed(0);
                        var CurrentShipState = HI.GetShipState(HI.GetHudContainer());
                        if (CurrentShipState != null)
                        {
                            CurrentState = CurrentShipState.CurrentState;
                        }
                    }
                }

                Thread.Sleep(5 * 1000);
            }
        }

        static void Wrapper()
        {
            try
            {
                NavigationMonitor();
            }
            catch (ThreadInterruptedException)
            {
            }

        }
    }
}
