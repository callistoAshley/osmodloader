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
        private Theme theme;

        public OCILoadingBuddy(Theme theme)
        {
            this.theme = theme;

            Size = new Size(1000, 1000);
            Location = new Point(400, 250);
            BackColor = Color.Transparent;
            
            Image = Image.FromFile(Static.spritesPath + "oci_friend_" + theme + ".gif");
            OCIForm.instance.Controls.Add(this);
        }

        protected override void OnClick(EventArgs e)
        {
            switch (theme)
            {
                case Theme.Blue:
                    Audio.PlaySound("sfx_robot.mp3", false);
                    break;
                case Theme.Green:
                    Audio.PlaySound("sfx_ram.mp3", false);
                    break;
                case Theme.Red:
                    Audio.PlaySound("sfx_scientist.mp3", false);
                    break;
            }   
        }

        public enum Theme
        {
            Blue,
            Green,
            Red
        }
    }
}
