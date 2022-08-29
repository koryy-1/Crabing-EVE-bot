﻿using System;
using System.Collections.Generic;
using System.Text;

namespace EVE_Bot.Models
{
    public class InventoryItem
    {
        public InventoryItem()
        {
            Pos = new Point();
        }
        public string Name { get; set; }
        public Point Pos { get; set; }
        public int Amount { get; set; } = 1;
    }
}
