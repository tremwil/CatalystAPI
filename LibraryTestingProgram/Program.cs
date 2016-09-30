using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

using Catalyst;
using Catalyst.Native;
using Catalyst.Memory;
using Catalyst.Display;
using Catalyst.Input;

namespace LibraryTestingProgram
{
    class Program
    {
        static void Main(string[] args)
        {
            var memory = new MemoryManager();
            memory.OpenProcess("MirrorsEdgeCatalyst");

            DeepPointer<float> ptr = new DeepPointer<float>(memory.ProcHandle, 0, 0x1423DA028, 0x20, 0x20, 0x40, 0x20, 0x04);
            long dyn = ptr.GetDynamicAddress();

            Stopwatch w = new Stopwatch();
            long gtime, stime;
            byte[] b = new byte[4];
            var z = IntPtr.Zero;

            w.Start();
            for (int i = 0; i < 500; i++)
            {
                WinAPI.ReadProcessMemory(memory.ProcHandle, dyn, b, 4, out z);
            }
            gtime = w.ElapsedTicks;
            w.Restart();
            for (int i = 0; i < 500; i++)
            {
                WinAPI.WriteProcessMemory(memory.ProcHandle, dyn, b, 4, out z);
            }
            stime = w.ElapsedTicks;

            Console.WriteLine("[AVERAGE OF 5000 GETS/SETS]");
            Console.WriteLine("get is faster by a factor of {0}", (double)stime / gtime);

            Console.Read();
        }
    }
}
