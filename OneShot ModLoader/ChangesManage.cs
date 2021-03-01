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
                await loadingBar.SetLoadingStatus("creating temp directory");
                string tempPath = Directory.GetCurrentDirectory() + "/Temp/MODCOPY";

                // first, make the temp folder
                if (!Directory.Exists(tempPath))
                    Directory.CreateDirectory(tempPath);
                if (Directory.Exists(Form1.baseOneShotPath)) // delete the oneshot path if it exists
                    new DirectoryInfo(Form1.baseOneShotPath).Delete(true);

                foreach (TreeNode t in ActiveMods.instance.Nodes)
                {
                    activeMods.Add(t.Text);

                    await loadingBar.SetLoadingStatus(string.Format("mod {0} out of {1}", t.Index + 1, ActiveMods.instance.Nodes.Count));

                    string mod = t.Text;
                    Console.WriteLine("mod name is " + mod);
                    string fullModPath = Form1.modsPath + "/" + mod;
                    Console.WriteLine("mod path {0}", fullModPath);

                    string[] modDirs = Directory.GetDirectories(fullModPath, "*", SearchOption.AllDirectories);
                    Console.WriteLine("modDirs.Length{0}", modDirs.Length);
                    string modDirCut = Directory.GetCurrentDirectory() + "/Mods/" + mod;
                    foreach (string s in modDirs)
                    {
                        await loadingBar.SetLoadingStatus(string.Format("mod {0} out of {1}: {2}", t.Index + 1, ActiveMods.instance.Nodes.Count, s));

                        string mod2 = s.Replace(modDirCut, "");
                        if (!Directory.Exists(tempPath + mod2))
                            Console.WriteLine("--creating directory: " + Directory.CreateDirectory(tempPath + mod2));
                    }

                    // now the files
                    string[] modFiles = Directory.GetFiles(fullModPath, "*", SearchOption.AllDirectories);
                    foreach (string s in modFiles)
                    {
                        string mod2 = s.Replace(modDirCut, "");
                        if (!File.Exists(tempPath + mod2))
                        {
                            await loadingBar.SetLoadingStatus(string.Format("mod {0} out of {1}: {2}", t.Index + 1, ActiveMods.instance.Nodes.Count, s));
                            File.Copy(fullModPath + mod2, tempPath + mod2);
                        }
                    }
                }
                // i use the DirectoryInfo class here and not in the above section because i didn't learn about it until i started writing this section
                // and i didn't feel like changing it
                // i still don't

                await loadingBar.SetLoadingStatus("finished up in temp");

                Console.WriteLine("finished up in temp");

                // get the directories in temp
                string dir = Directory.GetCurrentDirectory() + "\\Temp\\MODCOPY";
                DirectoryInfo tempDirs = new DirectoryInfo(dir);
                foreach (DirectoryInfo d in tempDirs.GetDirectories("*", SearchOption.AllDirectories))
                {
                    // first, cut off the start of the directory
                    string dShortName = d.FullName;
                    dShortName = dShortName.Replace(dir, "");

                    // make the path using the shortened directory name
                    string fullCopyPath = Form1.baseOneShotPath + dShortName;
                    Console.WriteLine("fullCopyPath is " + fullCopyPath);

                    await loadingBar.SetLoadingStatus(string.Format("final {0}", fullCopyPath));

                    // finally, if the directory doesn't exist, create it
                    if (!Directory.Exists(fullCopyPath))
                        Console.WriteLine("-creating directory: " + Directory.CreateDirectory(fullCopyPath));
                }

                Console.WriteLine("finished creating directories");

                // now we move the files
                foreach (FileInfo f in tempDirs.GetFiles("*", SearchOption.AllDirectories))
                {
                    string fullCopyPath = Form1.baseOneShotPath + f.FullName.Replace(dir, "");

                    await loadingBar.SetLoadingStatus(string.Format("final {0}", fullCopyPath));

                    // again, cut off the start of the directory
                    File.Copy(f.FullName, fullCopyPath, true);
                    Console.WriteLine("-copied {0} to {1}", f.FullName, fullCopyPath);
                }

                await loadingBar.SetLoadingStatus(loadingBar.text.Text = "almost done!");

                Console.WriteLine("finished copying files");

                new DirectoryInfo(Directory.GetCurrentDirectory() + "/Temp").Delete(true);
                Console.WriteLine("successfully deleted temp");

                Console.Beep();
                MessageBox.Show("All done!");

                loadingBar.text.Dispose();

                Console.WriteLine("finished applying changes");

                Console.WriteLine("activeMods.Count " + activeMods.Count);

                // write the active mods to a file
                if (File.Exists(Constants.appDataPath + "activemods.molly"))
                    File.Delete(Constants.appDataPath + "activemods.molly");
                File.WriteAllLines(Constants.appDataPath + "activemods.molly", activeMods);

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
    }
}
