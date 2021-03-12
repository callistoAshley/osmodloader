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
        public DTModMetadataForm(string modPath)
        {
            Console.WriteLine("initalized mod metadata form with path: " + modPath);

            instance = this;

            Size = new Size(300, 500);
            Text = "Mod Metadata: " + modPath;
            BackgroundImage = Image.FromFile(Constants.spritesPath + "lines.png");
            BackgroundImageLayout = ImageLayout.Center;
            BackColor = Color.Black;
            Icon = new Icon(Constants.spritesPath + "devtools.ico");

            FormBorderStyle = FormBorderStyle.FixedSingle;
            Show();
        }
    }
}
