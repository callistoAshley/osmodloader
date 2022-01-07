using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;

namespace OneShot_ModLoader.Backend
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

        public static void ActuallyDoStuff(string path)
        {
            try
            {
                // start by creating the loading bar
                LoadingBar loadingBar = new LoadingBar(MainForm.instance);

                // then the background worker
                BackgroundWorker pleaseSpareMyLife = new BackgroundWorker();
                pleaseSpareMyLife.WorkerReportsProgress = true;
                pleaseSpareMyLife.ProgressChanged += loadingBar.ReportProgress;
                pleaseSpareMyLife.DoWork += DoStuff;

                // run
                pleaseSpareMyLife.RunWorkerAsync(new SetupArgs(path, loadingBar));
            }
            catch (Exception ex)
            {
                ExceptionMessage.New(ex, true, "OneShot ModLoader will now close.");

                if (MainForm.instance.InvokeRequired)
                    MainForm.instance.Invoke(new Action(() => MainForm.instance.Close()));
                else
                    MainForm.instance.Close();
            }
        }

        private static void DoStuff(object sender, DoWorkEventArgs e)
        {
            SetupArgs? booga = e.Argument as SetupArgs?;
            if (booga is null) throw new Exception("absolutely no idea how you did that but good work");

            string path = booga.Value.path;
            LoadingBar loadingBar = booga.Value.loadingBar;

            Audio.PlaySound(loadingBar.GetLoadingBGM(), true);

            if (!Verify(path)) // verify the installation
            {
                MessageBox.Show("Could not find a valid OneShot installation at that location. Please double-check for typos in your path.");
                Audio.Stop();
                loadingBar.ReportProgress(sender, new ProgressChangedEventArgs(0, LoadingBar.ProgressType.ReturnToMenu));
                return;
            }

            try
            {
                Logger.WriteLine("setup begin");

                loadingBar.ReportProgress(sender, new ProgressChangedEventArgs(0, "working, please wait a moment"));

                // get directories and files from base os
                DirectoryInfo baseOs = new DirectoryInfo(path);
                DirectoryInfo[] directories = baseOs.GetDirectories("*", SearchOption.AllDirectories);
                FileInfo[] files = baseOs.GetFiles("*", SearchOption.AllDirectories);

                // set the maximum of the loading bar to the length of the directories and files
                loadingBar.ReportProgress(sender, new ProgressChangedEventArgs
                    (directories.Length + files.Length, LoadingBar.ProgressType.SetMaximumProgress));

                // create directories
                loadingBar.ReportProgress(sender, new ProgressChangedEventArgs(0, "setting up directories"));
                foreach (DirectoryInfo d in directories)
                {
                    string create = d.FullName.Replace(path, string.Empty); // create the name of the directory to create

                    // and create it
                    if (!Directory.Exists(Static.modsPath + "/base oneshot/" + create))
                        Directory.CreateDirectory(Static.modsPath + "/base oneshot/" + create);

                    // update loading bar
                    if (loadingBar.displayType == LoadingBar.LoadingBarType.Detailed)
                        loadingBar.ReportProgress(sender, new ProgressChangedEventArgs(0, $"setup: {d.FullName}"));
                    loadingBar.ReportProgress(sender, new ProgressChangedEventArgs(1, LoadingBar.ProgressType.UpdateProgress));

                    //loadingBar.UpdateProgress();
                }

                loadingBar.ReportProgress(sender, new ProgressChangedEventArgs(0, LoadingBar.ProgressType.ResetProgress));

                // copy files
                loadingBar.ReportProgress(sender, new ProgressChangedEventArgs(0, "setting up files"));
                foreach (FileInfo f in files)
                {
                    string copyPath = f.FullName.Replace(path, string.Empty); // create the name of the file to create

                    // and copy it
                    if (!File.Exists(Static.modsPath + "/base oneshot/" + copyPath))
                        f.CopyTo(Static.modsPath + "/base oneshot/" + copyPath, true); // overwrite

                    if (loadingBar.displayType == LoadingBar.LoadingBarType.Detailed)
                        loadingBar.ReportProgress(sender, new ProgressChangedEventArgs(0, $"setup: {f.FullName}"));
                    loadingBar.ReportProgress(sender, new ProgressChangedEventArgs(1, LoadingBar.ProgressType.UpdateProgress));

                    //loadingBar.UpdateProgress();
                }

                #region ---------------OLD---------------
                /*
                // first we need to copy all of the directories
                string[] dirs = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);

                Logger.WriteLine("dirs.Length {0}", dirs.Length);

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

                Logger.WriteLine("files.Length {0}", files.Length);
                for (int i = 0; i < files.Length; i++)
                {
                    string fileName = files[i].Replace(path, "");
                    File.Copy(files[i], finalPath + fileName, true);

                    await loadingBar.SetLoadingStatus("setup: " + fileName);
                }
                */
                #endregion

                loadingBar.ReportProgress(sender, new ProgressChangedEventArgs(0, "almost done"));

                if (File.Exists(Static.appDataPath + "path.molly"))
                    File.Delete(Static.appDataPath + "path.molly");
                File.WriteAllText(Static.appDataPath + "path.molly", path);

                Console.Beep();
                MessageBox.Show("All done!");

                Audio.Stop();

                Program.doneSetup = true;

                // return to menu
                loadingBar.ReportProgress(sender, new ProgressChangedEventArgs(0, LoadingBar.ProgressType.ReturnToMenu));

                // dispose loading bar
                loadingBar.ReportProgress(sender, new ProgressChangedEventArgs(0, LoadingBar.ProgressType.Dispose));
            }
            catch (Exception ex)
            {
                string message = "An exception was encountered:\n" +
                    ex.Message + "\n------------------\n" + ex.ToString() + "\nOneShot ModLoader will now close.";

                MessageBox.Show(message);
                loadingBar.ReportProgress(sender, new ProgressChangedEventArgs(0, LoadingBar.ProgressType.Forcequit));
            }
        }

        private struct SetupArgs
        {
            public string path;
            public LoadingBar loadingBar;

            public SetupArgs(string path, LoadingBar loadingBar)
            {
                this.path = path;
                this.loadingBar = loadingBar;
            }
        }
    }
}
