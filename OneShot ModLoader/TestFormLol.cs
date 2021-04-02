using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace OneShot_ModLoader
{
    // test form for testing stuff
    public class TestFormLol : Form
    {

        public TestFormLol()
        {
            FormBorderStyle = FormBorderStyle.Sizable;

            FlowLayoutPanel list = new FlowLayoutPanel();
            for (int i = 0; i < 20; i++) list.Controls.Add(new ModBox("C:\\Users\\maste\\source\\repos\\OneShot ModLoader\\OneShot ModLoader\\bin\\Debug\\Mods\\base oneshot"));
            list.AutoScroll = true;

            list.Size = new Size(176, 175);
            list.FlowDirection = FlowDirection.TopDown;
            list.AllowDrop = true;

            Controls.Add(list);

            Show();
        }
    }
}
