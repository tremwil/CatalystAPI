using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;

namespace Catalyst.Scripting
{
    class Field
    {
        public int priority;
        public string format;
    }

    class PythonVarField : Field
    {
        public string globalName;
    }

    class ManualUpdateField : Field
    {

    }

    public class OverlayWrapper
    {
        private OverlayForm overlayWnd;
        private Dictionary<string, Field> overlayMap;
        
        public Color Color
        {
            get { return overlayWnd.TextColor; }
            set { overlayWnd.TextColor = value; }
        }
        public Font Font
        {
            get { return overlayWnd.TextFont; }
            set { overlayWnd.TextFont = value; }
        }
        public int RefreshRate
        {
            get { return overlayWnd.RefreshRateMS; }
            set { overlayWnd.RefreshRateMS = value; }
        }
        public int Spacing
        {
            get { return overlayWnd.TextSpacing; }
            set { overlayWnd.TextSpacing = value; }
        }
        public Point Offset
        {
            get { return overlayWnd.TextOffset; }
            set { overlayWnd.TextOffset = value; }
        }

        public bool Enabled => overlayWnd.OverlayEnabled;
        public bool Displaying => overlayWnd.IsDisplaying;

        public OverlayWrapper()
        {
            overlayWnd = new OverlayForm("MirrorsEdgeCatalyst");
        }

        public void AddGlobalWatch(string fieldName, string formatString, string globalVarName)
        {
            
        }

        public void AddGlobalWatch(string fieldName, int priority, string formatString, string globalVarName)
        {

        }

        public void RemoveField(string fieldName)
        {

        }
    }
}
