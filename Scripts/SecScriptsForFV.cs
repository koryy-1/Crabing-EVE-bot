using EVE_Bot.Configs;
using EVE_Bot.Controllers;
using EVE_Bot.Models;
using EVE_Bot.Searchers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace EVE_Bot.Scripts
{
    static public class SecScriptsForFV
    {
        static public Random r = new Random();
        static public int AvgDeley = Config.AverageDelay;
        static ModulesInfo ModulesInfo = new ModulesInfo();


        static public void CheckReadyForFV()
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

            //проверить расположение модулей на F кнопках
            //if CheckModulesLocOnF_Buttons
            // General.DockToStationAndExit();

            //определить где находится корабль
            General.EnsureUndocked();
            ThreadManager.AllowDocking = false;

            //поменять вкладку в инвентаре
            (int XlocInventory, int YlocInventory) = Finders.FindLocWnd("InventoryPrimary");
            Emulators.ClickLB(XlocInventory + 60, YlocInventory + 55);
            Thread.Sleep(AvgDeley + r.Next(-100, 100));

            //узнать количество ракет
            //CheckMissilesAmount();


            ThreadManager.AllowDScan = true;

            //поменять вкладку в гриде
            General.ChangeTab("General");
        }

    }
}
