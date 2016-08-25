using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing.Text;

using Catalyst.Unmanaged;

namespace Catalyst.Display
{
    partial class OverlayForm : Form
    {
        private RECT tgtWindowRect;
        private IntPtr TargetHandle;
        public string TargetProcName { get; set; }

        private Timer refreshTimer;

        public bool autorizedToDraw;
        private Rectangle[] invalidRegions;
        private string[] formattedStrings;

        public int RefreshRateMS
        {
            get { return refreshTimer.Interval; }
            set { refreshTimer.Interval = value; }
        }

        public bool OverlayEnabled { get; private set; }

        public bool Attached { get; private set; }
        public bool IsDisplaying => Attached;

        public int TextSpacing { get; set; }
        public Point TextOffset { get; set; }

        public Font TextFont { get; set; }
        public Color TextColor { get; set; }

        public List<OverlayField> Overlays;

        public OverlayForm()
        {
            InitializeComponent();
            ShowInTaskbar = false;
            Size = Screen.PrimaryScreen.Bounds.Size;

            refreshTimer = new Timer();
            refreshTimer.Tick += RefreshTimer_Tick;

            Overlays = new List<OverlayField>();
            RefreshRateMS = 70;
            TargetProcName = "";

            autorizedToDraw = false;
            OverlayEnabled = false;
            Attached = false;

            TextFont = new Font("Courier New", 25, FontStyle.Bold);
            TextColor = Color.Red;
            TextSpacing = 0;
            TextOffset = new Point(30, 30);
        }

        public OverlayForm(string targetProcName) : this()
        {
            TargetProcName = targetProcName;
        }

        private void OverlayForm_Shown(object sender, EventArgs e)
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

        public void EnableOverlay()
        {
            if (OverlayEnabled) return;

            OverlayEnabled = true;
            refreshTimer.Start();

            invalidRegions = new Rectangle[0];
        }

        public void DisableOverlay()
        {
            if (!OverlayEnabled) return;

            DetachFromTarget();
            OverlayEnabled = false;
            refreshTimer.Stop();

            Invalidate();
        }

        private bool AttachToTarget()
        {
            // The Attach() function allows the form to cover
            // the target window even if the target is topmost.
            // Returns true if the attach was sucessful.

            Process[] processes = Process.GetProcessesByName(TargetProcName);

            if (processes.Length == 0)
            {
                TargetHandle = IntPtr.Zero;
                return false;
            }

            TargetHandle = processes[0].MainWindowHandle;
            WinAPI.SetWindowLongPtr(Handle, -8, TargetHandle);

            // We are attached, so set our Z order to the target's one
            WinAPI.SetWindowZOrder(Handle, TargetHandle, 0x210); // No owner Z order & no activate

            Attached = true;
            return true;
        }

        private void DetachFromTarget()
        {
            // Setting IntPtr.Zero as owner removes it
            TargetHandle = IntPtr.Zero;
            WinAPI.SetWindowLongPtr(Handle, -8, IntPtr.Zero);
            Attached = false;

            InvalidateOld();
        }

        private void InvalidateOld()
        {
            foreach (var rgn in invalidRegions)
                Invalidate(rgn);
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            UpdateWindowInfo();

            if (Attached)
            {
                // Invalidate previous regions to
                InvalidateOld();

                Graphics g = Graphics.FromHwnd(IntPtr.Zero);

                int pX = tgtWindowRect.x2 - TextOffset.X;
                int pY = tgtWindowRect.y2 - TextOffset.Y;

                string text;
                formattedStrings = new string[Overlays.Count];
                invalidRegions = new Rectangle[Overlays.Count];

                Point loc;
                SizeF strSizeF;
                Size strSize;

                for (int i = Overlays.Count - 1; i > -1; i--)
                {
                    text = Overlays[i].ToString();
                    formattedStrings[i] = text;

                    strSizeF = g.MeasureString(text, TextFont);
                    strSize = new Size((int)strSizeF.Width, (int)strSizeF.Height);

                    pY -= strSize.Height;

                    loc = new Point(pX - strSize.Width, pY);
                    Rectangle invalidRgn = new Rectangle(loc, strSize);
                    invalidRegions[i] = invalidRgn;
                    Invalidate(invalidRgn, false);

                    pY -= TextSpacing;
                }

                autorizedToDraw = true;
                g.Dispose();
            }
            else
            {
                AttachToTarget();
            }
        }

        private void UpdateWindowInfo()
        {
            if (TargetHandle == IntPtr.Zero)
                return;

            if (!WinAPI.IsWindow(TargetHandle))
            {
                DetachFromTarget();
                return;
            }

            WinAPI.GetWindowRect(TargetHandle, out tgtWindowRect);
        }

        private bool forceClose = false;
        private void OverlayForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!forceClose) e.Cancel = true;
        }

        public new void Close()
        {
            forceClose = true;
            base.Close();
        }

        private void OverlayForm_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            if (!autorizedToDraw)
                return;

            // Remove AA problems
            g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;

            var brush = new SolidBrush(TextColor);
            Point loc;
            string text;

            for (int i = 0; i < Overlays.Count; i++)
            {
                loc = invalidRegions[i].Location;
                text = formattedStrings[i];

                g.DrawString(text, TextFont, brush, loc);
            }

            brush.Dispose();
            autorizedToDraw = false;
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

        // Message procedure - remove mouse click activation

        private const int WM_MOUSEACTIVATE = 0x0021, MA_NOACTIVATE = 0x0003;

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_MOUSEACTIVATE)
            {
                m.Result = (IntPtr)MA_NOACTIVATE;
                return;
            }
            base.WndProc(ref m);
        }
    }
}
