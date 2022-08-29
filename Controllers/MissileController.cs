using EVE_Bot.Models;
using EVE_Bot.Parsers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using read_memory_64_bit;
using EVE_Bot.Scripts;

namespace EVE_Bot.Controllers
{
    static public class MissileController
    {
        static ModulesInfo ModulesInfo = new ModulesInfo();
        static int CountTimeFor0Amount = 0;


        static public Thread MissileControlSystem = new Thread(() =>
        {
            Thread MissileMonitor = new Thread(Wrapper);
            while (true)
            {
                if (ThreadManager.AllowToAttack && ThreadManager.AllowShipControl)
                {
                    if (!MissileMonitor.IsAlive)
                    {
                        MissileMonitor = new Thread(Wrapper);
                        MissileMonitor.Start();
                        //Console.WriteLine("starting thread MissileMonitor");
                    }
                }
                else
                {
                    if (MissileMonitor.IsAlive)
                    {
                        //Console.WriteLine("stoping thread MissileMonitor");
                        MissileMonitor.Interrupt();
                        MissileMonitor.Join();
                        Emulators.AllowControlEmulator = true;
                        Emulators.ClickRB(500, 100);
                    }
                }

                Thread.Sleep(ThreadManager.MultiplierSleep * 250);
            }
        });
        static void MonitorWorkingMissiles()
        {
            while (true)
            {
                Module MissileLauncher = HI.GetAllModulesInfo(HI.GetHudContainer())
                    .Find(module => module.Name == ModulesInfo.MissileLauncher);

                if (MissileLauncher == null)
                {
                    Console.WriteLine($"{ModulesInfo.MissileLauncher} not found");
                    General.DockToStationAndExit();
                }

                if (MissileLauncher.Mode == "idle" && MissileLauncher.AmountСharges != 0)
                {
                    //Console.WriteLine("missiles are idle");
                    General.ModuleActivityManager(ModulesInfo.MissileLauncher, true);
                    CountTimeFor0Amount = 0;
                }
                else if (MissileLauncher.Mode == "reloading")
                {
                    CountTimeFor0Amount = 0;
                }
                else if (MissileLauncher.Mode == "idle" && MissileLauncher.AmountСharges == 0)
                {
                    CountTimeFor0Amount++;
                }
                if (CountTimeFor0Amount > 10)
                {
                    Console.WriteLine("missiles are over, amount = {0}", MissileLauncher.AmountСharges);
                    General.DockToStationAndExit();
                }
                
                General.EnsureTargetIsAvailable();

                Thread.Sleep(300);
            }
        }
        static void EnsureStartThread(Thread MissileMonitor)
        {
            if (!MissileMonitor.IsAlive)
            {
                MissileMonitor = new Thread(Wrapper);
                MissileMonitor.Start();
                Console.WriteLine("starting thread MissileMonitor");
            }
        }
        static void EnsureDestroyThread(Thread MissileMonitor)
        {
            if (MissileMonitor.IsAlive)
            {
                Console.WriteLine("stoping thread MissileMonitor");
                MissileMonitor.Interrupt();
                MissileMonitor.Join();
            }
        }
        static void Wrapper()
        {
            try
            {
                MonitorWorkingMissiles();
            }
            catch (ThreadInterruptedException)
            {
                //if (OV.GetInfo().Exists(item => item.TargetLocked))
                //{
                //    General.ModuleActivityManager(ModulesInfo.RapidMissileLauncher, false);
                //}
                //Console.WriteLine(e.Message);
            }
        }
    }
}
