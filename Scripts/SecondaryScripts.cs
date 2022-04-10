using EVE_Bot.Controllers;
using EVE_Bot.Searchers;
using read_memory_64_bit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EVE_Bot.Scripts
{
    static public class SecondaryScripts
    {
        static public void CheckSituation()
        {
            //после перезахода
            //поменять вкладку чата
            (int XlocChatWindowStack, int YlocChatWindowStack) = Finders.FindLocWnd("ChatWindowStack");
            if (XlocChatWindowStack != 0)
            {
                Emulators.ClickLB(XlocChatWindowStack + 20, YlocChatWindowStack + 15);
                System.Threading.Thread.Sleep(1000 * 2);
            }

            //определить где находится корабль
            (int XlocNotepadWindow, int YlocNotepadWindow) = Finders.FindLocWnd("OverView");
            if (XlocNotepadWindow == 0) // ship docked
            {
                Emulators.ClickLB(2420, 205);
                System.Threading.Thread.Sleep(500);
                Emulators.ClickLB(2420, 230);
                System.Threading.Thread.Sleep(1000 * 15);//wait for 20 sec after undock
                Emulators.ClickLB(1300, 600);
                System.Threading.Thread.Sleep(1000 * 3);
                Console.WriteLine("undocked");
            }
            ThreadManager.AllowDocking = false;

            //узнать количество дронов, реконнект
            (XlocNotepadWindow, YlocNotepadWindow) = Finders.FindLocWnd("DroneView");
            if (XlocNotepadWindow == 0)
            {
                SecondaryScripts.DockToStationAndExit();
            }
            var DronesQuantity = Checkers.CheckQuantityDrones();
            if (DronesQuantity <= 3)
            {
                //reconnect drones
                Emulators.ClickRB(1280, 1280);
                System.Threading.Thread.Sleep(1000);
                Emulators.ClickLB(1280 + 100, 1295);
                System.Threading.Thread.Sleep(1000 * 3);
                DroneController.ScoopDrones();
            }

            //поменять вкладку в инвентаре
            (int XlocInventory, int YlocInventory) = Finders.FindLocWnd("InventoryPrimary");
            Emulators.ClickLB(XlocInventory + 60, YlocInventory + 55);
            System.Threading.Thread.Sleep(500);

            //узнать количество дронов, докнуться если меньше 3-х
            DronesQuantity = Checkers.CheckQuantityDrones();
            if (DronesQuantity < 3)
            {
                SecondaryScripts.DockToStationAndExit();
            }

            //продолжить фармить экспу или аномальку
            (int XBlock, int YBlock) = Finders.FindExpBlock();
            (int XAcGate, int YAcGate) = Finders.FindAccelerationGate();
            if (XBlock != 0 || XAcGate != 0)
            {
                if (XBlock != 0)
                {
                    Emulators.ClickLB(XBlock, YBlock);
                    System.Threading.Thread.Sleep(500);
                    Emulators.ClickLB(2200, 100); //3 button
                    System.Threading.Thread.Sleep(500);
                    Emulators.PressButton((int)WinApi.VirtualKeyShort.VK_F1);
                }
                else if (XAcGate != 0)
                {
                    Emulators.ClickLB(XAcGate, YAcGate);
                    System.Threading.Thread.Sleep(500);
                    Emulators.ClickLB(2200 + 33, 100); //4 button
                    System.Threading.Thread.Sleep(500);
                    Emulators.PressButton((int)WinApi.VirtualKeyShort.VK_F1);
                }
                MainScripts.ClearExpRoom();
                MainScripts.StartClearExp();
            }
            else
            {
                MainScripts.ClearRoom();
            }
        }

        static public void DockToStationAndExit()
        {
            (int XStationCoords, int YStationCoords) = Finders.FindObjectByWordInOverview("Station");
            if (XStationCoords > 0)
            {
                Emulators.ClickLB(XStationCoords, YStationCoords);
                System.Threading.Thread.Sleep(500);
                Emulators.ClickLB(2200, 100); //3 button
                System.Threading.Thread.Sleep(500);
            }
            Environment.Exit(10);
        }

        static public void DockingAndCheckingForSuicides()
        {
            int IsCriminal = 1;
            while (IsCriminal == 1)
            {
                System.Threading.Thread.Sleep(1000 * 60 * 2);
                var ChatWnd = GetUITrees().FindEntityOfString("ChatWindowStack");
                if (ChatWnd == null)
                    return;
                var ChatWndEntry = ChatWnd.handleEntity("ChatWindowStack");
                var Persons = ChatWndEntry.FindEntityOfString("XmppChatSimpleUserEntry");
                if (Persons == null)
                    return;
                var PersonsEntry = Persons.handleEntity("XmppChatSimpleUserEntry");
                IsCriminal = 0;
                for (int i = 0; i < PersonsEntry.children.Length; i++)
                {
                    if (PersonsEntry.children[i] == null)
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
                        IsCriminal = 1;
                        Console.WriteLine("criminal pilot still in system");
                        break;
                    }
                }
            }
            Emulators.ClickLB(2420, 190);
            System.Threading.Thread.Sleep(1000 * 20);//wait for 20 sec after undock
            Emulators.ClickLB(1300, 600);
            System.Threading.Thread.Sleep(1000 * 3);
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
                            System.Threading.Thread.Sleep(20);
                        }
                        Emulators.AllowControlEmulator = false;
                        Emulators.ClickLBForLockTargets(XBlock, YBlock);
                        System.Threading.Thread.Sleep(500);
                        Emulators.ClickLBForLockTargets(2200, 100); //3 button orbit
                        System.Threading.Thread.Sleep(500);
                        Emulators.ClickLBForLockTargets(XBlock, YBlock);
                        System.Threading.Thread.Sleep(500);
                        Emulators.ClickLBForLockTargets(2200, 100); //3 button orbit
                        Emulators.AllowControlEmulator = true;
                    }
                    else if (XAcGate > 0)
                    {
                        while (!Emulators.AllowControlEmulator)
                        {
                            System.Threading.Thread.Sleep(20);
                        }
                        Emulators.AllowControlEmulator = false;
                        Emulators.ClickLBForLockTargets(XAcGate, YAcGate);
                        System.Threading.Thread.Sleep(500);
                        Emulators.ClickLBForLockTargets(2200 + 33, 100); //4 button orbit
                        System.Threading.Thread.Sleep(500);
                        Emulators.ClickLBForLockTargets(XAcGate, YAcGate);
                        System.Threading.Thread.Sleep(500);
                        Emulators.ClickLBForLockTargets(2200 + 33, 100); //4 button orbit
                        Emulators.AllowControlEmulator = true;
                    }
                    else
                    {
                        while (!Emulators.AllowControlEmulator)
                        {
                            System.Threading.Thread.Sleep(20);
                        }
                        Emulators.AllowControlEmulator = false;
                        (int XCont, int YCont) = Finders.FindObjectByWordInOverview("Cargo Container");
                        Emulators.ClickLBForLockTargets(XCont, YCont);
                        System.Threading.Thread.Sleep(500);
                        Emulators.ClickLBForLockTargets(2200 + 33, 100); //4 button orbit
                        System.Threading.Thread.Sleep(500);
                        Emulators.ClickLBForLockTargets(XCont, YCont);
                        System.Threading.Thread.Sleep(500);
                        Emulators.ClickLBForLockTargets(2200 + 33, 100); //4 button orbit
                        Emulators.AllowControlEmulator = true;
                    }
                    return;
                }
                System.Threading.Thread.Sleep(1000 * 8);
            }
        }

        static public void FlyOff()
        {
            while (!Emulators.AllowControlEmulator)
            {
                System.Threading.Thread.Sleep(20);
            }
            Emulators.AllowControlEmulator = false;

            (int XStationOrGate, int YStationOrGate) = Finders.FindObjectByWordInOverview("Station");

            if (XStationOrGate == 0)
            {
                (XStationOrGate, YStationOrGate) = Finders.FindObjectByWordInOverview("Stargate");
            }

            Emulators.ClickLBForLockTargets(XStationOrGate, YStationOrGate);
            System.Threading.Thread.Sleep(500);
            Emulators.ClickLBForLockTargets(2200 - 66, 100); //1 button approach
            System.Threading.Thread.Sleep(500);
            Emulators.ClickLBForLockTargets(XStationOrGate, YStationOrGate);
            System.Threading.Thread.Sleep(500);
            Emulators.ClickLBForLockTargets(2200 - 66, 100); //1 button approach
            Emulators.AllowControlEmulator = true;
        }

        static public bool CheckCargo(int price = 30)
        {
            if (CheckCargoPrice(price))
            {
                return true;
            }
            else if (CheckVolumeCargo())
            {
                return true;
            }
            return false;
        }

        static public bool CheckCargoPrice(int NeedPrice)
        {
            var InventoryPrimary = GetUITrees().FindEntityOfStringByDictEntriesOfInterest("_name", "totalPriceLabel");
            if (InventoryPrimary == null)
            {
                Console.WriteLine("InventoryPrimary and totalPriceLabel not found");
                return false;
            }
            var InventoryPrimaryEntry = InventoryPrimary.handleEntityByDictEntriesOfInterest("_name", "totalPriceLabel");

            try
            {
                var totalPriceLabel = InventoryPrimaryEntry.children[Convert.ToInt32(InventoryPrimaryEntry.dictEntriesOfInterest["needIndex"])]
                .dictEntriesOfInterest;

                if (totalPriceLabel["_name"].ToString() == "totalPriceLabel")
                {
                    var CargoPrice = totalPriceLabel["_setText"].ToString();
                    int PriceValue;
                    int.TryParse(string.Join("", CargoPrice.Where(c => char.IsDigit(c))), out PriceValue);
                    int KK = 1000 * 1000;
                    Console.WriteLine("PriceValue = " + PriceValue + " CargoPrice = " + CargoPrice);
                    if (PriceValue > NeedPrice * KK)
                    {
                        return true;
                    }
                }
            }
            catch
            {
                Console.WriteLine("totalPriceLabel in InventoryPrimary not found");
            }
            return false;
        }

        static public bool CheckVolumeCargo(int NeedVolume = 70)
        {
            var InventoryPrimary = GetUITrees().FindEntityOfStringByDictEntriesOfInterest("_name", "capacityText");
            if (InventoryPrimary == null)
            {
                Console.WriteLine("InventoryPrimary and totalPriceLabel not found");
                return false;
            }
            var InventoryPrimaryEntry = InventoryPrimary.handleEntityByDictEntriesOfInterest("_name", "capacityText");

            try
            {
                var totalPriceLabel = InventoryPrimaryEntry.children[Convert.ToInt32(InventoryPrimaryEntry.dictEntriesOfInterest["needIndex"])]
                .dictEntriesOfInterest;

                if (totalPriceLabel["_name"].ToString() == "capacityText")
                {
                    var CargoVolume = totalPriceLabel["_setText"].ToString();
                    int position = CargoVolume.IndexOf("/");
                    CargoVolume = CargoVolume.Substring(0, position);
                    position = CargoVolume.IndexOf(",");
                    if (position > -1)
                    {
                        CargoVolume = CargoVolume.Substring(0, position);
                    }

                    int Volume;
                    int.TryParse(string.Join("", CargoVolume.Where(c => char.IsDigit(c))), out Volume);
                    Console.WriteLine("Volume = " + Volume);
                    if (Volume > NeedVolume)
                    {
                        return true;
                    }
                }
            }
            catch
            {
                Console.WriteLine("totalPriceLabel in InventoryPrimary not found");
            }
            return false;
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

            Emulators.ClickLB(XStationCoords, YStationCoords);
            System.Threading.Thread.Sleep(500);
            Emulators.ClickLB(2200, 100); //3 button
            Console.WriteLine("wait 1 min for dock");
            System.Threading.Thread.Sleep(1000 * 60);

            (int XlocChatWindowStack, int YlocChatWindowStack) = Finders.FindLocWnd("InventoryPrimary");
            if (XlocChatWindowStack == 0)
            {
                Emulators.ClickLB(2420, 190); // undock
                System.Threading.Thread.Sleep(1000 * 15);
                Emulators.ClickLB(1300, 600);
                System.Threading.Thread.Sleep(1000 * 3);
                return;
            }
            Emulators.ClickRB(XlocChatWindowStack + 170, YlocChatWindowStack + 75);
            System.Threading.Thread.Sleep(500);
            Emulators.ClickLB(XlocChatWindowStack + 170 + 60, YlocChatWindowStack + 75 + 12);
            System.Threading.Thread.Sleep(500);

            Emulators.Drag(XlocChatWindowStack + 210, YlocChatWindowStack + 115, XlocChatWindowStack + 60, YlocChatWindowStack + 125);
            System.Threading.Thread.Sleep(500);

            Emulators.ClickLB(2420, 190); // undock
            System.Threading.Thread.Sleep(1000 * 15);
            Emulators.ClickLB(1300, 600);
            System.Threading.Thread.Sleep(1000 * 3);
            ThreadManager.AllowDocking = false;
        }

        static public void StartLayRoute()
        {
            (int XlocNotepadWindow, int YlocNotepadWindow) = Finders.FindLocWnd("NotepadWindow");
            if (XlocNotepadWindow == 0)
                return;
            int X = 0;
            int Y = 0;
            for (int i = 0; i < 7; i++)
            {
                X = 0;
                for (int j = 0; j < 3; j++)
                {
                    Emulators.ClickRB(XlocNotepadWindow + 140 + X, YlocNotepadWindow + 110 + Y);
                    System.Threading.Thread.Sleep(500);
                    Emulators.ClickLB(XlocNotepadWindow + 140 + 60 + X, YlocNotepadWindow + 110 + 60 + Y);
                    System.Threading.Thread.Sleep(500);
                    X += 85;
                }
                Y += 16;
            }
        }

        static public bool RemoveWaypoint()
        {
            (int XStationOrGate, int YStationOrGate) = Finders.FindObjectByWordInOverview("Station");
            if (XStationOrGate != 0)
            {
                Emulators.ClickRB(XStationOrGate, YStationOrGate);
                System.Threading.Thread.Sleep(500);
                Emulators.ClickLB(XStationOrGate + 60, YStationOrGate + 163);
                System.Threading.Thread.Sleep(1000);
            }
            Console.WriteLine("remove waypoint");
            Emulators.ClickRB(110, 275);
            System.Threading.Thread.Sleep(500);
            Emulators.ClickLB(110 + 70, 275 + 200);
            System.Threading.Thread.Sleep(500);
            var (XGate, _) = MainScripts.GetCoordsNextSystem();
            if (XGate == 0)
            {
                return true;
            }
            return false;
        }

        static public UITreeNode GetUITrees()
        {
            return ReadMemory.GetUITrees(Window.RootAddress, Window.processId);
        }
    }
}
