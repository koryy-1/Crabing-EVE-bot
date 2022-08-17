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
    static public class MainScripts
    {
        static public Random r = new Random();
        static public int AvgDeley = Config.AverageDelay;
        static ModulesInfo ModulesInfo = new ModulesInfo();

        static public void BotStart()
        {
            int RandomHours = r.Next(2, 4); //for ints
            DateTime TimeForBreak = DateTime.Now;
            Console.WriteLine("RandomHours {0}", RandomHours);
            Console.WriteLine("TimeForBreak {0}", TimeForBreak);
            int PauseDuration = r.Next(10, 40);

            SecScripts.CheckSituation();

            for (int i = 0; i < 300; i++)
            {
                DateTime StartTime = DateTime.Now;
                Console.WriteLine(StartTime);

                FarmAnomalies(StartTime);
                if (i % 4 == 0)
                    (RandomHours, TimeForBreak) = Pause(TimeForBreak, RandomHours, PauseDuration);

                if (Config.FarmExp)
                {
                    GotoFarmExp();
                }
            }
        }

        static public (int, DateTime) Pause(DateTime TimeForBreak, int RandomHours, int PauseDuration)
        {
            if ((DateTime.Now - TimeForBreak).TotalHours > RandomHours)
            {
                RandomHours = r.Next(3, 5); //for ints
                TimeForBreak = DateTime.Now;
                PauseDuration = r.Next(20, 40);


                (int XStationCoords, int YStationCoords) = Finders.FindObjectByWordInOverview("Station");
                if (XStationCoords == 0)
                {
                    Console.WriteLine("Station not found");
                    Console.WriteLine("pause for {0} minutes, start at {1}", PauseDuration, TimeForBreak);
                    sleep(PauseDuration * 60 * 1000);
                    return (RandomHours, TimeForBreak);
                }
                ThreadManager.AllowDocking = true;
                ThreadManager.AllowDScan = false;
                //Console.WriteLine("unload cargo");

                General.GotoInActiveItem("Station", "Dock");

                Console.WriteLine("wait 1 min for dock");
                Checkers.WatchState();

                var LobbyWnd = GetUITrees().FindEntityOfString("LobbyWnd");
                while (LobbyWnd == null)
                {
                    Thread.Sleep(1000 + r.Next(-100, 100));
                    LobbyWnd = GetUITrees().FindEntityOfString("LobbyWnd");
                }
                Thread.Sleep(5 * 1000 + r.Next(-100, 100));

                //SecondaryScripts.UnloadCargo();


                Console.WriteLine("pause for {0} minutes, start at {1}", PauseDuration, TimeForBreak);
                sleep(PauseDuration * 60 * 1000);

                General.Undock();
                ThreadManager.AllowDocking = false;
                ThreadManager.AllowDScan = true;
            }
            return (RandomHours, TimeForBreak);
        }

        static public void FarmAnomalies(DateTime StartTime)
        {
            for (int i = 0; i < 100; i++)
            {
                var (XWarpToAnomaly, YWarpToAnomaly) = Finders.CheckCurrentSystemForCombatSites();
                if (XWarpToAnomaly == 1)
                    continue;
                if (XWarpToAnomaly == 0)
                {
                    GotoNextSystem();
                    continue;
                }

                Emulators.ClickLB(XWarpToAnomaly, YWarpToAnomaly);

                Checkers.WatchState();

                // чек на нужную аномальку вблизи
                (_, YWarpToAnomaly) = Finders.CheckCurrentSystemForCombatSites();
                (_, int YlocProbeScannerWindow) = Finders.FindLocWnd("ProbeScannerWindow");
                if (YWarpToAnomaly - YlocProbeScannerWindow - 104 != 0)
                {
                    Console.WriteLine("not that anomaly near");
                    continue;
                }

                DestroyTargetsInRoom();

                SecScripts.CheckMissilesAmount();

                if (i % 10 == 0)
                {
                    if ((DateTime.Now - StartTime).TotalHours > 1)
                        return;
                }
            }
        }

        static public void DestroyTargetsInRoom()
        {
            var CheckForEnemies = Checkers.GetCoordsEnemies();

            if (0 == CheckForEnemies.Count)
            {
                Console.WriteLine("room already clear");
                return;
            }
            Console.WriteLine("start clear at: {0}", DateTime.Now);

            PreparationForCombatReadiness();

            for (int i = 0; i < 15; i++)
            {
                var EnemyCoordsArray = Checkers.GetCoordsEnemies();

                if (0 == EnemyCoordsArray.Count)
                {
                    Console.WriteLine("room is clear");
                    break;
                }
                if (1 == EnemyCoordsArray[0])
                {
                    WatchDistance();
                    continue;
                }
                if (2 != EnemyCoordsArray.Last())
                {
                    Emulators.LockTargets(EnemyCoordsArray);

                    if (!Checkers.CheckLocking())
                        continue;
                }

                //CheckLockedTargets();
                General.UnlockNotEnemyTargets();
                ThreadManager.AllowToAttack = true;

                //процесс захвата цели
                //object.dictEntriesOfInterest["_name"] = "targeting"

                //метка на захваченной цели
                //object.dictEntriesOfInterest["_name"] = "myActiveTargetIndicator"
                for (int j = 0; j < 20; j++)
                {
                    if (!Checkers.WatchLockingTargets())
                    {
                        ThreadManager.AllowToAttack = false;
                        break;
                    }
                    if (j % 3 == 0 && ThreadManager.AllowDroneControl)
                    {
                        General.UnlockNotEnemyTargets();
                        //Emulators.PressButton((int)WinApi.VirtualKeyShort.VK_F);
                    }
                    Thread.Sleep(1000 * 3);
                }
            }
            Console.WriteLine("end clear at: {0}", DateTime.Now);

            СancellationOfCombatReadiness();

            // поменять вкладку
            General.ChangeTab("Mining");
            Thread.Sleep(1000);
            GotoLootCont("Dread Guristas", 1);
            GotoLootCont("Shadow Serpentis", 1);
            General.ChangeTab("General");

            //while (DroneController.DroneLaunchInProgress)
            //    Thread.Sleep(1000);

            //while (DroneController.DronesDroped)
            //    Thread.Sleep(1000);

            if (General.CheckCargo())
                SecScripts.UnloadCargo();
            ThreadManager.MultiplierSleep = 10;
        }

        static public void PreparationForCombatReadiness()
        {
            ThreadManager.MultiplierSleep = 2;
            ThreadManager.AllowDroneControl = true;
            ThreadManager.AllowDroneRescoop = true;
            DroneController.DroneLaunchInProgress = true;
        }

        static public void СancellationOfCombatReadiness()
        {
            ThreadManager.AllowDroneControl = false;
            ThreadManager.AllowDroneRescoop = false;
        }

        static public void GotoFarmExp()
        {
            for (int i = 0; i < 10; i++)
            {
                if (!FindExpidition())
                    return;

                for (int j = 0; j < 60; j++)
                {
                    if (!GotoNextSystem(false))
                        break;
                }

                if (!WarpToLocExp())
                    continue;
                Checkers.WatchState();

                StartClearExp();
                if (General.CheckCargo())
                    SecScripts.UnloadCargo();

                SecScripts.CheckMissilesAmount();
            }
        }

        static public bool FindExpidition()
        {
            var AgencyWnd = GetUITrees().FindEntityOfString("AgencyWndNew");
            if (AgencyWnd == null)
            {
                Console.WriteLine("no agency window, try to open it");
                Emulators.ClickLB(15, 150);
                Thread.Sleep(1000 * 2);
            }

            AgencyWnd = GetUITrees().FindEntityOfString("AgencyWndNew");
            if (AgencyWnd == null)
            {
                Console.WriteLine("no agency window");
                return false;
            }


            var (XlocAgencyWnd, YlocAgencyWnd) = Finders.FindLocWnd("AgencyWndNew");


            Emulators.ClickLB(XlocAgencyWnd + 200, YlocAgencyWnd + 680);
            Thread.Sleep(1000 * 2);


            var CurrentContentGroup = GetUITrees().FindEntityOfString("CurrentContentGroupEntry");
            if (CurrentContentGroup == null)
            {
                Console.WriteLine("no CurrentContentGroupEntry");
                Emulators.ClickLB(XlocAgencyWnd + 1150, YlocAgencyWnd + 10);
                return false;
            }

            var CurrentContentGroupEntry = CurrentContentGroup.handleEntity("CurrentContentGroupEntry");
            if (CurrentContentGroupEntry.children[Convert.ToInt32(CurrentContentGroupEntry.dictEntriesOfInterest["needIndex"])]
                .children[0].dictEntriesOfInterest["_setText"].ToString() != "Escalations")
            {
                Console.WriteLine("try to open escalation");
                Emulators.ClickLB(XlocAgencyWnd + 200, YlocAgencyWnd + 680);
                Thread.Sleep(1000 * 4);
            }

            CurrentContentGroupEntry = GetUITrees().FindEntityOfString("CurrentContentGroupEntry").handleEntity("CurrentContentGroupEntry");
            if (CurrentContentGroupEntry.children[Convert.ToInt32(CurrentContentGroupEntry.dictEntriesOfInterest["needIndex"])]
                .children[0].dictEntriesOfInterest["_setText"].ToString() != "Escalations")
            {
                Console.WriteLine("no escalation");
                Emulators.ClickLB(XlocAgencyWnd + 1150, YlocAgencyWnd + 10);
                return false;
            }

            var EscalationCards = GetUITrees().FindEntityOfString("EscalationsSystemContentCard");
            if (EscalationCards == null)
            {
                Console.WriteLine("no Escalations System Content Card");
                Emulators.ClickLB(XlocAgencyWnd + 1150, YlocAgencyWnd + 10);
                return false;
            }
            var EscalationCardEntry = EscalationCards.handleEntity("EscalationsSystemContentCard");

            //List<string> SS = new List<string>();
            //SS.Add("0.5");
            //SS.Add("0.6");
            //SS.Add("0.7");
            //SS.Add("0.8");
            //SS.Add("0.9");
            //SS.Add("1.0");
            var Routed = false;
            Emulators.ClickLB(XlocAgencyWnd + 360, YlocAgencyWnd + 220);
            Thread.Sleep(200);
            for (int i = 0; i < EscalationCardEntry.children.Length; i++)
            {
                //var isLowSec = false;
                //for (int j = 0; j < SS.Count; j++)
                //{
                //    if (!EscalationCardEntry.children[i].children[2].children[0].dictEntriesOfInterest["_setText"].ToString().Contains(SS[j]))
                //    {
                //        Console.WriteLine("skip naxyu");
                //        isLowSec = true;
                //        break;
                //    }
                //}
                //if (isLowSec)
                //    continue;

                Emulators.ClickLB(XlocAgencyWnd + 900, YlocAgencyWnd + 590);// чекать кнопку set dest/warpto
                Thread.Sleep(1000 * 3);
                Routed = true;

                var AutopilotDestinationIcon = GetUITrees().FindEntityOfString("AutopilotDestinationIcon");
                if (AutopilotDestinationIcon == null)
                {
                    Console.WriteLine("apparently we are there");
                    Emulators.ClickLB(XlocAgencyWnd + 1150, YlocAgencyWnd + 10);
                    return true;
                }
                var AutopilotDestinationIconEntity = AutopilotDestinationIcon.handleEntity("AutopilotDestinationIcon");
                var CntLowSec = 0;
                for (int j = 0; j < AutopilotDestinationIconEntity.children.Length; j++)
                {
                    if (Convert.ToInt32(AutopilotDestinationIconEntity.children[j].children[0].dictEntriesOfInterest["_color"]["rPercent"]) >= 90
                        &&
                        Convert.ToInt32(AutopilotDestinationIconEntity.children[j].children[0].dictEntriesOfInterest["_color"]["gPercent"]) < 100)
                    {
                        CntLowSec++;
                        Console.WriteLine("skip");
                        break;
                    }
                }
                if (CntLowSec == 0)
                {
                    Console.WriteLine("found highsec exp, route laid");
                    Emulators.ClickLB(XlocAgencyWnd + 1150, YlocAgencyWnd + 10);
                    return true;
                }
                Emulators.ClickLB(XlocAgencyWnd + 360, YlocAgencyWnd + 270);
                Thread.Sleep(200);
                Emulators.PressButton((int)WinApi.VirtualKeyShort.VK_DOWN);
                Thread.Sleep(AvgDeley + r.Next(-100, 100));
            }
            Console.WriteLine("no highsec exp");
            if (Routed)
            {
                if (SecScripts.RemoveWaypoint())
                {
                    Console.WriteLine("bad try to remove waypoint");
                    General.DockToStationAndExit();
                }
            }
            Thread.Sleep(AvgDeley + r.Next(-100, 100));
            Emulators.ClickLB(XlocAgencyWnd + 1150, YlocAgencyWnd + 10); // close agency
            return false;
        }

        static public bool WarpToLocExp()
        {
            var AgencyWnd = GetUITrees().FindEntityOfString("AgencyWndNew");
            if (AgencyWnd == null)
            {
                Console.WriteLine("no agency window, try to open it");
                Emulators.ClickLB(15, 150);
                Thread.Sleep(1000 * 2);
            }

            AgencyWnd = GetUITrees().FindEntityOfString("AgencyWndNew");
            if (AgencyWnd == null)
            {
                Console.WriteLine("no agency window");
                return false;
            }

            var (XlocAgencyWnd, YlocAgencyWnd) = Finders.FindLocWnd("AgencyWndNew");

            Thread.Sleep(AvgDeley + r.Next(-100, 100));
            Emulators.ClickLB(XlocAgencyWnd + 900, YlocAgencyWnd + 590);
            Thread.Sleep(1000 * 2);
            Emulators.ClickLB(XlocAgencyWnd + 1150, YlocAgencyWnd + 10);
            return true;
        }

        static public void StartClearExp()
        {
            Console.WriteLine(DateTime.Now);
            //General.ModuleActivityManager(2, true); // shield
            for (int i = 0; i < 5; i++)
            {
                (int XAcGate, int YAcGate) = Finders.FindAccelerationGate();
                if (XAcGate == 0 && YAcGate == 0)
                    break;
                Emulators.ClickLB(XAcGate, YAcGate);
                Thread.Sleep(200);
                var JumpBtn = General.GetCoordsButtonActiveItem("Jump");
                if (JumpBtn.Item1 == 0)
                {
                    JumpBtn = General.GetCoordsButtonActiveItem("ActivateGate");
                }
                Emulators.ClickLB(JumpBtn.Item1, JumpBtn.Item2);
                Checkers.WatchState();

                (XAcGate, YAcGate) = Finders.FindAccelerationGate();
                if (XAcGate == 0 && YAcGate == 0)
                    break;
                Emulators.ClickLB(XAcGate, YAcGate);
                Thread.Sleep(200);
                var OrbBtn = General.GetCoordsButtonActiveItem("Orbit");
                Emulators.ClickLB(OrbBtn.Item1, OrbBtn.Item2);
                General.ModuleActivityManager(ModulesInfo.MWD, true);
                MainScripts.ClearExpRoom();
            }

            (int XIconF, int YIconF) = Finders.FindIconFilter();
            Emulators.ClickLB(XIconF, YIconF);
            Thread.Sleep(1000 * 2);

            Console.WriteLine("try to find ExpBlock");
            (int XBlock, int YBlock) = Finders.FindExpBlock();
            if (XBlock != 0)
            {
                Emulators.ClickLB(XBlock, YBlock);
                Thread.Sleep(200);
                var OrbBtn = General.GetCoordsButtonActiveItem("Orbit");
                Emulators.ClickLB(OrbBtn.Item1, OrbBtn.Item2);
                General.ModuleActivityManager(ModulesInfo.MWD, true);
                Thread.Sleep(200);
                Emulators.ClickLB(XBlock, YBlock + 200);
                Thread.Sleep(200);
            }
            //else
            //    Emulators.PressButton((int)WinApi.VirtualKeyShort.VK_F3);
            MainScripts.ClearExpRoom();

            General.ModuleActivityManager(ModulesInfo.MWD, false); // off prop module
            Console.WriteLine("try to find ExpBlock");
            (XBlock, YBlock) = Finders.FindExpBlock();
            if (XBlock != 0 && YBlock != 0)
            {
                List<int> Coords = new List<int>();
                Coords.Add(XBlock);
                Coords.Add(YBlock);

                Emulators.LockTargets(Coords);
                Thread.Sleep(AvgDeley + r.Next(-100, 100));
                var ApproachBtn = General.GetCoordsButtonActiveItem("Approach");
                Emulators.ClickLB(ApproachBtn.Item1, ApproachBtn.Item2); //1 button approach
                Thread.Sleep(AvgDeley + r.Next(-100, 100));

                ThreadManager.AllowDroneControl = true;
                ThreadManager.AllowDroneRescoop = true;
                Checkers.CheckLocking();
                ThreadManager.AllowToAttack = true;
                for (int i = 0; i < 120; i++)
                {
                    if (!Checkers.WatchLockingTargets())
                        break;
                    Thread.Sleep(1000 * 10);
                }
                ThreadManager.AllowToAttack = false;
                ThreadManager.AllowDroneControl = false;
                ThreadManager.AllowDroneRescoop = false;
            }
            (XIconF, YIconF) = Finders.FindIconFilter();
            Emulators.ClickLB(XIconF, YIconF);
            Thread.Sleep(1000 * 2);

            GotoLootCont("Cargo Container");
            Console.WriteLine(DateTime.Now);
        }

        static public void ClearExpRoom()
        {
            var CheckForEnemies = Checkers.GetCoordsEnemies();

            if (0 == CheckForEnemies.Count)
            {
                Console.WriteLine("room already clear");
                return;
            }

            ThreadManager.AllowDroneControl = true;
            ThreadManager.AllowDroneRescoop = true;
            ThreadManager.AllowShieldHPControl = true;
            DestroyTargets();
            ThreadManager.AllowDroneControl = false;
            ThreadManager.AllowDroneRescoop = false;
            ThreadManager.AllowShieldHPControl = false;

            while (DroneController.DronesDroped)
                Thread.Sleep(1000);
        }

        static public void CheckForConnectionLost()
        {
            var uiTreePreparedForFile = GetUITrees();
            var MessageBox = uiTreePreparedForFile.FindEntityOfString("MessageBox");
            if (MessageBox == null)
                return;

            var (XlocMessageBox, YlocMessageBox) = Finders.FindLocWnd("MessageBox");

            Console.WriteLine("qiut in MessageBox on X : {0}, Y : {1}. Exit from program", XlocMessageBox + 170, YlocMessageBox + 195 + 23);
            //return (XlocMessageBox + 170, YlocMessageBox + 195);

            Emulators.ClickLB(XlocMessageBox + 170, YlocMessageBox + 195);
            Environment.Exit(0);
        }

        static public bool CurrentSystemIsDanger()
        {
            List<ChatPlayer> ChatInfo = Chat.GetInfo();
            if (ChatInfo == null)
                return false;

            for (int i = 0; i < ChatInfo.Count; i++)
            {
                if (ChatInfo[i].PlayerType == "Pilot is a suspect")
                {
                    Console.WriteLine("suspect pilot in system");
                }
                if (ChatInfo[i].PlayerType == "Pilot is a criminal")
                {
                    Console.WriteLine("criminal pilot in system");
                    return true;
                }
            }
            return false;
        }

        static public bool CheckDScan()
        {
            if (!ThreadManager.AllowDScan)
                return false;

            Emulators.PressButton((int)WinApi.VirtualKeyShort.VK_V);
            Thread.Sleep(AvgDeley + r.Next(-100, 100));

            List<DScanItem> DScanInfo = DS.GetInfo();
            if (DScanInfo == null)
            {
                return false;
            }
            for (int i = 0; i < DScanInfo.Count; i++)
            {
                if (DScanInfo[i].Type.Contains("Catalyst"))
                {
                    Console.WriteLine("catalyst called {0} is in system", DScanInfo[i].Name);
                    Console.WriteLine("pidaras detected");
                    return true;
                }
            }
            return false;
        }

        static public void DockingFromSuicides()
        {
            (int XlocNotepadWindow, int YlocNotepadWindow) = Finders.FindLocWnd("OverView");
            if (XlocNotepadWindow == 0) //ship docked
            {
                SecScripts.DockingAndCheckingForSuicides();
                return;
            }

            //Emulators.PressButton((int)WinApi.VirtualKeyShort.VK_H);
            (int XStationCoords, int YStationCoords) = Finders.FindObjectByWordInOverview("Station");
            var TryTosebat = false;
            if (XStationCoords > 0)
            {
                Emulators.ClickLB(XStationCoords, YStationCoords);
                Thread.Sleep(AvgDeley + r.Next(-100, 100));
                var DockBtn = General.GetCoordsButtonActiveItem("Dock");
                Emulators.ClickLB(DockBtn.Item1, DockBtn.Item2);

                SecScripts.DockingAndCheckingForSuicides();
            }
            else
            {
                (int XGate, int YGate) = GetCoordsNextSystem();
                if (XGate > 1)
                {
                    TryTosebat = true;
                    Emulators.ClickLB(XGate, YGate);
                    Thread.Sleep(AvgDeley + r.Next(-100, 100));
                    var JumpBtn = General.GetCoordsButtonActiveItem("Jump");
                    Emulators.ClickLB(JumpBtn.Item1, JumpBtn.Item2);
                    Checkers.WatchState();
                    Thread.Sleep(1000 * 10);
                }
                if (!TryTosebat)
                {
                    (int XStargateCoords, int YStargateCoords) = Finders.FindObjectByWordInOverview("Stargate");
                    if (XStargateCoords > 0)
                    {
                        Emulators.ClickLB(XStargateCoords, YStargateCoords);
                        Thread.Sleep(AvgDeley + r.Next(-100, 100));
                        var JumpBtn = General.GetCoordsButtonActiveItem("Jump");
                        Emulators.ClickLB(JumpBtn.Item1, JumpBtn.Item2);
                    }
                }
            }
        }

        static public bool GotoNextSystem(bool NeedToLayRoute = true)
        {
            var (XGate, YGate) = MainScripts.GetCoordsNextSystem();
            if (YGate == 1)
            {
                Thread.Sleep(1000);
                return false;
            }
            if (YGate == 0)
            {
                if (NeedToLayRoute)
                {
                    Console.WriteLine("no route, start to laying a new route");
                    SecScripts.StartLayRoute();
                    return false;
                }
                else
                    return false;
            }
            Emulators.ClickLB(XGate, YGate);
            Thread.Sleep(AvgDeley + r.Next(-100, 100));
            var JumpBtn = General.GetCoordsButtonActiveItem("Jump");
            if (JumpBtn.Item1 == 0)
            {
                JumpBtn = General.GetCoordsButtonActiveItem("Dock");
            }
            Emulators.ClickLB(JumpBtn.Item1, JumpBtn.Item2);

            Checkers.WatchState();
            Thread.Sleep(1000 * 10);
            return true;
        }

        static public (int, int) GetCoordsNextSystem()
        {
            List<OverviewItem> OverviewInfo = OV.GetInfo();
            if (OverviewInfo == null)
            {
                return (1, 1);
            }
            for (int i = 0; i < OverviewInfo.Count; i++)
            {
                if (OV.GetColorInfo(OverviewInfo[i].Colors) is "yellow"
                    &&
                    (OverviewInfo[i].Type.Contains("Stargate") ||
                    OverviewInfo[i].Type.Contains("Station") ||
                    OverviewInfo[i].Type.Contains("Hub")))
                {
                    return (OverviewInfo[i].Pos.x, OverviewInfo[i].Pos.y);
                }
            }
            return (0, 0);
        }

        static public void DestroyTargets(string ExcludeTarget = null)
        {
            for (int i = 0; i < 40; i++)
            {
                var EnemyCoordsArray = Checkers.GetCoordsEnemies(ExcludeTarget);

                if (0 == EnemyCoordsArray.Count)
                {
                    Console.WriteLine("room is clear");
                    return;
                }
                if (1 == EnemyCoordsArray[0])
                {
                    WatchDistance();
                    continue;
                }
                if (2 != EnemyCoordsArray.Last())
                {
                    Emulators.LockTargets(EnemyCoordsArray);

                    if (!Checkers.CheckLocking())
                        continue;
                }

                EngageTarget();
            }
            Console.WriteLine("слишком долгий процесс уничтожения");
            General.DockToStationAndExit();
        }

        static public void EngageTarget()
        {
            General.UnlockNotEnemyTargets();
            ThreadManager.AllowToAttack = true;

            for (int j = 0; j < 100; j++)
            {
                if (!Checkers.WatchLockingTargets())
                {
                    ThreadManager.AllowToAttack = false;
                    break;
                }
                if (j % (9 / ThreadManager.MultipleSleepForDrones) == 0 && ThreadManager.AllowDroneControl)
                {
                    General.UnlockNotEnemyTargets();
                    //Emulators.PressButton((int)WinApi.VirtualKeyShort.VK_F);
                }
                Thread.Sleep(1000 * ThreadManager.MultipleSleepForDrones);
            }
        }

        static public void WatchDistance()
        {
            for (int i = 0; i < 1; i++)
            {
                var EnemyCoordsArray = Checkers.GetCoordsEnemies();
                if (EnemyCoordsArray.Count == 0 || EnemyCoordsArray[0] != 1)
                {
                    return;
                }
                //if (1 != EnemyCoordsArray[0])
                //{
                //    return;
                //}
                Console.WriteLine("enemies are far away");
                Thread.Sleep(1000 * 5);
            }
            Console.WriteLine("okay, orbit enemy");

            OverviewItem EnemyInfo = OV.GetInfo()
                .OrderBy(item => item.Distance.value).ToList()
                .Find(item => OV.GetColorInfo(item.Colors) is "red");

            //OverviewInfo = OverviewInfo.OrderBy(item => item.Distance.value).ToList();

            //var EnemyInfo = OverviewInfo.Find(item => OV.GetColorInfo(item.Colors) is "red");

            General.Orbiting(EnemyInfo.Name);
            //Emulators.ClickLB(EnemyInfo.Pos.x, EnemyInfo.Pos.y);
            //Thread.Sleep(AvgDeley + r.Next(-100, 100));
            //var OrbitBtn = General.GetCoordsButtonActiveItem("Orbit");
            //Emulators.ClickLB(OrbitBtn.Item1, OrbitBtn.Item2);

            General.ModuleActivityManager(ModulesInfo.MWD, true);

            for (int i = 0; i < 6; i++)
            {
                var EnemyCoordsArray = Checkers.GetCoordsEnemies();
                if (EnemyCoordsArray.Count == 0 || EnemyCoordsArray[0] != 1)
                {
                    //3 проверки на наличие ExpBlock AccelerationGate или cargo container
                    //если нет то выход
                    (int XBlock, int YBlock) = Finders.FindExpBlock();
                    (int XAcGate, int YAcGate) = Finders.FindAccelerationGate();
                    (int XCont, int YCont) = Finders.FindObjectByWordInOverview("Cargo Container");
                    if (XBlock != 0)
                    {
                        Emulators.ClickLB(XBlock, YBlock);
                    }
                    else if (XAcGate != 0)
                    {
                        Emulators.ClickLB(XAcGate, YAcGate);
                    }
                    else if (XCont != 0)
                    {
                        Emulators.ClickLB(XCont, YCont);
                    }
                    else
                    {
                        //ship stop
                        General.ModuleActivityManager(ModulesInfo.MWD, false);
                        General.SetSpeed(0);
                        break;
                    }
                    Thread.Sleep(AvgDeley + r.Next(-100, 100));
                    var OrbitBtn = General.GetCoordsButtonActiveItem("Orbit");
                    Emulators.ClickLB(OrbitBtn.Item1, OrbitBtn.Item2);
                    break;
                }
                Console.WriteLine("enemies are far away");
                Thread.Sleep(1000 * 5);
            }
        }

        static public void GotoLootCont(string ContName, int NeedPrice = 0)
        {
            var Inventory = GetUITrees().FindEntityOfString("InventoryPrimary");
            if (Inventory == null)
            {
                Console.WriteLine("try to open Inventory");
                Emulators.ClickLB(22, 230);
                Thread.Sleep(1000 * 2);
            }
            Inventory = GetUITrees().FindEntityOfString("InventoryPrimary");
            if (Inventory == null)
            {
                Console.WriteLine("no Inventory window");
            }


            int XlocInventory = 0;
            int YlocInventory = 0;
            int HeightInventory = 0;
            int WidthLeftSidebar = 0;

            var ContNameInCargo = "";
            if (ContName == "Cargo Container")
                ContNameInCargo = "ItemFloatingCargo";

            if (ContName == "Dread Guristas" || ContName == "Shadow Serpentis" || ContName == "Wreck")
                ContNameInCargo = "ItemWreck";

            (int XContCoords, int YContCoords) = Finders.FindObjectByWordInOverview(ContName);

            if (XContCoords == 0)
            {
                Console.WriteLine("no {0}", ContName);
                return;
            }

            for (int i = 0; i < 10; i++)
            {
                General.GotoInActiveItem(ContName, "OpenCargo");
                if (!Checkers.CheckDistance(ContName, 2500) && !Checkers.CheckState("Approaching"))
                    continue;

                var (XOpenCargo, YOpenCargo) = General.GetCoordsButtonActiveItem("OpenCargo");
                if (XOpenCargo == 0)
                    continue;
                Thread.Sleep(AvgDeley + r.Next(-100, 100));
                UITreeNode OpenedCont = null;
                for (int j = 0; j < 300; j++)
                {
                    if (Checkers.CheckDistance(ContName, 3500))
                    {
                        Emulators.ClickLB(XOpenCargo, YOpenCargo + 10); //3 button loot
                    }
                    try
                    {
                        OpenedCont = GetUITrees().FindEntityOfString(ContNameInCargo);
                    }
                    catch
                    {
                        continue;
                    }
                    if (OpenedCont != null)
                    {
                        Console.WriteLine("{0} is here", ContNameInCargo);
                        break;
                    }
                    Thread.Sleep(1000);
                }
                Thread.Sleep(1000);
                if (General.CheckCargoPrice(NeedPrice))
                {
                    (XlocInventory, YlocInventory) = Finders.FindLocWnd("InventoryPrimary");

                    HeightInventory = Window.GetHeightWindow("InventoryPrimary");
                    WidthLeftSidebar = Window.GetWidthWindow("TreeViewEntryInventoryCargo");

                    Console.WriteLine("button loot all: {0}, {1}", XlocInventory + WidthLeftSidebar + 30, YlocInventory + HeightInventory - 20 + 23);

                    Emulators.ClickLB(XlocInventory + WidthLeftSidebar + 30, YlocInventory + HeightInventory - 20); // Loot all

                    Thread.Sleep(2 * 1000);
                    //проверка что конт залутан
                    OpenedCont = GetUITrees().FindEntityOfString(ContNameInCargo);
                    if (OpenedCont == null)
                    {
                        Console.WriteLine("cont looted");
                        //break;
                    }
                    Thread.Sleep(1000 * 5);
                }
                else
                {
                    Console.WriteLine("garbage in loot");
                    return;
                }
                
            }
        }

        static void sleep(int milliseconds) => Thread.Sleep(milliseconds);

        static public UITreeNode GetUITrees()
        {
            return ReadMemory.GetUITrees(Window.RootAddress, Window.processId);
        }
    }
}
