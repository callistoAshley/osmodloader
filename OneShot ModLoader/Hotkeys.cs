using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OneShot_ModLoader
{
    public class Hotkeys
    {
        public static bool toggleQuickChangeShortcut()
        {
            return Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.Q);
        }
    }
}
