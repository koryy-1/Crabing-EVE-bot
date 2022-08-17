using EVE_Bot.Searchers;
using read_memory_64_bit;
using System;
using System.Collections.Generic;
using System.Text;

namespace EVE_Bot.Parsers
{
    static public class UITreeReader
    {
        static public UITreeNode GetUITrees(string WindowName = null, int initialDepth = 2, bool TakeHigher = false)
        {
            if (WindowName == null)
            {
                return ReadMemory.GetUITrees(Window.RootAddress, Window.processId);
            }
            var UnfinishedUITree = ReadMemory.GetUITrees(Window.RootAddress, Window.processId, initialDepth);

            var UnfinishedWindowTree = UnfinishedUITree.FindEntityOfString(WindowName);
            if (UnfinishedWindowTree == null)
            {
                return null;
            }
            UnfinishedWindowTree = UnfinishedWindowTree.handleEntity(WindowName);

            string WindowAddress = "";
            if (TakeHigher)
            {
                WindowAddress = UnfinishedWindowTree.pythonObjectAddress.ToString();
            }
            else
            {
                WindowAddress = UnfinishedWindowTree.children[Convert.ToInt32(UnfinishedWindowTree.dictEntriesOfInterest["needIndex"])]
                    .pythonObjectAddress.ToString();
            }


            var WindowUITree = ReadMemory.GetUITrees(WindowAddress, Window.processId);

            return WindowUITree;
        }
    }
}
