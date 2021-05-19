using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace OneShot_ModLoader
{
    public class ChangesManage
    {
        public static async Task Apply(LoadingBar loadingBar)
        {
            Audio.PlaySound(loadingBar.GetLoadingBGM(), true);

            Console.WriteLine("applying changes");
            await Task.Delay(1);

            List<string> activeMods = new List<string>();

            try
            {
                // TreeNode t in ActiveMods.instance.Nodes
                await loadingBar.SetLoadingStatus("creating temp directory");

                DirectoryInfo tempDir = new DirectoryInfo(Constants.directory + "temp DO NOT OPEN\\MODCOPY\\");
                DirectoryInfo baseOs = new DirectoryInfo(Form1.baseOneShotPath);

                // create the temp directory
                if (tempDir.Exists) tempDir.Delete(true); // just in case it crashed previously
                if (!Directory.Exists(tempDir.FullName)) Directory.CreateDirectory(tempDir.FullName);

                // delete the base os path
                if (baseOs.Exists) baseOs.Delete(true);

                // now we do the cool stuff
                foreach (TreeNode t in ActiveMods.instance.Nodes)
                {
                    await loadingBar.SetLoadingStatus(string.Format("mod {0} out of {1}", t.Index + 1, ActiveMods.instance.Nodes.Count));
                    loadingBar.ResetProgress();

                    activeMods.Add(t.Text);

                    DirectoryInfo mod = new DirectoryInfo(Constants.directory + "Mods\\" + t.Text);

                    // get the files and directories from the mod
                    DirectoryInfo[] directories = mod.GetDirectories("*", SearchOption.AllDirectories);
                    FileInfo[] files = mod.GetFiles("*", SearchOption.AllDirectories);

                    // set the maximum value of the progress bar to the sum of the directories/files
                    loadingBar.progress.Maximum = directories.Length + files.Length;

                    Console.WriteLine("mod {0} out of {1}: {2}", t.Index + 1, ActiveMods.instance.Nodes.Count, mod.FullName);

                    foreach (DirectoryInfo d in directories)
                    {
                        string shorten = Constants.directory + "Mods\\" + t.Text + "\\";
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
                        string shorten = Constants.directory + "Mods\\" + t.Text + "\\";
                        string destination = tempDir.FullName + f.FullName.Replace(shorten, string.Empty);

                        if (!File.Exists(destination))
                        {
                            Console.WriteLine("copying {0} to {1}", f.FullName, destination);
                            File.Copy(f.FullName, destination);

                            // update progress
                            await loadingBar.UpdateProgress();
                            if (loadingBar.displayType == LoadingBar.LoadingBarType.Detailed)
                                await loadingBar.SetLoadingStatus(string.Format("mod {0} out of {1}: {2}", t.Index + 1, ActiveMods.instance.Nodes.Count, f.FullName));
                        }
                    }
                }
                Console.WriteLine("finished up in temp");

                await loadingBar.SetLoadingStatus("final");
                loadingBar.ResetProgress();

                // now we copy everything in temp to the oneshot path

                // get the directories and files
                DirectoryInfo[] directories2 = tempDir.GetDirectories("*", SearchOption.AllDirectories);
                FileInfo[] files2 = tempDir.GetFiles("*", SearchOption.AllDirectories);

                // set the maximum value of the progress bar to the sum of the directories and files
                loadingBar.progress.Maximum = directories2.Length + files2.Length;

                foreach (DirectoryInfo d in directories2)
                {
                    string shorten = Constants.directory + "temp DO NOT OPEN\\MODCOPY\\";
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
                    string shorten = Constants.directory + "temp DO NOT OPEN\\MODCOPY";
                    string destination = baseOs.FullName + f.FullName.Replace(shorten, string.Empty);

                    if (!File.Exists(destination))
                    {
                        Console.WriteLine("copying {0} to {1}", f.FullName, destination);
                        File.Copy(f.FullName, destination);

                        // update progress
                        await loadingBar.UpdateProgress();
                        if (loadingBar.displayType == LoadingBar.LoadingBarType.Detailed)
                            await loadingBar.SetLoadingStatus("final: " + destination);
                    }
                }

                // done!
                await loadingBar.SetLoadingStatus("almost done!");

                Console.WriteLine("finished copying files");

                new DirectoryInfo(Constants.directory + "\\temp DO NOT OPEN").Delete(true);
                Console.WriteLine("successfully deleted temp");

                Console.WriteLine("activeMods.Count " + activeMods.Count);
                // write the active mods to a file
                if (File.Exists(Constants.appDataPath + "activemods.molly"))
                    File.Delete(Constants.appDataPath + "activemods.molly");
                File.WriteAllLines(Constants.appDataPath + "activemods.molly", activeMods);

                Console.Beep();
                MessageBox.Show("All done!");

                loadingBar.text.Dispose();

                Console.WriteLine("finished applying changes");

                Audio.Stop();
            }
            catch (Exception ex)
            {
                string message = "An exception was encountered:\n---------------\n"
                    + ex.Message + "\n---------------\n" + ex.ToString() +
                    "\nOneShot ModLoader will now close.";

                Console.WriteLine(message);
                MessageBox.Show(message);
                Form1.instance.Close();
            }
        }

        public static async Task DirectApply(LoadingBar loadingBar, DirectoryInfo mod, bool uninstallExisting)
        {
            try
            {
                DirectoryInfo baseOs = new DirectoryInfo(Form1.baseOneShotPath);
                if (uninstallExisting && baseOs.Exists)
                    baseOs.Delete(true);
                if (!Directory.Exists(Form1.baseOneShotPath)) Directory.CreateDirectory(Form1.baseOneShotPath);

                string shorten = mod.FullName;

                // first, create the directories
                foreach (DirectoryInfo d in mod.GetDirectories("*", SearchOption.AllDirectories))
                {
                    string newDir = d.FullName.Replace(shorten, string.Empty);
                    //MessageBox.Show(string.Format("base os path: {0}\nnew dir to combine w: {1}", Form1.baseOneShotPath, newDir));
                    if (!Directory.Exists(Form1.baseOneShotPath + newDir))
                    {
                        Directory.CreateDirectory(Form1.baseOneShotPath + newDir);
                        Console.WriteLine("creating directory {0}", Form1.baseOneShotPath + newDir);
                        await loadingBar.SetLoadingStatus(string.Format("creating directory {0}", newDir));
                    }
                }

                // then copy the files
                foreach (FileInfo f in mod.GetFiles("*", SearchOption.AllDirectories))
                {
                    string newLocation = Form1.baseOneShotPath + f.FullName.Replace(shorten, string.Empty);
                    if (!File.Exists(newLocation))
                    {
                        File.Copy(f.FullName, newLocation);
                        Console.WriteLine("copied {0} to {1}", f.FullName, newLocation);
                        await loadingBar.SetLoadingStatus(newLocation);
                    }
                }

                // if the user chose to uninstall any existing mods, we copy over the stuff from the base oneshot path too
                DirectoryInfo cool = new DirectoryInfo(Constants.modsPath + "base oneshot/");
                shorten = cool.FullName;
                if (uninstallExisting)
                {
                    // the directories
                    foreach (DirectoryInfo d in cool.GetDirectories("*", SearchOption.AllDirectories))
                    {
                        string newDir = d.FullName.Replace(shorten, string.Empty);
                        if (!Directory.Exists(Form1.baseOneShotPath + "/" + newDir))
                        {
                            Console.WriteLine("creating directory " + Directory.CreateDirectory(Form1.baseOneShotPath + "/" + newDir).ToString());
                            await loadingBar.SetLoadingStatus(string.Format("final: creating directory {0}", newDir));
                        }
                    }

                    // the files
                    foreach (FileInfo f in cool.GetFiles("*", SearchOption.AllDirectories))
                    {
                        string newLocation = Form1.baseOneShotPath + "/" + f.FullName.Replace(shorten, string.Empty);
                        if (!File.Exists(newLocation))
                        {
                            File.Copy(f.FullName, newLocation);
                            Console.WriteLine("coping {0} to {1}", f.FullName, newLocation);
                            await loadingBar.SetLoadingStatus("final: " + newLocation);
                        }
                    }

                    // finally, write to the active mods file
                    List<string> currentlyActive = File.ReadAllLines(Constants.appDataPath + "activemods.molly").ToList<string>();
                    if (File.Exists(Constants.appDataPath + "activemods.molly")) // first, delete the file if it exists
                        File.Delete(Constants.appDataPath + "activemods.molly");
                    
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
                    File.WriteAllLines(Constants.appDataPath + "activemods.molly", currentlyActive);
                }
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

        public static async Task Apply2(LoadingBar loadingBar)
        {
            Audio.PlaySound(loadingBar.GetLoadingBGM(), true);

            Console.WriteLine("applying changes");
            await Task.Delay(1);

            List<string> activeMods = new List<string>();

            try
            {
                DirectoryInfo baseOs = new DirectoryInfo(Form1.baseOneShotPath);
                if (!baseOs.Exists)
                    baseOs.Create();

                List<string> filesToCache = new List<string>();

                // deal with any files that are no longer being used
                List<string> deletedFiles = new List<string>(); // add deleted files to a list so mod stacking doesn't screw everything over
                foreach (string s in Directory.GetDirectories(Constants.modInfoPath)) // get the mods in the modinfo folder
                {
                    // note to future self: make the above foreach loop work backwards with a for loop instead so it doesn't the uhhhhhhhhhhhhh
                    DirectoryInfo d = new DirectoryInfo(s);

                    // and determine whether they're still active
                    if (!ActiveMods.instance.Nodes.ContainsKey(s))
                    {
                        // directories
                        foreach (string ss in File.ReadAllLines(d.FullName + "\\directories.molly"))
                        {
                            if (Directory.Exists(baseOs.FullName + "\\" + ss))
                            {
                                await loadingBar.SetLoadingStatus("deleting directory: " + baseOs.FullName + "\\" + ss);
                                Console.WriteLine("deleting directory: " + baseOs.FullName + "\\" + ss);

                                // then delete it if it exists in base os
                                //Directory.Delete(baseOs.FullName + "\\" + ss);
                            }   
                        }

                        // files
                        foreach (string ss in File.ReadAllLines(d.FullName + "\\files.molly"))
                        {
                            if (File.Exists(baseOs.FullName + "\\" + ss))
                            {
                                await loadingBar.SetLoadingStatus("deleting file: " + ss);
                                Console.WriteLine("deleting file: " + baseOs.FullName + "\\" + ss);

                                // then delete it if it exists in base os
                                if (!deletedFiles.Contains(ss))
                                {
                                    File.Delete(baseOs.FullName + "\\" + ss);
                                    deletedFiles.Add(ss);
                                }       

                                // check whether the file exists in the cache
                                if (File.Exists(Constants.appDataPath + "cache\\" + ss))
                                {
                                    Console.WriteLine("restoring {0} from cache", ss);
                                    await loadingBar.SetLoadingStatus(string.Format("restoring {0} from cache", ss));

                                    // and if it does, return it to base os
                                    File.Copy(Constants.appDataPath + "cache\\" + ss, baseOs.FullName + "\\" + ss);
                                }
                            }
                        }

                        // finally, delete the modinfo folder
                        d.Delete(true);
                    }
                }

                foreach (TreeNode t in ActiveMods.instance.Nodes)
                {
                    bool isBase = t.Text == "base oneshot"; // just so it doesn't cache itself lol
                    Console.WriteLine("mod {0} out of {1}, isBase {2}", t.Index + 1, ActiveMods.instance.Nodes.Count, isBase);

                    activeMods.Add(t.Text);

                    DirectoryInfo mod = new DirectoryInfo(Constants.directory + "Mods\\" + t.Text);
                    Console.WriteLine("mod {0} out of {1}: {2}", t.Index + 1, ActiveMods.instance.Nodes.Count, mod.FullName);

                    // create the modinfo directory
                    DirectoryInfo modInfoDir = new DirectoryInfo(Constants.modInfoPath + t.Text);

                    if (!modInfoDir.Exists && !isBase)
                        modInfoDir.Create();

                    List<string> directories = new List<string>();
                    List<string> files = new List<string>();

                    foreach (DirectoryInfo d in mod.GetDirectories("*", SearchOption.AllDirectories))
                    {
                        // cut the mod directory out of the string to make the name of the directory to create
                        string shorten = Constants.directory + "Mods\\" + t.Text;
                        string create = d.FullName.Replace(shorten, string.Empty);

                        // add the name of the directory to a list
                        directories.Add(create);

                        Console.WriteLine("creating directory: " + create);
                        await loadingBar.SetLoadingStatus(string.Format("mod {0} out of {1}: {2}", t.Index + 1, ActiveMods.instance.Nodes.Count, create));
                    }

                    // write the directories list to a file
                    if (!isBase)
                        File.WriteAllLines(modInfoDir.FullName + "\\directories.molly", directories);

                    // then do the files
                    foreach (FileInfo f in mod.GetFiles("*", SearchOption.AllDirectories))
                    {
                        // cut the mod directory out of the string to make the name of the directory to copy
                        string shorten = Constants.directory + "Mods\\" + t.Text;
                        string copy = f.FullName.Replace(shorten, string.Empty);

                        // add the name of the file to a list
                        files.Add(copy);

                        if (File.Exists(baseOs.FullName + copy) && !isBase)
                            filesToCache.Add(copy);

                        Console.WriteLine("copying {0} to {1}", f.FullName, baseOs.FullName + copy);
                        await loadingBar.SetLoadingStatus(string.Format("mod {0} out of {1}: {2}", t.Index + 1, ActiveMods.instance.Nodes.Count, copy));

                        File.Copy(f.FullName, baseOs.FullName + copy, true);
                    }

                    // write the files list to a file
                    if (!isBase)
                        File.WriteAllLines(modInfoDir.FullName + "\\files.molly", files);
                }

                Console.WriteLine("finished up with mod stuff");

                // copy the files in the list from base os to the cache folder
                if (!Directory.Exists(Constants.appDataPath + "cache")) // if the cache directory doesn't exist, create it
                    Directory.CreateDirectory(Constants.appDataPath + "cache");

                foreach (string s in filesToCache)
                {
                    // double check if the string contains the name of a directory
                    string s2 = s.Substring(0, s.LastIndexOf("\\"));
                    if (s2 != string.Empty)
                        Console.WriteLine("creating directory: " + Directory.CreateDirectory(Constants.appDataPath + "cache\\" + s2)); // and if it does, create it

                    Console.WriteLine("caching " + s);
                    await loadingBar.SetLoadingStatus("caching " + s);

                    // copy the file to the cache directory
                    File.Copy(Constants.modsPath + "base oneshot\\" + s, Constants.appDataPath + "cache\\" + s);
                }

                // done!
                await loadingBar.SetLoadingStatus("almost done!");

                Console.WriteLine("activeMods.Count " + activeMods.Count);
                // write the active mods to a file
                if (File.Exists(Constants.appDataPath + "activemods.molly"))
                    File.Delete(Constants.appDataPath + "activemods.molly");
                File.WriteAllLines(Constants.appDataPath + "activemods.molly", activeMods);

                Console.Beep();
                MessageBox.Show("All done!");

                loadingBar.text.Dispose();

                Console.WriteLine("finished applying changes (Apply2)");

                Audio.Stop();
            }
            catch (Exception ex)
            {
                new ExceptionMessage(ex, true, "\nOneShot ModLoader will now close.");
                Form1.instance.Close();
            }
        }
    }
}
