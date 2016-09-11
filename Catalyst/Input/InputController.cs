using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalyst.Settings;
using AutoHotkey.Interop;
using System.Threading;
using System.Windows.Forms;

using Catalyst.Native;

namespace Catalyst.Input
{
    /// <summary>
    /// A class to handle keyboard and mouse input.
    /// </summary>
    public static class InputController
    {
        #region sending related fields

        private static InputBinding[] bindings;
        private static AutoHotkeyEngine ahk;

        /* flags for WINAPI function mouse_event */

        private const uint MOUSEEVENTF_ABSOLUTE = 0x8000;
        private const uint MOUSEEVENTF_MOVE = 0x0001;

        private const uint MOUSEEVENTF_VIRTUALDESK = 0x4000;

        private const int MOUSEEVENTF_WHEEL = 0x0800;
        private const int WHEEL_DELTA = 120;

        #endregion

        #region hook related properties/fields

        private static InputForm hookWnd = new InputForm();

        /// <summary>
        /// True if the hook is enabled.
        /// </summary>
        public static bool Enabled => SafeInvoke(f => f.HookEnabled);
        /// <summary>
        /// The name of the target process for this hook. Empty string it's global.
        /// </summary>
        public static string TargetProcess => SafeInvoke(f => f.TargetProcName);
        /// <summary>
        /// True if the hook is set to only monitor events in a certain process.
        /// </summary>
        public static bool IsProcessSpecific => SafeInvoke(f => f.TargetProcName != "");

        #endregion

        #region static constructor

        static InputController()
        {
            ahk = new AutoHotkeyEngine();
            ahk.Reset();

            try
            {
                var sets = new Settings.Settings(); sets.Load();
                bindings = sets.Controls.AsArray();
            }
            catch (Exception)
            {
                bindings = new InputBinding[0];
            }

            Thread t = new Thread(() => Application.Run(hookWnd));
            t.SetApartmentState(ApartmentState.STA);
            t.IsBackground = true;
            t.Start();
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
            int scode = (int)code;

            // Move the DIK entension flag to a higher byte
            // ex: DIK_RCONTROL (bin 1001 1100) -> bin 1 0001 1100
            scode = (scode & 0x80) << 1 | scode & 0x7F;

            string state = pressed ? "down" : "up";
            string cmd = "send {{sc{0:X3} {1}}}";

            ahk.ExecRaw(string.Format(cmd, scode, state));
        }

        /// <summary>
        /// Set the state of the given mouse button.
        /// <para></para>NOTE : Mousewheel up/down is only pressed, not held.
        /// </summary>
        /// <param name="btn"></param>
        /// <param name="pressed"></param>
        public static void SetButtonState(MouseButton btn, bool pressed)
        {
            string sbtn = btn.ToString();
            string state = pressed ? "D" : "U";
            
            string cmd = "MouseClick, {0} , , , 1 , , {1}, ";
            ahk.ExecRaw(string.Format(cmd, sbtn, state));
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
            if (bindings.Length == 0) return;

            var kb = bindings[(int)action].KeyBinding;
            var ms = bindings[(int)action].MouseBinding;

            if (kb != DIKCode.NONE) SetKeyState(kb, pressed);
            if (ms != MouseCode.None) SetButtonState(ms, pressed);
        }

        /// <summary>
        /// triggers a game action by pressing its key. 
        /// </summary>
        /// <param name="action"></param>
        public static void PressAction(GameAction action)
        {
            PressAction(action, 20);
        }

        /// <summary>
        /// triggers a game action by pressing its key. 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="pressTimeMS">The amount of time to hold the key.</param>
        public static void PressAction(GameAction action, int pressTimeMS)
        {
            SetGameActionState(action, true);
            Thread.Sleep(pressTimeMS);
            SetGameActionState(action, false);
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

        private delegate void SafeInvokeDelegate(InputForm f);
        private static void SafeInvoke(Action<InputForm> action)
        {
            if (hookWnd.InvokeRequired)
                hookWnd.Invoke(new SafeInvokeDelegate(action), hookWnd);

            else action(hookWnd);
        }

        private delegate T SafeInvokeSetDelegate<T>(InputForm f);
        private static T SafeInvoke<T>(Func<InputForm, T> action)
        {
            if (hookWnd.InvokeRequired)
                return (T)hookWnd.Invoke(new SafeInvokeSetDelegate<T>(action), hookWnd);

            else return action(hookWnd);
        }

        /// <summary>
        /// Enable the input hook.
        /// </summary>
        public static void EnableInputHook()
        {
            SafeInvoke(f => f.EnableInputHook());
        }

        /// <summary>
        /// Disable the input hook.
        /// </summary>
        public static void DisableInputHook()
        {
            SafeInvoke(f => f.DisableInputHook());
        }

        /// <summary>
        /// Make the hook only monitor input when the given process is focused.
        /// </summary>
        /// <param name="procName"></param>
        public static void MakeProcessSpecific(string procName)
        {
            SafeInvoke(f => f.MakeLocal(procName));
        }

        /// <summary>
        /// Make the hook monitor input regardless of the focused process.
        /// </summary>
        public static void MakeGlobal()
        {
            SafeInvoke(f => f.MakeGlobal());
        }
        
        /// <summary>
        /// Test if the given key is pressed.
        /// </summary>
        /// <param name="keyCode">The key code.</param>
        /// <returns></returns>
        public static bool IsKeyPressed(DIKCode keyCode)
        {
            return SafeInvoke(f => f.IsKeyPressed(keyCode));
        }

        /// <summary>
        /// Test if the given key is toggled.
        /// </summary>
        /// <param name="keyCode">The key code.</param>
        /// <returns></returns>
        public static bool IsKeyToggled(DIKCode keyCode)
        {
            return SafeInvoke(f => f.IsKeyToggled(keyCode));
        }

        /// <summary>
        /// Test if the given key was put down before the call. Use it in loops to toggle stuff.
        /// </summary>
        /// <param name="keyCode">The key code.</param>
        /// <returns></returns>
        public static bool OnKeyDown(DIKCode keyCode)
        {
            return SafeInvoke(f => f.WasKeyPressed(keyCode));
        }

        /// <summary>
        /// Return all the pressed keys.
        /// </summary>
        /// <returns></returns>
        public static DIKCode[] GetPressedKeys()
        {
            return SafeInvoke(f => f.GetPressedKeys());
        }

        /// <summary>
        /// Test if the given mouse button is pressed.
        /// </summary>
        /// <param name="btn">The mouse button.</param>
        /// <returns></returns>
        public static bool IsButtonPressed(MouseButton btn)
        {
            return SafeInvoke(f => f.IsButtonPressed(btn));
        }

        /// <summary>
        /// Test if the given mouse code is pressed.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static bool IsButtonPressed(MouseCode code)
        {
            return IsButtonPressed(code.ToMouseButton());
        }

        /// <summary>
        /// Test if the given mouse button is toggled.
        /// </summary>
        /// <param name="btn">The mouse button.</param>
        /// <returns></returns>
        public static bool IsButtonToggled(MouseButton btn)
        {
            return SafeInvoke(f => f.IsButtonToggled(btn));
        }

        /// <summary>
        /// Test if the given mouse button is toggled.
        /// </summary>
        /// <param name="code">The mouse code.</param>
        /// <returns></returns>
        public static bool IsButtonToggled(MouseCode code)
        {
            return IsButtonToggled(code.ToMouseButton());
        }

        /// <summary>
        /// Test if the given game action is pressed.
        /// </summary>
        /// <param name="action">The action to test for.</param>
        /// <returns></returns>
        public static bool IsGameActionPressed(GameAction action)
        {
            if (bindings.Length == 0) return false;

            var binding = bindings[(int)action];
            return IsKeyPressed(binding.KeyBinding) || IsButtonPressed(binding.MouseBinding);
        }

        /// <summary>
        /// Get the current position of the mouse as an integer tuple.
        /// </summary>
        /// <returns></returns>
        public static Tuple<int, int> GetMousePos()
        {
            return SafeInvoke(f => f.GetMousePos());
        }

        #endregion
    }
}
