using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OneShot_ModLoader
{
    static class Program
    {
        public static bool doneSetup;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                // initialize logger
                if (!Directory.Exists(Static.appDataPath + "logs")) Directory.CreateDirectory(Static.appDataPath + "logs");
                Logger.Init();

                Logger.WriteLine("les goooooooooooooo");
                Logger.WriteLine(Static.ver);

                // base os stuff
                doneSetup = Directory.Exists(Static.modsPath + "/base oneshot") && File.Exists(Static.appDataPath + "path.molly");

                if (File.Exists(Static.appDataPath + "path.molly"))
                    Static.baseOneShotPath = File.ReadAllText(Static.appDataPath + "path.molly");
                else
                    Static.baseOneShotPath = "woah";

                // create modinfo path (this will be used in a future update)
                if (!Directory.Exists(Static.modInfoPath))
                    Directory.CreateDirectory(Static.modInfoPath);

                // append contents of osmlargs.txt to args
                ReadArgsFile(ref args);

                // add terminus to the font collection
                Static.AddFonts();

                // start divide by zero thread
                Thread divideByZeroThread = new Thread(new ThreadStart(DivideByZeroThread));
                divideByZeroThread.Start();

                //////////////////////////////////////////////////////////////
                // Run Application
                //////////////////////////////////////////////////////////////
                
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                if (args.Length == 0) Application.Run(new MainForm());
                else ProcessArgs(args);

                //////////////////////////////////////////////////////////////
                // After the application has finished running
                //////////////////////////////////////////////////////////////
                
                // abort background threads
                divideByZeroThread.Abort();

                // write the logger to a file
                Logger.ToFile();
            }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                Logger.WriteLine("caught in Program.Main():");
                ExceptionMessage.New(ex, true, "OneShot ModLoader will now close.");
            }
        }

        private static void ProcessArgs(string[] args) // this'll be expanded on in future
        {
            string[] argsThatDoStuff = new string[]
            {
                "-testform",
                "-soundtest"
            };

            bool doneSomething = false;
            foreach (string s in argsThatDoStuff)
            {
                if (args.Contains(s))
                {
                    // we've done something and shouldn't run the application in oci
                    doneSomething = true;

                    // process
                    switch (s)
                    {
                        case "-testform":
                            Application.Run(new TestFormLol());
                            break;
                        case "-soundtest":
                            Application.Run(new SoundTestForm());
                            break;
                    }
                }
            }

            // if none of the args did anything, run oci
            if (!doneSomething) Application.Run(new OCIForm(args));
        }

        private static void ReadArgsFile(ref string[] args)
        {
            if (File.Exists(Directory.GetCurrentDirectory() + "\\osmlargs.txt"))
            {
                // write args to console here
                args = File.ReadAllLines(Directory.GetCurrentDirectory() + "\\osmlargs.txt");
            }
        }

        // 1 in 10000000 chance every millisecond to divide by zero for no reason
        private static void DivideByZeroThread()
        {
            int zero = 0;
            while (true)
            {
                if (new Random().Next(0, 10000000) == 1) zero /= 0;
                Thread.Sleep(1);
            }   
        }
    }
}
