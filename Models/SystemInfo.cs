using System;
using System.Collections.Generic;
using System.Text;

namespace EVE_Bot.Models
{
    public class SystemInfo
    {
        public SystemInfo()
        {
            Colors = new Colors();
        }
        //public string IconType { get; set; } // waypoint / station / endpoint
        public Colors Colors { get; set; }
        //public string SystemName { get; set; }
    }
}
