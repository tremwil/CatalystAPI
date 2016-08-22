using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Catalyst.Scripting
{
    public static class Time
    {
        public static float DeltaTime { get; private set; }

        public static readonly Stopwatch Watch = new Stopwatch();
    }
}
