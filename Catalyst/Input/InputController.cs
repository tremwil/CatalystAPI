using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalyst.Settings;
using AutoIt;
using System.Threading;

namespace Catalyst.Input
{
    /// <summary>
    /// A class to handle keyboard and mouse input.
    /// </summary>
    public static class InputController
    {
        private static InputBinding[] bindings;

        private static int _ = Init();
        static int Init()
        {
            var sets = new Settings.Settings();
            sets.Load();

            bindings = sets.Controls.AsArray();

            return 0;
        }

        /// <summary>
        /// Set the state of the given key code.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="pressed"></param>
        public static void SetKeyState(DIKCode code, bool pressed)
        {
            var str = "{" + code.ToAutoItString() + (pressed? " down}" : " up}");
            AutoItX.Send(str);
        }

        /// <summary>
        /// Set the state of the given mouse code.
        /// <para></para>NOTE : Mousewheel up/down is only pressed, not held.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="pressed"></param>
        public static void SetButtonState(MouseCode code, bool pressed)
        {
            var raw = code.ToRawCode();

            if (raw.Axis == 24)
            {
                string[] names = new string[3] { "LEFT", "RIGHT", "MIDDLE" };
                if (pressed) AutoItX.MouseDown(names[raw.Button]);
                else AutoItX.MouseUp(names[raw.Button]);
            }
            else
            {
                string dir = (raw.Negate == 0) ? "up" : "down";
                AutoItX.MouseWheel(dir, 10);
            }
        }

        /// <summary>
        /// Set the state of a game action key. 
        /// <para></para> NOTE: Actions bound to the mousewheel will only be pressed and not held.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="pressed"></param>
        public static void SetGameActionState(GameAction action, bool pressed)
        {
            var binding = bindings[(int)action];
            if (binding.KeyBinding != DIKCode.DIK_NONE)
                SetKeyState(binding.KeyBinding, pressed);
            else
                SetButtonState(binding.MouseBinding, pressed);
        }

        /// <summary>
        /// triggers a game action by pressing its key. 
        /// </summary>
        /// <param name="action"></param>
        public static void PressAction(GameAction action)
        {
            var binding = bindings[(int)action];
            if (binding.KeyBinding != DIKCode.DIK_NONE)
                PressKey(binding.KeyBinding);
            else
                PressButton(binding.MouseBinding);
        }

        /// <summary>
        /// Simulate a mouse click on the given button.
        /// </summary>
        /// <param name="code">The key code.</param>
        public static void PressButton(MouseCode code)
        {
            PressButton(code, 20);
        }

        /// <summary>
        /// Simulate a mouse click on the given button.
        /// </summary>
        /// <param name="code">The key code.</param>
        /// <param name="pressTimeMS">The time the key will be pressed.</param>
        public static void PressButton(MouseCode code, int pressTimeMS)
        {
            SetButtonState(code, true);
            Thread.Sleep(pressTimeMS);
            SetButtonState(code, false);
        }

        /// <summary>
        /// Simulate a keypress on the given key.
        /// </summary>
        /// <param name="code">The key code.</param>
        public static void PressKey(DIKCode code)
        {
            PressKey(code, 20);
        }

        /// <summary>
        /// Simulate a keypress on the given key.
        /// </summary>
        /// <param name="code">The key code.</param>
        /// <param name="pressTimeMS">The time the key will be pressed.</param>
        public static void PressKey(DIKCode code, int pressTimeMS)
        {
            SetKeyState(code, true);
            Thread.Sleep(pressTimeMS);
            SetKeyState(code, false);
        }
    }
}
