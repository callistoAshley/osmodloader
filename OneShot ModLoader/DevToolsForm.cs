using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace OneShot_ModLoader
{
    public class DevToolsForm : Form
    {
        public static DevToolsForm instance;

        public DevToolsForm ()
        {
            instance = this;

            Size = new Size(300, 500);
            Text = "Dev Tools";
            BackgroundImage = Image.FromFile(Static.spritesPath + "terminal.png");
            BackgroundImageLayout = ImageLayout.Center;
            BackColor = Color.Black;
            Icon = new Icon(Static.spritesPath + "devtools.ico");

            // add ? icon
            MinimizeBox = false;
            MaximizeBox = false;
            HelpButton = true;

            FormBorderStyle = FormBorderStyle.FixedSingle;
            Show();

            Init();
        }

        public void Init()
        {
            Controls.Add(new DTCompressionButton());
            Controls.Add(new DTMetadataButton());
        }

        protected override void OnClosed(EventArgs e)
        {
            instance = null;
            Audio.PlaySound("sfx_back.mp3", false);
        }
    }
}
