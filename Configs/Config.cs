using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace EVE_Bot.Configs
{
    static public class Config
    {
        static Config()
        {
            string json = System.IO.File.ReadAllText("Config.json");
            ConfigFromJson Config = JsonSerializer.Deserialize<ConfigFromJson>(json);
            SpotName = Config.SpotName;
            LockRange = Config.LockRange;
            WeaponsRange = Config.WeaponsRange;
            PropModule = Config.PropModule;
            ActiveTank = Config.ActiveTank;
            NickName = Config.NickName;
            ShipName = Config.ShipName;
            AutopilotMode = Config.AutopilotMode;
            SecondModule = Config.SecondModule;
            AverageDelay = Config.AverageDelay;
            AutoMissile = Config.AutoMissile;
            SpecialUnloadingMode = Config.SpecialUnloadingMode;
            StationForUnload = Config.StationForUnload;
            FarmExp = Config.FarmExp;
            Missiles = Config.Missiles;
            LimiteCargoVolumeForUnload = Config.LimiteCargoVolumeForUnload;
            Console.WriteLine($"SpotName: {SpotName}\nLockRange: {LockRange}\nPropModule: {PropModule}\n" +
                $"ActiveTank: {ActiveTank}\nNickName: {NickName}\nShipName: {ShipName}\n" +
                $"AutopilotMode: {AutopilotMode}\nSecondModule: {SecondModule}\nAverageDelay: {AverageDelay}\n" +
                $"AutoMissile: {AutoMissile}\nSpecialModeOfUnloading: {SpecialUnloadingMode}\nStationForUnload: {StationForUnload}\n" +
                $"FarmExp: {FarmExp}\nWeaponsRange: {WeaponsRange}\nMissiles: {Missiles}\n" +
                $"LimiteCargoVolumeForUnload: {LimiteCargoVolumeForUnload}");
        }
        static public string SpotName { get; set; }
        static public int LockRange { get; set; }
        static public int WeaponsRange { get; set; }
        static public string PropModule { get; set; }
        static public bool ActiveTank { get; set; }
        static public bool AutoMissile { get; set; }
        static public string NickName { get; set; }
        static public string ShipName { get; set; }
        static public bool AutopilotMode { get; set; }
        static public bool SecondModule { get; set; }
        static public int AverageDelay { get; set; }
        static public bool SpecialUnloadingMode { get; set; }
        static public string StationForUnload { get; set; }
        static public bool FarmExp { get; set; }
        static public string Missiles { get; set; }
        static public int LimiteCargoVolumeForUnload { get; set; }
    }
    public class ConfigFromJson
    {
        public string SpotName { get; set; }
        public int LockRange { get; set; }
        public int WeaponsRange { get; set; }
        public string PropModule { get; set; }
        public bool ActiveTank { get; set; }
        public bool AutoMissile { get; set; }
        public string NickName { get; set; }
        public string ShipName { get; set; }
        public bool AutopilotMode { get; set; }
        public bool SecondModule { get; set; }
        public int AverageDelay { get; set; }
        public bool SpecialUnloadingMode { get; set; }
        public string StationForUnload { get; set; }
        public bool FarmExp { get; set; }
        public string Missiles { get; set; }
        public int LimiteCargoVolumeForUnload { get; set; }
    }
}
