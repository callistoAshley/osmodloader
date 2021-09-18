using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;

namespace OneShot_ModLoader
{
    public static class ChangesManage
    {
        // i am actually going to cry
        public static void MultithreadStuff(bool directApply)
        {
            // start by creating the loading bar
            LoadingBar loadingBar = new LoadingBar(Form1.instance);

            // then the background worker
            BackgroundWorker pleaseSpareMyLife = new BackgroundWorker();
            pleaseSpareMyLife.DoWork += Apply;
            pleaseSpareMyLife.WorkerReportsProgress = true;
            pleaseSpareMyLife.ProgressChanged += loadingBar.ReportProgress;

            // run
            pleaseSpareMyLife.RunWorkerAsync(new ApplyArgs(pleaseSpareMyLife, loadingBar));
        }

        public static async void Apply(object sender, DoWorkEventArgs e)
        {
            // get the parameter from the event args
            ApplyArgs? bogus = e.Argument as ApplyArgs?;
            if (bogus is null) throw new Exception("absolutely no idea how you did that but good work");
            BackgroundWorker backgroundWorker = bogus.Value.backgroundWorker;
            LoadingBar loadingBar = bogus.Value.loadingBar;

            // start bgm
            Audio.PlaySound(loadingBar.GetLoadingBGM(), true);

            // if there is just one mod queued, wrap to DirectApply and return
            if (ActiveMods.instance.Nodes.Count == 1)
            {
                Console.WriteLine("ActiveMods tree only has 1 mod, switching to DirectApply instead");

                await DirectApply(loadingBar,
                    new DirectoryInfo(Static.directory + "Mods\\" + ActiveMods.instance.Nodes[0].Text),
                    true);
                return;
            }

            Console.WriteLine("applying changes");
            await Task.Delay(1);

            List<string> activeMods = new List<string>();

            try
            {
                // using a string as the progress parameter sets the loading status
                loadingBar.ReportProgress(sender, new ProgressChangedEventArgs(0, "creating temp directory"));
                
                DirectoryInfo tempDir = new DirectoryInfo(Static.GetOrCreateTempDirectory().FullName + "\\MODCOPY\\");
                DirectoryInfo baseOs = new DirectoryInfo(Static.baseOneShotPath);

                // create the temp directory
                if (tempDir.Exists) tempDir.Delete(true); // just in case it crashed previously
                if (!Directory.Exists(tempDir.FullName)) Directory.CreateDirectory(tempDir.FullName);

                // delete the base os path
                File.SetAttributes(Static.baseOneShotPath, FileAttributes.Normal); // set file attributes
                if (baseOs.Exists) baseOs.Delete(true);

                // now we do the cool stuff
                foreach (TreeNode t in ActiveMods.instance.Nodes)
                {
                    loadingBar.ReportProgress(sender, new ProgressChangedEventArgs(0, $"mod {t.Index + 1} out of {ActiveMods.instance.Nodes.Count}: {t.Text}"));
                    loadingBar.ResetProgress();

                    activeMods.Add(t.Text);

                    DirectoryInfo mod = new DirectoryInfo(Static.directory + "Mods\\" + t.Text);

                    // get the files and directories from the mod
                    DirectoryInfo[] directories = mod.GetDirectories("*", SearchOption.AllDirectories);
                    FileInfo[] files = mod.GetFiles("*", SearchOption.AllDirectories);

                    // set the maximum value of the progress bar to the sum of the directories/files
                    loadingBar.progress.Maximum = directories.Length + files.Length;

                    Console.WriteLine("mod {0} out of {1}: {2}", t.Index + 1, ActiveMods.instance.Nodes.Count, mod.FullName);

                    foreach (DirectoryInfo d in directories)
                    {
                        string shorten = Static.directory + "Mods\\" + t.Text + "\\";
                        string create = tempDir.FullName + d.FullName.Replace(shorten, string.Empty); // the full name of the directory to create

                        if (!Directory.Exists(create))
                        {
                            Console.WriteLine("creating directory: " + create);
                            Directory.CreateDirectory(create);

                            // update progress
                            loadingBar.ReportProgress(sender, new ProgressChangedEventArgs(1, $"mod {t.Index + 1} out of {ActiveMods.instance.Nodes.Count}: {create}"));
                        }
                    }
                    
                    foreach (FileInfo f in files)
                    {
                        string shorten = Static.directory + "Mods\\" + t.Text + "\\";
                        string destination = tempDir.FullName + f.FullName.Replace(shorten, string.Empty);

                        if (!File.Exists(destination))
                        {
                            Console.WriteLine("copying {0} to {1}", f.FullName, destination);
                            f.CopyTo(destination, true);

                            // update progress
                            loadingBar.ReportProgress(sender, new ProgressChangedEventArgs(1, $"mod {t.Index + 1} out of {ActiveMods.instance.Nodes.Count}: {f.FullName}"));
                        }
                    }
                }
                Console.WriteLine("finished up in temp");

                await loadingBar.SetLoadingStatus("finalizing, please wait");
                loadingBar.ResetProgress();

                // now we copy everything in temp to the oneshot path

                // get the directories and files
                DirectoryInfo[] directories2 = tempDir.GetDirectories("*", SearchOption.AllDirectories);
                FileInfo[] files2 = tempDir.GetFiles("*", SearchOption.AllDirectories);

                // set the maximum value of the progress bar to the sum of the directories and files
                loadingBar.progress.Maximum = directories2.Length + files2.Length;

                foreach (DirectoryInfo d in directories2)
                {
                    string shorten = Static.directory + "temp DO NOT OPEN\\MODCOPY\\";
                    string create = baseOs.FullName + "\\" + d.FullName.Replace(shorten, string.Empty);

                    if (!Directory.Exists(create))
                    {
                        Console.WriteLine("creating directory: " + create);
                        Directory.CreateDirectory(create);

                        // update progress
                        await loadingBar.UpdateProgress();
                        if (loadingBar.displayType == LoadingBar.LoadingBarType.Detailed)
                            await loadingBar.SetLoadingStatus("final: creating directory: " + create);
                    }
                }

                // and then finally, the files
                foreach (FileInfo f in files2)
                {
                    string shorten = Static.directory + "temp DO NOT OPEN\\MODCOPY";
                    string destination = baseOs.FullName + f.FullName.Replace(shorten, string.Empty);

                    if (!File.Exists(destination))
                    {
                        Console.WriteLine("copying {0} to {1}", f.FullName, destination);
                        f.CopyTo(destination, true);

                        // update progress
                        await loadingBar.UpdateProgress();
                        if (loadingBar.displayType == LoadingBar.LoadingBarType.Detailed)
                            await loadingBar.SetLoadingStatus("final: " + destination);
                    }
                }

                // done!
                await loadingBar.SetLoadingStatus("almost done!");

                Console.WriteLine("finished copying files");

                Static.GetOrCreateTempDirectory().Delete(true);
                Console.WriteLine("successfully deleted temp");

                Console.WriteLine("activeMods.Count " + activeMods.Count);
                // write the active mods to a file
                if (File.Exists(Static.appDataPath + "activemods.molly"))
                    File.Delete(Static.appDataPath + "activemods.molly");
                File.WriteAllLines(Static.appDataPath + "activemods.molly", activeMods);

                Console.Beep();
                MessageBox.Show("All done!");

                loadingBar.Dispose();

                Console.WriteLine("finished applying changes");

                Audio.Stop();
            }
            catch (Exception ex)
            {
                ExceptionMessage.New(ex, true, "OneShot ModLoader will now close.");
                Form1.instance.Close();
            }
        }

        public static async Task DirectApply(LoadingBar loadingBar, DirectoryInfo mod, bool uninstallExisting)
        {
            try
            {
                DirectoryInfo baseOs = new DirectoryInfo(Static.baseOneShotPath);
                if (uninstallExisting && baseOs.Exists)
                    baseOs.Delete(true);
                if (!Directory.Exists(Static.baseOneShotPath)) 
                    Directory.CreateDirectory(Static.baseOneShotPath);

                string shorten = mod.FullName;

                // get the files and directories from the mod
                DirectoryInfo[] directories = mod.GetDirectories("*", SearchOption.AllDirectories);
                FileInfo[] files = mod.GetFiles("*", SearchOption.AllDirectories);

                // set the maximum value of the progress bar to the sum of the directories/files
                loadingBar.progress.Maximum = directories.Length + files.Length;

                await loadingBar.SetLoadingStatus("working, please wait (via direct apply)");

                // first, create the directories
                foreach (DirectoryInfo d in directories)
                {
                    string newDir = d.FullName.Replace(shorten, string.Empty);
                    //MessageBox.Show(string.Format("base os path: {0}\nnew dir to combine w: {1}", Form1.baseOneShotPath, newDir));
                    if (!Directory.Exists(Static.baseOneShotPath + newDir))
                    {
                        Directory.CreateDirectory(Static.baseOneShotPath + newDir);
                        Console.WriteLine("creating directory {0}", Static.baseOneShotPath + newDir);

                        await loadingBar.UpdateProgress();
                        if (loadingBar.displayType == LoadingBar.LoadingBarType.Detailed)
                            await loadingBar.SetLoadingStatus(string.Format("creating directory {0}", newDir));
                    }
                }

                // then copy the files
                foreach (FileInfo f in files)
                {
                    string newLocation = Static.baseOneShotPath + f.FullName.Replace(shorten, string.Empty);
                    if (!File.Exists(newLocation))
                    {
                        File.Copy(f.FullName, newLocation);
                        Console.WriteLine("copied {0} to {1}", f.FullName, newLocation);

                        await loadingBar.UpdateProgress();
                        if (loadingBar.displayType == LoadingBar.LoadingBarType.Detailed)
                            await loadingBar.SetLoadingStatus(newLocation);
                    }
                }

                loadingBar.ResetProgress();

                // if the user chose to uninstall any existing mods, we copy over the stuff from the base oneshot path too
                DirectoryInfo cool = new DirectoryInfo(Static.modsPath + "base oneshot/");
                shorten = cool.FullName;

                // get the files and directories from the mod
                DirectoryInfo[] directories2 = cool.GetDirectories("*", SearchOption.AllDirectories);
                FileInfo[] files2 = cool.GetFiles("*", SearchOption.AllDirectories);

                // set the maximum value of the progress bar to the sum of the directories/files
                loadingBar.progress.Maximum = directories.Length + files.Length;

                if (uninstallExisting)
                {
                    // the directories
                    foreach (DirectoryInfo d in directories2)
                    {
                        string newDir = d.FullName.Replace(shorten, string.Empty);
                        if (!Directory.Exists(Static.baseOneShotPath + "/" + newDir))
                        {
                            Console.WriteLine("creating directory " + Directory.CreateDirectory(Static.baseOneShotPath + "/" + newDir).ToString());

                            await loadingBar.UpdateProgress();
                            if (loadingBar.displayType == LoadingBar.LoadingBarType.Detailed)
                                await loadingBar.SetLoadingStatus(string.Format("final: creating directory {0}", newDir));
                        }
                    }

                    // the files
                    foreach (FileInfo f in files2)
                    {
                        string newLocation = Static.baseOneShotPath + "/" + f.FullName.Replace(shorten, string.Empty);
                        if (!File.Exists(newLocation))
                        {
                            File.Copy(f.FullName, newLocation);
                            Console.WriteLine("coping {0} to {1}", f.FullName, newLocation);

                            await loadingBar.UpdateProgress();
                            if (loadingBar.displayType == LoadingBar.LoadingBarType.Detailed)
                                await loadingBar.SetLoadingStatus("final: " + newLocation);
                        }
                    }

                    // finally, write to the active mods file
                    List<string> currentlyActive = File.ReadAllLines(Static.appDataPath + "activemods.molly").ToList<string>();
                    if (File.Exists(Static.appDataPath + "activemods.molly")) // first, delete the file if it exists
                        File.Delete(Static.appDataPath + "activemods.molly");
                    
                    // then insert the name of the mod at the beginning of a new collection
                    if (!uninstallExisting)
                        currentlyActive.Insert(0, mod.Name);
                    else
                    {
                        currentlyActive = new List<string>
                        {
                            mod.Name,
                            "base oneshot"
                        };
                    }
                    // write the file
                    File.WriteAllLines(Static.appDataPath + "activemods.molly", currentlyActive);
                }

                Console.Beep();
                MessageBox.Show("All done!");

                Audio.Stop();
            }
            catch (Exception ex)
            {
                string message = "An exception was encountered:\n---------------\n"
                    + ex.Message + "\n---------------\n" + ex.ToString() +
                    "\nOneShot ModLoader will now close.";

                Console.WriteLine(message);
                MessageBox.Show(message);

                OCIForm.instance.Close();
            }
        }

        public static bool ConfirmValid(string modPath)
        {
            string[] directoriesToSearch = new string[]
            {
                "/Audio",
                "/Data",
                "/Fonts",
                "/Graphics",
                "/Languages",
                "/Wallpaper"
            };

            foreach (string s in directoriesToSearch)
                if (Directory.Exists(modPath + s)) return true;

            // that didn't work? let's search for some files instead. maybe it's modshot or something
            DirectoryInfo mod = new DirectoryInfo(modPath);
            List<FileInfo> files = new List<FileInfo>();

            // search for .exe and .dll files
            files.AddRange(mod.EnumerateFiles("*.exe"));
            files.AddRange(mod.EnumerateFiles("*.dll"));

            if (files.Count > 0) return true; // awesome! we found something!
            return false; // uh oh, looks like this isn't a mod - or maybe the contents aren't in the root of the directory. we should warn the player
        }

        public struct ApplyArgs
        {
            public BackgroundWorker backgroundWorker;
            public LoadingBar loadingBar;

            public ApplyArgs(BackgroundWorker backgroundWorker, LoadingBar loadingBar)
            {
                this.backgroundWorker = backgroundWorker;
                this.loadingBar = loadingBar;
            }
        }
    }
}
