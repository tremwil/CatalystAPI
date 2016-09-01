using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Catalyst.Unmanaged;
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

        private const int WM_LBUTTONDOWN = 0x201;
        private const int WM_LBUTTONUP = 0x202;

        private const int WM_RBUTTONDOWN = 0x204;
        private const int WM_RBUTTONUP = 0x205;

        private const int WM_MBUTTONDOWN = 0x207;
        private const int WM_MBUTTONUP = 0x208;

        private const int WM_XBUTTONDOWN = 0x20B;
        private const int WM_XBUTTONUP = 0x20C;

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
        private HashSet<DIKCode> pressedKeys = new HashSet<DIKCode>();
        private HashSet<MouseButton> pressedBtns = new HashSet<MouseButton>();

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

            hookEnabled = true;
            WindowCheck.Start();
        }

        public void DisableInputHook()
        {
            if (!hookEnabled) return;

            WinAPI.UnhookWindowsHookEx(hookKB);
            WinAPI.UnhookWindowsHookEx(hookMS);

            pressedKeys.Clear();
            pressedBtns.Clear();
            wheelState = 0;

            hookEnabled = false;
            WindowCheck.Stop();
        }

        public DIKCode[] GetPressedKeys()
        {
            if (!keyInScope) { return new DIKCode[0]; }

            return pressedKeys.ToArray();
        }

        public MouseButton[] GetPressedButtons()
        {
            if (!keyInScope) { return new MouseButton[0]; }

            uint tickcount = unchecked((uint)Environment.TickCount);

            if ((tickcount - MW_TICK) < MW_TIME_MS && wheelState != 0)
                return pressedBtns.Concat(
                    new MouseButton[1] { (MouseButton)wheelState }).ToArray();

            return pressedBtns.ToArray();
        }

        public bool IsKeyPressed(DIKCode keyCode)
        {
            if (!keyInScope) return false;

            return pressedKeys.Contains(keyCode);
        }

        public bool IsButtonPressed(MouseButton btn)
        {
            if (!keyInScope) return false;

            if (btn == MouseButton.WheelDown || btn == MouseButton.WheelUp)
            {
                uint tickcount = unchecked((uint)Environment.TickCount);
                return wheelState == (int)btn && (tickcount - MW_TICK) < MW_TIME_MS;
            }

            return pressedBtns.Contains(btn);
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

        private IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam)
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
    }
}
