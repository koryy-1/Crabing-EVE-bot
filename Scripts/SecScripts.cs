using EVE_Bot.Configs;
using EVE_Bot.Controllers;
using EVE_Bot.Models;
using EVE_Bot.Parsers;
using EVE_Bot.Searchers;
using read_memory_64_bit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace EVE_Bot.Scripts
{
    static public class SecScripts
    {
        static public Random r = new Random();
        static public int AvgDeley = Config.AverageDelay;

        static public void CheckSituation()
        {
            //после перезахода
            //поменять вкладку чата
            (int XlocChatWindowStack, int YlocChatWindowStack) = Finders.FindLocWnd("ChatWindowStack");
            if (XlocChatWindowStack != 0)
            {
                Emulators.ClickLB(XlocChatWindowStack + 20, YlocChatWindowStack + 15);
                Thread.Sleep(AvgDeley + r.Next(-100, 100));
                Emulators.ClickLB(500, 100);
                Thread.Sleep(AvgDeley + r.Next(-100, 100));
            }

            //определить где находится корабль
            General.EnsureUndocked();
            ThreadManager.AllowDocking = false;

            //узнать количество ракет
            CheckMissilesAmount();


            //узнать количество дронов, реконнект
            //var (XlocNotepadWindow, _) = Finders.FindLocWnd("DroneView");
            //if (XlocNotepadWindow == 0)
            //{
            //    General.DockToStationAndExit();
            //}
            //var DronesQuantity = Checkers.CheckQuantityDrones();
            //if (DronesQuantity <= 3)
            //{
            //    //reconnect drones
            //    var HudContainer = GetUITrees().FindEntityOfString("HudContainer").handleEntity("HudContainer");
            //    var (XHudContainer, YHudContainer) = General.GetCoordsEntityOnScreen(HudContainer
            //            .children[Convert.ToInt32(HudContainer.dictEntriesOfInterest["needIndex"])]
            //            );

            //    var CenterHudContainer = HudContainer.FindEntityOfString("CenterHudContainer").handleEntity("CenterHudContainer");
            //    var (XCenterHudContainer, _) = General.GetCoordsEntityOnScreen(CenterHudContainer
            //            .children[Convert.ToInt32(CenterHudContainer.dictEntriesOfInterest["needIndex"])]
            //            );

            //    Emulators.ClickRB(XHudContainer + XCenterHudContainer + 95, YHudContainer + 100);
            //    Thread.Sleep(AvgDeley + r.Next(-100, 100));
            //    General.ClickInContextMenu("Reconnect Drones");
            //    Thread.Sleep(3 * 1000);

            //    DroneController.ScoopDrones();

            //    //поменять вкладку в инвентаре
            //    (int XlocInventory, int YlocInventory) = Finders.FindLocWnd("InventoryPrimary");
            //    Emulators.ClickLB(XlocInventory + 60, YlocInventory + 55);
            //    Thread.Sleep(AvgDeley + r.Next(-100, 100));
            //}



            ThreadManager.AllowDScan = true;

            //узнать количество дронов, докнуться если меньше 3-х
            //DronesQuantity = Checkers.CheckQuantityDrones();
            //if (DronesQuantity < 3)
            //{
            //    General.DockToStationAndExit();
            //}

            //поменять вкладку в гриде
            General.ChangeTab("General");

            //продолжить фармить экспу или аномальку
            (int XBlock, int YBlock) = Finders.FindExpBlock();
            (int XAcGate, int YAcGate) = Finders.FindAccelerationGate();
            if (XBlock != 0 || XAcGate != 0)
            {
                if (XBlock != 0)
                {
                    Emulators.ClickLB(XBlock, YBlock);
                }
                else if (XAcGate != 0)
                {
                    Emulators.ClickLB(XAcGate, YAcGate);
                }
                Thread.Sleep(AvgDeley + r.Next(-100, 100));
                var OrbBtn = General.GetCoordsButtonActiveItem("Orbit");
                Emulators.ClickLB(OrbBtn.Item1, OrbBtn.Item2);
                Thread.Sleep(AvgDeley + r.Next(-100, 100));
                General.ModuleActivityManager(1, true);

                MainScripts.ClearExpRoom();
                MainScripts.StartClearExp();
            }
            else
            {
                MainScripts.DestroyTargetsInRoom();
            }
            ThreadManager.MultiplierSleep = 10;
        }

        static public void DockingAndCheckingForSuicides()
        {
            int IsCriminal = 1;
            while (IsCriminal == 1)
            {
                IsCriminal = 0;
                Thread.Sleep(1000 * 60 * 2);

                List<ChatPlayer> ChatInfo = Chat.GetInfo();

                for (int i = 0; i < ChatInfo.Count; i++)
                {
                    if (ChatInfo[i].PlayerType == "Pilot is a suspect")
                    {
                        Console.WriteLine("suspect pilot in system");
                    }
                    if (ChatInfo[i].PlayerType == "Pilot is a criminal")
                    {
                        IsCriminal = 1;
                        Console.WriteLine("criminal pilot still in system");
                        break;
                    }
                }
            }
            General.Undock();
        }

        static public void FlyOffInLowHP()
        {
            FlyOff();
            for (int j = 0; j < 38; j++) // прим 5 мин
            {
                if (!Checkers.ShipInLowHP(50))
                {
                    (int XBlock, int YBlock) = Finders.FindExpBlock();
                    (int XAcGate, int YAcGate) = Finders.FindAccelerationGate();
                    if (XBlock > 0)
                    {
                        while (!Emulators.AllowControlEmulator)
                        {
                            Thread.Sleep(20);
                        }
                        
                        Emulators.AllowControlEmulator = false;
                        for (int i = 0; i < 5; i++)
                        {
                            Emulators.ClickLBForLockTargets(XBlock, YBlock);
                            Thread.Sleep(AvgDeley + r.Next(-100, 100));
                            var OrbBtn = General.GetCoordsButtonActiveItem("Orbit");
                            Emulators.ClickLBForLockTargets(OrbBtn.Item1, OrbBtn.Item1);
                            if (Checkers.CheckState("Orbiting"))
                                break;
                        }
                        Emulators.AllowControlEmulator = true;
                    }
                    else if (XAcGate > 0)
                    {
                        while (!Emulators.AllowControlEmulator)
                        {
                            Thread.Sleep(20);
                        }

                        Emulators.AllowControlEmulator = false;
                        for (int i = 0; i < 5; i++)
                        {
                            Emulators.ClickLBForLockTargets(XAcGate, YAcGate);
                            Thread.Sleep(AvgDeley + r.Next(-100, 100));
                            var OrbBtn = General.GetCoordsButtonActiveItem("Orbit");
                            Emulators.ClickLBForLockTargets(OrbBtn.Item1, OrbBtn.Item1);
                            if (Checkers.CheckState("Orbiting"))
                                break;
                        }
                        Emulators.AllowControlEmulator = true;
                    }
                    else
                    {
                        while (!Emulators.AllowControlEmulator)
                        {
                            Thread.Sleep(20);
                        }

                        (int XCont, int YCont) = Finders.FindObjectByWordInOverview("Cargo Container");
                        Emulators.AllowControlEmulator = false;
                        for (int i = 0; i < 5; i++)
                        {
                            Emulators.ClickLBForLockTargets(XCont, YCont);
                            Thread.Sleep(AvgDeley + r.Next(-100, 100));
                            var OrbBtn = General.GetCoordsButtonActiveItem("Orbit");
                            Emulators.ClickLBForLockTargets(OrbBtn.Item1, OrbBtn.Item1);
                            if (Checkers.CheckState("Orbiting"))
                                break;
                        }
                        Emulators.AllowControlEmulator = true;
                    }
                    return;
                }
                Thread.Sleep(1000 * 8);
            }
        }

        static public void FlyOff()
        {
            while (!Emulators.AllowControlEmulator)
            {
                Thread.Sleep(20);
            }

            (int XStationOrGate, int YStationOrGate) = Finders.FindObjectByWordInOverview("Station");

            if (XStationOrGate == 0)
            {
                (XStationOrGate, YStationOrGate) = Finders.FindObjectByWordInOverview("Stargate");
            }

            Emulators.AllowControlEmulator = false;
            for (int i = 0; i < 5; i++)
            {
                Emulators.ClickLBForLockTargets(XStationOrGate, YStationOrGate);
                Thread.Sleep(AvgDeley + r.Next(-100, 100));
                var OrbBtn = General.GetCoordsButtonActiveItem("Approach");
                Emulators.ClickLBForLockTargets(OrbBtn.Item1, OrbBtn.Item1);
                if (Checkers.CheckState("Approaching"))
                    break;
            }
            Emulators.AllowControlEmulator = true;
        }

        static public void UnloadCargo()
        {
            (int XStationCoords, int YStationCoords) = Finders.FindObjectByWordInOverview("Station");
            if (XStationCoords == 0)
            {
                Console.WriteLine("Station not found");
                return;
            }
            ThreadManager.AllowDocking = true;
            ThreadManager.AllowDScan = false;
            Console.WriteLine("unload cargo");

            General.GotoInActiveItem("Station", "Dock");

            Console.WriteLine("wait 1 min for dock");
            Checkers.WatchState();
            //Thread.Sleep(8 * 1000 + r.Next(-100, 100));

            var LobbyWnd = GetUITrees().FindEntityOfString("LobbyWnd");
            while (LobbyWnd == null)
            {
                Thread.Sleep(1000 + r.Next(-100, 100));
                LobbyWnd = GetUITrees().FindEntityOfString("LobbyWnd");
            }
            Thread.Sleep(5 * 1000 + r.Next(-100, 100));

            General.RepairShip();

            (int XlocChatWindowStack, int YlocChatWindowStack) = Finders.FindLocWnd("InventoryPrimary");
            if (XlocChatWindowStack == 0)
            {
                General.Undock();
                ThreadManager.AllowDocking = false;
                ThreadManager.AllowDScan = true;
                return;
            }
            Emulators.ClickRB(XlocChatWindowStack + 170, YlocChatWindowStack + 75);
            Thread.Sleep(AvgDeley + r.Next(-100, 100));
            Emulators.ClickLB(XlocChatWindowStack + 170 + 60, YlocChatWindowStack + 75 + 12);
            Thread.Sleep(AvgDeley + r.Next(-100, 100));

            Emulators.Drag(XlocChatWindowStack + 210, YlocChatWindowStack + 115, XlocChatWindowStack + 60, YlocChatWindowStack + 125);
            Thread.Sleep(AvgDeley + r.Next(-100, 100));

            General.Undock();
            ThreadManager.AllowDocking = false;
            ThreadManager.AllowDScan = true;
        }

        static public void StartLayRoute()
        {
            List<NotepadItem> NotepadInfo = NP.GetInfo();
            if (NotepadInfo == null)
                return;

            ThreadManager.AllowDScan = false;
            Thread.Sleep(3 * 1000);

            for (int i = 0; i < NotepadInfo.Count; i++)
            {
                Emulators.ClickRB(NotepadInfo[i].Pos.x, NotepadInfo[i].Pos.y);
                Thread.Sleep(AvgDeley + r.Next(-100, 100));
                var success = General.ClickInContextMenu("Add Waypoint");
                if (!success)
                {
                    Emulators.ClickLB(500, 100);
                }
                Thread.Sleep(AvgDeley + r.Next(-100, 100));
            }

            Emulators.ClickLB(500, 100);
            Thread.Sleep(AvgDeley + r.Next(-100, 100));
            ThreadManager.AllowDScan = true;
        }

        static public bool RemoveWaypoint()
        {
            for (int i = 0; i < 5; i++)
            {
                List<SystemInfo> RouteInfo = Route.GetInfo();
                if (RouteInfo == null)
                    return true;

                Console.WriteLine("remove waypoint");

                var YOffset = 10 * (RouteInfo.Count / 25);
                if (RouteInfo.Count % 25 == 0)
                {
                    YOffset = YOffset - 10;
                }

                Emulators.ClickRB(165, 360 + YOffset - 23); //white stripe
                Thread.Sleep(AvgDeley + r.Next(-100, 100));
                General.ClickInContextMenu("<color=4291559424>! </color>Remove Waypoint");
                Thread.Sleep(1000 + r.Next(-100, 100));
            }
            return false;
        }

        static public void CheckMissilesAmount()
        {
            var Inventory = Invent.GetInfo();
            if (Inventory == null)
            {
                Console.WriteLine("try to open inventary");
                Emulators.PressButton((int)WinApi.VirtualKeyShort.VK_NUMPAD3);
                Inventory = Invent.GetInfo();
                if (Inventory == null)
                {
                    Console.WriteLine("inventary not found");
                    General.DockToStationAndExit();
                }
            }
            
            foreach (var item in Inventory)
            {
                if (item.Name.Contains("Guristas Inferno Light Missile") && item.Amount < 100)
                {
                    General.DockToStationAndExit();
                }
                else if (item.Name.Contains("Guristas Inferno Light Missile") && item.Amount >= 100)
                {
                    Console.WriteLine("Amount missiles = {0}", item.Amount);
                }
            }

            //if (Inventory.Find(item => item.Name.Contains("Guristas Inferno Light Missile")).Amount < 100)
            //    General.DockToStationAndExit();

            //if (Checkers.CheckQuantityDrones() < 3)
            //    General.DockToStationAndExit();
        }

        static public UITreeNode GetUITrees()
        {
            return ReadMemory.GetUITrees(Window.RootAddress, Window.processId);
        }
    }
}
