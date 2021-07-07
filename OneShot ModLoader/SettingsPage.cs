using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OneShot_ModLoaderm
{
    public class SettingsPage : TabControl
    {
        public SettingsPage()
        {
            Size = new Size(500, 250);
            Location = new Point(10, 15);

            TabPages.Add(new TabPage
            {
                Text = "cool page",
                BackColor = Color.Black
            });
            TabPages.Add(new TabPage
            {
                Text = "yo mama",
                BackColor = Color.Black
            });
        }
    }
}
