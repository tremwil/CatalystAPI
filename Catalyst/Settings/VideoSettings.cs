using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Catalyst.Settings
{
    /// <summary>
    /// The window mode.
    /// </summary>
    public enum WindowMode
    {
        /// <summary>
        /// A borderless window. Takes up all the screen but is rendered inside a normal window.
        /// </summary>
        WindowBorderless,
        /// <summary>
        /// A normal window.
        /// </summary>
        Windowed,
        /// <summary>
        /// A mode where the game takes up all the screen and has a custom rendering procedure (not in window).
        /// </summary>
        Fullscreen
    }

    /// <summary>
    /// The video settings.
    /// </summary>
    public class VideoSettings : SettingsCategory
    {
        static VideoSettings()
        {
            @namespace = "GstRender";
        }

        internal VideoSettings(Settings parent) : base(parent) { }

        /// <summary>
        /// The window mode (windowed, fullscreen, etc).
        /// </summary>
        public WindowMode WindowMode
        {
            get
            {
                int fsc = (int)this["FullscreenEnabled"];
                int bde = (int)this["WindowBordersEnable"];
                return (WindowMode)(fsc * 2 | bde);
            }
            set
            {
                int fsc = (int)value / 2;
                int bde = (int)value & 1;

                this["FullscreenEnabled"] = fsc;
                this["WindowBordersEnable"] = bde;
            }
        }

        /// <summary>
        /// The game's resolution.
        /// </summary>
        public Size Resolution
        {
            get
            {
                return new Size(
                    (int)this["ResolutionWidth"],
                    (int)this["ResolutionHeight"]
                );
            }
            set
            {
                this["ResolutionWidth"] = value.Width;
                this["ResolutionHeight"] = value.Height;
            }
        }

        /// <summary>
        /// True if VSync (Vertical Syncronisation) is enabled.
        /// </summary>
        public bool VSyncEnabled
        {
            get { return Convert.ToBoolean((int)this["VSyncEnabled"]); }
            set { this["VSyncEnabled"] = Convert.ToInt32(value); }
        }
    }
}
