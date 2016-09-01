using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalyst.Settings;
using AutoHotkey.Interop;
using System.Threading;
using System.Windows.Forms;

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

        #endregion

        #region hook related fields

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

            var sets = new Settings.Settings(); sets.Load();
            bindings = sets.Controls.AsArray();

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

            if (binding.KeyBinding != DIKCode.NONE)
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
            if (binding.KeyBinding != DIKCode.NONE)
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
        /// <param name="procName"></param>
        public static void MakeGlobal(string procName)
        {
            SafeInvoke(f => f.MakeGlobal());
        }

        /// <summary>
        /// Gets all the currently pressed keys.
        /// </summary>
        /// <returns></returns>
        public static DIKCode[] GetPressedKeys()
        {
            return SafeInvoke(f => f.GetPressedKeys());
        }

        /// <summary>
        /// Gets the currently pressed mouse buttons.
        /// </summary>
        /// <returns></returns>
        public static MouseButton[] GetPressedButtons()
        {
            return SafeInvoke(f => f.GetPressedButtons());
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
            return SafeInvoke(f => f.GetMousePos());
        }

        #endregion
    }
}
