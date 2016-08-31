using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalyst.Settings;
using AutoHotkey.Interop;
using System.Threading;

using Catalyst.Unmanaged;

namespace Catalyst.Input
{
    /// <summary>
    /// A class to handle keyboard and mouse input.
    /// </summary>
    public static class InputController
    {
        #region fields

        private static InputBinding[] bindings;
        private static AutoHotkeyEngine ahk;

        private static TimerCallback tcallback = WindowCheck_Tick;
        private static Timer WindowCheck = new Timer(tcallback, null, -1, 50);
        private static bool keyInScope = false;

        /// <summary>
        /// True if the hook is enabled.
        /// </summary>
        public static bool Enabled { get; private set;}
        /// <summary>
        /// The name of the target process for this hook. Empty string it's global.
        /// </summary>
        public static string TargetProcess { get; private set; }
        /// <summary>
        /// True if the hook is set to only monitor events in a certain process.
        /// </summary>
        public static bool IsProcessSpecific => TargetProcess != "";

        #endregion

        #region static constructor

        static InputController()
        {
            ahk = new AutoHotkeyEngine();

            var sets = new Settings.Settings(); sets.Load();
            bindings = sets.Controls.AsArray();

            WindowCheck.Change(0, 50);
        }

        #endregion

        #region key sending funcs

        /// <summary>
        /// Set the state of the given key code.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="pressed"></param>
        public static void SetKeyState(DIKCode code, bool pressed)
        {
            string scode = ((int)code).ToString("x3");
            string state = pressed ? " up" : " down";

            ahk.ExecRaw("Send {sc" + scode + state + "}");
        }

        /// <summary>
        /// Set the state of the given mouse button.
        /// <para></para>NOTE : Mousewheel up/down is only pressed, not held.
        /// </summary>
        /// <param name="btn"></param>
        /// <param name="pressed"></param>
        public static void SetButtonState(MouseButton btn, bool pressed)
        {
            string scode = btn.ToString();
            string state = pressed ? "D" : "U";
            string cmd = "MouseClick, {0}, , , 1, , {1}";

            ahk.ExecRaw(string.Format(cmd, scode, state));
        }

        /// <summary>
        /// Set the state of the given mouse code.
        /// <para></para>NOTE : Mousewheel up/down is only pressed, not held.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="pressed"></param>
        public static void SetButtonState(MouseCode code, bool pressed)
        {
            SetButtonState(code.ToMouseButton(), pressed);
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

            if (binding.KeyBinding != DIKCode.None)
                SetKeyState(binding.KeyBinding, pressed);

            else if (binding.MouseBinding != MouseCode.None)
                SetButtonState(binding.MouseBinding, pressed);
        }

        /// <summary>
        /// triggers a game action by pressing its key. 
        /// </summary>
        /// <param name="action"></param>
        public static void PressAction(GameAction action)
        {
            var binding = bindings[(int)action];
            if (binding.KeyBinding != DIKCode.None)
                PressKey(binding.KeyBinding);
            else
                PressButton(binding.MouseBinding);
        }

        /// <summary>
        /// Simulate a mouse click on the given button.
        /// </summary>
        /// <param name="code">The mouse code.</param>
        public static void PressButton(MouseCode code)
        {
            PressButton(code, 20);
        }

        /// <summary>
        /// Simulate a mouse click on the given button.
        /// </summary>
        /// <param name="code">The mouse button.</param>
        public static void PressButton(MouseButton code)
        {
            PressButton(code, 20);
        }

        /// <summary>
        /// Simulate a mouse click on the given button.
        /// </summary>
        /// <param name="code">The mouse code.</param>
        /// <param name="pressTimeMS">The time the key will be pressed.</param>
        public static void PressButton(MouseCode code, int pressTimeMS)
        {
            SetButtonState(code, true);
            Thread.Sleep(pressTimeMS);
            SetButtonState(code, false);
        }

        /// <summary>
        /// Simulate a mouse click on the given button.
        /// </summary>
        /// <param name="code">The mouse button.</param>
        /// <param name="pressTimeMS">The time the key will be pressed.</param>
        public static void PressButton(MouseButton code, int pressTimeMS)
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

        #endregion

        #region input hook functions

        private static void WindowCheck_Tick(object state)
        {
            if (TargetProcess != "")
            {
                keyInScope = false;

                IntPtr hWnd = WinAPI.GetForegroundWindow();
                int pid; WinAPI.GetWindowThreadProcessId(hWnd, out pid);
                IntPtr hProcess = WinAPI.OpenProcess(Memory.ProcessAccessFlags.QueryLimitedInformation, false, pid);

                int capacity = 1024;
                StringBuilder buff = new StringBuilder(capacity);

                if (WinAPI.QueryFullProcessImageName(hProcess, 0, buff, ref capacity))
                {
                    string fullPath = buff.ToString();
                    keyInScope = TargetProcess == System.IO.Path.GetFileNameWithoutExtension(fullPath);
                }
            }
            else
            {
                keyInScope = true;
            }
        }

        /// <summary>
        /// Make the hook only monitor input when the given process is focused.
        /// </summary>
        /// <param name="procName"></param>
        public static void MakeProcessSpecific(string procName)
        {
            TargetProcess = procName;
        }

        /// <summary>
        /// Make the hook monitor input regardless of the focused process.
        /// </summary>
        public static void MakeGlobal()
        {
            TargetProcess = "";
        }

        /// <summary>
        /// Gets all the currently pressed keys.
        /// </summary>
        /// <returns></returns>
        public static DIKCode[] GetPressedKeys()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the currently pressed mouse buttons.
        /// </summary>
        /// <returns></returns>
        public static MouseButton[] GetPressedButtons()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Test if the given key is pressed.
        /// </summary>
        /// <param name="keyCode">The key code.</param>
        /// <returns></returns>
        public static bool IsKeyPressed(DIKCode keyCode)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Test if the given mouse button is pressed.
        /// </summary>
        /// <param name="btn">The mouse button.</param>
        /// <returns></returns>
        public static bool IsButtonPressed(MouseButton btn)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Test if the given mouse code is pressed.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static bool IsButtonPressed(MouseCode code)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Test if the given game action is pressed.
        /// </summary>
        /// <param name="action">The action to test for.</param>
        /// <returns></returns>
        public static bool IsGameActionPressed(GameAction action)
        {
            var binding = bindings[(int)action];
            return IsKeyPressed(binding.KeyBinding) || IsButtonPressed(binding.MouseBinding);
        }

        /// <summary>
        /// Get the current position of the mouse as an integer tuple.
        /// </summary>
        /// <returns></returns>
        public static Tuple<int, int> GetMousePos()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
