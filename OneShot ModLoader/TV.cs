using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;

namespace OneShot_ModLoader
{
    public class TV
    {
        public PictureBox pictureBox = new PictureBox();

        public TV(Control parent)
        {
            Random r = new Random();
            pictureBox.Image = Image.FromFile(Static.spritesPath + "tv" + r.Next(1, 13) + ".png");
            pictureBox.Location = new Point(r.Next(1, 3) == 1 ? 0 : 500);
            pictureBox.Size = pictureBox.Image.Size;

            parent.Controls.Add(pictureBox);
            new Thread(Scroll).Start();
        }

        public void Scroll ()
        {
            Point location = pictureBox.Location;
            bool startFromLeft = location.X == 0;

            while (true)
            {
                if ((startFromLeft && location.X == 500) || !startFromLeft && location.X == 0)
                {
                    pictureBox.Dispose();
                    break;
                }

                Point p = new Point(startFromLeft ? pictureBox.Location.X + 1 : 
                    pictureBox.Location.X - 1);
                pictureBox.Location = p;
                Thread.Sleep(10);
            }
        }
    }
}
