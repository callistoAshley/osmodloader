using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Security.AccessControl;
using System.Security.Principal;

namespace OneShot_ModLoader
{
    static class Program
    {
        public static bool doneSetup;

        /// <summary>
        /// The main entry point for the application.
        /// we also do some pretty bonkers wild stuff here like create the registry key that makes your computer go "ayo look a .osml file"
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

            if (!Directory.Exists(Static.modInfoPath))
                Directory.CreateDirectory(Static.modInfoPath);

            // reg key
            //CreateOsmlRegKey();

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                if (args.Length == 0) Application.Run(new Form1());
                else ProcessArgs(args);
            }
            catch (ObjectDisposedException) { }
        }

        private static void ProcessArgs(string[] args) // this'll be expanded on in future
        {
            Console.WriteLine(args.AsParallel());

            if (args.Contains("testform")) new Form1(true);
            else Application.Run(new OCIForm(args));
        }

        // create a registry key that associates .osml files with OneShot ModLoader if it doesn't already exist
        private static void CreateOsmlRegKey()
        {
            // also check out the stack overflow post i took this from! https://www.inspiredtaste.net/38940/spaghetti-with-meat-sauce-recipe/

            try
            {
                if (File.Exists(Static.appDataPath + "\\doneregstuff.molly")) return;

                // create access control that will be added to classes root
                RegistrySecurity accessControl = new RegistrySecurity();
                accessControl.AddAccessRule(new RegistryAccessRule("Users",
                    RegistryRights.FullControl,
                    AccessControlType.Allow));
                // (and get its current access control so it can be restored after)
                RegistrySecurity classesRootAccessControl = Registry.ClassesRoot.GetAccessControl();
                // les go! set it:
                Registry.ClassesRoot.SetAccessControl(accessControl);

                // point to the .osml_auto_file key that's about to be created
                RegistryKey osmlKey = Registry.ClassesRoot.CreateSubKey(".osml");
                osmlKey.SetAccessControl(accessControl);
                osmlKey.SetValue("(Default)", ".osml_auto_file");

                // then create .osml_auto_file key
                RegistryKey autoFileKey = Registry.ClassesRoot.CreateSubKey(".osml_auto_file");
                RegistryKey command = autoFileKey.CreateSubKey("shell").CreateSubKey("open").CreateSubKey("command");
                command.SetValue("(Default)", Application.ExecutablePath + " %1");

                // yea we've done that!
                File.WriteAllText(Static.appDataPath + "\\doneregstuff.molly", "yup we've done that");
            }
            catch (Exception ex)
            {
                ExceptionMessage.New(ex, true, "\n\nOneShot ModLoader will continue to function as normal, but .osml files may not be able to be opened.");
                File.WriteAllText(Static.appDataPath + "\\doneregstuff.molly", "well we tried but it didn't like it that much\n" + ex.ToString());
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
    }
}
