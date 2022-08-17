﻿using EVE_Bot.Models;
using EVE_Bot.Searchers;
using read_memory_64_bit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EVE_Bot.Parsers
{
    static public class NP
    {
        static public List<NotepadItem> GetInfo()
        {
            var (XlocOverview, YlocOverview) = Finders.FindLocWnd("NotepadWindow");

            var NotepadWindow = UITreeReader.GetUITrees("NotepadWindow");
            if (NotepadWindow == null)
                return null;

            var SE_EditTextlineCore = NotepadWindow.FindEntityOfString("SE_EditTextlineCore");
            if (SE_EditTextlineCore == null)
                return null;

            var SE_EditTextlineCoreEntry = SE_EditTextlineCore.handleEntity("SE_EditTextlineCore");

            int LeftSidebarWidth = Window.GetWidthWindow("ListGroup");

            List<NotepadItem> NotepadInfo = new List<NotepadItem>();

            for (int i = 0; i < SE_EditTextlineCoreEntry.children.Length; i++)
            {
                if (SE_EditTextlineCoreEntry.children[i] == null)
                    continue;
                if (SE_EditTextlineCoreEntry.children[i].children == null)
                    continue;
                if (SE_EditTextlineCoreEntry.children[i].children.Length < 2)
                    continue;
                if (SE_EditTextlineCoreEntry.children[i].children[1] == null)
                    continue;
                if (SE_EditTextlineCoreEntry.children[i].children[1].children == null)
                    continue;

                int YItem = 0;
                if (SE_EditTextlineCoreEntry.children[i].dictEntriesOfInterest["_displayY"] is Newtonsoft.Json.Linq.JObject)
                    YItem = Convert.ToInt32(SE_EditTextlineCoreEntry.children[i].dictEntriesOfInterest["_displayY"]["int_low32"]);
                else
                    YItem = Convert.ToInt32(SE_EditTextlineCoreEntry.children[i].dictEntriesOfInterest["_displayY"]);

                if (i == 0)
                {
                    YItem = YItem + 5;
                }

                for (int j = 0; j < SE_EditTextlineCoreEntry.children[i].children[1].children.Length; j++)
                {
                    NotepadItem NotepadItemInfo = new NotepadItem();

                    int XItem = 0;
                    if (SE_EditTextlineCoreEntry.children[i].children[1].children[j].dictEntriesOfInterest["_displayX"] is Newtonsoft.Json.Linq.JObject)
                        XItem = Convert.ToInt32(SE_EditTextlineCoreEntry.children[i].children[1].children[j]
                            .dictEntriesOfInterest["_displayX"]["int_low32"]);
                    else
                        XItem = Convert.ToInt32(SE_EditTextlineCoreEntry.children[i].children[1].children[j]
                            .dictEntriesOfInterest["_displayX"]);

                    NotepadItemInfo.Pos.x = XlocOverview + LeftSidebarWidth + 10 + XItem + 10;
                    NotepadItemInfo.Pos.y = YlocOverview + YItem + 110;

                    NotepadInfo.Add(NotepadItemInfo);
                }
            }

            return NotepadInfo;
        }
    }
}
