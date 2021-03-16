using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace OneShot_ModLoader
{
    public class DTModMetadataForm : Form
    {
        public static DTModMetadataForm instance;

        public static TextBox displayNameInstance;
        public static TextBox authorInstance;
        public static TextBox versionInstance;
        public static TextBox descriptionInstance;

        public DTModMetadataForm(string modPath)
        {
            Console.WriteLine("initalized mod metadata form with path: " + modPath);

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

            Controls.Add(displayName);
            Controls.Add(author);
            Controls.Add(description);
            Controls.Add(version);

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
}
