using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Catalyst;
using Catalyst.Memory;
using Catalyst.Display;
using Catalyst.Input;

namespace LibraryTestingProgram
{
    class Program
    {
        static void Main(string[] args)
        {
            MemoryManager mem = new MemoryManager();
            mem.OpenProcess("MirrorsEdgeCatalyst");

            var pinfo = new PlayerInfo(mem);
            var ginfo = new GameInfo(mem);

            Overlay.AddAutoField("yaw", () => pinfo.GetCameraYaw() * Mathf.RadToDeg);
            Overlay.Enable(true);

            //ginfo.SetTimescale(0);
            var angle = pinfo.GetCameraYaw();
            var pos = pinfo.GetPosition();
            while (!Console.KeyAvailable)
            {
                angle += 0.004f;
                pos += Vec3.AxisY * 0.05f;
                pinfo.SetCameraYaw(angle);
                pinfo.SetPosition(pos);
                Thread.Sleep(5);
            }

            ginfo.SetTimescale(1);
        }
    }
}
