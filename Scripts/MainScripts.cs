using EVE_Bot.Controllers;
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
        static public void BotStart()
        {
            Random r = new Random();
            int RandomHours = r.Next(2, 4); //for ints
            DateTime TimeForBreak = DateTime.Now;
            Console.WriteLine("RandomHours {0}", RandomHours);
            Console.WriteLine("TimeForBreak {0}", TimeForBreak);
            int PauseDuration;

            SecondaryScripts.CheckSituation();

            for (int i = 0; i < 300; i++)
            {
                DateTime StartTime = DateTime.Now;
                Console.WriteLine(StartTime);

                FarmAnomalies(StartTime);
                if ((DateTime.Now - TimeForBreak).TotalHours > RandomHours)
                {
                    RandomHours = r.Next(2, 4); //for ints
                    TimeForBreak = DateTime.Now;
                    PauseDuration = r.Next(10, 40);
                    Console.WriteLine("pause for {0} minutes, continue at {1}", PauseDuration, TimeForBreak);
                    sleep(PauseDuration * 60 * 1000);
                }
                TimeToFarmExp();

                //SecondaryScripts.RemoveWaypoint();
            }
            Console.WriteLine("Automatic system for farming isk has successfully worked");
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

                ClearRoom();

                if (Checkers.CheckQuantityDrones() < 3)
                    SecondaryScripts.DockToStationAndExit();

                if (i % 10 == 0)
                {
                    if ((DateTime.Now - StartTime).TotalHours > 1)
                        return;
                }
            }
        }

        static public void ClearRoom()
        {

            var CheckForEnemies = Checkers.GetCoordsEnemies();

            if (0 == CheckForEnemies.Count)
            {
                Console.WriteLine("room already clear");
                return;
            }
            Console.WriteLine("start clear at: {0}", DateTime.Now);
            ThreadManager.AllowDroneControl = true;
            ThreadManager.AllowDroneRescoop = true;

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


                Emulators.LockTargets(EnemyCoordsArray);

                Thread.Sleep(1000);


                var ActiveTarget = Checkers.CheckLocking();// если таргет не лочится подлететь

                if (!ActiveTarget)
                    continue;

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
                    Thread.Sleep(1000 * 3);
                }
            }
            Console.WriteLine("end clear at: {0}", DateTime.Now);
            ThreadManager.AllowDroneControl = false;
            ThreadManager.AllowDroneRescoop = false;
            GotoLootCont("Dread Guristas", 1);
            GotoLootCont("Shadow Serpentis", 1);

            while (DroneController.DronesDroped)
                Thread.Sleep(1000);

            if (SecondaryScripts.CheckCargo())
                SecondaryScripts.UnloadCargo();
        }

        static public void TimeToFarmExp()
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
                if (SecondaryScripts.CheckCargo())
                    SecondaryScripts.UnloadCargo();

                if (Checkers.CheckQuantityDrones() < 3)
                    SecondaryScripts.DockToStationAndExit();
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
                Thread.Sleep(500);
            }
            Console.WriteLine("no highsec exp");
            if (Routed)
            {
                //SecondaryScripts.RemoveWaypoint();
                Console.WriteLine("remove waypoint");
                //Emulators.ClickRB(157, 327);
                //Thread.Sleep(500);
                //Emulators.ClickLB(157 + 70, 327 + 126);

                (int XlocNotepadWindow, int YlocNotepadWindow) = Finders.FindLocWnd("NotepadWindow");
                if (XlocNotepadWindow == 0)
                {
                    Emulators.ClickRB(157, 327);
                    Thread.Sleep(500);
                    Emulators.ClickLB(157 + 70, 327 + 126);
                }
                Emulators.ClickRB(XlocNotepadWindow + 310, YlocNotepadWindow + 215);
                System.Threading.Thread.Sleep(1000);
                Emulators.ClickLB(XlocNotepadWindow + 310 + 60, YlocNotepadWindow + 215 + 40); // set destination
                System.Threading.Thread.Sleep(500);
            }
            Thread.Sleep(500);
            Emulators.ClickLB(XlocAgencyWnd + 1150, YlocAgencyWnd + 10);
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

            Thread.Sleep(500);
            Emulators.ClickLB(XlocAgencyWnd + 900, YlocAgencyWnd + 590);
            Thread.Sleep(1000 * 2);
            Emulators.ClickLB(XlocAgencyWnd + 1150, YlocAgencyWnd + 10);
            return true;
        }

        static public void StartClearExp()
        {
            Console.WriteLine(DateTime.Now);
            Emulators.PressButton((int)WinApi.VirtualKeyShort.VK_F2);
            for (int i = 0; i < 5; i++)
            {
                (int XAcGate, int YAcGate) = Finders.FindAccelerationGate();
                if (XAcGate == 0 && YAcGate == 0)
                    break;
                Emulators.ClickLB(XAcGate, YAcGate);
                Thread.Sleep(200);
                Emulators.ClickLB(2200, 100); //3 button jump
                Checkers.WatchState();

                (XAcGate, YAcGate) = Finders.FindAccelerationGate();
                if (XAcGate == 0 && YAcGate == 0)
                    break;
                Emulators.ClickLB(XAcGate, YAcGate);
                Thread.Sleep(200);
                Emulators.ClickLB(2200 + 33, 100); //4 button orbit
                Emulators.PressButton((int)WinApi.VirtualKeyShort.VK_F1);
                MainScripts.ClearExpRoom();
            }

            (int XIconF, int YIconF) = Finders.FindIconFilter();
            Emulators.ClickLB(XIconF, YIconF);
            Thread.Sleep(1000 * 2);

            Console.WriteLine("try to find ExpBlock");
            (int XBlock, int YBlock) = Finders.FindExpBlock();
            if (XBlock != 0 && YBlock != 0)
            {
                Emulators.ClickLB(XBlock, YBlock);
                Thread.Sleep(200);
                Emulators.ClickLB(2200, 100); //3 button orbit
                Emulators.PressButton((int)WinApi.VirtualKeyShort.VK_F1);
                Thread.Sleep(200);
                Emulators.ClickLB(XBlock, YBlock + 200);
                Thread.Sleep(200);
            }
            else
                Emulators.PressButton((int)WinApi.VirtualKeyShort.VK_F3);
            MainScripts.ClearExpRoom();

            Emulators.PressButton((int)WinApi.VirtualKeyShort.VK_F1);
            Console.WriteLine("try to find ExpBlock");
            (XBlock, YBlock) = Finders.FindExpBlock();
            if (XBlock != 0 && YBlock != 0)
            {
                List<int> Coords = new List<int>();
                Coords.Add(XBlock);
                Coords.Add(YBlock);

                Emulators.LockTargets(Coords);
                Thread.Sleep(500);
                Emulators.ClickLB(2200 - 66, 100); //1 button approach
                Thread.Sleep(500);

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

        static public bool CheckForSuicidesInChat()
        {
            var ChatWnd = GetUITrees().FindEntityOfString("ChatWindowStack");
            if (ChatWnd == null)
                return false;
            var ChatWndEntry = ChatWnd.handleEntity("ChatWindowStack");
            var Persons = ChatWndEntry.FindEntityOfString("XmppChatSimpleUserEntry");
            if (Persons == null)
                return false;
            var PersonsEntry = Persons.handleEntity("XmppChatSimpleUserEntry");
            var IsCriminal = false;
            for (int i = 0; i < PersonsEntry.children.Length; i++)
            {
                if (PersonsEntry.children[i] == null)
                    continue;
                if (PersonsEntry.children[i].children == null)
                    continue;
                if (PersonsEntry.children[i].children.Length < 3)
                    continue;
                //Unhandled exception. System.IndexOutOfRangeException: Index was outside the bounds of the array.
                if (PersonsEntry.children[i].children[2] == null)
                    continue;
                if (PersonsEntry.children[i].children[2].children == null)
                    continue;
                if (PersonsEntry.children[i].children[2].children.Length == 0)
                    continue;

                //System.IndexOutOfRangeException: Index was outside the bounds of the array.
                if (PersonsEntry.children[i].children[2].children[0].pythonObjectTypeName != "FlagIconWithState")
                    continue;

                if (PersonsEntry.children[i].children[2].children[0]
                .dictEntriesOfInterest["_hint"].ToString() == "Pilot is a suspect")
                {
                    Console.WriteLine("suspect pilot in system");
                }
                if (PersonsEntry.children[i].children[2].children[0]
                .dictEntriesOfInterest["_hint"].ToString() == "Pilot is a criminal")
                {
                    IsCriminal = true;
                    Console.WriteLine("criminal pilot in system");
                    break;
                }
            }
            return IsCriminal;
            //Pilot is a criminal
            //Pilot is a suspect
            //FlagIconWithState
        }

        static public void DockingFromSuicides()
        {
            (int XlocNotepadWindow, int YlocNotepadWindow) = Finders.FindLocWnd("OverView");
            if (XlocNotepadWindow == 0) //ship docked
            {
                SecondaryScripts.DockingAndCheckingForSuicides();
                return;
            }

            Emulators.PressButton((int)WinApi.VirtualKeyShort.VK_H);
            (int XStationCoords, int YStationCoords) = Finders.FindObjectByWordInOverview("Station");
            var TryTosebat = false;
            if (XStationCoords > 0)
            {
                Emulators.ClickLB(XStationCoords, YStationCoords);
                Thread.Sleep(500);
                Emulators.ClickLB(2200, 100); //3 button
                SecondaryScripts.DockingAndCheckingForSuicides();
            }
            else
            {
                (int XGate, int YGate) = GetCoordsNextSystem();
                if (XGate > 1)
                {
                    TryTosebat = true;
                    Emulators.ClickLB(XGate, YGate);
                    Thread.Sleep(500);
                    Emulators.ClickLB(2200, 100); //3 button
                    Checkers.WatchState();
                    Thread.Sleep(1000 * 10);
                }
                if (!TryTosebat)
                {
                    (int XStargateCoords, int YStargateCoords) = Finders.FindObjectByWordInOverview("Stargate");
                    if (XStargateCoords > 0)
                    {
                        Emulators.ClickLB(XStargateCoords, YStargateCoords);
                        Thread.Sleep(500);
                        Emulators.ClickLB(2200, 100); //3 button
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
            if (YGate == 0 && NeedToLayRoute)
            {
                Console.WriteLine("no route, start to laying a new route");
                SecondaryScripts.StartLayRoute();
                return false;
            }
            Emulators.ClickLB(XGate, YGate);
            Thread.Sleep(500);
            Emulators.ClickLB(2200, 100); //3 button

            Checkers.WatchState();
            Thread.Sleep(1000 * 10);
            return true;
        }

        static public (int, int) GetCoordsNextSystem()
        {
            var Overview = GetUITrees().FindEntityOfString("OverviewScrollEntry");
            if (Overview == null)
            {
                return (1, 1);
            }

            (int XlocOverview, int YlocOverview) = Finders.FindLocWnd("OverView");

            var OverviewEntry = Overview.handleEntity("OverviewScrollEntry");
            for (int k = 0; k < OverviewEntry.children.Length; k++)
            {
                if (OverviewEntry.children[k] == null)
                    continue;
                if (OverviewEntry.children[k].children == null)
                    continue;
                if (OverviewEntry.children[k].children.Length == 0)
                    continue;

                int YLocEnemyRelOverview = 0;
                if (OverviewEntry.children[k].dictEntriesOfInterest["_displayY"] is Newtonsoft.Json.Linq.JObject)
                {
                    YLocEnemyRelOverview = Convert.ToInt32(OverviewEntry.children[k].dictEntriesOfInterest["_displayY"]["int_low32"]);
                }
                else
                {
                    YLocEnemyRelOverview = Convert.ToInt32(OverviewEntry.children[k].dictEntriesOfInterest["_displayY"]);
                }

                var IsCargo = false;
                for (int j = 0; j < OverviewEntry.children[k].children.Length - 1; j++)// without SpaceObjectIcon
                {
                    if (OverviewEntry.children[k].children[j] == null)
                    {
                        continue;
                    }
                    // Unhandled exception. System.NullReferenceException: Object reference not set to an instance of an object.
                    if (OverviewEntry.children[k].children[j].dictEntriesOfInterest.ContainsKey("_text"))
                    {
                        if (OverviewEntry.children[k].children[j].dictEntriesOfInterest["_text"].ToString().Contains("Cargo Container")
                        ||
                        OverviewEntry.children[k].children[j].dictEntriesOfInterest["_text"].ToString().Contains("Hangar Container")
                        ||
                        OverviewEntry.children[k].children[j].dictEntriesOfInterest["_text"].ToString().Contains("Wreck"))
                        {
                            IsCargo = true;
                            break;
                        }
                    }
                }
                if (IsCargo)
                    continue;

                if (OverviewEntry.children[k].children.Last().pythonObjectTypeName == "SpaceObjectIcon")
                {
                    for (int j = 0; j < OverviewEntry.children[k].children.Last().children.Length; j++)
                    {
                        if (OverviewEntry.children[k].children.Last().children[j].pythonObjectTypeName == "Sprite"
                            &&
                            OverviewEntry.children[k].children.Last().children[j].dictEntriesOfInterest["_name"].ToString() == "iconSprite")
                        {
                            var RGBColorIcon = OverviewEntry.children[k].children.Last().children[j].dictEntriesOfInterest["_color"];
                            if (RGBColorIcon == null)
                                break;
                            if (Convert.ToInt32(RGBColorIcon["rPercent"]) == 100
                            && Convert.ToInt32(RGBColorIcon["gPercent"]) == 100
                            && Convert.ToInt32(RGBColorIcon["bPercent"]) == 0)
                            {
                                Console.WriteLine(YLocEnemyRelOverview); // расоложение строки по Y
                                Console.WriteLine("routed gate on X : {0}, Y : {1}!", XlocOverview + 50, YlocOverview + YLocEnemyRelOverview + 77 + 23);
                                return (XlocOverview + 50, YlocOverview + YLocEnemyRelOverview + 77);
                            }
                        }
                    }
                }
            }
            return (0, 0);
        }

        static public void DestroyTargets()
        {
            for (int i = 0; i < 40; i++)
            {
                var EnemyCoordsArray = Checkers.GetCoordsEnemies();

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
                
                Emulators.LockTargets(EnemyCoordsArray);

                Thread.Sleep(1000);

                if (!Checkers.CheckLocking())// если таргет не лочится подлететь
                    continue;
                ThreadManager.AllowToAttack = true;

                for (int j = 0; j < 60; j++)
                {
                    if (!Checkers.WatchLockingTargets())
                    {
                        ThreadManager.AllowToAttack = false;
                        break;
                    }
                    if (j % 3 == 0 && ThreadManager.AllowDroneControl)
                        Emulators.PressButton((int)WinApi.VirtualKeyShort.VK_F);
                    Thread.Sleep(1000 * 5);
                }
            }
            Console.WriteLine("слишком долгий процесс уничтожения");
            SecondaryScripts.DockToStationAndExit();
        }

        static public void WatchDistance()
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 20; j++) // 2 min
                {
                    var EnemyCoordsArray = Checkers.GetCoordsEnemies();
                    if (EnemyCoordsArray == null || EnemyCoordsArray.Count == 0)
                    {
                        return;
                    }
                    //Unhandled exception. System.ArgumentOutOfRangeException: Index was out of range.
                    //Must be non-negative and less than the size of the collection. (Parameter 'index')
                    if (1 != EnemyCoordsArray[0])
                    {
                        return;
                    }
                    Console.WriteLine("enemies are far away");
                    Thread.Sleep(1000 * 5);
                }
                //3 проверки на наличие ExpBlock AccelerationGate
                //если нет то выход
                (int XBlock, int YBlock) = Finders.FindExpBlock();
                (int XAcGate, int YAcGate) = Finders.FindAccelerationGate();
                if (XBlock != 0 || XAcGate != 0)
                {
                    if (XBlock != 0)
                    {
                        Emulators.ClickLB(XBlock, YBlock);
                        Thread.Sleep(500);
                        Emulators.ClickLB(2200, 100); //3 button
                    }
                    else if (XAcGate != 0)
                    {
                        Emulators.ClickLB(XAcGate, YAcGate);
                        Thread.Sleep(500);
                        Emulators.ClickLB(2200 + 33, 100); //4 button
                    }
                    MainScripts.ClearExpRoom();
                    MainScripts.StartClearExp();
                }
                else
                {
                    Console.WriteLine("слишком долго летел к цели");
                    SecondaryScripts.DockToStationAndExit();
                }
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

            for (int i = 0; i < 10; i++)
            {
                (int XContCoords, int YContCoords) = Finders.FindObjectByWordInOverview(ContName);

                if (XContCoords == 0)
                {
                    Console.WriteLine("no {0}", ContName);
                    return;
                }

                Emulators.ClickLB(XContCoords, YContCoords);
                Thread.Sleep(200);
                Emulators.ClickLB(2200, 100); //3 button loot
                UITreeNode OpenedCont = null;
                for (int j = 0; j < 300; j++)
                {
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
                if (SecondaryScripts.CheckCargoPrice(NeedPrice))
                {
                    (XlocInventory, YlocInventory) = Finders.FindLocWnd("InventoryPrimary");

                    HeightInventory = Window.GetHeightWindow("InventoryPrimary");
                    WidthLeftSidebar = Window.GetWidthWindow("TreeViewEntryInventoryCargo");

                    Console.WriteLine("button loot all: {0}, {1}", XlocInventory + WidthLeftSidebar + 30, YlocInventory + HeightInventory - 20 + 23);

                    Emulators.ClickLB(XlocInventory + WidthLeftSidebar + 30, YlocInventory + HeightInventory - 20); // Loot all
                    Console.WriteLine("cont looted");
                    Thread.Sleep(1000 * 5);
                }
                else
                {
                    Console.WriteLine("carbage in loot");
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
