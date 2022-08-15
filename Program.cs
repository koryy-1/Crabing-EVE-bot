using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Configuration;
using EVE_Bot.Searchers;
using EVE_Bot.Controllers;
using EVE_Bot.Scripts;
using EVE_Bot.Configs;
using System.Threading.Tasks;
using System.Threading;
using read_memory_64_bit;

static public class Program
{
    static int Main(string[] args)
    {
        void StartThreads() 
        {
            CancellationTokenSource cts = new CancellationTokenSource();


            var DisconnectChecker = new Thread(() =>
            {
                while (true)
                {
                    MainScripts.CheckForConnectionLost();

                    Thread.Sleep(60 * 1000);
                }
            });

            var ShipControl = new Thread(() =>
            {
                while (true)
                {
                    if (ThreadManager.AllowShipControl)
                    {
                        ThreadManager.AllowPVEMode = true;
                    }
                    else
                    {
                        ThreadManager.AllowPVEMode = false;
                        ThreadManager.AllowShieldHPControl= false;
                    }

                    Thread.Sleep(1000);
                }
            });

            var ShipState = new Thread(() =>
            {
                while (true)
                {
                    if (ThreadManager.AllowShipControl)
                    {
                        (int X, _) = Finders.FindLocWnd("OverView");
                        if (X == 0 && !ThreadManager.AllowDocking) // X == 0 ship docked
                        {
                            Console.WriteLine("ship docking, but allow to docking false");
                            ThreadManager.AllowShipControl = false;
                            while (ThreadManager.PVEModeRunning)
                            {
                                Thread.Sleep(1 * 1000);
                                //Console.WriteLine("PVEModeRunning = true");
                            }
                            
                            if (!MainScripts.CurrentSystemIsDanger())
                            {
                                ThreadManager.AllowShipControl = true;
                            }
                        }
                    }

                    Thread.Sleep(2 * 60 * 1000);
                }
            });

            var PVEMode = new Thread(() =>
            {
                Thread ScriptExecutor = new Thread(() =>
                {
                    try
                    {
                        MainScripts.BotStart();
                    }
                    catch (ThreadInterruptedException e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    
                });
                while (true)
                {
                    if (ThreadManager.AllowPVEMode && ThreadManager.AllowShipControl)
                    {
                        if (!ScriptExecutor.IsAlive)
                        {
                            Console.WriteLine("starting ScriptExecutor thread");
                            ThreadManager.PVEModeRunning = true;
                            ScriptExecutor.Start();
                        }
                    }
                    else
                    {
                        if (ScriptExecutor.IsAlive)
                        {
                            ThreadManager.AllowDroneControl = false;
                            ThreadManager.AllowDroneRescoop = false;
                            Console.WriteLine("stoping ScriptExecutor thread");
                            ScriptExecutor.Interrupt();
                            ScriptExecutor.Join();

                            ScriptExecutor = new Thread(() =>
                            {
                                try
                                {
                                    MainScripts.BotStart();
                                }
                                catch (ThreadInterruptedException e)
                                {
                                    Console.WriteLine(e.Message);
                                }
                            });
                            ThreadManager.PVEModeRunning = false;
                        }
                    }
                    Thread.Sleep(1000);
                }
            });

            var ShipHPWatcher = new Thread(() =>
            {
                Thread FlyOff = new Thread(() =>
                {
                    try
                    {
                        SecScripts.FlyOffInLowHP();
                    }
                    catch (ThreadInterruptedException e)
                    {
                        Console.WriteLine(e.Message);
                    }
                });
                while (true)
                {
                    if (ThreadManager.AllowShieldHPControl && ThreadManager.AllowShipControl)
                    {
                        if (Checkers.ShipInLowHP(40))
                        {
                            if (!FlyOff.IsAlive)
                            {
                                FlyOff = new Thread(() =>
                                {
                                    try
                                    {
                                        SecScripts.FlyOffInLowHP();
                                    }
                                    catch (ThreadInterruptedException e)
                                    {
                                        Console.WriteLine(e.Message);
                                    }
                                });
                                Console.WriteLine("starting thread FlyOff");
                                FlyOff.Start();
                            }
                        }
                    }
                    else
                    {
                        if (FlyOff.IsAlive)
                        {
                            Console.WriteLine("stoping thread FlyOff");
                            FlyOff.Interrupt();
                            FlyOff.Join();
                        }
                    }

                    Thread.Sleep(1000);
                }
            });

            var EnemyWatcher = new Thread(() =>
            {
                while (true)
                {
                    if (ThreadManager.AllowEnemyWatcher && ThreadManager.AllowShipControl)
                    {
                        ThreadManager.AllowDroneControl = true;

                    }
                    else
                        ThreadManager.AllowDroneControl = false;

                    Thread.Sleep(1000);
                }
            });

            

            var CheckerRedMarker = new Thread(() =>
            {
                while (true)
                {
                    if (ThreadManager.DangerAnalyzerEnable)
                    {
                        if (MainScripts.CurrentSystemIsDanger() || MainScripts.CheckDScan())
                        {
                            ThreadManager.AllowShipControl = false;
                            while (ThreadManager.PVEModeRunning)
                                Thread.Sleep(1 * 1000);

                            Emulators.AllowControlEmulator = true;
                            MainScripts.DockingFromSuicides();
                            ThreadManager.AllowShipControl = true;
                        }
                    }
                    Thread.Sleep(ThreadManager.MultiplierSleep * 1000);
                }
            });

            DisconnectChecker.Start();
            ShipControl.Start();
            ShipState.Start();
            PVEMode.Start();
            ShipHPWatcher.Start();
            //EnemyWatcher.Start();
            //DroneController.DroneControl.Start();
            //DroneController.DroneRescoopControl.Start();
            MissileController.MissileControlSystem.Start();
            CheckerRedMarker.Start();

            PVEMode.Join();
            Console.WriteLine("threads have completed.");
        }

        if (!Config.AutopilotMode)
            StartThreads();
        else if (Config.AutopilotMode)
            Autopilot.Start();

        //SecScripts.StartLayRoute();


        return 0;
    }
}