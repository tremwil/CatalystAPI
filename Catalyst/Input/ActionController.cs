using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalyst.Settings;
using SharpDX.DirectInput;
using AutoIt;

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

        private static MouseState mstate;
        private static KeyboardState kbstate;
        
        static ActionController()
        {
            inputHandler = new DirectInput();
            keyboard = new Keyboard(inputHandler); keyboard.Acquire();
            mouse = new Mouse(inputHandler); mouse.Acquire();
        }
    }
}
