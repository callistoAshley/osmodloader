using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using IniParser.Model;
using IniParser;

namespace OneShot_ModLoader
{
    public class ModBox : Control 
    {
        public string modPath;
        public string modName;

        public ModBox(string path)
        {
            try
            {
                modPath = path;
                Size = new Size(230, 50);

                Label displayName = new Label();
                Label author = new Label();
                Label desc = new Label();
                PictureBox icon = new PictureBox();

                modName = modPath.Substring(modPath.LastIndexOf("Mods") + 5);
                desc.Text = "Metadata is nonexistent or unreadable";
                icon.Image = Image.FromFile(Static.spritesPath + "mmd_icon_default.png");

                // read from metadata
                if (File.Exists(modPath + "\\.osml\\metadata.ini"))
                {
                    IniData data = INIManage.Read(modPath + "\\.osml\\metadata.ini");

                    modName = data["config"]["displayName"] + " - " + data["config"]["version"];
                    author.Text = "by " + data["config"]["author"];
                    desc.Text = data["config"]["description"];
                }
                // and the icon
                if (File.Exists(modPath + "\\.osml\\icon.png"))
                    icon.Image = Image.FromFile(modPath + "\\.osml\\icon.png");

                displayName.Text = modName;

                // position
                icon.Location = new Point(0, 0);
                displayName.Location = new Point(50, 0);
                author.Location = new Point(50, 10);
                desc.Location = new Point(50, 20);

                // icon size
                icon.Size = new Size(50, 50);
                icon.SizeMode = PictureBoxSizeMode.StretchImage;

                // font
                displayName.Font = Static.GetTerminusFont(9);
                author.Font = Static.GetTerminusFont(9);
                desc.Font = Static.GetTerminusFont(9);

                // colour
                displayName.ForeColor = Color.MediumPurple;
                author.ForeColor = Color.MediumPurple;
                desc.ForeColor = Color.MediumPurple;

                displayName.AutoSize = true;
                author.AutoSize = true;
                desc.AutoSize = true;

                Controls.Add(displayName);
                Controls.Add(author);
                Controls.Add(desc);
                Controls.Add(icon);
            }
            catch (Exception ex)
            {
                new ExceptionMessage(ex, true);
            }
        }
    }
}
