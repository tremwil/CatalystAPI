using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System.Runtime.Remoting;
using Catalyst;
using System.Drawing;
using System.Reflection;
using Debug = System.Console;

namespace Catalyst.Scripting
{
    public class Scripter
    {
        private ScriptEngine engine;
        private ScriptScope scope;
        private OverlayWrapper wrapper;

        private Memory.MemoryManager Mem;
        
        public Scripter()
        {
            engine = Python.CreateEngine();
            Mem = new Memory.MemoryManager();

            wrapper = new OverlayWrapper();
        }

        private void CreateScope()
        {
            Assembly catalyst = Assembly.GetAssembly(typeof(Memory.MovementState));
            engine.Runtime.LoadAssembly(catalyst);

            Assembly drawing = Assembly.GetAssembly(typeof(Color));
            engine.Runtime.LoadAssembly(drawing);

            Assembly scripting = Assembly.GetExecutingAssembly();
            engine.Runtime.LoadAssembly(scripting);

            scope = engine.CreateScope();

            engine.Execute("from System.Drawing import *");
            engine.Execute("from Catalyst.Input import *", scope);
            engine.Execute("from Catalyst.Mathf import *", scope);
            engine.Execute("from Catalyst.Memory import *", scope);
            engine.Execute("from Catalyst.Settings import *", scope);

            Mem.OpenProcess("MirrorsEdgeCatalyst");
            scope.SetVariable("Memory", Mem);

            scope.SetVariable("Input", new Input.InputController());
            scope.SetVariable("Settings", new Settings.Settings());
            scope.SetVariable("Overlay", wrapper);
            scope.SetVariable("Game", new Memory.GameInfo(Mem));
            scope.SetVariable("Player", new Memory.PlayerInfo(Mem));
        }
    }
}