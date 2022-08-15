using read_memory_64_bit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;
using EVE_Bot.Controllers;
using EVE_Bot.Searchers;

namespace EVE_Bot.Controllers
{
    static public class DroneController
    {
        static public Random r = new Random();
        volatile static public bool DronesDroped = false;
        volatile static public bool DroneLaunchInProgress = false;

        public struct OneDroneInfo
        {
            public OneDroneInfo(int y, int HpStructure, int HpArmor, int HpShield)
            {
                Y = y;
                HPStructure = HpStructure;
                HPArmor = HpArmor;
                HPShield = HpShield;
            }
            public int Y { get; set; }
            public int HPStructure { get; set; }
            public int HPArmor { get; set; }
            public int HPShield { get; set; }

            public override string ToString() => $"Y = {Y}, HPStructure = {HPStructure}, HPArmor = {HPArmor}, HPShield = {HPShield}";
        }

        static public Thread DroneRescoopControl = new Thread(() =>
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
                    if ((Checkers.CheckEnemyAbsenceAgr("attackingMe") && DroneController.DroneIsUnderAttack()) || ThreadManager.SpecialFocusOnDrones)
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

                Thread.Sleep(ThreadManager.MultiplierSleep * 1000);
            }
        });

        static public Thread DroneControl = new Thread(() =>
        {
            while (true)
            {
                if (ThreadManager.AllowDroneControl && ThreadManager.AllowShipControl)
                {
                    if (!DroneController.DronesDroped)
                        DroneController.DropDrones();

                    DroneController.EngageIdleDrones();
                }
                else
                {
                    if (DroneController.DronesDroped)
                        DroneController.ScoopDrones();
                }

                Thread.Sleep(ThreadManager.MultiplierSleep * 1000);
            }
        });

        static public bool DropDrones()
        {
            var (X, Y) = Finders.FindLocWnd("DroneView");
            if (X == 0)
                return false;
            var DronesInBayBeforeDrop = Checkers.CheckQuantityDrones();

            //var InfoDrones = GetCoordsLessDamageDrone();
            //if (InfoDrones.Last().HPStructure != 22 ||
            //    InfoDrones.Last().HPArmor != 22 ||
            //    InfoDrones.Last().HPShield < 20)
            //{
            //    Emulators.Drag(X + 20, Y + InfoDrones[0].Y + 60, X - 100, Y + InfoDrones[0].Y + 60);
            //    Thread.Sleep(700 + r.Next(-400, 400));
            //    InfoDrones = GetCoordsLessDamageDrone();
            //    if (InfoDrones != null && InfoDrones.Count > 0)
            //        Emulators.Drag(X + 20, Y + InfoDrones[0].Y + 60, X - 100, Y + InfoDrones[0].Y + 60);

            //    //var QuantityDronesInSpace = GetUITrees().FindEntityOfStringByDictEntriesOfInterest("_setText", "Drones in Local Space (2)");
            //    //for (int i = 0; i < 10; i++)
            //    //{
            //    //    Thread.Sleep(2 * 1000 + r.Next(-100, 100));
            //    //    QuantityDronesInSpace = GetUITrees().FindEntityOfStringByDictEntriesOfInterest("_setText", "Drones in Local Space (2)");
            //    //    if (QuantityDronesInSpace != null)
            //    //        break;
            //    //    InfoDrones = GetCoordsLessDamageDrone();
            //    //    Emulators.Drag(X + 20, Y + InfoDrones[0].Y + 60, X - 100, Y + InfoDrones[0].Y + 60);
            //    //}
            //}
            //else
            //    Emulators.PressButton((int)WinApi.VirtualKeyShort.VK_G);
            //Thread.Sleep(1000 + r.Next(-100, 100));
            Emulators.PressButton((int)WinApi.VirtualKeyShort.VK_G);
            Thread.Sleep(2 * 1000 + r.Next(-100, 100));

            for (int i = 0; i < 5; i++)
            {
                var QuantityDronesInSpace = GetUITrees()
                    .FindEntityOfStringByDictEntriesOfInterest("_setText", "Drones in Local Space (5)");
                if (QuantityDronesInSpace != null)
                {
                    Console.WriteLine("drones droped");
                    DronesDroped = true;
                    DroneLaunchInProgress = false;
                    return true;
                }
                QuantityDronesInSpace = GetUITrees()
                    .FindEntityOfStringByDictEntriesOfInterest("_setText", $"Drones in Bay ({DronesInBayBeforeDrop - 5})");
                if (QuantityDronesInSpace != null)
                {
                    Console.WriteLine("drones droped");
                    DronesDroped = true;
                    DroneLaunchInProgress = false;
                    return true;
                }

                //var deepDrones = GetUITrees().FindEntityOfString("DroneEntry");
                //if (deepDrones == null)
                //    continue;
                //var needDrone = deepDrones.handleEntity("DroneEntry");

                //вывод инфы по дронам
                //for (int k = 0; k < needDrone.children.Length; k++)
                //{
                //    if (needDrone.children[k] == null)
                //        continue;

                //    if (!needDrone.children[k].dictEntriesOfInterest.ContainsKey("_hint"))
                //        continue;

                //    Console.WriteLine("drones droped");
                //    DronesDroped = true;
                //    DroneLaunchInProgress = false;
                //    return true;
                //}
                Console.WriteLine("drones not launch");
                Emulators.PressButton((int)WinApi.VirtualKeyShort.VK_G);
                Thread.Sleep(1000);
            }
            Console.WriteLine("drones dont want to launch");
            DroneLaunchInProgress = false;
            return false;
        }

        static public bool ScoopDrones()
        {
            Emulators.PressButton((int)WinApi.VirtualKeyShort.VK_H);
            Thread.Sleep(1000 * 3);

            int DronesInSpace;

            for (int i = 0; i < 60; i++)
            {
                var deepDrones = GetUITrees().FindEntityOfString("DroneEntry");
                if (deepDrones == null)
                    continue;
                var needDrone = deepDrones.handleEntity("DroneEntry");

                DronesInSpace = 0;

                //вывод инфы по дронам
                for (int k = 0; k < needDrone.children.Length; k++)
                {
                    if (needDrone.children[k] == null)
                        continue;

                    if (!needDrone.children[k].dictEntriesOfInterest.ContainsKey("_hint"))
                        continue;

                    DronesInSpace++;
                    if (needDrone.children[k].dictEntriesOfInterest["_hint"].ToString().Contains("Returning"))
                    {
                        Console.WriteLine("drones are returning");
                        break;
                    }
                    else
                    {
                        Console.WriteLine("drones dont want to return");
                        Emulators.PressButton((int)WinApi.VirtualKeyShort.VK_H);
                        break;
                    }
                }
                if (DronesInSpace == 0)
                {
                    Console.WriteLine("drones returned");
                    DronesDroped = false;
                    return true;
                }
                Thread.Sleep(1000 * 2);
            }
            Console.WriteLine("failed to scoop drones");
            return false;
        }

        static public void RescoopDrones()
        {
            Console.WriteLine("some drone take damage, rescoop");
            ThreadManager.AllowDroneControl = false;
            while (DroneController.DronesDroped)
                Thread.Sleep(1000);
            Thread.Sleep(3 * 1000);
            ThreadManager.AllowDroneControl = true;
            ThreadManager.SpecialFocusOnDrones = false;
            Thread.Sleep(15 * 1000);
        }


        static public void EngageIdleDrones()
        {
            var deepDrones = GetUITrees().FindEntityOfString("DroneEntry");
            if (deepDrones == null)
                return;
            var needDrone = deepDrones.handleEntity("DroneEntry");

            for (int k = 0; k < needDrone.children.Length; k++)
            {
                if (needDrone.children[k] != null)
                {
                    if (needDrone.children[k].dictEntriesOfInterest.ContainsKey("_hint"))
                    {
                        if (needDrone.children[k].dictEntriesOfInterest["_hint"].ToString().Contains("Idle"))
                        {
                            //Console.WriteLine(needDrone.children[k].dictEntriesOfInterest["_hint"]);
                            if (ThreadManager.AllowToAttack && Checkers.WatchLockingTargets())
                            {
                                Console.WriteLine("some drone idle");
                                Emulators.PressButton((int)WinApi.VirtualKeyShort.VK_F);
                            }
                            return;
                        }
                    }
                }
            }
        }

        static public List<OneDroneInfo> GetCoordsLessDamageDrone()
        {
            var deepDrones = GetUITrees().FindEntityOfString("DroneEntry");
            if (deepDrones == null)
                return null;

            var needDrone = deepDrones.handleEntity("DroneEntry");


            List<OneDroneInfo> InfoArmorDrones = new List<OneDroneInfo>();


            for (int k = 0; k < needDrone.children.Length; k++)
            {
                //lesenka
                if (needDrone.children[k] == null)
                    continue;
                if (needDrone.children[k].children == null)
                    continue;
                if (needDrone.children[k].children.Length == 0)
                    continue;
                if (needDrone.children[k].children[0] == null)
                    continue;
                if (needDrone.children[k].children[0].children == null)
                    continue;
                if (needDrone.children[k].children[0].children.Length == 0)
                    continue;
                if (needDrone.children[k].children[0].children[0] == null)
                    continue;
                if (needDrone.children[k].children[0].children[0].children == null)
                    continue;
                if (needDrone.children[k].children[0].children[0].children.Length < 2)
                    continue;
                if (needDrone.children[k].children[0].children[0].children[1] == null)
                    continue;
                if (needDrone.children[k].children[0].children[0].children[1].children == null)
                    continue;

                if (needDrone.children[k].dictEntriesOfInterest.ContainsKey("_hint"))
                    continue;

                int HPStruct = 0, HPArmor = 0, HPShield = 0;

                for (int j = 0; j < needDrone.children[k].children[0].children[0].children[1].children.Length; j++)
                {
                    if (needDrone.children[k].children[0].children[0].children[1].children[j].children == null)
                        continue;
                    if (needDrone.children[k].children[0].children[0].children[1].children[j].children.Length < 1)
                        continue;
                    if (needDrone.children[k].children[0].children[0].children[1].children[j].children[1] == null)
                        continue;


                    if (needDrone.children[k].children[0].children[0].children[1].children[j].dictEntriesOfInterest["_name"].ToString() == "gauge_struct")
                    {
                        HPStruct = 22 - Convert.ToInt32(needDrone.children[k].children[0].children[0].children[1].children[j].children[1]
                            .dictEntriesOfInterest["_displayWidth"]);
                    }
                    if (needDrone.children[k].children[0].children[0].children[1].children[j].dictEntriesOfInterest["_name"].ToString() == "gauge_armor")
                    {
                        HPArmor = 22 - Convert.ToInt32(needDrone.children[k].children[0].children[0].children[1].children[j].children[1]
                            .dictEntriesOfInterest["_displayWidth"]);
                    }
                    if (needDrone.children[k].children[0].children[0].children[1].children[j].dictEntriesOfInterest["_name"].ToString() == "gauge_shield")
                    {
                        HPShield = 22 - Convert.ToInt32(needDrone.children[k].children[0].children[0].children[1].children[j].children[1]
                            .dictEntriesOfInterest["_displayWidth"]);
                    }
                }
                InfoArmorDrones.Add(
                        new OneDroneInfo(Convert.ToInt32(needDrone.children[k].dictEntriesOfInterest["_displayY"]) - 23, HPStruct, HPArmor, HPShield)
                        );
            }

            //InfoArmorDrones.ForEach(x => Console.WriteLine(x));
            //Console.WriteLine();
            InfoArmorDrones = InfoArmorDrones
                .OrderByDescending(OneDrone => OneDrone.HPShield)
                .ThenByDescending(OneDrone => OneDrone.HPArmor)
                .ThenByDescending(OneDrone => OneDrone.HPStructure)
                .ToList();
            //InfoArmorDrones.ForEach(x => Console.WriteLine(x));
            return InfoArmorDrones;
        }

        static public bool DroneIsUnderAttack()
        {
            var deepDrones = GetUITrees().FindEntityOfString("DroneEntry");
            if (deepDrones == null)
                return false;

            var needDrone = deepDrones.handleEntity("DroneEntry");

            for (int k = 0; k < needDrone.children.Length; k++)
            {
                //lesenka
                if (needDrone.children[k] == null)
                    continue;
                if (needDrone.children[k].children == null)
                    continue;
                if (needDrone.children[k].children.Length == 0)
                    continue;
                if (needDrone.children[k].children[0] == null)
                    continue;
                if (needDrone.children[k].children[0].children == null)
                    continue;
                if (needDrone.children[k].children[0].children.Length == 0)
                    continue;
                if (needDrone.children[k].children[0].children[0] == null)
                    continue;
                if (needDrone.children[k].children[0].children[0].children == null)
                    continue;
                if (needDrone.children[k].children[0].children[0].children.Length < 2)
                    continue;
                if (needDrone.children[k].children[0].children[0].children[1] == null)
                    continue;
                if (needDrone.children[k].children[0].children[0].children[1].children == null)
                    continue;

                //System.NullReferenceException: Object reference not set to an instance of an object.
                for (int j = 0; j < needDrone.children[k].children[0].children[0].children[1].children.Length; j++)
                {

                    if (needDrone.children[k].children[0].children[0].children[1].children[j].dictEntriesOfInterest["_name"].ToString() == "gauge_shield")
                    {
                        if (needDrone.children[k].children[0].children[0].children[1].children[j].children == null)
                            continue;
                        if (needDrone.children[k].children[0].children[0].children[1].children[j].children.Length < 2)
                            continue;
                        if (needDrone.children[k].children[0].children[0].children[1].children[j].children[1] == null)
                            continue;

                        //Unhandled exception. System.NullReferenceException: Object reference not set to an instance of an object.
                        if (12 < Convert.ToInt32(needDrone.children[k].children[0].children[0].children[1].children[j].children[1].dictEntriesOfInterest["_displayWidth"]))
                        {
                            //Console.WriteLine(needDrone.children[k].dictEntriesOfInterest["_name"]);
                            //Console.WriteLine(needDrone.children[k].children[0].children[0].children[1].children[j].dictEntriesOfInterest["_name"]);
                            Console.WriteLine("HP {0}/22", 22 - Convert.ToInt32(needDrone.children[k].children[0].children[0].children[1].children[j].children[1].dictEntriesOfInterest["_displayWidth"]));
                            return true;
                        }
                    }

                }
            }
            return false;
        }

        static UITreeNode GetUITrees()
        {
            return ReadMemory.GetUITrees(Window.RootAddress, Window.processId);
        }
    }
}
