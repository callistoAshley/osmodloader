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
        public static void CommonOnComplete()
        {
            Form1.instance.Controls.Clear();
            Form1.instance.InitStartMenu();
        }

        public static async void Apply(object sender, DoWorkEventArgs e)
        {
            try
            {
                // initialization stuff for background worker
                ApplyArgs? args = e.Argument as ApplyArgs?;
                if (args is null)
                    throw new Exception("argument supplied into Apply is not an ApplyArgs structure");
                LoadingBar loadingBar = args.Value.loadingBar;
                BackgroundWorker backgroundWorker = args.Value.backgroundWorker;

                Audio.PlaySound(loadingBar.GetLoadingBGM(), true);

                // if there is just one mod queued, wrap to DirectApply and return
                if (ActiveMods.instance.Nodes.Count == 1)
                {
                    Console.WriteLine("ActiveMods tree only has 1 mod, switching to DirectApply instead");

                    backgroundWorker.DoWork -= Apply;
                    backgroundWorker.DoWork += DirectApply;

                    backgroundWorker.RunWorkerAsync(new DirectApplyArgs(
                        loadingBar, 
                        new DirectoryInfo(Static.directory + "Mods\\" + ActiveMods.instance.Nodes[0].Text),
                        true,
                        ref backgroundWorker
                    ));
                    /*
                    await DirectApply(loadingBar,
                        new DirectoryInfo(Static.directory + "Mods\\" + ActiveMods.instance.Nodes[0].Text),
                        true);
                    */
                    return;
                }

                Console.WriteLine("applying changes");
                await Task.Delay(1);

                List<string> activeMods = new List<string>();

                // TreeNode t in ActiveMods.instance.Nodes
                await loadingBar.SetLoadingStatus("creating temp directory");
                
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
                    await loadingBar.SetLoadingStatus($"mod {t.Index + 1} out of {ActiveMods.instance.Nodes.Count}: {t.Text}");
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
                            await loadingBar.UpdateProgress();
                            if (loadingBar.displayType == LoadingBar.LoadingBarType.Detailed)
                                await loadingBar.SetLoadingStatus(string.Format("mod {0} out of {1}: {2}", t.Index + 1, ActiveMods.instance.Nodes.Count, create));
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
                            await loadingBar.UpdateProgress();
                            if (loadingBar.displayType == LoadingBar.LoadingBarType.Detailed)
                                await loadingBar.SetLoadingStatus(string.Format("mod {0} out of {1}: {2}", t.Index + 1, ActiveMods.instance.Nodes.Count, f.FullName));
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

                CommonOnComplete();
            }
            catch (Exception ex)
            {
                ExceptionMessage.New(ex, true, "OneShot ModLoader will now close.");
                Form1.instance.Close();
            }
        }

        public static async void DirectApply(object sender, DoWorkEventArgs e)
        {
            try
            {
                // args
                DirectApplyArgs? args = e.Argument as DirectApplyArgs?;
                if (args is null)
                    throw new Exception("argument supplied into DirectApply is not a DirectApplyArgs structure");

                LoadingBar loadingBar = args.Value.loadingBar;
                DirectoryInfo mod = args.Value.mod;
                bool uninstallExisting = args.Value.uninstallExisting;

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

                CommonOnComplete();
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

        // structures provided as arguments when the BackgroundWorker is started
        public struct ApplyArgs
        {
            public LoadingBar loadingBar;
            public BackgroundWorker backgroundWorker;

            public ApplyArgs(LoadingBar loadingBar, ref BackgroundWorker backgroundWorker)
            {
                this.loadingBar = loadingBar;
                this.backgroundWorker = backgroundWorker;
            }
        }

        public struct DirectApplyArgs
        {
            public LoadingBar loadingBar;
            public DirectoryInfo mod;
            public bool uninstallExisting;
            public BackgroundWorker backgroundWorker;

            public DirectApplyArgs(LoadingBar loadingBar, DirectoryInfo mod, bool uninstallExisting, ref BackgroundWorker backgroundWorker)
            {
                this.loadingBar = loadingBar;
                this.mod = mod;
                this.uninstallExisting = uninstallExisting;
                this.backgroundWorker = backgroundWorker;
            }
        }
    }
}
