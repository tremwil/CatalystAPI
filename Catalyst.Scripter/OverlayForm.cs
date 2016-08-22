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
using Catalyst.Memory;
using System.IO;
using System.Diagnostics;
using System.Drawing.Text;

namespace Catalyst.Scripting
{
    public partial class OverlayForm : Form
    {
        private RECT tgtWindowRect;
        private IntPtr TargetHandle;
        public string TargetProcName { get; set; }

        private Timer refreshTimer;

        public int RefreshRateMS
        {
            get { return refreshTimer.Interval; }
            set { refreshTimer.Interval = value; }
        }

        public bool IsDisplaying { get; private set; }
        public bool OverlayEnabled { get; private set; }
        public bool Attached { get; private set; }

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

            IsDisplaying = false;
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

        private void OverlayForm_Load(object sender, EventArgs e)
        {
            Hide();
        }

        public void EnableOverlay()
        {
            if (OverlayEnabled) return;

            IsDisplaying = AttachToTarget();
            OverlayEnabled = true;
            refreshTimer.Start();

            Show();
        }

        public void DisableOverlay()
        {
            if (!OverlayEnabled) return;

            DetachFromTarget();
            OverlayEnabled = false;
            refreshTimer.Stop();

            IsDisplaying = false;
            Invalidate();
            Hide();
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
            NativeMethods.SetWindowLongPtr(Handle, -8, TargetHandle);

            Attached = true;
            return true;
        }

        private void DetachFromTarget()
        {
            // Setting IntPtr.Zero as owner removes it
            TargetHandle = IntPtr.Zero;
            NativeMethods.SetWindowLongPtr(Handle, -8, IntPtr.Zero);
            Attached = false;
        } 

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            UpdateWindowInfo();

            if (Attached)
            {
                Invalidate();
            }
            else
            {
                var temp = IsDisplaying;
                IsDisplaying = AttachToTarget();

                if (temp == true)
                    Invalidate();
            }
        }

        private void UpdateWindowInfo()
        {
            if (TargetHandle == IntPtr.Zero)
                return;

            if (!NativeMethods.IsWindow(TargetHandle))
            {
                DetachFromTarget();
                return;
            }

            NativeMethods.GetWindowRect(TargetHandle, out tgtWindowRect);
        }

        private void OverlayForm_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // Remove AA problems
            g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;

            if (!IsDisplaying)
                return;

            var brush = new SolidBrush(TextColor);

            float pX = tgtWindowRect.x2 - TextOffset.X;
            float pY = tgtWindowRect.y2 - TextOffset.Y;

            string text;
            SizeF strSize;

            for (int i = Overlays.Count - 1; i > -1; i--)
            {
                text = Overlays[i].ToString();
                strSize = g.MeasureString(text, TextFont);

                pY -= strSize.Height;
                g.DrawString(text, TextFont, brush, pX - strSize.Width, pY);
                pY -= TextSpacing;
            }
        }

        private bool forceClose = false;
        private void OverlayForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!forceClose) e.Cancel = true;
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
