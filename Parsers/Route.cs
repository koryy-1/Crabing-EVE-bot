using EVE_Bot.Models;
using EVE_Bot.Searchers;
using read_memory_64_bit;
using System;
using System.Collections.Generic;
using System.Text;

namespace EVE_Bot.Parsers
{
    static public class Route
    {
        static public List<SystemInfo> GetInfo()
        {
            List<SystemInfo> Route = new List<SystemInfo>();
            var AutopilotDestinationIcon = GetUITrees().FindEntityOfString("AutopilotDestinationIcon");
            if (AutopilotDestinationIcon == null)
            {
                return null;
            }
            var AutopilotDestinationIconEntity = AutopilotDestinationIcon.handleEntity("AutopilotDestinationIcon");
            for (int j = 0; j < AutopilotDestinationIconEntity.children.Length; j++)
            {
                SystemInfo SystemInfo = new SystemInfo();

                SystemInfo.Colors.Red = Convert.ToInt32(AutopilotDestinationIconEntity.children[j].children[0].dictEntriesOfInterest["_color"]["rPercent"]);
                SystemInfo.Colors.Green= Convert.ToInt32(AutopilotDestinationIconEntity.children[j].children[0].dictEntriesOfInterest["_color"]["gPercent"]);
                SystemInfo.Colors.Blue = Convert.ToInt32(AutopilotDestinationIconEntity.children[j].children[0].dictEntriesOfInterest["_color"]["bPercent"]);

                Route.Add(SystemInfo);
            }
            return Route;
        }
        static public UITreeNode GetUITrees()
        {
            return ReadMemory.GetUITrees(Window.RootAddress, Window.processId);
        }
    }
}
