using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Drawing.Text;
using System.IO.Compression;

namespace OneShot_ModLoader
{
    public partial class Form1 : Form
    {
        public static Form1 instance;
        public static readonly string modsPath = Directory.GetCurrentDirectory() + "/Mods";
        public static bool debugMode;
        public static string baseOneShotPath;
        public static TextWriter consoleOut = Console.Out;
        public static StreamWriter consoleOutStream;

        public Form1()
        {
            InitializeComponent();
            Init();
        }

        public void Init()
        {
            if (!Directory.Exists(Constants.appDataPath))
                Directory.CreateDirectory(Constants.appDataPath);
            baseOneShotPath = "UNSET";

            // console out stuff
            consoleOutStream = new StreamWriter(Directory.GetCurrentDirectory() + "/output.txt");
            Console.SetOut(consoleOutStream);
            Console.SetError(consoleOutStream);
            if (debugMode)
                MessageBox.Show("debug mode is on");

            // init stuff
            instance = this;
            BackColor = Color.Black;

            InitStartMenu();

            /*
            Controls.Add(new ActiveMods());
            Controls.Add(new InactiveMods());
            Controls.Add(new AddToList());
            Controls.Add(new RemoveFromList());
            Controls.Add(new RefreshMods());
            Controls.Add(new ApplyChanges());
            */
        }

        protected override void OnClosed(EventArgs e)
        {
            Console.SetOut(consoleOut);
            Console.SetError(consoleOut);

            consoleOutStream.Close();
            consoleOut.Close();
        }

        public void InitStartMenu()
        {
            Console.WriteLine("drawing start menu"); 

            // logo
            PictureBox logo = new PictureBox();
            Image image = Image.FromFile(Constants.spritesPath + "logo2.png");
            logo.Image = image;
            logo.Size = image.Size;
            logo.Enabled = true;
            logo.BackColor = Color.Transparent;

            Controls.Add(logo);

            // buttons
            new ModsButton();
            //new BrowseMods();
            new Setup();
        }

        public void InitModsMenu()
        {
            Console.WriteLine("drawing mods menu");
            Controls.Add(new BackButton());
            Controls.Add(new ActiveMods());
            Controls.Add(new RemoveFromList());
            Controls.Add(new AddToList());
            Controls.Add(new ApplyChanges());
            Controls.Add(new InactiveMods());
        }

        public void InitSetupMenu()
        {
            Console.WriteLine("drawing setup menu");

            // first, initialize the instructions
            Label instructions = new Label();
            instructions.Text = "Please click on the textbox below and enter the path to your\nOneShot installation.\nEnsure that you have a clean installation with no mods.";
            instructions.ForeColor = Color.MediumPurple;
            instructions.AutoSize = true;
            instructions.Location = new Point(0, 20);

            PrivateFontCollection f = new PrivateFontCollection();
            f.AddFontFile(Constants.fontsPath + "TerminusTTF-Bold.ttf");
            instructions.Font = new Font(f.Families[0], 12, FontStyle.Bold);

            Controls.Add(instructions);
            Controls.Add(new SetupPrompt());
            Controls.Add(new SetupDone());
            Controls.Add(new BackButton());
        }

        private void Form1_Load(object sender, EventArgs e) {}
    }

    public class InactiveMods : TreeView
    {
        public static InactiveMods instance;
        public InactiveMods()
        {
            instance = this;

            // first, initalize the title
            Label title = new Label();
            title.Text = "Inactive Mods";
            title.ForeColor = Color.MediumPurple;
            title.Font = new Font(title.Font, FontStyle.Bold);
            title.Location = new Point(120, 35);
            title.Size = new Size(100, 15);
            Form1.instance.Controls.Add(title);

            // then the treeview
            Enabled = true;
            Location = new Point(70, 50);
            Size = new Size(176, 175);

            RefreshMods();
        }

        public async void RefreshMods ()
        {
            Nodes.Clear();

            // create the mods directory if it doesn't exist
            if (!Directory.Exists(Form1.modsPath))
                Directory.CreateDirectory(Form1.modsPath);
            else if ((!Directory.Exists(Form1.modsPath + "/base oneshot") || !File.Exists(
                Constants.appDataPath + "path.molly")) && !Form1.debugMode)
            {
                MessageBox.Show("A base oneshot could not be found. Please open the setup page and follow the instructions.");
                Form1.instance.Controls.Clear();
                Form1.instance.InitStartMenu();
                return;
            }

            Form1.baseOneShotPath = File.ReadAllText(Constants.appDataPath + "path.molly");
            Console.WriteLine("oneshot path is " + Form1.baseOneShotPath);

            LoadingBar loadingBar = new LoadingBar();

            // now we extract any existing zip files
            foreach (FileInfo zip in new DirectoryInfo(Form1.modsPath).GetFiles()) 
            {
                await loadingBar.SetLoadingStatus(string.Format("attempting to extract {0},\nplease wait a moment", zip.Name));
                Console.WriteLine("attempting to extract {0}", zip.FullName);
                try
                {
                    ZipFile.ExtractToDirectory(zip.FullName, Form1.modsPath + "/" + zip.Name.Replace(".zip", ""));
                }
                catch (Exception ex)
                {
                    string message = zip.Name + " was detected as a possible zip file,\nbut an exception was encountered while trying to extract it:\n---------------\n"
                    + ex.Message + "\n---------------\n" + ex.ToString() +
                    "\nThis exception will be ignored but trying to use this mod may cause issues.";

                    MessageBox.Show(message);
                }

                // delete the corresponding zip file
                File.Delete(zip.FullName);
            }

            // add the mods to the treeview
            string[] mods = Directory.GetDirectories(Form1.modsPath);
            foreach (string s in mods)
            {
                string s2 = s.Substring(s.LastIndexOf("Mods") + 5);
                if (!ActiveMods.instance.Nodes.ContainsKey(s2) && s2 != "base oneshot")
                    Nodes.Add(s2, s2);
            }

            // try to read from the active mods file and add any of the mods found within to the active mods treeview
            FileInfo activeModsFile = new FileInfo(Constants.appDataPath + "activemods.molly");
            if (activeModsFile.Exists)
            {
                Console.WriteLine("active mods file exists");
                string[] active = File.ReadAllLines(Constants.appDataPath + "activemods.molly");
                foreach (string s in active)
                {
                    Console.WriteLine("found {0} in the active mods file", s);
                    if (Nodes.ContainsKey(s))
                    {
                        Console.WriteLine("removing {0} from InactiveMods and adding it to ActiveMods", s);
                        Nodes.RemoveByKey(s);
                        ActiveMods.instance.ActivateMod(s);
                    }
                }
            }

            loadingBar.text.Dispose();
        }
    }

    public class ActiveMods : TreeView
    {
        public static ActiveMods instance;
        public ActiveMods()
        {
            instance = this;

            // first, initalize the title
            Label title = new Label();
            title.Text = "Active Mods";
            title.ForeColor = Color.MediumPurple;
            title.Font = new Font(title.Font, FontStyle.Bold);
            title.Location = new Point(330, 35);
            title.Size = new Size(100, 15);
            Form1.instance.Controls.Add(title);

            // then the treeview
            Enabled = true;
            Location = new Point(280, 50);
            Size = new Size(176, 175);
            BackgroundImage = Image.FromFile(Constants.spritesPath + "button_box.png");

            // add base
            Nodes.Add("base oneshot");
        }

        public void ActivateMod (string mod)
        {
            List<string> currentActivatedMods = new List<string>();
            // ordering this backwards to make it look tidier
            foreach (TreeNode t in Nodes)
                currentActivatedMods.Add(t.Text);

            Nodes.Clear();
            Nodes.Add(mod);

            // now readd the nodes from the cloned collection
            foreach (string s in currentActivatedMods)
                Nodes.Add(s);
        }
    }

    public class AddToList : Button
    {
        public static AddToList instance;

        public AddToList()
        {
            instance = this;

            Enabled = true;
            Location = new Point(125, 230);
            Size = new Size(50, 50);
            Text = "Add to List";

            PrivateFontCollection f = new PrivateFontCollection();
            f.AddFontFile(Constants.fontsPath + "TerminusTTF-Bold.ttf");
            Font = new Font(f.Families[0], 8, FontStyle.Bold);

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderColor = Color.MediumPurple;
            FlatAppearance.BorderSize = 3;
            ForeColor = Color.MediumPurple;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            Audio.PlaySound("sfx_select.mp3", false);
        }

        protected override void OnClick(EventArgs e)
        {
            if (InactiveMods.instance.SelectedNode != null)
            {
                TreeNode node = InactiveMods.instance.SelectedNode;
                ActiveMods.instance.ActivateMod(node.Text);
                InactiveMods.instance.Nodes.Remove(node);

                Audio.PlaySound("sfx_decision.mp3", false);
            }
        }
    }

    public class RemoveFromList : Button
    {
        public static RemoveFromList instance;
        public RemoveFromList()
        {
            instance = this;

            Enabled = true;
            Location = new Point(230, 230);
            Size = new Size(55, 50);
            Text = "Remove from List";

            PrivateFontCollection f = new PrivateFontCollection();
            f.AddFontFile(Constants.fontsPath + "TerminusTTF-Bold.ttf");
            Font = new Font(f.Families[0], 8, FontStyle.Bold);

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderColor = Color.MediumPurple;
            FlatAppearance.BorderSize = 3;
            ForeColor = Color.MediumPurple;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            Audio.PlaySound("sfx_select.mp3", false);
        }

        protected override void OnClick(EventArgs e)
        {
            if (ActiveMods.instance.SelectedNode != null && ActiveMods.instance.SelectedNode.Text != "base oneshot")
            {
                TreeNode node = ActiveMods.instance.SelectedNode;

                InactiveMods.instance.Nodes.Add((TreeNode)node.Clone());
                ActiveMods.instance.Nodes.Remove(node);

                Audio.PlaySound("sfx_back.mp3", false);
            }
        }
    }

    public class ApplyChanges : Button
    {
        public static ApplyChanges instance;
        public ApplyChanges()
        {
            instance = this;

            Enabled = true;
            Location = new Point(335, 230);
            Size = new Size(65, 50);
            Text = "Apply\nChanges";

            PrivateFontCollection f = new PrivateFontCollection();
            f.AddFontFile(Constants.fontsPath + "TerminusTTF-Bold.ttf");
            Font = new Font(f.Families[0], 8, FontStyle.Bold);

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderColor = Color.MediumPurple;
            FlatAppearance.BorderSize = 3;
            ForeColor = Color.MediumPurple;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            Audio.PlaySound("sfx_select.mp3", false);
        }

        protected override async void OnClick(EventArgs e)
        {
            Form1.instance.Controls.Clear();

            // initialize loading box
            PictureBox pb = new PictureBox();
            pb.Image = Image.FromFile(Constants.spritesPath + "loading.png");
            pb.Size = pb.Image.Size;
            pb.Location = new Point(20, 20);
            Form1.instance.Controls.Add(pb);

            Audio.PlaySound(LoadingBar.GetLoadingBGM(), true);
            await Task.Delay(1);
            try { await Apply(new LoadingBar()); }
            catch { }
            Form1.instance.Controls.Clear();
            Form1.instance.InitStartMenu();
        }

        public async Task Apply (LoadingBar loadingBar)
        {
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
                        await loadingBar.SetLoadingStatus(string.Format("mod {0} out of {1}: {2}", t.Index + 1, ActiveMods.instance.Nodes.Count, s));

                        string mod2 = s.Replace(modDirCut, "");
                        if (!File.Exists(tempPath + mod2))
                            File.Copy(fullModPath + mod2, tempPath + mod2);
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

    public class RefreshMods : Button
    {
        public static RefreshMods instance;
        public RefreshMods()
        {
            instance = this;

            Enabled = true;
            Location = new Point(5, 100);
            Size = new Size(60, 50);
            Text = "Refresh Mods";
        }

        protected override void OnClick(EventArgs e)
        {
            InactiveMods.instance.RefreshMods();
        }
    }
}
