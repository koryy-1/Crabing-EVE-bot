using EVE_Bot.Models;
using EVE_Bot.Scripts;
using EVE_Bot.Searchers;
using read_memory_64_bit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EVE_Bot.Parsers
{
    static public class HI
    {
        static public HudInterface GetInfo()
        {
            var HudContainer = GetHudContainer();
            if (HudContainer == null)
            {
                return null;
            }

            HudInterface HudInterface = new HudInterface();

            //Pos
            HudInterface.Pos = GetCenterPos(HudContainer);


            //HP
            HudInterface.HealthPoints = GetShipHP(HudContainer);


            //CurrentSpeed
            HudInterface.CurrentSpeed = GetCurrentSpeed(HudContainer);


            //Module
            HudInterface.AllModules = GetAllModulesInfo(HudContainer);


            return HudInterface;
        }
        static public Point GetCenterPos(UITreeNode HudContainer)
        {
            var (XHudContainer, YHudContainer) = General.GetCoordsEntityOnScreen(HudContainer
                    .children[Convert.ToInt32(HudContainer.dictEntriesOfInterest["needIndex"])]
                    );

            var CenterHudContainerEntity = HudContainer.children[0].children[0];
            var (XCenterHudContainer, _) = General.GetCoordsEntityOnScreen(CenterHudContainerEntity);

            Point Pos = new Point();

            Pos.x = XHudContainer + XCenterHudContainer + 95;
            Pos.y = YHudContainer + 100;

            return Pos;
        }
        static public HealthPoints GetShipHP(UITreeNode HudContainer)
        {
            //var HudReadout = HudContainer.FindEntityOfString("HudReadout");
            //if (HudReadout == null)
            //{
            //    return null;
            //}
            //var HudReadoutEntry = HudReadout.handleEntity("HudReadout");

            //if (HudContainerEntity.children.Length < 6)
            //{
            //    return null;
            //}
            var HudReadoutEntry = HudContainer.children[0].children[5];

            var ShieldStr = HudReadoutEntry
                .children[0].children[0].dictEntriesOfInterest["_setText"].ToString();
            int Shield;
            int.TryParse(string.Join("", ShieldStr.Where(c => char.IsDigit(c))), out Shield);

            var ArmorStr = HudReadoutEntry
                .children[1].children[0].dictEntriesOfInterest["_setText"].ToString();
            int Armor;
            int.TryParse(string.Join("", ArmorStr.Where(c => char.IsDigit(c))), out Armor);

            var StructureStr = HudReadoutEntry
                .children[2].children[0].dictEntriesOfInterest["_setText"].ToString();
            int Structure;
            int.TryParse(string.Join("", StructureStr.Where(c => char.IsDigit(c))), out Structure);

            HealthPoints HealthPoints = new HealthPoints();
            HealthPoints.Shield = Shield;
            HealthPoints.Armor = Armor;
            HealthPoints.Structure = Structure;

            return HealthPoints;
        }
        static public int GetCurrentSpeed(UITreeNode HudContainer)
        {
            var CenterHudContainerEntity = HudContainer.children[0].children[0];

            if (CenterHudContainerEntity.children[5].children[0].children[0].children[0]
                .dictEntriesOfInterest["_setText"].ToString() != "(Warping)")
            {
                return Convert.ToInt32(CenterHudContainerEntity
                    .children[5].children[0].children[0].children[0]
                    .dictEntriesOfInterest["_setText"].ToString().Split(".")[0].Split()[0]);
            }
            return -1;
        }
        static public List<Module> GetAllModulesInfo(UITreeNode HudContainer)
        {
            var SlotsContainer = HudContainer.FindEntityOfString("ShipSlot").handleEntity("ShipSlot");

            List<Module> AllModules = new List<Module>();

            for (int i = 0; i < SlotsContainer.children.Length; i++)
            {
                if (SlotsContainer.children[i] == null)
                    continue;

                Module Module = new Module();

                //Name
                var RawModuleName = SlotsContainer.children[i].children[0].dictEntriesOfInterest["_name"].ToString();

                var ModulesInfo = new ModulesInfo();
                if (ModulesInfo.ModuleNamesDict.ContainsKey(RawModuleName))
                    Module.Name = ModulesInfo.ModuleNamesDict[RawModuleName];
                else
                    Module.Name = RawModuleName;

                //Virtual key
                var (X, Y) = General.GetCoordsEntityOnScreen(SlotsContainer.children[i]);
                if (Y == 0)
                    Module.VirtualKey = X / 51 + 0x70;

                //SlotNum
                var ModuleType = SlotsContainer.children[i].dictEntriesOfInterest["_name"].ToString();

                int SlotNum;
                int.TryParse(string.Join("", ModuleType.Where(c => char.IsDigit(c))), out SlotNum);
                Module.SlotNum = SlotNum;

                //quantityParent
                var QuantityParent = SlotsContainer.children[i].children[0].children[0];
                if (QuantityParent.dictEntriesOfInterest.ContainsKey("_name")
                    &&
                    QuantityParent.dictEntriesOfInterest["_name"].ToString() == "quantityParent")
                {
                    Module.AmountСharges = Convert.ToInt32(QuantityParent.children[0]
                    .dictEntriesOfInterest["_setText"]);
                }

                //Mode
                var RampIsActive = SlotsContainer.children[i].children[2].pythonObjectTypeName;

                var IsGlowInActiveMode = SlotsContainer.children[i].children.Last().dictEntriesOfInterest["_name"].ToString();

                if (IsGlowInActiveMode == "glow")
                {
                    Module.Mode = "glow";
                }
                else if (IsGlowInActiveMode == "busy")
                {
                    Module.Mode = "busy";
                }
                else if (RampIsActive == "ShipModuleButtonRamps"
                    &&
                    IsGlowInActiveMode != "glow" && IsGlowInActiveMode != "busy")
                {
                    Module.Mode = "reloading";
                }
                else if (RampIsActive != "ShipModuleButtonRamps"
                    &&
                    IsGlowInActiveMode != "glow" && IsGlowInActiveMode != "busy")
                {
                    Module.Mode = "idle";
                }

                //Type
                if (ModuleType.Contains("High"))
                {
                    Module.Type = "high";
                }
                else if (ModuleType.Contains("Medium"))
                {
                    Module.Type = "med";
                }
                else if (ModuleType.Contains("Low"))
                {
                    Module.Type = "low";
                }
                AllModules.Add(Module);
            }

            return AllModules;
        }
        static public string GetShipState(UITreeNode HudContainer)
        {
            if (HudContainer == null)
            {
                return null;
            }

            if (HudContainer.children[Convert.ToInt32(HudContainer.dictEntriesOfInterest["needIndex"])]
                .children[4].children.Length == 0)
            {
                return null;
            }

            var CurrentState = HudContainer.children[Convert.ToInt32(HudContainer.dictEntriesOfInterest["needIndex"])]
                    .children[4].children[1].dictEntriesOfInterest["_setText"].ToString();

            return CurrentState;
        }
        static public UITreeNode GetHudContainer()
        {
            var HudContainer = UITreeReader.GetUITrees("ShipUI", 3);
            if (HudContainer == null)
            {
                return null;
            }

            return HudContainer;
        }
    }
}
