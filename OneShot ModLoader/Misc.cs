using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace OneShot_ModLoader
{
    public static class Misc
    {
        public static void NormalifyFilePermissions(string filePath)
        {
            File.SetAttributes(filePath, FileAttributes.Normal);
        }
    }
}
