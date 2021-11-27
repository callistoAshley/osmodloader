﻿using Microsoft.Scripting.Utils;
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
            //Interop.RPGScript();

            // console out stuff
            if (!Directory.Exists(Static.appDataPath + "logs")) Directory.CreateDirectory(Static.appDataPath + "logs");

            Form1.consoleOutStream = new StreamWriter(Static.appDataPath + "logs\\output " + DateTime.Now.ToString().Replace("/", ".").Replace(":", ".") + ".txt");
            Console.SetOut(Form1.consoleOutStream);
            Console.SetError(Form1.consoleOutStream);

            Console.WriteLine("les goooooooooooooo");
            Console.WriteLine(Static.ver);

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

            // start divide by zero thread
            Thread divideByZeroThread = new Thread(new ThreadStart(DivideByZeroThread));
            divideByZeroThread.Start();

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                if (args.Length == 0) Application.Run(new Form1());
                else ProcessArgs(args);
            }
            catch (ObjectDisposedException) { }

            // abort divide by zero thread 
            divideByZeroThread.Abort();
        }

        private static void ProcessArgs(string[] args) // this'll be expanded on in future
        {
            if (args.Contains("testform")) Application.Run(new Form1(true));
            else Application.Run(new OCIForm(args));
        }

        private static void ReadArgsFile(ref string[] args)
        {
            if (File.Exists(Directory.GetCurrentDirectory() + "\\osmlargs.txt"))
            {
                // write args to console here
                args = File.ReadAllLines(Directory.GetCurrentDirectory() + "\\osmlargs.txt");
            }
        }

        public static void ConsoleToFile()
        {
            List<string> delete = new List<string>();
            foreach (FileInfo f in new DirectoryInfo(Static.appDataPath + "/logs").GetFiles())
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
