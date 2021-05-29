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
using System.Security.AccessControl;
using System.Windows.Input;
using Microsoft.Win32;

namespace OneShot_ModLoader
{
    public partial class Form1 : Form
    {
        public static Form1 instance;
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
            //Audio.PlaySound("bgm_menu.mp3", true);
            //new TestFormLol();
            if (!Directory.Exists(Constants.appDataPath))
                Directory.CreateDirectory(Constants.appDataPath);

            // init stuff
            instance = this;
            BackColor = Color.Black;

            InitStartMenu();
        }

        protected override void OnClosed(EventArgs e)
        {
            Program.ConsoleToFile();
        }

        public void InitStartMenu()
        {
            Console.WriteLine("drawing start menu");

            // logo
            Image image = Image.FromFile(Constants.spritesPath + "logo2.png");
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
            Controls.Add(new Setup());
            Controls.Add(new DevToolsButton());
            //Controls.Add(new MoveScreen(new Point(390, 225), MoveScreen.Direction.Right));
            //Controls.Add(new MoveScreen(new Point(430, 225), MoveScreen.Direction.Left));

            // secret
            Controls.Add(new CloverSecret());
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

            Cool();
        }

        private async void Cool() => await RefreshMods(); // can't await in ctor lol

        public async Task RefreshMods ()
        {
            StringBuilder sb = new StringBuilder("samuel"); // i can't remember why i made this but i'm scared to remove it in case it'll break everything beyond repair
            Nodes.Clear();

            // create the mods directory if it doesn't exist
            if (!Directory.Exists(Constants.modsPath))
                Directory.CreateDirectory(Constants.modsPath);
            if (!Program.doneSetup)
            {
                MessageBox.Show("A base oneshot could not be found. Please open the setup page and follow the instructions.");
                Form1.instance.Controls.Clear();
                Form1.instance.InitStartMenu();
                return;
            }

            Form1.baseOneShotPath = File.ReadAllText(Constants.appDataPath + "path.molly");
            Console.WriteLine("oneshot path is " + Form1.baseOneShotPath);

            LoadingBar loadingBar = new LoadingBar(Form1.instance);

            // now we extract any existing zip files
            foreach (FileInfo zip in new DirectoryInfo(Constants.modsPath).GetFiles()) 
            {
                await loadingBar.SetLoadingStatus(string.Format("attempting to extract {0},\nplease wait a moment", zip.Name));
                Console.WriteLine("attempting to extract {0}", zip.FullName);
                try
                {
                    if (zip.Extension == ".zip")
                        ZipFile.ExtractToDirectory(zip.FullName, Constants.modsPath + "/" + zip.Name.Replace(".zip", ""));
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
            string[] mods = Directory.GetDirectories(Constants.modsPath);
            foreach (string s in mods)
            {
                string modName = s.Substring(s.LastIndexOf("Mods") + 5); // create the name of the mod to add to the treeview

                // check if the mod is valid. if not, warn the player
                if (!ChangesManage.ConfirmValid(s))
                {
                    Audio.PlaySound("sfx_denied.mp3", false);
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

        public async void ToggleQuickChange()
        {
            if (!quickChange)
            {
                title.Text = "Quick Changes";
                Nodes.Clear();
                await InactiveMods.instance.RefreshMods();
            }
            else
            {
                title.Text = "Active Mods";
                Nodes.Clear();
                RefreshMods();
                await InactiveMods.instance.RefreshMods();
            }
            quickChange = !quickChange;
        }

        public void RefreshMods()
        {
            // try to read from the active mods file and add any of the mods found within to the active mods treeview
            FileInfo activeModsFile = new FileInfo(Constants.appDataPath + "activemods.molly");
            if (activeModsFile.Exists)
            {
                Console.WriteLine("active mods file exists");
                string[] active = File.ReadAllLines(Constants.appDataPath + "activemods.molly");
                foreach (string s in active)
                {
                    Console.WriteLine("found {0} in the active mods file", s);
                    if (InactiveMods.instance.Nodes.ContainsKey(s))
                    {
                        Console.WriteLine("removing {0} from InactiveMods and adding it to ActiveMods", s);
                        InactiveMods.instance.Nodes.RemoveByKey(s);
                        ActivateMod(s);
                    }
                }
            }
        }
    }
}
