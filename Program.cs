using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Configuration;
using EVE_Bot.Searchers;
using EVE_Bot.Controllers;
using EVE_Bot.Scripts;
using System.Threading.Tasks;
using System.Threading;

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
                            
                            if (!MainScripts.CheckForSuicidesInChat())
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
                Thread CleanerRoom = new Thread(() =>
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
                        if (!CleanerRoom.IsAlive)
                        {
                            Console.WriteLine("starting thread CleanerRoom");
                            ThreadManager.PVEModeRunning = true;
                            CleanerRoom.Start();
                        }
                    }
                    else
                    {
                        if (CleanerRoom.IsAlive)
                        {
                            ThreadManager.AllowDroneControl = false;
                            ThreadManager.AllowDroneRescoop = false;
                            Console.WriteLine("stoping thread CleanerRoom");
                            CleanerRoom.Interrupt();
                            CleanerRoom.Join();

                            CleanerRoom = new Thread(() =>
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
                        SecondaryScripts.FlyOffInLowHP();
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
                                        SecondaryScripts.FlyOffInLowHP();
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

            var DroneRescoopController = new Thread(() =>
            {
                Thread DroneRescooper = new Thread(() =>
                {
                    try
                    {
                        DroneController.RescoopDrones();
                    }
                    catch (ThreadInterruptedException e)
                    {
                        Console.WriteLine(e.Message);
                    }
                });
                while (true)
                {
                    if (ThreadManager.AllowDroneRescoop && ThreadManager.AllowShipControl)
                    {
                        if (DroneController.GetInfoHPDrones())
                        {
                            if (!DroneRescooper.IsAlive)
                            {
                                DroneRescooper = new Thread(() =>
                                {
                                    try
                                    {
                                        DroneController.RescoopDrones();
                                    }
                                    catch (ThreadInterruptedException e)
                                    {
                                        Console.WriteLine(e.Message);
                                    }
                                });
                                DroneRescooper.Start();
                            }

                        }
                    }
                    else
                    {
                        if (DroneRescooper.IsAlive)
                        {
                            Console.WriteLine("stoping thread DroneRescooper");
                            DroneRescooper.Interrupt();
                            DroneRescooper.Join();
                        }
                    }

                    Thread.Sleep(5 * 1000);
                }
            });

            var DroneControl = new Thread(() =>
            {
                while (true)
                {
                    if (ThreadManager.AllowDroneControl && ThreadManager.AllowShipControl)
                    {
                        if (!DroneController.DronesDroped)
                        {
                            DroneController.DropDrones();
                        }

                        DroneController.EngageTarget();
                    }
                    else
                    {
                        if (DroneController.DronesDroped)
                        {
                            DroneController.ScoopDrones();
                        }
                    }

                    Thread.Sleep(5 * 1000);
                }
            });

            var CheckerRedMarker = new Thread(() =>
            {
                while (true)
                {
                    if (ThreadManager.AllowCheckRedMarker)
                    {
                        if (MainScripts.CheckForSuicidesInChat())
                        {
                            ThreadManager.AllowShipControl = false;
                            Thread.Sleep(5 * 1000);
                            Emulators.AllowControlEmulator = true;
                            MainScripts.DockingFromSuicides();
                            ThreadManager.AllowShipControl = true;
                        }
                    }
                    Thread.Sleep(5 * 1000);
                }
            });

            DisconnectChecker.Start();
            ShipControl.Start();
            ShipState.Start();
            PVEMode.Start();
            ShipHPWatcher.Start();
            //EnemyWatcher.Start();
            DroneControl.Start();
            DroneRescoopController.Start();
            CheckerRedMarker.Start();

            PVEMode.Join();
            Console.WriteLine("threads have completed.");
        }

        StartThreads();

        //Autopilot.Start();

        return 0;
    }
}