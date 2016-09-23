using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

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
            var memory = new MemoryManager();
            memory.OpenProcess("MirrorsEdgeCatalyst");

            var ginfo = new GameInfo(memory);
            var pinfo = new PlayerInfo(memory);

            while (true)
            {
                float y = pinfo.GetPosition().y;
                pinfo.SetLastGroundYCoord(y);

                Thread.Sleep(10);
            }
        }
    }
}
