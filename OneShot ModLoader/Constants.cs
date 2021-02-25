using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Drawing.Text;

namespace OneShot_ModLoader
{
    public class Constants
    {
        // yes i know these are readonly not constant but shut up i feel cool ok
        public static readonly string spritesPath = Directory.GetCurrentDirectory() + "/Sprites/";
        public static readonly string audioPath = Directory.GetCurrentDirectory() + "/Audio/";
        public static readonly string fontsPath = Directory.GetCurrentDirectory() + "/Fonts/";
        public static readonly string modsPath = Directory.GetCurrentDirectory() + "/Mods/";
        public static readonly string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/OneShotModLoader/";

        public static readonly Color wowPurple = Color.FromArgb(133, 63, 204);
    }
}
