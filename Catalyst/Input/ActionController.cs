using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalyst.Settings;
using SharpDX.DirectInput;
using AutoIt;
using System.Threading;

namespace Catalyst.Input
{
    /// <summary>
    /// A class to control or query game actions.
    /// </summary>
    public class ActionController
    {
        private static DirectInput inputHandler;
        private static Keyboard keyboard;
        private static Mouse mouse;

        private static Thread UpdateThread;
        private static bool endUpdater = false;

        private static object lockVal = new object();

        private static KeyboardState kbstate;
        private static MouseState mstate;

        static ActionController()
        {
            inputHandler = new DirectInput();
            keyboard = new Keyboard(inputHandler); keyboard.Acquire();
            mouse = new Mouse(inputHandler); mouse.Acquire();

            kbstate = new KeyboardState();
            mstate = new MouseState();

            UpdateThread = new Thread(UpdateKBMState);
            UpdateThread.Start();
        }

        private static void UpdateKBMState()
        {
            while (!endUpdater)
            {
                lock (lockVal)
                {
                    keyboard.GetCurrentState(ref kbstate);
                    mouse.GetCurrentState(ref mstate);
                }

                // 100times / second update on keyboard/mouse input
                Thread.Sleep(10);
            }
        }
    }
}
