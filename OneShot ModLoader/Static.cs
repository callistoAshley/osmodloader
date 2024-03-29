﻿using System;
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
        public static readonly string ver = "1.4";

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

        // can't just keep this in GetTerminusFont because it goes out of scope
        private static readonly PrivateFontCollection fontCollection = new PrivateFontCollection();
        public static void AddFonts()
        {
            fontCollection.AddFontFile(fontsPath + "TerminusTTF-Bold.ttf");
        }

        public static Font GetTerminusFont(float size)
        {
            try
            {
                return new Font(fontCollection.Families[0], size, FontStyle.Bold);
            }
            catch (ArgumentException)
            {
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
                Logger.WriteLine("starting oneshot");
                if (File.Exists(baseOneShotPath + "\\steamshim.exe"))
                {
                    Logger.WriteLine("steamshim.exe");
                    Process.Start(baseOneShotPath + "\\steamshim.exe");
                }
                else
                {
                    Logger.WriteLine("oneshot.exe");
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
