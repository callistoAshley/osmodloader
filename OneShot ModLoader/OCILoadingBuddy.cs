using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OneShot_ModLoader
{
    public class OCILoadingBuddy : PictureBox
    {
        private string themeMain;

        public OCILoadingBuddy(string theme)
        {
            themeMain = theme;

            Size = new Size(1000, 1000);
            Location = new Point(400, 250);
            BackColor = Color.Transparent;
            
            Image = Image.FromFile(Static.spritesPath + "oci_friend_" + theme + ".gif");
            OCIForm.instance.Controls.Add(this);
        }

        protected override void OnClick(EventArgs e)
        {
            switch (themeMain)
            {
                case "blue":
                    Audio.PlaySound("sfx_robot.mp3", false);
                    break;
                case "green":
                    Audio.PlaySound("sfx_ram.mp3", false);
                    break;
                case "red":
                    Audio.PlaySound("sfx_scientist.mp3", false);
                    break;
            }   
        }
    }
}
