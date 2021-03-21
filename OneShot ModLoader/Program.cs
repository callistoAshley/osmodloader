using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OneShot_ModLoader
{
    static class Program
    {
        public static bool doneSetup;
        public static bool initForm;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (initForm) return;
            initForm = true;

            // console out stuff
            if (!Directory.Exists(Constants.appDataPath + "logs")) Directory.CreateDirectory(Constants.appDataPath + "logs");

            Form1.consoleOutStream = new StreamWriter(Constants.appDataPath + "logs\\output " + DateTime.Now.ToString().Replace("/", ".").Replace(":", ".") + ".txt");
            Console.SetOut(Form1.consoleOutStream);
            Console.SetError(Form1.consoleOutStream);

            // base os stuff
            doneSetup = Directory.Exists(Constants.modsPath + "/base oneshot") && File.Exists(Constants.appDataPath + "path.molly");

            if (File.Exists(Constants.appDataPath + "path.molly"))
                Form1.baseOneShotPath = File.ReadAllText(Constants.appDataPath + "path.molly");
            else
                Form1.baseOneShotPath = "woah";

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                if (args.Length == 0) Application.Run(new Form1());
                else Application.Run(new OCIForm(args));
            }
            catch (ObjectDisposedException) { }
        }

        public static void ConsoleToFile()
        {
            List<string> delete = new List<string>();
            foreach (FileInfo f in new DirectoryInfo(Constants.appDataPath + "/logs").GetFiles())
            {
                // for every file in the /logs folder, check if it has been alive for more than 6 hours
                DateTime lifespan = DateTime.Now - new TimeSpan(6, 0, 0);

                if (f.CreationTime <= lifespan) delete.Add(f.FullName); // and if it has, add it to a collection
            }
            foreach (string s in delete) new FileInfo(s).Delete(); // then delete every file in that collection

            Console.SetOut(Form1.consoleOut);
            Console.SetError(Form1.consoleOut);

            Form1.consoleOutStream.Close();
            Form1.consoleOut.Close();
        }
    }
}
