using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace OneShot_ModLoader
{
    public class DTUnpiracyButton : Button
    {
        private LoadingBar formLoadingBar;
        private string modPath;

        public DTUnpiracyButton()
        {
            Location = new Point(10, 60);
            Size = new Size(150, 20);
            BackColor = Color.White;
            Text = "Compare mod to base";

            Focus();
        }

        protected override void OnClick(EventArgs e)
        {
            Console.WriteLine("doin the unpiracyifier");
            DevToolsForm.instance.Controls.Clear();

            // bring up a folder browser to browse to the mod's path
            if (Program.doneSetup)
            {
                using (FolderBrowserDialog browse = new FolderBrowserDialog())
                {
                    browse.Description = "Please navigate to your mod's path.";
                    browse.ShowDialog();

                    if (browse.SelectedPath != string.Empty)
                    {
                        try
                        {
                            modPath = browse.SelectedPath;
                            CompareToBaseOS(modPath);
                        }
                        catch (Exception ex)
                        {
                            ExceptionMessage.New(ex, true);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("A base oneshot could not be found. Please open the setup page and follow the instructions.");
                DevToolsForm.instance.Init();
            }
        }

        // the FileInfo to string converter in CompareToBaseOs refers to this method
        private string FileInfoToString(FileInfo file) => file.FullName;

        public async void CompareToBaseOS(string modPath)
        {
            try
            {
                DirectoryInfo mod = new DirectoryInfo(modPath);
                FileInfo[] modFiles = mod.GetFiles("*", SearchOption.AllDirectories); // get files
                List<FileInfo> matches = new List<FileInfo>();

                formLoadingBar = new LoadingBar(DevToolsForm.instance);

                formLoadingBar.progress.Maximum = modFiles.Length; // set maximum progress

                foreach (FileInfo f in modFiles) 
                {
                    // compare each file
                    Console.WriteLine("comparing file to base os: " + f.FullName);

                    string fileName = Static.baseOneShotPath + f.FullName.Replace(modPath, string.Empty);

                    // if a file of the same name exists in base oneshot, add it to the matches list
                    if (File.Exists(fileName)
                        && File.ReadAllBytes(fileName) == File.ReadAllBytes(f.FullName))
                    {
                        Console.WriteLine("unchanged file found! " + f.FullName);
                        matches.Add(f);
                    }
                    await formLoadingBar.UpdateProgress();
                }

                // make string array from file matches
                List<string> matchNames = new List<string>{ "CLOSE THIS FILE BEFORE TAKING ANY FURTHER ACTION", string.Empty };
                // convert all of the files in the matches list to their string values by creating a converter 
                // that refers to the FileInfoToString method
                matchNames.AddRange(matches.ConvertAll<string>(new Converter<FileInfo, string>(FileInfoToString)));

                Console.WriteLine("all done! creating dialog");
                await formLoadingBar.SetLoadingStatus("done!");

                // create temp directory
                Console.WriteLine("also creating temp directory lol");
                File.WriteAllLines(Static.GetOrCreateTempDirectory().FullName + "\\file matches.txt", matchNames);

                // then create dialog form
                Console.Beep();
                Form dialog = new Form
                {
                    Text = "All done!",
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                };

                Label text = new Label
                {
                    AutoSize = true,
                    Text = $"OneShot ModLoader found {matchNames.Count} unchanged files. What would you like to do?"
                };

                // initialize buttons
                Button deleteFiles = new Button
                {
                    Text = "Delete",
                };
                Button moveFiles = new Button
                {
                    Text = "Move to ModName-Unchanged\\",
                };
                Button listFiles = new Button
                {
                    Text = "List",
                };

                // hook events to the associated methods
                deleteFiles.Click += DeleteFiles;
                moveFiles.Click += MoveFiles;
                listFiles.Click += ListFiles;

                // add controls
                dialog.Controls.Add(text);
                dialog.Controls.Add(deleteFiles);
                dialog.Controls.Add(moveFiles);
                dialog.Controls.Add(listFiles);

                dialog.ShowDialog(DevToolsForm.instance);
            }
            catch (Exception ex)
            {
                ExceptionMessage.New(ex, true);
            }
        }

        public async void DeleteFiles(object sender, EventArgs e)
        {
            Console.WriteLine("deleting unchanged files");

            try
            {
                string file = Static.GetOrCreateTempDirectory().FullName + "\\file matches.txt";

                // reset loading bar progress
                formLoadingBar.ResetProgress();
                await formLoadingBar.SetLoadingStatus("deleting unchanged files");

                // get files
                string[] files = File.ReadAllLines(Static.GetOrCreateTempDirectory().FullName + "\\file matches.txt");
                formLoadingBar.progress.Maximum = files.Length; // set maximum progress

                foreach (string s in files)
                {
                    Console.WriteLine($"deleting {s}");
                    File.Delete(s);
                    await formLoadingBar.UpdateProgress();
                }

                // done!
                Console.WriteLine("done!");
                MessageBox.Show("All done!");

                // clear controls
                DevToolsForm.instance.Controls.Clear();
                DevToolsForm.instance.Init();
            }
            catch (Exception ex)
            {
                ExceptionMessage.New(ex, true);
            }
        }

        public async void MoveFiles(object sender, EventArgs e)
        {
            Console.WriteLine("moving unchanged files");

            try
            {
                string file = Static.GetOrCreateTempDirectory().FullName + "\\file matches.txt";

                // reset loading bar progress
                formLoadingBar.ResetProgress();
                await formLoadingBar.SetLoadingStatus("moving unchanged files");

                // get files
                string[] files = File.ReadAllLines(Static.GetOrCreateTempDirectory().FullName + "\\file matches.txt");
                formLoadingBar.progress.Maximum = files.Length; // set maximum progress

                // get modPath's parent directory by initializing a DirectoryInfo and getting the parent's full name
                DirectoryInfo mod = new DirectoryInfo(modPath);
                string parent = mod.Parent.FullName;

                string moveFilesHere = string.Empty;

                // not commenting this lmao
                for (int i = 0; ; i++)
                {
                    string dir = parent + $"\\{mod.Name}-Unchanged{i}\\";
                    if (!Directory.Exists(dir))
                    {
                        moveFilesHere = dir;
                        Console.WriteLine($"creating directory: {dir}");
                        Directory.CreateDirectory(dir);
                        break;
                    }
                }

                foreach (string s in files)
                {
                    string moveTo = moveFilesHere + $"\\{s.Replace(modPath, string.Empty)}";
                    Console.WriteLine($"moving {s} to {moveTo}");
                    File.Move(s, moveTo);

                    await formLoadingBar.UpdateProgress();
                }

                // done!
                Console.WriteLine("done!");
                MessageBox.Show("All done!");

                // clear controls
                DevToolsForm.instance.Controls.Clear();
                DevToolsForm.instance.Init();
            }
            catch (Exception ex)
            {
                ExceptionMessage.New(ex, true);
            }
        }

        public void ListFiles(object sender, EventArgs e) 
            => Process.Start(Static.GetOrCreateTempDirectory().FullName + "\\file matches.txt");
    }
}
