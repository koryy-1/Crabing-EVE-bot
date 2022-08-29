using EVE_Bot.Configs;
using EVE_Bot.Controllers;
using EVE_Bot.Scripts;
using read_memory_64_bit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EVE_Bot.Models;
using EVE_Bot.Parsers;
using System.Threading;

namespace EVE_Bot.Searchers
{
    static public class Checkers
    {
        static public List<int> GetCoordsEnemies(string ExcludeTarget = null, bool ConsiderDistance = true)
        {
            List<OverviewItem> OverviewInfo = OV.GetInfo();

            List<int> EnemyCoords = new List<int>();

            List<string> ExcludeSomeTargets = new List<string>();
            if (ExcludeTarget != null)
            {
                ExcludeSomeTargets.Add(ExcludeTarget);
            }
            int EnemyHere = 0;
            int LockedEnemyHere = 0;

            foreach (var item in OverviewInfo)
            {
                // check exclude target
                var ExcludedTarget = false;
                for (int i = 0; i < ExcludeSomeTargets.Count; i++)
                {
                    if (item.Name.Contains(ExcludeSomeTargets[i]) ||
                        item.Type.Contains(ExcludeSomeTargets[i]))
                    {
                        ExcludedTarget = true;
                    }
                }
                if (ExcludedTarget)
                    continue;

                // check enemy target
                if (Finders.GetColorInfo(item.Colors) != "red")
                    continue;

                // check distance to target
                if (item.Distance.measure == "km"
                    &&
                    item.Distance.value < Config.LockRange
                    &&
                    item.Distance.value < Config.WeaponsRange
                    ||
                    item.Distance.measure == "m"
                    ||
                    !ConsiderDistance)
                {
                    EnemyCoords.Add(item.Pos.x);
                    EnemyCoords.Add(item.Pos.y);
                    if (EnemyCoords.Count / 2 > 4) // макс количество захваченных целей
                    {
                        if (LockedEnemyHere == 2)
                        {
                            EnemyCoords.Add(2);
                        }
                        return EnemyCoords;
                    }
                }
                else
                {
                    EnemyHere = 1;
                }
                if (item.TargetLocked)
                {
                    LockedEnemyHere = 2;
                }
            }
            if (EnemyHere == 1 && EnemyCoords.Count == 0)
            {
                EnemyCoords.Add(1);
            }
            return EnemyCoords;
        }

        static public bool CheckDistance(string ItemInSpace, int MinDistance, string unit = "m")
        {
            var (XlocOverview, YlocOverview) = Finders.FindLocWnd("OverView");

            var OverviewEntry = GetUITrees().FindEntityOfString("OverviewScrollEntry").handleEntity("OverviewScrollEntry");

            for (int k = 0; k < OverviewEntry.children.Length; k++)
            {
                if (OverviewEntry.children[k] == null)
                    continue;
                if (OverviewEntry.children[k].children == null)
                    continue;
                if (OverviewEntry.children[k].children.Length == 0)
                    continue;
                if (OverviewEntry.children[k].children.Last().pythonObjectTypeName != "SpaceObjectIcon")
                    continue;
                if (OverviewEntry.children[k].children.Last().children == null)
                    continue;

                var DistanceToItem = 0;
                var DistanceToItemStr = OverviewEntry.children[k].children[OverviewEntry.children[k].children.Length - 2]
                                .dictEntriesOfInterest["_text"].ToString();
                var CurItemName = OverviewEntry.children[k].children[OverviewEntry.children[k].children.Length - 4]
                                .dictEntriesOfInterest["_text"].ToString();

                int.TryParse(string.Join("", DistanceToItemStr.Where(c => char.IsDigit(c))), out DistanceToItem);
                if (unit == "km"
                    &&
                    ((DistanceToItemStr.Contains($" km")
                    &&
                    DistanceToItem < MinDistance)
                    ||
                    (DistanceToItemStr.Contains($" m")
                    &&
                    DistanceToItem < 10000))
                    &&
                    CurItemName.Contains(ItemInSpace))
                {
                    return true;
                }
                else if (DistanceToItemStr.Contains($" {unit}")
                    &&
                    DistanceToItem < MinDistance
                    &&
                    CurItemName.Contains(ItemInSpace))
                {
                    return true;
                }
            }
            return false;
        }

        static public bool CheckEnemyAbsenceAgr(string AgrMode)
        {
            // Hostile AttackingMe

            var OverviewEntry = GetUITrees().FindEntityOfString("OverviewScrollEntry").handleEntity("OverviewScrollEntry");

            for (int k = 0; k < OverviewEntry.children.Length; k++)
            {
                if (OverviewEntry.children[k] == null)
                    continue;
                if (OverviewEntry.children[k].children == null)
                    continue;
                if (OverviewEntry.children[k].children.Length == 0)
                    continue;
                if (OverviewEntry.children[k].children.Last().pythonObjectTypeName != "SpaceObjectIcon")
                    continue;
                if (OverviewEntry.children[k].children.Last().children == null)
                    continue;

                var IsEnemy = false;
                for (int j = 0; j < OverviewEntry.children[k].children.Last().children.Length; j++)
                {
                    if (OverviewEntry.children[k].children.Last().children[j].pythonObjectTypeName == "Sprite"
                        &&
                        OverviewEntry.children[k].children.Last().children[j].dictEntriesOfInterest["_name"].ToString() == "iconSprite")
                    {
                        var RGBColorIcon = OverviewEntry.children[k].children.Last().children[j].dictEntriesOfInterest["_color"];
                        //NullReferenceException
                        if (Convert.ToInt32(RGBColorIcon["rPercent"]) == 100
                        && Convert.ToInt32(RGBColorIcon["gPercent"]) == 10
                        && Convert.ToInt32(RGBColorIcon["bPercent"]) == 10)
                        {
                            IsEnemy = true;
                        }
                    }
                }

                //Unhandled exception. System.NullReferenceException: Object reference not set to an instance of an object.
                for (int j = 0; j < OverviewEntry.children[k].children.Last().children.Length; j++)
                {
                    var CurAgrMode = OverviewEntry.FindEntityOfStringByDictEntriesOfInterest("_name", AgrMode);
                    if (IsEnemy && CurAgrMode == null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        static public bool CheckLocking()
        {
            for (int i = 0; i < 8; i++)
            {
                var OverviewEntry = GetUITrees().FindEntityOfString("OverviewScrollEntry").handleEntity("OverviewScrollEntry");

                int cntLocking = 0;

                for (int k = 0; k < OverviewEntry.children.Length; k++)
                {
                    if (OverviewEntry.children[k] == null)
                        continue;
                    if (OverviewEntry.children[k].children == null)
                        continue;
                    if (OverviewEntry.children[k].children.Length == 0)
                        continue;

                    if (OverviewEntry.children[k].children.Last().pythonObjectTypeName == "SpaceObjectIcon")
                    {
                        //Unhandled exception. System.NullReferenceException: Object reference not set to an instance of an object.
                        for (int j = 0; j < OverviewEntry.children[k].children.Last().children.Length; j++)
                        {
                            if (OverviewEntry.children[k].children.Last().children[j].pythonObjectTypeName == "Sprite"
                                &&
                                OverviewEntry.children[k].children.Last().children[j].dictEntriesOfInterest["_name"].ToString() == "myActiveTargetIndicator")
                            {
                                Console.WriteLine("target locked");
                                return true;
                            }
                            if (OverviewEntry.children[k].children.Last().children[j].pythonObjectTypeName == "Container"
                                &&
                                OverviewEntry.children[k].children.Last().children[j].dictEntriesOfInterest["_name"].ToString() == "targeting")
                            {
                                cntLocking++;
                            }
                        }

                    }
                }
                if (cntLocking == 0)
                {
                    Console.WriteLine("targets not locking");
                    System.Threading.Thread.Sleep(1000);
                    return false;
                }
                System.Threading.Thread.Sleep(1000);

            }
            return false;

        }

        static public bool WatchLockingTargets()
        {
            List<OverviewItem> OverviewInfo = OV.GetInfo();

            foreach (var item in OverviewInfo)
            {
                if (item.TargetLocked && 
                    (item.Distance.measure == "km" && item.Distance.value < Config.WeaponsRange || item.Distance.measure == "m"))
                {
                    return true;
                }
            }
            return false;
        }

        static public void WatchState()
        {
            ThreadManager.DangerAnalyzerEnable = false;
            System.Threading.Thread.Sleep(1000 * 4);
            int ApproachCnt = 0;

            for (int i = 0; i < 36; i++)
            {
                var CurrentState = HI.GetShipState(HI.GetHudContainer());
                if (CurrentState == null)
                {
                    Console.WriteLine("ship has reached destination");
                    ThreadManager.DangerAnalyzerEnable = true;
                    return;
                }

                Console.WriteLine(CurrentState.CurrentState);
                if (CurrentState.CurrentState.Contains("Jumping"))
                {
                    Console.WriteLine("ship has reached destination");
                    ThreadManager.DangerAnalyzerEnable = true;
                    return;
                }
                else if (CurrentState.CurrentState.Contains("Click target"))
                {
                    Console.WriteLine("ship has reached destination");
                    ThreadManager.DangerAnalyzerEnable = true;
                    return;
                }
                else if (CurrentState.CurrentState.Contains("Approaching"))
                {
                    ApproachCnt++;
                    ThreadManager.DangerAnalyzerEnable = true;
                    if (ApproachCnt > 30) // 2.5 min
                    {
                        Console.WriteLine("ship hit the curb");

                        return;
                    }

                }
                else if (CurrentState.CurrentState.Contains("Orbiting"))
                {
                    Console.WriteLine("ship hit the curb");
                    ThreadManager.DangerAnalyzerEnable = true;
                    return;
                }

                System.Threading.Thread.Sleep(1000 * 5);
            }
            Console.WriteLine("proizowla xyunya");
            ThreadManager.DangerAnalyzerEnable = true;
        }

        static public bool CheckState(string State, string ItemInSpace = "", string Distance = "")
        {
            //Jumping
            //Click target
            //Approaching
            //Orbiting
            //Warping
            //Aligning

            var CaptionLabel = GetUITrees().FindEntityOfString("HudContainer");
            if (CaptionLabel == null)
                return false;

            CaptionLabel = CaptionLabel.handleEntity("HudContainer")
                .FindEntityOfStringByDictEntriesOfInterest("_name", "indicationContainer");

            if (CaptionLabel == null)
                return false;

            CaptionLabel = CaptionLabel.handleEntityByDictEntriesOfInterest("_name", "indicationContainer");

            if (CaptionLabel.children[Convert.ToInt32(CaptionLabel.dictEntriesOfInterest["needIndex"])].children == null)
                return false;
            if (CaptionLabel.children[Convert.ToInt32(CaptionLabel.dictEntriesOfInterest["needIndex"])].children.Length < 2)
                return false;
            if (CaptionLabel.children[Convert.ToInt32(CaptionLabel.dictEntriesOfInterest["needIndex"])].children[0] == null)
                return false;
            if (!CaptionLabel.children[Convert.ToInt32(CaptionLabel.dictEntriesOfInterest["needIndex"])]
                .children[0].dictEntriesOfInterest.ContainsKey("_setText"))
                return false;
            if (CaptionLabel.children[Convert.ToInt32(CaptionLabel.dictEntriesOfInterest["needIndex"])].children[1] == null)
                return false;
            if (!CaptionLabel.children[Convert.ToInt32(CaptionLabel.dictEntriesOfInterest["needIndex"])]
                .children[1].dictEntriesOfInterest.ContainsKey("_setText"))
                return false;

            var CurrentItem = CaptionLabel.children[Convert.ToInt32(CaptionLabel.dictEntriesOfInterest["needIndex"])]
                .children[0].dictEntriesOfInterest["_setText"].ToString();
            var CurrentState = CaptionLabel.children[Convert.ToInt32(CaptionLabel.dictEntriesOfInterest["needIndex"])]
                .children[1].dictEntriesOfInterest["_setText"].ToString();

            if (Distance != "" && ItemInSpace != ""
                &&
                CurrentItem.Contains(Distance)
                &&
                CurrentItem.Contains(ItemInSpace)
                &&
                CurrentState.Contains(State))
            {
                //Console.WriteLine($"ship is in state of {State} on {ItemInSpace} in {Distance}");
                return true;
            }
            else if (Distance == "" && ItemInSpace != ""
                &&
                CurrentItem.Contains(ItemInSpace)
                &&
                CurrentState.Contains(State))
            {
                //Console.WriteLine($"ship is in state of {State} on {ItemInSpace}");
                return true;
            }
            else if (Distance == "" && ItemInSpace == ""
                &&
                CurrentState.Contains(State))
            {
                //Console.WriteLine($"ship is in state of {State}");
                return true;
            }
            else
            {
                Console.WriteLine($"ship is not in state of {State} on {ItemInSpace}");
                return false;
            }
        }

        static public int CheckQuantityDrones()
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    var DroneMainGroup = GetUITrees().FindEntityOfString("DroneView").FindEntityOfString("DroneMainGroup");
                    if (DroneMainGroup == null)
                        continue;
                    var DroneMainGroupEntry = DroneMainGroup.handleEntity("DroneMainGroup");
                    var SthingDronesInBay = DroneMainGroupEntry.children[0].children[3].children[0].dictEntriesOfInterest["_setText"].ToString();
                    Console.WriteLine(SthingDronesInBay);
                    if (Convert.ToInt32(SthingDronesInBay[SthingDronesInBay.Length - 2]) - 48 >= 3) // pidaras
                    {
                        Console.WriteLine("enough amount drones");
                        return Convert.ToInt32(SthingDronesInBay[SthingDronesInBay.Length - 2]) - 48;
                    }
                    if (Convert.ToInt32(SthingDronesInBay[SthingDronesInBay.Length - 2]) - 48 < 3)
                    {
                        Console.WriteLine("not enough amount drones");
                        return Convert.ToInt32(SthingDronesInBay[SthingDronesInBay.Length - 2]) - 48;
                    }
                }
                catch
                {
                    continue;
                }
            }
            return 0;
        }

        static public void ShipInLowHP(int HPthreshold)
        {
            int PrevShield = 0;
            while (true)
            {
                int CurrentShield = HI.GetShipHP(HI.GetHudContainer()).Shield;
                if (CurrentShield < HPthreshold && CurrentShield < PrevShield || CurrentShield < 10)
                {
                    Console.WriteLine("ship has {0}% shield", CurrentShield);
                    ThreadManager.ShipShieldIsLow = true;
                }
                else if (ThreadManager.ShipShieldIsLow)
                {
                    ThreadManager.ShipShieldIsLow = false;
                }

                PrevShield = CurrentShield;

                Thread.Sleep(ThreadManager.MultiplierSleep * 1000);
            }
        }


        static public UITreeNode GetUITrees()
        {
            return ReadMemory.GetUITrees(Window.RootAddress, Window.processId);
        }
    }
}
