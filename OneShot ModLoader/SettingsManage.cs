using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using IniParser.Model;
using OneShot_ModLoader.Backend;

namespace OneShot_ModLoader
{
    public static class SettingsManage
    {
        private static IniData config;

        public static void ReadSettingsData()
        {
            string settingsPath = Static.directory + "\\settings.ini";

            if (!File.Exists(settingsPath))
            {
                INIManage.Parse(settingsPath, new string[] { Constants.soundVolume }, new string[] { "100" });
            }
            else
            {
                config = INIManage.Read(settingsPath);
            }
        }

        public static object GetConfigValue(string input)
        {
            return config["config"][input];
        }

        public static class Constants
        {
            public const string soundVolume = "soundVolume";
        }
    }
}
