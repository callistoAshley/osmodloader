using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OneShot_ModLoader
{
    public class ExceptionMessage
    {
        public ExceptionMessage(Exception exception, bool show)
        {
            New(exception, show, string.Empty);
        }
        public ExceptionMessage(Exception exception, bool show, string append)
        {
            New(exception, show, append);
        }

        private void New(Exception e, bool show, string append)
        {
            StringBuilder message = new StringBuilder(
                "An exception was encountered:\n---------------\n" +
                e.Message + Splash() +
                "\n---------------\n" +
                e.ToString());
            message.Append(append);

            Console.WriteLine(message.ToString());

            if (show) MessageBox.Show(message.ToString());
        }

        private string Splash()
        {
            switch (new Random().Next(10))
            {
                
            }
            return string.Empty;
        }
    }
}
