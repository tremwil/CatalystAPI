using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Catalyst.Native;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Catalyst.Input
{
    /// <summary>
    /// A background form used to process global hotkey messages.
    /// </summary>
    partial class InputForm : Form
    {
        private const int WM_KEYDOWN = 0x100;
        private const int WM_KEYUP = 0x101;

        private const int WM_MOUSEMOVE = 0x200;
        private int MOUSE_X = 0;
        private int MOUSE_Y = 0;

        private const int WM_MOUSEWHEEL = 0x20A;
        private const int MW_TIME_MS = 200;
        private uint MW_TICK = 0;

        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;

        private IntPtr hookKB = IntPtr.Zero;
        private IntPtr hookMS = IntPtr.Zero;

        private bool hookEnabled = false;
        public bool HookEnabled => hookEnabled;

        private int wheelState = 0;
        private byte[] pressedKeys = new byte[0xfe];

        public string TargetProcName { get; private set; }
        private bool keyInScope;

        private LowLevelInputProc keyboardCallback;
        private LowLevelInputProc mouseCallback;

        private Timer WindowCheck;

        public InputForm()
        {
            InitializeComponent();

            TargetProcName = "";
            keyInScope = false;

            WindowCheck = new Timer();
            WindowCheck.Interval = 100;
            WindowCheck.Tick += WindowCheck_Tick;

            keyboardCallback = LowLevelKeyboardProc;
            mouseCallback = LowLevelMouseProc;
        }

        private void WindowCheck_Tick(object sender, EventArgs e)
        {
            if (TargetProcName != "")
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
                    keyInScope = TargetProcName == System.IO.Path.GetFileNameWithoutExtension(fullPath);
                }
            }
            else
            {
                keyInScope = true;
            }
        }

        private void InputForm_Shown(object sender, EventArgs e)
        {
            // Here we must give the focus back to the previously focused window.
            // This code iterates trough all the windows in z-axis order until it
            // finds a visible one, then gives focus back to it. Fixes focus
            // stealing when overlay is started while in game

            IntPtr cwindow = Handle;
            while (true)
            {
                cwindow = WinAPI.GetWindow(cwindow, 2);
                if (cwindow == IntPtr.Zero) break;

                if (WinAPI.IsWindowVisible(cwindow))
                {
                    WinAPI.SetForegroundWindow(cwindow);
                    break;
                }
            }
        }

        public void MakeLocal(string procName)
        {
            TargetProcName = procName;
        }

        public void MakeGlobal()
        {
            TargetProcName = "";
        }

        public void EnableInputHook()
        {
            if (hookEnabled) return;

            hookKB = WinAPI.SetWindowsHookEx(WH_KEYBOARD_LL, keyboardCallback, IntPtr.Zero, 0);
            hookMS = WinAPI.SetWindowsHookEx(WH_MOUSE_LL, mouseCallback, IntPtr.Zero, 0);

            if (hookKB == IntPtr.Zero || hookMS == IntPtr.Zero)
                throw new InvalidOperationException("Could not set global hook");

            // Get state of toggle keys
            pressedKeys[0x3a] |= (byte)(WinAPI.GetKeyState(0x14) & 0x01); // caps lock
            pressedKeys[0x46] |= (byte)(WinAPI.GetKeyState(0x91) & 0x01); // scroll lock
            pressedKeys[0x45] |= (byte)(WinAPI.GetKeyState(0x90) & 0x01); // num lock

            hookEnabled = true;
            WindowCheck.Start();
        }

        public void DisableInputHook()
        {
            if (!hookEnabled) return;

            WinAPI.UnhookWindowsHookEx(hookKB);
            WinAPI.UnhookWindowsHookEx(hookMS);

            pressedKeys = new byte[0xfe];
            wheelState = 0;

            hookEnabled = false;
            WindowCheck.Stop();
        }

        public bool IsKeyPressed(DIKCode keyCode)
        {
            if (!keyInScope) return false;

            return (pressedKeys[(int)keyCode] & 2) != 0;
        }

        public bool IsKeyToggled(DIKCode keyCode)
        {
            if (!keyInScope) return false;

            return (pressedKeys[(int)keyCode] & 1) != 0;
        }

        public bool IsButtonPressed(MouseButton btn)
        {
            if (!keyInScope) return false;

            int ibtn = (int)btn;

            if (btn == MouseButton.WheelDown || btn == MouseButton.WheelUp)
            {
                uint tickcount = unchecked((uint)Environment.TickCount);
                return wheelState == ibtn && (tickcount - MW_TICK) < MW_TIME_MS;
            }

            return WinAPI.GetKeyState((ibtn > 3)? ibtn + 1 : ibtn) < 0;
        }

        public bool IsButtonToggled(MouseButton btn)
        {
            if (!keyInScope) return false;

            if (btn == MouseButton.WheelDown || btn == MouseButton.WheelUp)
            {
                return false;
            }

            int ibtn = (int)btn;
            return (WinAPI.GetKeyState((ibtn > 3) ? ibtn + 1 : ibtn) & 1) == 1;
        }

        public Tuple<int, int> GetMousePos()
        {
            return new Tuple<int, int>(MOUSE_X, MOUSE_Y);
        }

        // Code to make the form completely invisible in alt-tab and similar menus
        protected override CreateParams CreateParams
        {
            get
            {
                var Params = base.CreateParams;
                Params.ExStyle |= 0x80;
                return Params;
            }
        }

        private IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            int message = wParam.ToInt32();

            if (nCode >= 0)
            {
                KBDLLHOOKSTRUCT kbInfo = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
                int scancode = kbInfo.scanCode;

                if ((kbInfo.flags & 1) == 1) // Extended key
                    scancode = (scancode & 0xff) + 0x80;

                if (message == WM_KEYDOWN)
                {
                    if ((pressedKeys[scancode] & 2) == 0) pressedKeys[scancode] ^= 1;
                    pressedKeys[scancode] |= 2;
                }

                if (message == WM_KEYUP)
                    pressedKeys[scancode] &= 1;
            }

            return WinAPI.CallNextHookEx(hookKB, nCode, wParam, lParam);
        }

        private IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            int message = wParam.ToInt32();

            if (nCode >= 0)
            {
                MSLLHOOKSTRUCT mInfo = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);

                switch (message)
                {
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
    }
}
