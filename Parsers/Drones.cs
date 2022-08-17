﻿using EVE_Bot.Models;
using EVE_Bot.Searchers;
using read_memory_64_bit;
using System;
using System.Collections.Generic;
using System.Text;

namespace EVE_Bot.Parsers
{
    internal class Drones
    {
        static public List<DroneInfo> GetInfo()
        {
            var (XDroneView, YDroneView) = Finders.FindLocWnd("DroneView");

            var deepDrones = UITreeReader.GetUITrees("DroneView");
            if (deepDrones == null)
                return null;

            deepDrones = deepDrones.FindEntityOfString("DroneEntry");
            if (deepDrones == null)
                return null;

            var needDrone = deepDrones.handleEntity("DroneEntry");


            List<DroneInfo> DronesInfo = new List<DroneInfo>();


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

                //if (needDrone.children[k].dictEntriesOfInterest.ContainsKey("_hint"))
                //    continue;

                DroneInfo OneDroneInfo = new DroneInfo();

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
                var DroneRelationLoc = Convert.ToInt32(needDrone.children[k].dictEntriesOfInterest["_displayY"]) - 23;

                OneDroneInfo.Pos.x = XDroneView + 20;
                OneDroneInfo.Pos.y = YDroneView + DroneRelationLoc + 60;

                if (needDrone.children[k].dictEntriesOfInterest.ContainsKey("_hint"))
                {
                    OneDroneInfo.WorkMode = "in space";
                }
                else
                {
                    OneDroneInfo.WorkMode = "in drone bay";
                }

                OneDroneInfo.HealthPoints.Shield = HPShield;
                OneDroneInfo.HealthPoints.Armor = HPArmor;
                OneDroneInfo.HealthPoints.Structure = HPStruct;

                DronesInfo.Add(OneDroneInfo);
            }
            return DronesInfo;
        }
    }
}
