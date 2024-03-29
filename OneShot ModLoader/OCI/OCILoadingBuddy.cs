﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OneShot_ModLoader.OCI
{
    // and OCILoadingBuddy is just a gif that shows in the corner of the main oci form
    // they're just a lil friend
    public class OCILoadingBuddy : PictureBox
    {
        private Theme theme;

        public OCILoadingBuddy(Theme theme)
        {
            this.theme = theme;

            Location = new Point(400, 250);
            BackColor = Color.Transparent;
            
            Image = Image.FromFile(Static.spritesPath + "oci_friend_" + theme + ".gif");
            Size = Image.Size;
            OCIForm.instance.Controls.Add(this);
        }

        protected override void OnClick(EventArgs e)
        {
            switch (theme)
            {
                case Theme.Blue:
                    Audio.PlaySound("sfx_robot", false);
                    break;
                case Theme.Green:
                    Audio.PlaySound("sfx_ram", false);
                    break;
                case Theme.Red:
                    Audio.PlaySound("sfx_scientist", false);
                    break;
            }   
        }

        protected override void OnHelpRequested(HelpEventArgs hevent)
        {
            string msg = "Your friend!";
            switch (theme)
            {
                case Theme.Blue:
                    msg += "\nThey are most humbled by your presence. Yes!";
                    break;
                case Theme.Green:
                    msg += "\nBAA";
                    break;
                case Theme.Red:
                    msg += "\nHello, Freeman.";
                    break;
            }
            MessageBox.Show(msg);
        }
        
        public enum Theme
        {
            Blue = 1,
            Green = 2,
            Red = 3,
        }
    }
}
