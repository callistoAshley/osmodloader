using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IniParser;
using IniParser.Model;

namespace OneShot_ModLoader
{
    public class INIManage
    {
        public static async Task Parse(string path, string[] valueNames, string[] values)
        {
            FileIniDataParser parser = new FileIniDataParser();
            IniData data = new IniData();

            for (int i = 0; i < valueNames.Length; i++)
                data["config"][valueNames[i]] = values[i];

            parser.WriteFile(path, data);

            await Task.Delay(0);
        }

        public static IniData Read(string path)
        {
            return new FileIniDataParser().ReadFile(path);
        }
    }
}
