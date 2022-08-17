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
                        Console.WriteLine("starting thread MissileMonitor");
                    }
                }
                else
                {
                    if (MissileMonitor.IsAlive)
                    {
                        Console.WriteLine("stoping thread MissileMonitor");
                        MissileMonitor.Interrupt();
                        MissileMonitor.Join();
                    }
                }

                Thread.Sleep(ThreadManager.MultiplierSleep * 250);
            }
        });
        static void MonitorWorkingMissiles()
        {
            while (true)
            {
                List<Module> AllModules = HI.GetAllModulesInfo(HI.GetHudContainer());

                for (int i = 0; i < AllModules.Count; i++)
                {
                    Module Module = AllModules[i];
                    if (Module.Type == "high" && Module.Mode == "idle" && Module.AmountСharges != 0)
                    {
                        Console.WriteLine("missiles are idle");
                        General.ModuleActivityManager(ModulesInfo.MissileLauncher, true);
                    }
                    else if (Module.Type == "high" && Module.Mode == "idle" && Module.AmountСharges == 0)
                    {
                        General.DockToStationAndExit();
                    }
                    General.EnsureTargetIsAvailable();
                }

                Thread.Sleep(500);
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
                General.ModuleActivityManager(ModulesInfo.MissileLauncher, false);
                //Console.WriteLine(e.Message);
            }
        }
    }
}
