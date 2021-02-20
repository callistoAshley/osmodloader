using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OneShot_ModLoader
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
            catch (ObjectDisposedException) { }
            catch (ArgumentException ae)
            {
                Console.WriteLine("caught an argument exception in Program.Main(),\n---------------\n" +
                    ae.Message + "\n---------------\n" + ae.ToString());
                Console.WriteLine("which is pretty bonkers if you ask me");
            }
        }
    }
}
