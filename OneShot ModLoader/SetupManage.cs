﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace OneShot_ModLoader
{
    internal class Yo
    {
        public bool Verify(string path)
        {
            string[] check =
            {
                "oneshot.exe",
                "_______.exe"
            };

            int amountFound = 0;
            foreach (string s in check)
                if (File.Exists(path + "/" + s)) amountFound++;

            return amountFound == check.Length;
        }
    }
    public class SetupManage
    {
        public static async Task DoStuff(string path)
        {
            LoadingBar loadingBar = new LoadingBar();
            Audio.PlaySound(loadingBar.GetLoadingBGM(), true);

            if (!new Yo().Verify(path)) // verify the installation
            {
                MessageBox.Show("Could not find a valid OneShot installation at that location. Please double-check for typos in your path.");
                Audio.Stop();
                Form1.instance.Controls.Clear();
                Form1.instance.InitSetupMenu();
                return;
            }

            try
            {
                Console.WriteLine("setup begin");

                await loadingBar.SetLoadingStatus("working, please wait a moment");

                // first we need to copy all of the directories
                string[] dirs = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);

                Console.WriteLine("dirs.Length {0}", dirs.Length);

                string shortDirCut = path; // shorten the directories ready to be cloned
                string[] shortDirs = dirs;
                for (int i = 0; i < shortDirs.Length; i++)
                    shortDirs[i] = shortDirs[i].Replace(shortDirCut, "");

                // create the new directory
                foreach (string s in shortDirs)
                {
                    await loadingBar.SetLoadingStatus("setup: " + s);

                    if (!Directory.Exists(Constants.modsPath + s))
                        Directory.CreateDirectory(Constants.modsPath + "base oneshot/" + s);
                }

                // finally, copy the files to the new location
                string[] files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
                string finalPath = Constants.modsPath + "base oneshot";

                Console.WriteLine("files.Length {0}", files.Length);
                for (int i = 0; i < files.Length; i++)
                {
                    string fileName = files[i].Replace(path, "");
                    File.Copy(files[i], finalPath + fileName, true);

                    await loadingBar.SetLoadingStatus("setup: " + fileName);
                }

                if (File.Exists(Constants.appDataPath + "path.molly"))
                    File.Delete(Constants.appDataPath + "path.molly");
                File.WriteAllText(Constants.appDataPath + "path.molly", path);

                await loadingBar.SetLoadingStatus("almost done!");

                Console.Beep();
                MessageBox.Show("All done!");

                Audio.Stop();

                Form1.instance.Controls.Clear();
                Form1.instance.InitStartMenu();
            }
            catch (Exception ex)
            {
                string message = "An exception was encountered:\n" +
                    ex.Message + "\n------------------\n" + ex.ToString() + "\nOneShot ModLoader will now close.";

                MessageBox.Show(message);
                Form1.instance.Close();
            }
        }
    }
}