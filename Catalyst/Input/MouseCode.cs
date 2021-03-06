﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Catalyst.Input
{
    /// <summary>
    /// The mouse buttons that can be detected and sent by the InputController.
    /// </summary>
    public enum MouseButton
    {
        /// <summary>
        /// Represents no button.
        /// </summary>
        None,
        /// <summary>
        /// The left mouse button.
        /// </summary>
        Left,
        /// <summary>
        /// The right mouse button.
        /// </summary>
        Right,
        /// <summary>
        /// The middle mouse button.
        /// </summary>
        Middle,
        /// <summary>
        /// The X1 mouse button.
        /// </summary>
        X1,
        /// <summary>
        /// The X2 mouse button.
        /// </summary>
        X2,
        /// <summary>
        /// The action of moving the mousewheel upwards.
        /// </summary>
        WheelUp,
        /// <summary>
        /// The action of moving the mousewheel downwards.
        /// </summary>
        WheelDown,
    }

    /// <summary>
    /// The game-supported mouse buttons.
    /// </summary>
    public enum MouseCode
    {
        // EXAMPLE: LEFT

        // int size  256       128      1
        // size    8 bits    7 bits   1 bit
        // hex       18         0       0
        // bin    00011000   0000000    0
        // calc  (24 << 8) | (0 << 1) | 0,
        // name     axis     button   negate

        /// <summary>
        /// Represents an empty mouse code.
        /// </summary>
        None = 0x00,
        /// <summary>
        /// The left mouse button.
        /// </summary>
        Left = 0x1800,
        /// <summary>
        /// The right mouse button.
        /// </summary>
        Right = 0x1802,
        /// <summary>
        /// The middle mouse button.
        /// </summary>
        Middle = 0x1804,
        /// <summary>
        /// The mousewheel upwards scrolling.
        /// </summary>
        WheelUp = 0x0D10,
        /// <summary>
        /// The mousewheel downwards scrolling.
        /// </summary>
        WheelDown = 0x0D11,
    }

    /// <summary>
    /// A class containing extension methods for the MouseCode enum.
    /// </summary>
    public static class MouseCodes
    {
        /// <summary>
        /// The normal button axis (left, middle, right).
        /// </summary>
        public static int NormalAxis => 24;
        /// <summary>
        /// The mousewheel axis.
        /// </summary>
        public static int MousewheelAxis => 13;

        /// <summary>
        /// Convert this mouse code to a raw code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        public static RawMouseCode ToRawCode(this MouseCode code)
        {
            return new RawMouseCode(code);
        }

        /// <summary>
        /// Convert this code to a mouse button.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        public static MouseButton ToMouseButton(this MouseCode code)
        {
            return (MouseButton)Enum.Parse(typeof(MouseButton), code.ToString());
        }
    }

    /// <summary>
    /// A class to handle raw game codes.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RawMouseCode
    {
        /// <summary>
        /// The axis of the code.
        /// </summary>
        public int Axis { get; }
        /// <summary>
        /// The button of the code.
        /// </summary>
        public int Button { get; }
        /// <summary>
        /// The negation value of the code.
        /// </summary>
        public int Negate { get; }

        /// <summary>
        /// Create a raw mouse code from an enum value.
        /// </summary>
        /// <param name="code"></param>
        public RawMouseCode(MouseCode code)
        {
            int btn = (int)code;

            Axis = (btn & 0xFF00) >> 8;
            Button = (btn & 0x00FE) >> 1;
            Negate = (btn & 0x0001) >> 0;
        }

        /// <summary>
        /// Create a mouse code from this instance.
        /// </summary>
        /// <returns></returns>
        public MouseCode ToMouseCode()
        {
            return (MouseCode)((Axis << 8) | (Button << 1) | Negate);
        }

        /// <summary>
        /// Create a mouse code from axis, button and negate values.
        /// </summary>
        /// <param name="axis">The axis of the code</param>
        /// <param name="button">The button of the code</param>
        /// <param name="negate">The negation boolean (0 or 1) of the code</param>
        /// <returns></returns>
        public static MouseCode ToMouseCode(int axis, int button, int negate)
        {
            return (MouseCode)((axis << 8) | (button << 1) | negate);
        }

        /// <summary>
        /// Creates a RawMouseCode from an enum value. Same as new RawMouseCode(code).
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static RawMouseCode FromMouseCode(MouseCode code)
        {
            return new RawMouseCode(code);
        }
    }
}
