using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.IO.Compression;

namespace OneShot_ModLoader
{
    public partial class Form1 : Form
    {
        public static Form1 instance;

        public Form1() 
        {
            InitializeComponent();
            Init();
        }

        public void Init()
        {
            if (!Directory.Exists(Static.appDataPath))
                Directory.CreateDirectory(Static.appDataPath);

            // init stuff
            instance = this;
            BackColor = Color.Black;

            InitStartMenu();
        }

        public void InitStartMenu()
        {
            Logger.WriteLine("drawing start menu");

            // logo
            Image image = Image.FromFile(Static.spritesPath + "logo2.png");
            PictureBox logo = new PictureBox
            {
                Image = image,
                Size = image.Size,
                Enabled = true,
                BackColor = Color.Transparent,
            };
            Controls.Add(logo);

            // buttons
            Controls.Add(new ModsButton());
            //new BrowseMods();
            Controls.Add(new SetupButton());
            //Controls.Add(new SettingsButton());
            Controls.Add(new DevToolsButton());
            Controls.Add(new Label
            {
                Text = Static.ver,
                ForeColor = Color.White,
                Size = new Size(50, 50),
                Font = new Font(FontFamily.GenericSansSerif, 8),
            });

            //Controls.Add(new MoveScreen(new Point(390, 225), MoveScreen.Direction.Right));
            //Controls.Add(new MoveScreen(new Point(430, 225), MoveScreen.Direction.Left));

            // secret
            Controls.Add(new CloverSecret());
        }

        public void InitModsMenu()
        {
            Logger.WriteLine("drawing mods menu");
            Controls.Add(new BackButton());
            Controls.Add(new ActiveMods());
            Controls.Add(new RemoveFromList());
            Controls.Add(new AddToList());
            Controls.Add(new ApplyChanges());
            Controls.Add(new InactiveMods());
        }
        
        public void InitSetupMenu()
        {
            Logger.WriteLine("drawing setup menu");

            // first, initialize the instructions
            Label instructions = new Label
            {
                Text = "Please click on the textbox below and enter the path to your\nOneShot installation.\nEnsure that you have a clean installation with no mods.",
                ForeColor = Color.MediumPurple,
                AutoSize = true,
                Location = new Point(0, 20),
                Font = Static.GetTerminusFont(12)
            };

            Controls.Add(instructions);
            Controls.Add(new SetupPrompt());
            Controls.Add(new SetupDone());
            Controls.Add(new BackButton());
        }

        public void ClearControls(bool dispose)
        {
            // y'all like python 
            if (dispose)
                foreach (Control c in Controls)
                    if (c.InvokeRequired)
                        c.Dispose();
            else
                Controls.Clear();
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
            Label title = new Label
            {
                Text = "Inactive Mods",
                ForeColor = Color.MediumPurple,
                Font = new Font(Font, FontStyle.Bold),
                Location = new Point(120, 35),
                Size = new Size(100, 15)
            };
            Form1.instance.Controls.Add(title);

            // then the treeview
            Enabled = true;
            Location = new Point(70, 50);
            Size = new Size(176, 175);

            RefreshMods();
        }

        public void RefreshMods ()
        {
            StringBuilder sb = new StringBuilder("samuel"); // i can't remember why i made this but i'm scared to remove it in case it'll break everything beyond repair
            Nodes.Clear();

            // create the mods directory if it doesn't exist
            if (!Directory.Exists(Static.modsPath))
                Directory.CreateDirectory(Static.modsPath);
            if (!Program.doneSetup)
            {
                MessageBox.Show("A base oneshot could not be found. Please open the setup page and follow the instructions.");
                Form1.instance.Controls.Clear();
                Form1.instance.InitStartMenu();
                return;
            }

            Static.baseOneShotPath = File.ReadAllText(Static.appDataPath + "path.molly");
            Logger.WriteLine("oneshot path is " + Static.baseOneShotPath);

            LoadingBar loadingBar = new LoadingBar(Form1.instance, showProgressBar: false);

            // now we extract any existing zip files
            foreach (FileInfo zip in new DirectoryInfo(Static.modsPath).GetFiles()) 
            {
                loadingBar.SetLoadingStatus(string.Format("attempting to extract {0},\nplease wait a moment", zip.Name));
                Logger.WriteLine($"attempting to extract {zip.FullName}");
                try
                {
                    if (zip.Extension == ".zip")
                        ZipFile.ExtractToDirectory(zip.FullName, Static.modsPath + "/" + zip.Name.Replace(".zip", ""));
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
            string[] mods = Directory.GetDirectories(Static.modsPath);
            foreach (string s in mods)
            {
                string modName = s.Substring(s.LastIndexOf("Mods") + 5); // create the name of the mod to add to the treeview

                // check if the mod is valid. if not, warn the player
                if (!ChangesManage.ConfirmValid(s))
                {
                    Audio.PlaySound("sfx_denied", false);
                    MessageBox.Show($"Could not confirm that {modName} is a valid OneShot mod or add-on." +
                        "\nThis could be because the contents of the mod are not in the root of the directory." +
                        "\nPlease double check that this is the case, and if so, move them." +
                        "\n\nOneShot ModLoader will ignore this just in case and continue as if it were valid, but there are no guarantees it will install correctly.");
                }

                if (!ActiveMods.instance.Nodes.ContainsKey(modName) && modName != "base oneshot")
                    Nodes.Add(modName, modName);
            }

            ActiveMods.instance.RefreshMods();

            loadingBar.text.Dispose();
        }
    }

    public class ActiveMods : TreeView
    {
        public static ActiveMods instance;
        public Label title;
        public bool quickChange;

        public ActiveMods()
        {
            instance = this;

            // first, initalize the title
            title = new Label();
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
            BackgroundImage = Image.FromFile(Static.spritesPath + "button_box.png");

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

            // now read the nodes from the cloned collection
            foreach (string s in currentActivatedMods)
                Nodes.Add(s);
        }

        public void RefreshMods()
        {
            // try to read from the active mods file and add any of the mods found within to the active mods treeview
            FileInfo activeModsFile = new FileInfo(Static.appDataPath + "activemods.molly");
            if (activeModsFile.Exists)
            {
                Logger.WriteLine("active mods file exists");
                string[] active = File.ReadAllLines(Static.appDataPath + "activemods.molly");
                foreach (string s in active)
                {
                    Logger.WriteLine($"found {s} in the active mods file");
                    if (InactiveMods.instance.Nodes.ContainsKey(s))
                    {
                        Logger.WriteLine($"removing {s} from InactiveMods and adding it to ActiveMods");
                        InactiveMods.instance.Nodes.RemoveByKey(s);
                        ActivateMod(s);
                    }
                }
            }
        }
    }
}
