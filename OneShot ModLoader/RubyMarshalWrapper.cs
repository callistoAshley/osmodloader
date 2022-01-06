using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OneShot_ModLoader.Kaitai;

// so this is a ruby marshal parser implimentation in something called kaitai
// kaitai is... a thing?
// to the peak of my understanding it looks like a parser to parse other parsers in a yaml-looking format
// here's where i found it if you want to look at it yourself: https://formats.kaitai.io/ruby_marshal/index.html
// i'm still kinda tweaking this and learning how it works so it won't come out in the next update

namespace OneShot_ModLoader
{
    public static class RubyMarshalWrapper
    {
        public static RubyMarshal FromFile(string path)
        {
            return RubyMarshal.FromFile(path);
        }
    }
}
