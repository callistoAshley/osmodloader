using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace OneShot_ModLoader
{
    public static class SetupManage
    {
        public static bool Verify(string path)
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

        public static async Task DoStuff(string path)
        {
            LoadingBar loadingBar = new LoadingBar(Form1.instance);
            Audio.PlaySound(loadingBar.GetLoadingBGM(), true);

            if (!Verify(path)) // verify the installation
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

                loadingBar.SetLoadingStatus("working, please wait a moment");

                // get directories and files from base os
                DirectoryInfo baseOs = new DirectoryInfo(path);
                DirectoryInfo[] directories = baseOs.GetDirectories("*", SearchOption.AllDirectories);
                FileInfo[] files = baseOs.GetFiles("*", SearchOption.AllDirectories);

                loadingBar.progress.Maximum = directories.Length + files.Length; // set the maximum of the loading bar to the length of the directories and files

                // create directories
                loadingBar.SetLoadingStatus("setting up directories");
                foreach (DirectoryInfo d in directories)
                {
                    string create = d.FullName.Replace(path, string.Empty); // create the name of the directory to create

                    // and create it
                    if (!Directory.Exists(Static.modsPath + "/base oneshot/" + create))
                        Directory.CreateDirectory(Static.modsPath + "/base oneshot/" + create);

                    // update loading bar
                    if (loadingBar.displayType == LoadingBar.LoadingBarType.Detailed)
                        loadingBar.SetLoadingStatus("setup: " + d.FullName);

                    //loadingBar.UpdateProgress();
                }

                // copy files
                loadingBar.SetLoadingStatus("setting up files");
                foreach (FileInfo f in files)
                {
                    string copyPath = f.FullName.Replace(path, string.Empty); // create the name of the file to create

                    // and copy it
                    if (!File.Exists(Static.modsPath + "/base oneshot/" + copyPath))
                        f.CopyTo(Static.modsPath + "/base oneshot/" + copyPath, true); // overwrite

                    if (loadingBar.displayType == LoadingBar.LoadingBarType.Detailed)
                        loadingBar.SetLoadingStatus("setup: " + f.FullName);

                    //loadingBar.UpdateProgress();
                }

                #region ---------------OLD---------------
                /*
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
                */
                #endregion

                loadingBar.SetLoadingStatus("almost done!");

                if (File.Exists(Static.appDataPath + "path.molly"))
                    File.Delete(Static.appDataPath + "path.molly");
                File.WriteAllText(Static.appDataPath + "path.molly", path);

                // dispose loading bar
                loadingBar.Dispose();

                Console.Beep();
                MessageBox.Show("All done!");

                Audio.Stop();

                Program.doneSetup = true;

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
