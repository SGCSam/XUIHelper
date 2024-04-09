using NXEControls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUIHelper.GUI
{
    internal class Constants
    {
        public static NXEPageManager PageManager { get; private set; }
        public static NXEHUDManager? HUDManager { get; private set; }

        public static string ExtensionsDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Extensions");

        static Constants()
        {
            PageManager = new NXEPageManager();
        }

        public static void Initialize(NXEHUD hud)
        {
            HUDManager = new NXEHUDManager(hud);
        }
    }
}
