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
        private static InputBinding[] bindings;
        private static AutoHotkeyEngine ahk;

        // Disgusting, but better than static constructor
        private static int _ = Init();
        static int Init()
        {
            ahk = new AutoHotkeyEngine();

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

        // Keyboard / mouse hooks

        private const int WM_KEYDOWN = 0x100;
        private const int WM_KEYUP = 0x101;

        private const int WM_LBUTTONDOWN = 0x201;
        private const int WM_LBUTTONUP = 0x202;

        private const int WM_RBUTTONDOWN = 0x204;
        private const int WM_RBUTTONUP = 0x205;

        private const int WM_MBUTTONDOWN = 0x207;
        private const int WM_MBUTTONUP = 0x208;

        private const int WM_MOUSEWHEEL = 0x20A;
        private const int WHEEL_DELTA = 120;

        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;

        private static IntPtr hookKB = IntPtr.Zero;
        private static IntPtr hookMS = IntPtr.Zero;
        private static bool hookEnabled = false;

        private static Type KBINFO_TYPE = typeof(KBINFO);
        private static Type MSINFO_TYPE = typeof(MSINFO);

        private static List<DIKCode> pressedKeys;
        private static List<MouseButton> pressedBtns;

        private static void SetLLHook()
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

        private static void UnsetLLHook()
        {
            if (!hookEnabled) return;

            WinAPI.UnhookWindowsHookEx(hookKB);
            WinAPI.UnhookWindowsHookEx(hookMS);

            hookEnabled = false;
        }

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

                //switch (message)
            }

            return WinAPI.CallNextHookEx(hookKB, nCode, wParam, lParam);
        }
    }
}
