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

namespace OneShot_ModLoader
{
    public partial class Form1 : Form
    {
        public static Form1 instance;
        public static readonly string modsPath = Directory.GetCurrentDirectory() + "/Mods";
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

            Cool();
        }

        private async void Cool()
        {
            await RefreshMods();
        }

        public async Task RefreshMods ()
        {
            Nodes.Clear();

            // create the mods directory if it doesn't exist
            if (!Directory.Exists(Form1.modsPath))
                Directory.CreateDirectory(Form1.modsPath);
            else if (!Directory.Exists(Form1.modsPath + "/base oneshot") || !File.Exists(
                Constants.appDataPath + "path.molly"))
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
                    if (zip.Extension == ".zip")
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
