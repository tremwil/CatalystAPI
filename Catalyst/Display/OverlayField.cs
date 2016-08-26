using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Runtime.InteropServices;

namespace Catalyst.Display
{
    class OverlayField
    {
        public string FormatString { get; set; }
        public Func<object> Getter { get; set; }

        public OverlayField(string format, Func<object> getter)
        {
            FormatString = format;
            Getter = getter;
        }

        public string GetString()
        {
            return string.Format(FormatString, Getter());
        }
    }
}