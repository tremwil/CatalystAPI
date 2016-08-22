using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalyst;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;

using Catalyst.Scripting;

namespace LibraryTestingProgram
{
    class Program
    {
        static int num = 0;

        [STAThread]
        static void Main(string[] args)
        {
            OverlayForm overlay = new OverlayForm("notepad++");
            overlay.Overlays.Add(new OverlayField("level : {0}", () => ++num));
            overlay.Overlays.Add(new OverlayField("left  : {0}", () => 1000 - num));

            overlay.EnableOverlay();
            Application.Run(overlay);
        }
    }
}
