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
using EVE_Bot.Parsers;
using System.Net;
using System.IO;
using System.Text.Json;

static public class Program
{
    static async Task<int> Main(string[] args)
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
                        ThreadManager.AllowNavigationControl = false;
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
                Thread ShieldChecker = new Thread(() =>
                {
                    try
                    {
                        Checkers.ShipInLowHP(40);
                    }
                    catch (ThreadInterruptedException)
                    {
                        //Console.WriteLine(e.Message);
                    }
                });
                while (true)
                {
                    if (ThreadManager.AllowShieldHPControl && ThreadManager.AllowShipControl)
                    {
                        if (!ShieldChecker.IsAlive)
                        {
                            ShieldChecker = new Thread(() =>
                            {
                                try
                                {
                                    Checkers.ShipInLowHP(40);
                                }
                                catch (ThreadInterruptedException)
                                {
                                    //Console.WriteLine(e.Message);
                                }
                            });
                            //Console.WriteLine("starting thread CheckShield");
                            ShieldChecker.Start();
                        }
                    }
                    else
                    {
                        if (ShieldChecker.IsAlive)
                        {
                            //Console.WriteLine("stoping thread CheckShield");
                            ShieldChecker.Interrupt();
                            ShieldChecker.Join();
                        }
                    }

                    Thread.Sleep(ThreadManager.MultiplierSleep * 1000);
                }
            });

            //var EnemyWatcher = new Thread(() =>
            //{
            //    while (true)
            //    {
            //        if (ThreadManager.AllowEnemyWatcher && ThreadManager.AllowShipControl)
            //        {
            //            ThreadManager.AllowDroneControl = true;

            //        }
            //        else
            //            ThreadManager.AllowDroneControl = false;

            //        Thread.Sleep(1000);
            //    }
            //});

            var CheckerRedMarker = new Thread(() =>
            {
                while (true)
                {
                    if (ThreadManager.DangerAnalyzerEnable)
                    {
                        if (MainScripts.CurrentSystemIsDanger())
                        {
                            ThreadManager.AllowShipControl = false;
                            ThreadManager.AllowToAttack = false;
                            while (ThreadManager.PVEModeRunning)
                                Thread.Sleep(1 * 1000);

                            Emulators.AllowControlEmulator = true;
                            Emulators.AllowHighLVLControl = false;
                            MainScripts.DockingFromSuicides();
                            ThreadManager.AllowShipControl = true;
                        }
                        else if (MainScripts.CheckDScan())
                        {
                            ThreadManager.AllowShipControl = false;
                            ThreadManager.AllowToAttack = false;
                            while (ThreadManager.PVEModeRunning)
                                Thread.Sleep(1 * 1000);

                            Emulators.AllowControlEmulator = true;
                            Emulators.AllowHighLVLControl = false;
                            if (!MainScripts.GotoNextSystem(NeedToLayRoute: false))
                            {
                                MainScripts.DockingFromSuicides();
                            }
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
            ShipController.NavigationControlSystem.Start();
            //EnemyWatcher.Start();
            //DroneController.DroneControl.Start();
            //DroneController.DroneRescoopControl.Start();
            MissileController.MissileControlSystem.Start();
            CheckerRedMarker.Start();

            PVEMode.Join();
            Console.WriteLine("threads have completed.");
        }

        static async Task<string> GetRequestAsync(string Url)
        {
            WebRequest reqGET = WebRequest.Create(Url);
            WebResponse resp = await reqGET.GetResponseAsync();
            Stream stream = resp.GetResponseStream();
            StreamReader sr = new StreamReader(stream);
            string s = sr.ReadToEnd();
            //Console.WriteLine(s);
            resp.Close();
            return s;
        }

        static DateTime UnixTimeStampToDateTime(int unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }


        async Task<string> CheckState()
        {
            var Time = await GetRequestAsync("http://worldtimeapi.org/api/timezone/Europe/London");
            TimeJson TimeSeconds = JsonSerializer.Deserialize<TimeJson>(Time);

            //var unixTimestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            //Console.WriteLine(unixTimestamp);

            var unixTimestamp = TimeSeconds.Unixtime;

            var RandomTime = (int)new DateTime(2022, 9, 1).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

            var res = RandomTime - unixTimestamp;
            if (RandomTime - unixTimestamp < 0)
            {
                Console.WriteLine("prosro4en");
                Environment.Exit(10);
            }
            return Time;
        }
        string success = "";
        success = await CheckState();
        if (success == "")
        {
            Environment.Exit(10);
        }

        
        if (!Config.AutopilotMode)
            StartThreads();
        else if (Config.AutopilotMode)
            Autopilot.Start();


        return 0;
    }
}
class TimeJson
{
    public int Unixtime { get; set; }
}