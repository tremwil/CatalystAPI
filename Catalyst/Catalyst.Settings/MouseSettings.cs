using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalyst.Settings
{
    /// <summary>
    /// The mouse settings.
    /// </summary>
    public class MouseSettings : SettingsCategory
    {
        static MouseSettings()
        {
            @namespace = "GstInput";
        }

        internal MouseSettings(Settings parent) : base(parent) { }

        /// <summary>
        /// The mouse sensivity (from 0 to 1.5)
        /// </summary>
        public float Sensivity
        {
            get { return (float)this["MouseSensivity"]; }
            set { this["MouseSensivity"] = value; }
        }

        /// <summary>
        /// If true, the Y axis is inverted.
        /// </summary>
        public bool InvertYAxis
        {
            get { return Convert.ToBoolean((int)this["MouseSensivity"]); }
            set { this["MouseSensivity"] = Convert.ToInt32(value); }
        }
    }
}
