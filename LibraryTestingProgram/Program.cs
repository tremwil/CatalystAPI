using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Catalyst;
using Catalyst.Memory;
using Catalyst.Display;

namespace LibraryTestingProgram
{
    class Program
    {
        static void Main(string[] args)
        {
            MemoryManager mem = new MemoryManager();
            mem.OpenProcess("MirrorsEdgeCatalyst");

            var pinfo = new PlayerInfo(mem);

            Overlay.AddAutoField("yaw", () => pinfo.GetCameraYaw() * Mathf.RadToDeg);
            Overlay.Enable(true);
        }
    }
}
