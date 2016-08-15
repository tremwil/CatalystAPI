using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX.DirectInput;

namespace Catalyst.Input
{
    /// <summary>
    /// The Direct Input Keyboard (DIK) codes supported by ME:C.
    /// <para></para>Some have been renamed to facilitate integration
    /// with AutoItX.
    /// </summary>
    public enum DIKCode
    {
        // No need for documentation here...
        #pragma warning disable 1591

        DIK_NONE = 0x00,
        DIK_ESCAPE = 0x01,
        DIK_1 = 0x02,
        DIK_2 = 0x03,
        DIK_3 = 0x04,
        DIK_4 = 0x05,
        DIK_5 = 0x06,
        DIK_6 = 0x07,
        DIK_7 = 0x08,
        DIK_8 = 0x09,
        DIK_9 = 0x0A,
        DIK_0 = 0x0B,
        DIK_MINUS = 0x0C,
        DIK_EQUALS = 0x0D,
        DIK_BACKSPACE = 0x0E,
        DIK_TAB = 0x0F,
        DIK_Q = 0x10,
        DIK_W = 0x11,
        DIK_E = 0x12,
        DIK_R = 0x13,
        DIK_T = 0x14,
        DIK_Y = 0x15,
        DIK_U = 0x16,
        DIK_I = 0x17,
        DIK_O = 0x18,
        DIK_P = 0x19,
        DIK_LBRACKET = 0x1A,
        DIK_RBRACKET = 0x1B,
        DIK_ENTER = 0x1C,
        DIK_LCTRL = 0x1D,
        DIK_A = 0x1E,
        DIK_S = 0x1F,
        DIK_D = 0x20,
        DIK_F = 0x21,
        DIK_G = 0x22,
        DIK_H = 0x23,
        DIK_J = 0x24,
        DIK_K = 0x25,
        DIK_L = 0x26,
        DIK_SEMICOLON = 0x27,
        DIK_APOSTROPHE = 0x28,
        DIK_BACKQUOTE = 0x29,
        DIK_LSHIFT = 0x2A,
        DIK_BACKSLASH = 0x2B,
        DIK_Z = 0x2C,
        DIK_X = 0x2D,
        DIK_C = 0x2E,
        DIK_V = 0x2F,
        DIK_B = 0x30,
        DIK_N = 0x31,
        DIK_M = 0x32,
        DIK_COMMA = 0x33,
        DIK_PERIOD = 0x34,
        DIK_SLASH = 0x35,
        DIK_RSHIFT = 0x36,
        DIK_NUMPADMULT = 0x37,
        DIK_LALT = 0x38,
        DIK_SPACE = 0x39,
        DIK_CAPSLOCK = 0x3A,
        DIK_F1 = 0x3B,
        DIK_F2 = 0x3C,
        DIK_F3 = 0x3D,
        DIK_F4 = 0x3E,
        DIK_F5 = 0x3F,
        DIK_F6 = 0x40,
        DIK_F7 = 0x41,
        DIK_F8 = 0x42,
        DIK_F9 = 0x43,
        DIK_F10 = 0x44,
        DIK_NUMLOCK = 0x45,
        DIK_SCROLLLOCK = 0x46,
        DIK_NUMPAD7 = 0x47,
        DIK_NUMPAD8 = 0x48,
        DIK_NUMPAD9 = 0x49,
        DIK_NUMPADMINUS = 0x4A,
        DIK_NUMPAD4 = 0x4B,
        DIK_NUMPAD5 = 0x4C,
        DIK_NUMPAD6 = 0x4D,
        DIK_NUMPADPLUS = 0x4E,
        DIK_NUMPAD1 = 0x4F,
        DIK_NUMPAD2 = 0x50,
        DIK_NUMPAD3 = 0x51,
        DIK_NUMPAD0 = 0x52,
        DIK_NUMPADDOT = 0x53,
        DIK_OEM_102 = 0x56,
        DIK_F11 = 0x57,
        DIK_F12 = 0x58,
        DIK_ABNT_C1 = 0x73,
        DIK_ABNT_C2 = 0x7E,
        DIK_MEDIA_PREV = 0x90,
        DIK_MEDIA_NEXT = 0x99,
        DIK_NUMPADENTER = 0x9C,
        DIK_RCTRL = 0x9D,
        DIK_VOLUME_MUTE = 0xA0,
        DIK_CALCULATOR = 0xA1,
        DIK_PLAYPAUSE = 0xA2,
        DIK_MEDIA_STOP = 0xA4,
        DIK_VOLUME_DOWN = 0xAE,
        DIK_VOLUME_UP = 0xB0,
        DIK_BROWSER_HOME = 0xB2,
        DIK_NUMPADDIV = 0xB5,
        DIK_PRINTSCREEN = 0xB7,
        DIK_RALT = 0xB8,
        DIK_PAUSE = 0xC5,
        DIK_HOME = 0xC7,
        DIK_UP = 0xC8,
        DIK_PGUP = 0xC9,
        DIK_LEFT = 0xCB,
        DIK_RIGHT = 0xCD,
        DIK_END = 0xCF,
        DIK_DOWN = 0xD0,
        DIK_PGDN = 0xD1,
        DIK_INSERT = 0xD2,
        DIK_DELETE = 0xD3,
        DIK_LWIN = 0xDB,
        DIK_RWIN = 0xDC,
        DIK_APPSKEY = 0xDD,
        DIK_SLEEP = 0xDF,
        DIK_BROWSER_SEARCH = 0xE5,
        DIK_BROWSER_FAVORITES = 0xE6,
        DIK_BROWSER_REFRESH = 0xE7,
        DIK_BROWSER_STOP = 0xE8,
        DIK_BROWSER_FORWARD = 0xE9,
        DIK_BROWSER_BACK = 0xEA,
        DIK_LAUNCH_MAIL = 0xEC,
        DIK_LAUNCH_MEDIA = 0xED

        // Get the doc warning back
        #pragma warning restore 1591
    }

    /// <summary>
    /// A static class with functions on DIK codes.
    /// </summary>
    public static class DIKCodes
    {
        private static Dictionary<string, string> NonLitteralCodeMap = new Dictionary<string, string>()
        {
            { "NONE", "" },
            { "MINUS", "-" },
            { "EQUALS", "=" },
            { "LBRACKET", "[" },
            { "RBRACKET", "]" },
            { "SEMICOLON", ";" },
            { "APOSTROPHE", "'" },
            { "BACKQUOTE", "`" },
            { "BACKSLASH", "\\" },
            { "COMMA", "," },
            { "PERIOD", "." },
            { "SLASH", "/" },
            { "OEM_102", "<" },
            { "ABNT_C1", "/" },
            { "ABNT_C2", "NUMPADDOT" }
        };

        /// <summary>
        /// Convert to a key code to its string equivalent.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string CodeToKeyString(DIKCode code)
        {
            // Get the string of the code without the DIK_ part
            var cstr = code.ToString().Substring(4);

            // If non-litteral, get the appropriate symbol
            if (NonLitteralCodeMap.ContainsKey(cstr))
                return NonLitteralCodeMap[cstr];

            return cstr;
        }

        /// <summary>
        /// Convert a integer version of the code to its string equivalent.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string CodeToKeyString(int code)
        {
            return CodeToKeyString(Parse(code));
        }

        /// <summary>
        /// Parse a 
        /// </summary>
        /// <param name="sharpDXCode"></param>
        /// <returns></returns>
        public static DIKCode Parse(Key sharpDXCode)
        {
            try
            {
                return (DIKCode)sharpDXCode;
            }
            catch (InvalidCastException e)
            {
                throw new ArgumentException(
                    "The key code provided is not supported by Catalyst.",
                    "code",
                    e
                );
            }
        }

        /// <summary>
        /// Parse an integer representing a DIK code to its representing enum value.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static DIKCode Parse(int code)
        {
            try
            {
                return (DIKCode)code;
            }
            catch (InvalidCastException e)
            {
                throw new ArgumentException(
                    "The key code provided is invalid, requires a modifier or comes from an unsupported keyboard.",
                    "code",
                    e
                );
            }
        }

        /// <summary>
        /// Convert key names used by AutoItX to DIK codes.
        /// </summary>
        /// <param name="autoItName">The AutoItX name.</param>
        /// <param name="isLitteral">If the name contains non-alphanumeric characters.</param>
        /// <returns></returns>
        public static DIKCode Parse(string autoItName, bool isLitteral = true)
        {
            string cstr = autoItName;

            // First test if the character is litteral and get the non-litteral value
            if (NonLitteralCodeMap.ContainsValue(autoItName) && isLitteral)
                cstr = (from kvp in NonLitteralCodeMap
                       where kvp.Value == autoItName
                       select kvp.Key).First();

            try
            {
                // Parse the DIKCode string
                return (DIKCode)Enum.Parse(typeof(DIKCode), "DIK_" + cstr);
            }
            catch (Exception e)
            {
                throw new ArgumentException(
                    "The key code provided is invalid, requires a modifier or comes from an unsupported keyboard.",
                    "code",
                    e
                );
            }
        }

        /* EXTENSION METHODS */

        /// <summary>
        /// convert this DIK code to a SharpDX key.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static Key ToSharpDXCode(this DIKCode code)
        {
            return (Key)code;
        }

        /// <summary>
        /// Convert a SharpDX keycode to the appropriate DIKCode.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static DIKCode ToDIKCode(this Key code)
        {
            return Parse(code);
        }
    }
}
