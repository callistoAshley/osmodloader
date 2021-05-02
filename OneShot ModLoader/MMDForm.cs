using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using IniParser.Model;

namespace OneShot_ModLoader
{
    public class MMDForm : Form 
    {
        public static MMDForm instance;

        public static TextBox displayNameInstance;
        public static TextBox authorInstance;
        public static TextBox versionInstance;
        public static TextBox descriptionInstance;
        public static ModIcon icon;

        public static string modPath;

        public MMDForm(string path)
        {
            Console.WriteLine("initalized mod metadata form with path: " + path);
            modPath = path;

            instance = this;

            Size = new Size(400, 300);
            Text = "Mod Metadata: " + modPath.Replace(modPath.Substring(0, modPath.LastIndexOf("\\") + 1), string.Empty);
            BackgroundImage = Image.FromFile(Constants.spritesPath + "bg.png");
            BackgroundImageLayout = ImageLayout.Tile;
            BackColor = Color.Black;
            Icon = new Icon(Constants.spritesPath + "devtools.ico");

            FormBorderStyle = FormBorderStyle.FixedSingle; 
            Show();

            Init();
        }

        public void Init()
        {
            TextBox displayName = new TextBox();
            displayName.Text = "Display Name";
            displayName.Location = new Point(10, 10);
            displayName.MaxLength = 20;

            TextBox author = new TextBox();
            author.Text = "Author";
            author.Location = new Point(10, 30);
            author.MaxLength = 15;

            TextBox version = new TextBox();
            version.Text = "Version";
            version.Location = new Point(10, 50);
            version.MaxLength = 10;

            TextBox description = new TextBox();
            description.Text = "Short Description - Max: 50 char";
            description.Location = new Point(10, 70);
            description.MaxLength = 50;
            description.Size = new Size(200, 200);

            icon = new ModIcon();

            // if the metadata file already exists, try to read from it
            if (File.Exists(modPath + "\\.osml\\metadata.ini"))
            {
                IniData data = INIManage.Read(modPath + "\\metadata.ini");

                displayName.Text = data["config"]["displayName"];
                author.Text = data["config"]["author"];
                version.Text = data["config"]["version"];
                description.Text = data["config"]["description"];
            }

            // and the icon
            if (File.Exists(modPath + "\\.osml\\icon.png"))
                icon.Image = Image.FromFile(modPath + "\\icon.png");

            Controls.Add(displayName);
            Controls.Add(author);
            Controls.Add(description);
            Controls.Add(version);
            Controls.Add(icon);
            Controls.Add(new MMDDone());

            displayNameInstance = displayName; 
            authorInstance = author;
            versionInstance = version;
            descriptionInstance = description;
        }

        protected override void OnClosed(EventArgs e)
        {
            Audio.PlaySound("sfx_back.mp3", false);
        }
    }

    public class ModIcon : PictureBox
    {
        public ModIcon()
        {
            Location = new Point(250, 10);
            Image = Image.FromFile(Constants.spritesPath + "mmd_icon_default.png");
            Size = new Size(80, 80);
            SizeMode = PictureBoxSizeMode.StretchImage;
            BackColor = Color.Transparent;

            Label l = new Label();
            l.Text = "Click to change icon";
            l.Location = new Point(200, 95);
            l.AutoSize = true;
            l.ForeColor = Color.White;
            l.BackColor = Color.Transparent;
            l.Font = Constants.GetTerminusFont(10);
            MMDForm.instance.Controls.Add(l);
        }


        protected override void OnClick(EventArgs e)
        {
            try
            {
                OpenFileDialog browse = new OpenFileDialog();

                browse.Title = "Please browse to the image you want to set as your icon.";
                browse.ShowDialog();

                Image = Image.FromFile(browse.FileName);
                Audio.PlaySound("sfx_decision.mp3", false);
            }
            catch (Exception ex)
            {
                string message = "An exception was encountered:\n" + ex.Message +
                        "\n------------------\n" + ex.ToString();
                Console.WriteLine(message);
                MessageBox.Show(message);
            }
        }
    }

    public class MMDDone : Button
    {
        public MMDDone()
        {
            Location = new Point(150, 190);
            AutoSize = true;
            BackColor = Color.White;
            Font = Constants.GetTerminusFont(12);

            Text = "Generate";
        }

        protected override async void OnClick(EventArgs e)
        {
            try
            {
                MMDForm.instance.Controls.Clear();

                if (!Directory.Exists(MMDForm.modPath + "\\.osml")) Directory.CreateDirectory(MMDForm.modPath + "\\.osml");

                LoadingBar loadingBar = new LoadingBar(MMDForm.instance);
                await loadingBar.SetLoadingStatus("working, please wait...");
                Audio.PlaySound(loadingBar.GetLoadingBGM(), false);

                // values
                string displayName = MMDForm.displayNameInstance.Text;
                string author = MMDForm.authorInstance.Text;
                string version = MMDForm.versionInstance.Text;
                string description = MMDForm.descriptionInstance.Text;

                // parse the ini file
                await loadingBar.SetLoadingStatus("writing ini data to metadata.ini");

                await INIManage.Parse(MMDForm.modPath + "\\.osml\\metadata.ini",
                    new string[4]
                    {
                        "displayName",
                        "author",
                        "version",
                        "description"
                    },
                    new string[4]
                    {
                        displayName,
                        author,
                        version,
                        description
                    }
                );

                await loadingBar.SetLoadingStatus("saving icon");

                MMDForm.icon.Image.Save(MMDForm.modPath + "\\.osml\\icon.png");

                await loadingBar.SetLoadingStatus("almost done!");

                Console.Beep();
                MessageBox.Show("All done!");
                MMDForm.instance.Close();
                Audio.Stop();
                Audio.PlaySound("sfx_back.mp3", false);
            }

            catch (Exception ex)
            {
                string message = "An exception was encountered:\n" + ex.Message +
                        "\n------------------\n" + ex.ToString();
                Console.WriteLine(message);
                MessageBox.Show(message);
            }
        }
    }
}
