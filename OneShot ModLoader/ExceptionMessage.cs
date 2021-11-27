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
        public static void New(Exception exception, bool show) => Bruh(exception, show, string.Empty);
        public static void New(Exception exception, bool show, string append) => Bruh(exception, show, append);

        private static void Bruh(Exception e, bool show, string append)
        {
            string message = 
                "An exception was encountered:\n---------------\n" +
                e.Message + Splash() +
                "\n---------------\n" +
                e.ToString() +
                "\n---------------\n";

            message +=
                "Other Details:\n" +
                $"TargetSite: {e.TargetSite.Name}\n" +
                $"HResult: {e.HResult}\n" +
                "---------------\n";

            message += append;

            Console.WriteLine(message.ToString());

            if (show) MessageBox.Show(message.ToString());
        }

        private static string Splash()
        {
            switch (new Random().Next(10))
            {
                
            }
            return string.Empty;
        }
    }
}
