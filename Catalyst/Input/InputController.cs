using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalyst.Settings;
using AutoHotkey.Interop;
using System.Threading;
using System.Diagnostics;
using Catalyst.Unmanaged;
using System.Runtime.InteropServices;

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

        private const int WM_KEYDOWN = 0x100;
        private const int WM_KEYUP = 0x101;

        private const int WM_MOUSEMOVE = 0x200;
        private static int MOUSE_X = 0;
        private static int MOUSE_Y = 0; 

        private const int WM_LBUTTONDOWN = 0x201;
        private const int WM_LBUTTONUP = 0x202;

        private const int WM_RBUTTONDOWN = 0x204;
        private const int WM_RBUTTONUP = 0x205;

        private const int WM_MBUTTONDOWN = 0x207;
        private const int WM_MBUTTONUP = 0x208;

        private const int WM_XBUTTONDOWN = 0x20B;
        private const int WM_XBUTTONUP = 0x20C;

        private const int WM_MOUSEWHEEL = 0x20A;
        private const int MW_TIME_MS = 40;
        private static uint MW_TICK = 0;

        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;

        private static IntPtr hookKB = IntPtr.Zero;
        private static IntPtr hookMS = IntPtr.Zero;
        private static bool hookEnabled = false;

        private static int wheelState = 0;
        private static List<DIKCode> pressedKeys;
        private static List<MouseButton> pressedBtns;

        #endregion

        #region static constructor

        static InputController()
        {
            ahk = new AutoHotkeyEngine();

            var sets = new Settings.Settings();
            bindings = sets.Controls.AsArray();
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

            if (binding.KeyBinding != DIKCode.DIK_NONE)
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
            if (binding.KeyBinding != DIKCode.DIK_NONE)
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

        /// <summary>
        /// Enable the global input hook.
        /// </summary>
        public static void EnableInputHook()
        {
            if (hookEnabled) return;

            pressedKeys = new List<DIKCode>();
            pressedBtns = new List<MouseButton>();

            string mname = Process.GetCurrentProcess().MainModule.ModuleName;
            IntPtr hModule = WinAPI.GetModuleHandle(mname);

            hookKB = WinAPI.SetWindowsHookEx(WH_KEYBOARD_LL, LowLevelKeyboardProc, hModule, 0);
            hookMS = WinAPI.SetWindowsHookEx(WH_MOUSE_LL, LowLevelMouseProc, hModule, 0);

            if (hookKB == IntPtr.Zero || hookMS == IntPtr.Zero)
                throw new InvalidOperationException("Could not set global hook");

            hookEnabled = true;
        }

        /// <summary>
        /// Disable the global input hooks.
        /// </summary>
        public static void DisableInputHook()
        {
            if (!hookEnabled) return;

            WinAPI.UnhookWindowsHookEx(hookKB);
            WinAPI.UnhookWindowsHookEx(hookMS);

            hookEnabled = false;
        }

        /// <summary>
        /// Gets all the currently pressed keys.
        /// </summary>
        /// <returns></returns>
        public static DIKCode[] GetPressedKeys()
        {
            return pressedKeys.ToArray();
        }

        /// <summary>
        /// Gets the currently pressed mouse buttons.
        /// </summary>
        /// <returns></returns>
        public static MouseButton[] GetPressedButtons()
        {
            uint tickcount = unchecked((uint)Environment.TickCount);

            if ((tickcount - MW_TICK) < MW_TIME_MS && wheelState != 0)
                return pressedBtns.Concat(
                    new MouseButton[1] { (MouseButton)wheelState }).ToArray();

            return pressedBtns.ToArray();
        }

        /// <summary>
        /// Test if the given key is pressed.
        /// </summary>
        /// <param name="keyCode">The key code.</param>
        /// <returns></returns>
        public static bool IsKeyPressed(DIKCode keyCode)
        {
            return pressedKeys.Contains(keyCode);
        }

        /// <summary>
        /// Test if the given mouse button is pressed.
        /// </summary>
        /// <param name="btn">The mouse button.</param>
        /// <returns></returns>
        public static bool IsButtonPressed(MouseButton btn)
        {
            if (btn == MouseButton.WheelDown || btn == MouseButton.WheelUp)
            {
                uint tickcount = unchecked((uint)Environment.TickCount);
                return wheelState == (int)btn && (tickcount - MW_TICK) < MW_TIME_MS;
            }

            return pressedBtns.Contains(btn);
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

        #endregion

        #region input hook procedures

        private static IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            int message = wParam.ToInt32();

            if (nCode >= 0)
            {
                int scancode = Marshal.ReadInt32(lParam, 4);

                DIKCode dik;
                bool sucess = DIKCodes.TryParse(scancode, out dik);

                if (sucess && message == WM_KEYDOWN)
                    pressedKeys.Add(dik);

                if (sucess && message == WM_KEYUP)
                    pressedKeys.Remove(dik);
            }

            return WinAPI.CallNextHookEx(hookKB, nCode, wParam, lParam);
        }

        private static IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            int message = wParam.ToInt32();

            if (nCode >= 0)
            {
                MSINFO mInfo = Marshal.PtrToStructure<MSINFO>(lParam);
                MouseButton but;

                switch (message)
                {
                    case WM_LBUTTONDOWN:
                        pressedBtns.Add(MouseButton.Left);
                        break;
                    case WM_LBUTTONUP:
                        pressedBtns.Remove(MouseButton.Left);
                        break;

                    case WM_RBUTTONDOWN:
                        pressedBtns.Add(MouseButton.Right);
                        break;
                    case WM_RBUTTONUP:
                        pressedBtns.Remove(MouseButton.Right);
                        break;

                    case WM_MBUTTONDOWN:
                        pressedBtns.Add(MouseButton.Middle);
                        break;
                    case WM_MBUTTONUP:
                        pressedBtns.Remove(MouseButton.Middle);
                        break;

                    case WM_XBUTTONDOWN:
                        but = (mInfo.mouseData >> 16 == 1) ? MouseButton.X1 : MouseButton.X2;
                        pressedBtns.Add(but);
                        break;
                    case WM_XBUTTONUP:
                        but = (mInfo.mouseData >> 16 == 1) ? MouseButton.X1 : MouseButton.X2;
                        pressedBtns.Remove(but);
                        break;
                        
                    case WM_MOUSEWHEEL:
                        wheelState = (mInfo.mouseData > 0) ? 6 : 7;
                        MW_TICK = mInfo.time;
                        break;

                    case WM_MOUSEMOVE:
                        MOUSE_X = mInfo.x;
                        MOUSE_Y = mInfo.y;
                        break;
                }
            }

            return WinAPI.CallNextHookEx(hookKB, nCode, wParam, lParam);
        }

        #endregion
    }
}
