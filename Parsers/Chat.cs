﻿using EVE_Bot.Models;
using EVE_Bot.Searchers;
using read_memory_64_bit;
using System;
using System.Collections.Generic;
using System.Text;

namespace EVE_Bot.Parsers
{
    static public class Chat
    {
        static public List<ChatPlayer> GetInfo()
        {
            var Persons = GetUITrees().FindEntityOfString("XmppChatSimpleUserEntry");
            if (Persons == null)
                return null;
            var PersonsEntry = Persons.handleEntity("XmppChatSimpleUserEntry");

            List<ChatPlayer> ChatInfo = new List<ChatPlayer>();
            for (int i = 0; i < PersonsEntry.children.Length; i++)
            {
                if (PersonsEntry.children[i] == null)
                    continue;
                if (PersonsEntry.children[i].children == null)
                    continue;
                if (PersonsEntry.children[i].children.Length < 3)
                    continue;
                if (PersonsEntry.children[i].children[2] == null)
                    continue;
                if (PersonsEntry.children[i].children[2].children == null)
                    continue;
                if (PersonsEntry.children[i].children[2].children.Length == 0)
                    continue;

                if (PersonsEntry.children[i].children[2].children[0].pythonObjectTypeName != "FlagIconWithState")
                    continue;

                ChatPlayer ChatPlayerInfo = new ChatPlayer();

                ChatPlayerInfo.PlayerType = PersonsEntry.children[i].children[2].children[0]
                .dictEntriesOfInterest["_hint"].ToString();

                ChatInfo.Add(ChatPlayerInfo);
            }
            return ChatInfo;
            //Pilot is a criminal
            //Pilot is a suspect
            //FlagIconWithState
        }
        static public UITreeNode GetUITrees()
        {
            return ReadMemory.GetUITrees(Window.RootAddress, Window.processId);
        }
    }
}
