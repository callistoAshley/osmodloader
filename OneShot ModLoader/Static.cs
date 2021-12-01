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
using System.Diagnostics;

namespace OneShot_ModLoader
{
    public static class Static
    {
        public static readonly string ver = "1.2.dev";

        // cry about it
        public static readonly string directory = Application.ExecutablePath.Replace(Application.ProductName + ".exe", string.Empty);

        public static readonly string spritesPath = directory + "\\Sprites\\";
        public static readonly string audioPath = directory + "\\Audio\\";
        public static readonly string fontsPath = directory + "\\Fonts\\";
        public static readonly string modsPath = directory + "\\Mods\\";
        public static readonly string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\OneShotModLoader\\";
        public static readonly string modInfoPath = appDataPath + "\\modinfo\\";
        public static readonly string tempPath = directory + "\\temp DO NOT OPEN\\";
        public static readonly string miscPath = directory + "\\Misc\\";
        public static string baseOneShotPath;

        public static Font GetTerminusFont(float size)
        {
            try
            {
                PrivateFontCollection f = new PrivateFontCollection();
                f.AddFontFile(fontsPath + "TerminusTTF-Bold.ttf");
                return new Font(f.Families[0], size, FontStyle.Bold);
            }
            catch (ArgumentException)
            {
                MessageBox.Show("caught argument exception");
                return new Font(FontFamily.GenericMonospace, size);
            }
        }
        
        public static DirectoryInfo GetOrCreateTempDirectory()
        {
            if (!Directory.Exists(tempPath))
                return Directory.CreateDirectory(tempPath);

            return new DirectoryInfo(tempPath);
        }

        public static void LaunchOneShot()
        {
            try
            {
                Console.WriteLine("starting oneshot");
                if (File.Exists(baseOneShotPath + "\\steamshim.exe"))
                {
                    Console.WriteLine("steamshim.exe");
                    Process.Start(baseOneShotPath + "\\steamshim.exe");
                }
                else
                {
                    Console.WriteLine("oneshot.exe");
                    Process.Start(baseOneShotPath + "\\oneshot.exe");
                }
            }
            catch (Exception ex)
            {
                ExceptionMessage.New(ex, false);
                MessageBox.Show("Failed to start OneShot.\nThe log may contain more information.");
            }
        }
    }
}
