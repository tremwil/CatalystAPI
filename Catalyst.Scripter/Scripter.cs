using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.Scripting.Hosting;
using System.Runtime.Remoting;
using Catalyst;
using System.Drawing;
using System.Reflection;
using System.Timers;
using Debug = System.Console;

namespace Catalyst.Scripting
{
    public class Scripter
    {
        private ScriptEngine engine;
        public ScriptScope scope;
        private OverlayWrapper wrapper;

        private Memory.MemoryManager mem;

        private GameInfoWrapper gameInfo;
        private PlayerInfoWrapper playerInfo;

        private Timer infoRefreshTimer;
        
        public Scripter()
        {
            engine = Python.CreateEngine();
            mem = new Memory.MemoryManager();

            wrapper = new OverlayWrapper(this);

            gameInfo = new GameInfoWrapper(mem);
            playerInfo = new PlayerInfoWrapper(mem);

            infoRefreshTimer = new Timer(70);
            infoRefreshTimer.Elapsed += InfoRefreshTimer_Elapsed;
        }

        private void InfoRefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            gameInfo.IsLoading.Update(gameInfo.ginfo.IsLoading());
            playerInfo.MovementState.Update(playerInfo.pinfo.GetMovementState());
            playerInfo.Position.Update(playerInfo.pinfo.GetPosition());
            playerInfo.Yaw.Update(playerInfo.pinfo.GetCameraYaw());

            playerInfo.Velocity.Update((playerInfo.Position.Current - playerInfo.Position.Old) * 14.2857f);
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
            engine.Execute("import Catalyst.Input.InputController as Input", scope);
            engine.Execute("import System.Console as Debug", scope);

            scope.SetVariable("Memory", mem);
            scope.SetVariable("Settings", new Settings.Settings());
            scope.SetVariable("Overlay", wrapper);
            scope.SetVariable("Game", gameInfo);
            scope.SetVariable("Player", playerInfo);
        }

        private void Load(string file)
        {
            CreateScope();

            var code = engine.CreateScriptSourceFromFile(file);
            var compiled = code.Compile();
            compiled.Execute(scope);
        }
    }
}