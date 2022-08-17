using System;
using System.Collections.Generic;
using System.Text;

namespace EVE_Bot.Models
{
    public class ModulesInfo
    {
        public Dictionary<string, string> ModuleNamesDict = new Dictionary<string, string>()
        {
            { "ModuleButton_35658", "MWD"},
            { "ModuleButton_8089", "Missile Launcher"},
            { "module ...", "Adaptive"}
        };
        public string MissileLauncher { get; set; } = "Missile Launcher";
        public string MWD { get; set; } = "MWD";
        public string Adaptive { get; set; } = "Adaptive";
    }
}
