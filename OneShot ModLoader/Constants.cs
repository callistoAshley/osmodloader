using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Input;
using System.Windows.Forms;

namespace OneShot_ModLoader
{
    public class Constants
    {
        // yes i know these are readonly not constant but shut up i feel cool ok
        public static readonly string directory = Application.ExecutablePath.Replace(Application.ProductName + ".exe", string.Empty);

        public static readonly string spritesPath = directory + "/Sprites/";
        public static readonly string audioPath = directory + "/Audio/";
        public static readonly string fontsPath = directory + "/Fonts/";
        public static readonly string modsPath = directory + "/Mods/";
        public static readonly string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/OneShotModLoader/";

        public static Font GetTerminusFont(float size)
        {
            PrivateFontCollection f = new PrivateFontCollection();
            f.AddFontFile(fontsPath + "TerminusTTF-Bold.ttf");
            return new Font(f.Families[0], size, FontStyle.Bold);
        }
    }
}
