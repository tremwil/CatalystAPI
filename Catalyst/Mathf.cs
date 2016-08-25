using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalyst
{
    /// <summary>
    /// A bunch of math-related functions.
    /// </summary>
    public static class Mathf
    {
        /// <summary>
        /// Returns the remainder of a/b using B's sign. Use it for angle stuff.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float DivMod(float a, float b)
        {
            return ((a % b) + b) % b;
        }

        #pragma warning disable 1591

        public const float PI = 3.14159265359f;
        public const float PI2 = 6.28318530718f;

        public const float E = 2.71828182846f;

        public const float SQRT2 = 1.41421356237f;

        public const float DegToRad = 0.01745329251f;
        public const float RadToDeg = 57.2957795131f;
    }
}
