using System;
using System.Collections.Generic;
using System.Text;

namespace EVE_Bot.Models
{
    public class ModulesInfo
    {
        public Dictionary<string, string> ModuleNamesDict = new Dictionary<string, string>()
        {
            { "ModuleButton_35658", "MWD"}, // 5MN
            { "ModuleButton_35660", "MWD"}, // 50MN
            { "ModuleButton_6005", "MWD"}, // 10MN AB
            { "ModuleButton_8089", "Missile Launcher"}, // Arbalest Light Missile Launcher
            { "ModuleButton_8027", "Missile Launcher"}, // Arbalest Rapid Light Missile Launcher
            { "ModuleButton_8007", "Missile Launcher"}, // Experimental Rapid Light Missile Launcher
            { "ModuleButton_8105", "Missile Launcher"}, // Arbalest Heavy Missile Launcher
            { "ModuleButton_54295", "Thermal Hardener"},
            { "ModuleButton_54294", "Kinetic Hardener"},
            { "ModuleButton_54291", "Multispectrum Hardener"},
        };
        public string MissileLauncher { get; set; } = "Missile Launcher";
        //public string RapidMissileLauncher { get; set; } = "Rapid Missile Launcher";
        //public string HeavyMissileLauncher { get; set; } = "Heavy Missile Launcher";
        public string MWD { get; set; } = "MWD";
        public string AB { get; set; } = "AB";
        public string ThermalHardener { get; set; } = "Thermal Hardener";
        public string KineticHardener { get; set; } = "Kinetic Hardener";
        public string MultispectrumHardener { get; set; } = "Multispectrum Hardener";
    }
}
