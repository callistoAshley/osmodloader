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
            BackgroundImage = Image.FromFile(Static.spritesPath + "bg.png");
            BackgroundImageLayout = ImageLayout.Tile;
            BackColor = Color.Black;
            Icon = new Icon(Static.spritesPath + "devtools.ico");

            FormBorderStyle = FormBorderStyle.FixedSingle; 
            Show();

            Init();
        }

        public void Init()
        {
            TextBox displayName = new TextBox
            {
                Text = "Display Name",
                Location = new Point(10, 10),
                MaxLength = 20,
            };
            TextBox author = new TextBox
            {
                Text = "Author",
                Location = new Point(10, 30),
                MaxLength = 15,
            };
            TextBox version = new TextBox
            {
                Text = "Version",
                Location = new Point(10, 50),
                MaxLength = 10,
            };
            TextBox description = new TextBox
            {
                Text = "Short Description - Max: 50 char",
                Location = new Point(10, 70),
                MaxLength = 50,
                Size = new Size(200, 200),
            };

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
            Controls.Add(version);
            Controls.Add(description);
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
            Image = Image.FromFile(Static.spritesPath + "mmd_icon_default.png");
            Size = new Size(80, 80);
            SizeMode = PictureBoxSizeMode.StretchImage;
            BackColor = Color.Transparent;

            Label l = new Label();
            l.Text = "Click to change icon";
            l.Location = new Point(200, 95);
            l.AutoSize = true;
            l.ForeColor = Color.White;
            l.BackColor = Color.Transparent;
            l.Font = Static.GetTerminusFont(10);
            MMDForm.instance.Controls.Add(l);
        }


        protected override void OnClick(EventArgs e)
        {
            try
            {
                using (OpenFileDialog browse = new OpenFileDialog())
                {
                    browse.Title = "Please browse to the image you want to set as your icon.";
                    browse.ShowDialog();

                    Image = Image.FromFile(browse.FileName);
                    Audio.PlaySound("sfx_decision.mp3", false);
                }
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
            Font = Static.GetTerminusFont(12);

            Text = "Generate";
        }

        protected override async void OnClick(EventArgs e)
        {
            try
            {
                MMDForm.instance.Controls.Clear();

                DirectoryInfo path = new DirectoryInfo(MMDForm.modPath + "\\.osml");
                if (!path.Exists) path.Create(); // TODO: mark this as hidden

                LoadingBar loadingBar = new LoadingBar(MMDForm.instance);
                loadingBar.SetLoadingStatus("working, please wait...");
                Audio.PlaySound(loadingBar.GetLoadingBGM(), false);

                // values
                string displayName = MMDForm.displayNameInstance.Text;
                string author = MMDForm.authorInstance.Text;
                string version = MMDForm.versionInstance.Text;
                string description = MMDForm.descriptionInstance.Text;

                // parse the ini file
                loadingBar.SetLoadingStatus("writing ini data to metadata.ini");

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

                loadingBar.SetLoadingStatus("saving icon");

                MMDForm.icon.Image.Save(MMDForm.modPath + "\\.osml\\icon.png");

                loadingBar.SetLoadingStatus("almost done!");

                Console.Beep();
                MessageBox.Show("All done!");
                loadingBar.Dispose();
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
