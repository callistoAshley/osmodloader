using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronRuby;
using System.Runtime.InteropServices;
using IronRuby.Runtime.Calls;
using IronRuby.Runtime;
using Microsoft.Scripting.Hosting;

// THIS LIBRARY HAS NO. DOCUMENTATION. WHATSOEVER. EXCEPT FOR SOME OUTDATED ARTICLE I FOUND ON MSDN MAGAZINE 2009
// AND IT HASN'T BEEN UPDATED IN 10 YEARS.
// I AM SCREWED

// should've just done c++ istg

namespace OneShot_ModLoader
{
    public static class Interop
    {
        public static void CreateSrc(string file)
        {
            ScriptEngine scriptEngine = Ruby.CreateEngine();
            ScriptScope scriptScope = scriptEngine.CreateScope();
            ScriptSource src = scriptEngine.CreateScriptSourceFromFile(file);
        }

        [RubyMethod("", RubyMethodAttributes.ModuleFunction)]
        public static extern void RPGScript();
    }
}
