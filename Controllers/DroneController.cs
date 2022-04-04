using EVE_Bot.Searchers;
using read_memory_64_bit;
using System;
using System.Collections.Generic;
using System.Text;

namespace EVE_Bot.Controllers
{
    static public class DroneController
    {
        volatile static public bool DronesDroped = false;

        static public bool DropDrones()
        {
            Emulators.PressButton((int)WinApi.VirtualKeyShort.VK_G);
            System.Threading.Thread.Sleep(1000 * 3);

            for (int i = 0; i < 5; i++)
            {
                UITreeNode uiTreePreparedForFile = null;
                UITreeNode deepDrones = null;
                UITreeNode needDrone = null;

                uiTreePreparedForFile = GetUITrees();
                deepDrones = uiTreePreparedForFile.FindEntityOfString("DroneEntry");
                if (deepDrones == null)
                    continue;
                needDrone = deepDrones.handleEntity("DroneEntry");

                //вывод инфы по дронам
                for (int k = 0; k < needDrone.children.Length; k++)
                {
                    if (needDrone.children[k] != null)
                    {
                        if (needDrone.children[k].dictEntriesOfInterest.ContainsKey("_hint"))
                        {
                            Console.WriteLine("drones droped");
                            DronesDroped = true;
                            return true;
                        }
                    }
                }
                Console.WriteLine("drones not launch");
                Emulators.PressButton((int)WinApi.VirtualKeyShort.VK_G);
                System.Threading.Thread.Sleep(1000);
            }
            Console.WriteLine("drones dont want to launch");
            return false;
        }

        static public bool ScoopDrones()
        {
            Emulators.PressButton((int)WinApi.VirtualKeyShort.VK_H);
            System.Threading.Thread.Sleep(1000 * 3);

            int DronesInSpace;

            for (int i = 0; i < 60; i++)
            {
                UITreeNode uiTreePreparedForFile = null;
                UITreeNode deepDrones = null;
                UITreeNode needDrone = null;

                uiTreePreparedForFile = GetUITrees();
                deepDrones = uiTreePreparedForFile.FindEntityOfString("DroneEntry");
                if (deepDrones == null)
                    continue;
                needDrone = deepDrones.handleEntity("DroneEntry");

                DronesInSpace = 0;

                //вывод инфы по дронам
                for (int k = 0; k < needDrone.children.Length; k++)
                {
                    if (needDrone.children[k] != null)
                    {
                        if (needDrone.children[k].dictEntriesOfInterest.ContainsKey("_hint"))
                        {
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
                    }
                }
                if (DronesInSpace == 0)
                {
                    Console.WriteLine("drones returned");
                    DronesDroped = false;
                    return true;
                }
                System.Threading.Thread.Sleep(1000 * 2);
            }
            Console.WriteLine("failed to scoop drones");
            return false;
        }

        static public void RescoopDrones()
        {
            ThreadManager.AllowDroneControl = false;
            while (DroneController.DronesDroped)
                System.Threading.Thread.Sleep(1000);
            System.Threading.Thread.Sleep(1000 * 3);
            ThreadManager.AllowDroneControl = true;
        }


        static public void EngageTarget()
        {
            var uiTreePreparedForFile = GetUITrees();
            var deepDrones = uiTreePreparedForFile.FindEntityOfString("DroneEntry");
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
                            if (ThreadManager.AllowToAttack)
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

        static public bool GetInfoHPDrones()
        {
            var uiTreePreparedForFile = GetUITrees();

            var deepDrones = uiTreePreparedForFile.FindEntityOfString("DroneEntry");
            if (deepDrones == null)
            {
                return false;
            }

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
                        //Unhandled exception. System.NullReferenceException: Object reference not set to an instance of an object.
                        if (12 < Convert.ToInt32(needDrone.children[k].children[0].children[0].children[1].children[j].children[1].dictEntriesOfInterest["_displayWidth"]))
                        {
                            Console.WriteLine(needDrone.children[k].dictEntriesOfInterest["_name"]);
                            Console.WriteLine(needDrone.children[k].children[0].children[0].children[1].children[j].dictEntriesOfInterest["_name"]);
                            Console.WriteLine("HP {0}/22", 22 - Convert.ToInt32(needDrone.children[k].children[0].children[0].children[1].children[j].children[1].dictEntriesOfInterest["_displayWidth"]));
                            Console.WriteLine("some drone take damage, rescoop");
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
