using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace OneShot_ModLoader
{
    public class TVManage
    {
        public static PictureBox tvs = new PictureBox();
        public static TVManage instance;

        public TVManage(Control parent)
        {
            if (instance == null) instance = this;
            parent.Controls.Add(tvs);

            new Thread(DoStuff).Start();
        }

        private void DoStuff()
        {
            while (true)
            {
                Thread.Sleep(1000);
                new TV(tvs);
                Logger.WriteLine("new tv");
            }
        }
    }
}
