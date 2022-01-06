using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace OneShot_ModLoader
{
    public static class Logger
    {
        private static StreamWriter writer;

        public static void Init()
        {
            writer = new StreamWriter(Static.appDataPath + "logs\\output " + DateTime.Now.ToString().Replace("/", ".").Replace(":", ".") + ".txt");
        }

        public static void WriteLine(string line)
        {
            // regex replace C:\Users\username\ with just C:\username\ so we don't catch someone's real name in case they have to send a log file
            line = new Regex(@"C:\\Users\\.*?\\").Replace(line, "C:\\Users\\username\\");
            // and one more replace here just in case
            line = line.Replace(Environment.UserName, "username");

            writer.WriteLine(line);
        }

        public static void ToFile()
        {
            List<string> delete = new List<string>();
            foreach (FileInfo f in new DirectoryInfo(Static.appDataPath + "/logs").GetFiles())
            {
                // for every file in the /logs folder, check if it has been alive for more than 6 hours
                DateTime lifespan = DateTime.Now - new TimeSpan(6, 0, 0);

                if (f.CreationTime <= lifespan) delete.Add(f.FullName); // and if it has, add it to a collection
            }
            foreach (string s in delete) new FileInfo(s).Delete(); // then delete every file in that collection

            // close the stream writer
            writer.Close();
        }
    }
}
