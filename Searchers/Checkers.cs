using EVE_Bot.Controllers;
using EVE_Bot.Scripts;
using read_memory_64_bit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EVE_Bot.Searchers
{
    static public class Checkers
    {
        static public List<int> GetCoordsEnemies()
        {
            var (XlocOverview, YlocOverview) = Finders.FindLocWnd("OverView");

            var OverviewEntry = GetUITrees().FindEntityOfString("OverviewScrollEntry").handleEntity("OverviewScrollEntry");

            List<int> EnemyCoordsArray = new List<int>();

            int EnemyHere = 0;

            int DistanceToEnemy = 0;
            var DistanceToEnemyStr = "";

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

                int YLocEnemyRelOverview = 0;
                if (OverviewEntry.children[k].dictEntriesOfInterest["_displayY"] is Newtonsoft.Json.Linq.JObject)
                {
                    YLocEnemyRelOverview = Convert.ToInt32(OverviewEntry.children[k].dictEntriesOfInterest["_displayY"]["int_low32"]);
                }
                else
                {
                    YLocEnemyRelOverview = Convert.ToInt32(OverviewEntry.children[k].dictEntriesOfInterest["_displayY"]);
                }

                DistanceToEnemy = 0;
                DistanceToEnemyStr = "";


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
                            DistanceToEnemyStr = OverviewEntry.children[k].children[OverviewEntry.children[k].children.Length - 2]
                                .dictEntriesOfInterest["_text"].ToString();

                            int.TryParse(string.Join("", DistanceToEnemyStr.Where(c => char.IsDigit(c))), out DistanceToEnemy);
                            if (DistanceToEnemyStr.Contains(" km")
                                &&
                                DistanceToEnemy < 53
                                ||
                                DistanceToEnemyStr.Contains(" m"))
                            {
                                EnemyCoordsArray.Add(XlocOverview + 50);
                                EnemyCoordsArray.Add(YlocOverview + YLocEnemyRelOverview + 77);
                                //Console.WriteLine("enemy on X : {0}, Y : {1}!", XlocOverview + 50, YlocOverview + YLocEnemyRelOverview + 77 + 23);
                                if (EnemyCoordsArray.Count / 2 > 4) // количество отмеченных целей
                                    return EnemyCoordsArray;
                            }
                            else
                            {
                                EnemyHere = 1;
                            }

                        }
                    }
                }
            }
            if (EnemyHere == 1 && EnemyCoordsArray.Count == 0)
            {
                EnemyCoordsArray.Add(1);
            }
            return EnemyCoordsArray;
        }

        static public bool CheckLocking()
        {
            for (int i = 0; i < 8; i++)
            {
                var uiTreePreparedForFile = GetUITrees();

                var Overview = uiTreePreparedForFile.FindEntityOfString("OverviewScrollEntry");

                var OverviewEntry = Overview.handleEntity("OverviewScrollEntry");

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
                    return false;
                }
                System.Threading.Thread.Sleep(1000);

            }
            return false;

        }

        static public bool WatchLockingTargets()
        {
            UITreeNode uiTreePreparedForFile = GetUITrees();

            var Overview = uiTreePreparedForFile.FindEntityOfString("OverviewScrollEntry");

            var OverviewEntry = Overview.handleEntity("OverviewScrollEntry");

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
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        static public void WatchState()
        {
            ThreadManager.AllowCheckRedMarker = false;
            System.Threading.Thread.Sleep(1000 * 4);
            int ApproachCnt = 0;
            for (int i = 0; i < 36; i++)
            {
                //NullReferenceException Object reference not set to an instance of an object
                var CaptionLabel = GetUITrees().FindEntityOfString("HudContainer");
                if (CaptionLabel == null)
                {
                    continue;
                }
                var CaptionLabelEntity = CaptionLabel.handleEntity("HudContainer");

                if (CaptionLabelEntity.children[Convert.ToInt32(CaptionLabelEntity.dictEntriesOfInterest["needIndex"])].children == null)
                    continue;
                if (CaptionLabelEntity.children[Convert.ToInt32(CaptionLabelEntity.dictEntriesOfInterest["needIndex"])].children[4] == null)
                    continue;
                if (CaptionLabelEntity.children[Convert.ToInt32(CaptionLabelEntity.dictEntriesOfInterest["needIndex"])].children[4].children == null)
                    continue;


                if (0 < CaptionLabelEntity.children[Convert.ToInt32(CaptionLabelEntity.dictEntriesOfInterest["needIndex"])].children[4].children.Length)
                {
                    var CurrentState = CaptionLabelEntity.children[Convert.ToInt32(CaptionLabelEntity.dictEntriesOfInterest["needIndex"])]
                        .children[4].children[1].dictEntriesOfInterest["_setText"];
                    Console.WriteLine(CurrentState);
                    if (CurrentState.ToString().Contains("Jumping"))
                    {
                        Console.WriteLine("ship has reached destination");
                        ThreadManager.AllowCheckRedMarker = true;
                        return;
                    }
                    if (CurrentState.ToString().Contains("Approaching"))
                    {
                        ApproachCnt++;
                        ThreadManager.AllowCheckRedMarker = true;
                        if (ApproachCnt > 11)
                        {
                            Console.WriteLine("ship hit the curb");
                            
                            return;
                        }
                        
                    }
                    if (CurrentState.ToString().Contains("Orbiting"))
                    {
                        ThreadManager.AllowCheckRedMarker = true;
                        Console.WriteLine("ship hit the curb");
                        return;
                    }
                }
                else
                {
                    Console.WriteLine("ship has reached destination");
                    ThreadManager.AllowCheckRedMarker = true;
                    return;
                }
                System.Threading.Thread.Sleep(1000 * 5);
            }
            Console.WriteLine("proizowla xyunya");
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

        static public bool ShipInLowHP(int HP)
        {
            var uiTree = GetUITrees();
            var HudReadout = uiTree.FindEntityOfString("HudReadout");
            if (HudReadout == null)
                return false;

            var HudReadoutEntry = HudReadout.handleEntity("HudReadout");

            var ShipHPWithPercent = HudReadoutEntry.children[Convert.ToInt32(HudReadoutEntry.dictEntriesOfInterest["needIndex"])]
                .children[0].children[0].dictEntriesOfInterest["_setText"].ToString();

            int ShipHP;
            int.TryParse(string.Join("", ShipHPWithPercent.Where(c => char.IsDigit(c))), out ShipHP);

            if (ShipHP < HP)
            {
                Console.WriteLine("ship has {0}% shield", ShipHP);
                return true;
            }
            else
                return false;
        }


        static public UITreeNode GetUITrees()
        {
            return ReadMemory.GetUITrees(Window.RootAddress, Window.processId);
        }
    }
}
