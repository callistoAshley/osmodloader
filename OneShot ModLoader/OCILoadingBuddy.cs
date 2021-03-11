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
        public OCILoadingBuddy(string theme)
        {
            Size = new Size(1000, 1000);
            Location = new Point(400, 250);
            BackColor = Color.Transparent;
            
            Image = Image.FromFile(Constants.spritesPath + "oci_friend_" + theme + ".gif");
            OCIForm.instance.Controls.Add(this);
        }
    }
}
