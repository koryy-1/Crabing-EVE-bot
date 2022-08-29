﻿using read_memory_64_bit;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EVE_Bot.Searchers
{
    static public class Window
    {
        static public string RootAddress;
        static public int processId;
        static public IntPtr hWnd;

        static Window()
        {
            GetSysValues();
        }


        static public IntPtr GetSysValues()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            var PIDFromFile = config.AppSettings.Settings["pid"].Value;
            RootAddress = config.AppSettings.Settings["RootAddress"].Value;

            Console.WriteLine("pid in config = {0}", PIDFromFile);
            Console.WriteLine("RootAddress in config = {0}", RootAddress);


            hWnd = WinApi.FindWindow("trinityWindow", null);
            // hWnd = WinApi.FindWindow("trinityWindow", $"EVE - {Config.NickName}");

            if (hWnd.ToInt32() == 0)
            {
                Console.WriteLine("failed to find HWND by window name");
                Environment.Exit(10);
            }
            uint uintprocessId = 0;
            WinApi.GetWindowThreadProcessId(hWnd, out uintprocessId);

            processId = Convert.ToInt32(uintprocessId);

            Console.WriteLine("pid by hWnd {0}", processId);

            if (processId.ToString() != PIDFromFile)
            {
                //string Response = "";
                //for (int i = 0; i < 40; i++) // 20 min
                //{
                //    Response = PostRequestAsync("get root address").Result;
                //    if (Response == "access to find root address allowed")
                //    {
                //        break;
                //    }
                //    Console.WriteLine("wait for 30 sec to send request");
                //    Thread.Sleep(30 * 1000);
                //}
                //if (Response == "access to find root address denied" || Response == "")
                //{
                //    Console.WriteLine("слишком долго стоял в очереди");
                //    Environment.Exit(10);
                //}
                RootAddress = "";

                Console.WriteLine("please wait...");
                RootAddress = GetRootAddress(RootAddress, processId);
                if (0 == RootAddress?.Length)
                {
                    Console.WriteLine("failed to find root address by pid or HWND");
                    Environment.Exit(10);
                }
                config.AppSettings.Settings["pid"].Value = processId.ToString();
                config.AppSettings.Settings["RootAddress"].Value = RootAddress;
                config.Save();
                ConfigurationManager.RefreshSection("appSettings");

                Console.WriteLine("new PID in file = {0}",
                    config.AppSettings.Settings["pid"].Value);
                Console.WriteLine("new RootAddress in file = {0}",
                    config.AppSettings.Settings["RootAddress"].Value);

                //PostRequestAsync("release queue for root address").Wait();
            }
            return hWnd;
        }

        static async Task<string> PostRequestAsync(string message)
        {
            string Response = "";
            WebRequest request = WebRequest.Create("http://localhost:3000");
            request.Method = "POST"; // для отправки используется метод Post
                                     // данные для отправки
            string data = "{ \"NickName\":\"" + Configs.Config.NickName + "\",  \"message\": \"" + message + "\" }";
            // преобразуем данные в массив байтов
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(data);
            // устанавливаем тип содержимого - параметр ContentType
            request.ContentType = "application/x-www-form-urlencoded";
            // Устанавливаем заголовок Content-Length запроса - свойство ContentLength
            request.ContentLength = byteArray.Length;

            //записываем данные в поток запроса
            using (Stream dataStream = request.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
            }

            try
            {
                WebResponse response = await request.GetResponseAsync();

                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        Response = reader.ReadToEnd();
                        Console.WriteLine(Response);

                    }
                }
                response.Close();
                Console.WriteLine("Запрос выполнен...");
                return Response;
            }
            catch (WebException ex)
            {
                if (ex.Response == null) throw;
                Console.WriteLine("Запрос выполнен...");
                return Response;
            }

        }


        static string GetRootAddress(string RootAddress, int processId)
        {

            var (uiRootCandidatesAddresses, memoryReader) = ReadMemory.GetRootAddressesAndMemoryReader(RootAddress, processId);

            IImmutableList<UITreeNode> ReadUITrees() =>
                    uiRootCandidatesAddresses
                    .Select(uiTreeRoot => EveOnline64.ReadUITreeFromAddress(uiTreeRoot, memoryReader, 99))
                    .Where(uiTree => uiTree != null)
                    .ToImmutableList();

            var readUiTreesStopwatch = System.Diagnostics.Stopwatch.StartNew();

            var uiTrees = ReadUITrees();

            readUiTreesStopwatch.Stop();

            var uiTreesWithStats =
                uiTrees
                .Select(uiTree =>
                new
                {
                    uiTree = uiTree,
                    nodeCount = uiTree.EnumerateSelfAndDescendants().Count()
                })
                .OrderByDescending(uiTreeWithStats => uiTreeWithStats.nodeCount)
                .ToImmutableList();

            var uiTreesReport =
                uiTreesWithStats
                .Select(uiTreeWithStats => $"\n0x{uiTreeWithStats.uiTree.pythonObjectAddress:X}: {uiTreeWithStats.nodeCount} nodes.")
                .ToImmutableList();

            //Console.WriteLine($"Read {uiTrees.Count} UI trees in {(int)readUiTreesStopwatch.Elapsed.TotalMilliseconds} milliseconds:" + string.Join("", uiTreesReport));

            var largestUiTree =
                uiTreesWithStats
                .OrderByDescending(uiTreeWithStats => uiTreeWithStats.nodeCount)
                .FirstOrDefault().uiTree;

            if (largestUiTree != null)
            {
                var ReadyRootAdress = largestUiTree.pythonObjectAddress.ToString();
                return ReadyRootAdress;
            }
            else
            {
                Console.WriteLine("No largest UI tree.");
                return "";
            }
        }



        static public (int, int) GetCoordWindow(string WindowName)
        {
            var uiTreeWithPathToWindow = ReadMemory.GetUITrees(RootAddress, processId).FindEntityOfString(WindowName);
            if (uiTreeWithPathToWindow == null)
            {
                Console.WriteLine("failed to find {0}", WindowName);
                return (0, 0);
            }
            var WindowEntry = uiTreeWithPathToWindow.handleEntity(WindowName);
            int XlocWindow = 0;
            int YlocWindow = 0;

            if (WindowEntry.children[Convert.ToInt32(WindowEntry.dictEntriesOfInterest["needIndex"])]
                .dictEntriesOfInterest["_displayX"] is Newtonsoft.Json.Linq.JObject)
            {
                XlocWindow = Convert.ToInt32(WindowEntry.children[Convert.ToInt32(WindowEntry.dictEntriesOfInterest["needIndex"])]
                .dictEntriesOfInterest["_displayX"]["int_low32"]);
            }
            else
                XlocWindow = Convert.ToInt32(WindowEntry.children[Convert.ToInt32(WindowEntry.dictEntriesOfInterest["needIndex"])]
                .dictEntriesOfInterest["_displayX"]);

            if (WindowEntry.children[Convert.ToInt32(WindowEntry.dictEntriesOfInterest["needIndex"])]
                .dictEntriesOfInterest["_displayY"] is Newtonsoft.Json.Linq.JObject)
            {
                YlocWindow = Convert.ToInt32(WindowEntry.children[Convert.ToInt32(WindowEntry.dictEntriesOfInterest["needIndex"])]
                .dictEntriesOfInterest["_displayY"]["int_low32"]);
            }
            else
                YlocWindow = Convert.ToInt32(WindowEntry.children[Convert.ToInt32(WindowEntry.dictEntriesOfInterest["needIndex"])]
                .dictEntriesOfInterest["_displayY"]);

            //Console.WriteLine("X location {0}: {1}", WindowName, XlocWindow);
            //Console.WriteLine("Y location {0}: {1}", WindowName, YlocWindow);
            return (XlocWindow, YlocWindow);
        }

        static public int GetHeightWindow(string WindowName)
        {
            var uiTreeWithPathToWindow = ReadMemory.GetUITrees(RootAddress, processId).FindEntityOfString(WindowName);
            var WindowEntry = uiTreeWithPathToWindow.handleEntity(WindowName);

            var WindowHeight = 0;

            if (WindowEntry.children[Convert.ToInt32(WindowEntry.dictEntriesOfInterest["needIndex"])]
                .dictEntriesOfInterest["_height"] is Newtonsoft.Json.Linq.JObject)
            {
                WindowHeight = Convert.ToInt32(WindowEntry.children[Convert.ToInt32(WindowEntry.dictEntriesOfInterest["needIndex"])]
                .dictEntriesOfInterest["_height"]["int_low32"]);
            }
            else
                WindowHeight = Convert.ToInt32(WindowEntry.children[Convert.ToInt32(WindowEntry.dictEntriesOfInterest["needIndex"])]
                .dictEntriesOfInterest["_height"]);

            //Console.WriteLine("Height of {0}: {1}", WindowName, WindowHeight);
            return WindowHeight;
        }

        static public int GetWidthWindow(string WindowName)
        {
            var TreeViewEntryInventory = ReadMemory.GetUITrees(RootAddress, processId).FindEntityOfString(WindowName).handleEntity(WindowName);

            var WindowWidth = 0;

            if (TreeViewEntryInventory.children[Convert.ToInt32(TreeViewEntryInventory.dictEntriesOfInterest["needIndex"])]
                .dictEntriesOfInterest["_displayWidth"] is Newtonsoft.Json.Linq.JObject)
            {
                WindowWidth = Convert.ToInt32(TreeViewEntryInventory.children[Convert.ToInt32(TreeViewEntryInventory.dictEntriesOfInterest["needIndex"])]
                .dictEntriesOfInterest["_displayWidth"]["int_low32"]);
            }
            else
                WindowWidth = Convert.ToInt32(TreeViewEntryInventory.children[Convert.ToInt32(TreeViewEntryInventory.dictEntriesOfInterest["needIndex"])]
                .dictEntriesOfInterest["_displayWidth"]);

            //Console.WriteLine("Width of {0}: {1}", WindowName, WindowWidth);
            return WindowWidth;
        }

        
    }

    static public class UITreeReader
    {
        static public UITreeNode GetUITrees()
        {
            return ReadMemory.GetUITrees(Window.RootAddress, Window.processId);
        }
    }

    public class SomeWindow
    {
        public int XlocWnd;
        public int YlocWnd;

        public int WidthLeftSidebar;
        public int HeightInventory;

        public SomeWindow(string WindowName)
        {
            (XlocWnd, YlocWnd) = GetCoordWindow(WindowName);

            if (WindowName == "InventoryPrimary")
            {
                WidthLeftSidebar = GetWidthWindow("TreeViewEntryInventoryCargo");
                HeightInventory = GetHeightWindow("InventoryPrimary");
            }
        }

        private (int, int) GetCoordWindow(string WindowName)
        {
            var uiTreeWithPathToWindow = ReadMemory.GetUITrees(Window.RootAddress, Window.processId).FindEntityOfString(WindowName);
            if (uiTreeWithPathToWindow == null)
            {
                Console.WriteLine("failed to find {0}", WindowName);
                return (0, 0);
            }
            var WindowEntry = uiTreeWithPathToWindow.handleEntity(WindowName);
            int XlocWindow = 0;
            int YlocWindow = 0;

            if (WindowEntry.children[Convert.ToInt32(WindowEntry.dictEntriesOfInterest["needIndex"])]
                .dictEntriesOfInterest["_displayX"] is Newtonsoft.Json.Linq.JObject)
            {
                XlocWindow = Convert.ToInt32(WindowEntry.children[Convert.ToInt32(WindowEntry.dictEntriesOfInterest["needIndex"])]
                .dictEntriesOfInterest["_displayX"]["int_low32"]);
            }
            else
                XlocWindow = Convert.ToInt32(WindowEntry.children[Convert.ToInt32(WindowEntry.dictEntriesOfInterest["needIndex"])]
                .dictEntriesOfInterest["_displayX"]);

            if (WindowEntry.children[Convert.ToInt32(WindowEntry.dictEntriesOfInterest["needIndex"])]
                .dictEntriesOfInterest["_displayY"] is Newtonsoft.Json.Linq.JObject)
            {
                YlocWindow = Convert.ToInt32(WindowEntry.children[Convert.ToInt32(WindowEntry.dictEntriesOfInterest["needIndex"])]
                .dictEntriesOfInterest["_displayY"]["int_low32"]);
            }
            else
                YlocWindow = Convert.ToInt32(WindowEntry.children[Convert.ToInt32(WindowEntry.dictEntriesOfInterest["needIndex"])]
                .dictEntriesOfInterest["_displayY"]);

            Console.WriteLine("X location {0}: {1}", WindowName, XlocWindow);
            Console.WriteLine("Y location {0}: {1}", WindowName, YlocWindow);
            return (XlocWindow, YlocWindow);
        }

        private int GetHeightWindow(string WindowName)
        {
            var uiTreeWithPathToWindow = ReadMemory.GetUITrees(Window.RootAddress, Window.processId).FindEntityOfString(WindowName);
            var WindowEntry = uiTreeWithPathToWindow.handleEntity(WindowName);

            var WindowHeight = 0;

            if (WindowEntry.children[Convert.ToInt32(WindowEntry.dictEntriesOfInterest["needIndex"])]
                .dictEntriesOfInterest["_height"] is Newtonsoft.Json.Linq.JObject)
            {
                WindowHeight = Convert.ToInt32(WindowEntry.children[Convert.ToInt32(WindowEntry.dictEntriesOfInterest["needIndex"])]
                .dictEntriesOfInterest["_height"]["int_low32"]);
            }
            else
                WindowHeight = Convert.ToInt32(WindowEntry.children[Convert.ToInt32(WindowEntry.dictEntriesOfInterest["needIndex"])]
                .dictEntriesOfInterest["_height"]);

            Console.WriteLine("Height inventory: {0}", WindowHeight);
            return WindowHeight;
        }

        private int GetWidthWindow(string WindowName)
        {
            var TreeViewEntryInventory = ReadMemory.GetUITrees(Window.RootAddress, Window.processId).FindEntityOfString(WindowName).handleEntity(WindowName);

            var WindowWidth = 0;

            if (TreeViewEntryInventory.children[Convert.ToInt32(TreeViewEntryInventory.dictEntriesOfInterest["needIndex"])]
                .dictEntriesOfInterest["_displayWidth"] is Newtonsoft.Json.Linq.JObject)
            {
                WindowWidth = Convert.ToInt32(TreeViewEntryInventory.children[Convert.ToInt32(TreeViewEntryInventory.dictEntriesOfInterest["needIndex"])]
                .dictEntriesOfInterest["_displayWidth"]["int_low32"]);
            }
            else
                WindowWidth = Convert.ToInt32(TreeViewEntryInventory.children[Convert.ToInt32(TreeViewEntryInventory.dictEntriesOfInterest["needIndex"])]
                .dictEntriesOfInterest["_displayWidth"]);

            Console.WriteLine("Width left sidebar inventory: {0}", WindowWidth);
            return WindowWidth;
        }

        private (int, int) GetBtnLootAll(string WindowName)
        {
            return (XlocWnd + WidthLeftSidebar + 30, YlocWnd + HeightInventory - 20);
        }
    }
}
