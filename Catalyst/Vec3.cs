using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Catalyst
{
    /// <summary>
    /// A vector in 3-dimensional space with XYZ components.
    /// The coordinate system is right-handed.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Vec3
    {
        /// <summary>
        /// The X component.
        /// </summary>
        public readonly float x;
        /// <summary>
        /// The Y component.
        /// </summary>
        public readonly float y;
        /// <summary>
        /// The Z component.
        /// </summary>
        public readonly float z;

        /// <summary>
        /// The zero vector.
        /// </summary>
        public static Vec3 Zero => new Vec3(0, 0, 0);
        /// <summary>
        /// A unit vector pointing towards the X axis.
        /// </summary>
        public static Vec3 AxisX => new Vec3(1, 0, 0);
        /// <summary>
        /// A unit vector pointing towards the Y axis.
        /// </summary>
        public static Vec3 AxisY => new Vec3(0, 1, 0);
        /// <summary>
        /// A unit vector pointing towards the Z axis.
        /// </summary>
        public static Vec3 AxisZ => new Vec3(0, 0, 1);

        /// <summary>
        /// The square of the magnitude of this vector.
        /// </summary>
        public float MagnitudeSquared => x * x + y * y + z * z;

        /// <summary>
        /// The magnitude of this vector.
        /// </summary>
        public float Magnitude => (float)Math.Sqrt(MagnitudeSquared);

        /// <summary>
        /// The vector of length 1 pointing in the same direction.
        /// </summary>
        public Vec3 UnitVector => this / Magnitude;

        /// <summary>
        /// The perpendicular vector on the right, ignoring the Y component.
        /// </summary>
        public Vec3 Right => new Vec3(-z, 0, x);

        /// <summary>
        /// The perpendicular vector on the left, ignoring the Y component.
        /// </summary>
        public Vec3 Left => new Vec3(z, 0, -x);


        /// <summary>
        /// Create a Vec3 fronm its components.
        /// </summary>
        /// <param name="x">The x component.</param>
        /// <param name="y">The y component.</param>
        /// <param name="z">The z component.</param>
        public Vec3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// Create a Vec3 from another one.
        /// </summary>
        /// <param name="other">The Vec3 to copy.</param>
        public Vec3(Vec3 other)
        {
            x = other.x;
            y = other.y;
            z = other.z;
        }

        /// <summary>
        /// Construct a unit-vector from euler angles.
        /// </summary>
        /// <param name="pitch">The pitch (in radians) from pi/2 to -pi/2</param>
        /// <param name="yaw">The yaw (in radians) counterclocwise from the X axis.</param>
        public Vec3(float pitch, float yaw)
        {
            x = (float)( Math.Cos(yaw) * Math.Cos(pitch) );
            y = (float)( Math.Sin(pitch) );
            z = (float)( Math.Sin(yaw) * Math.Cos(pitch) );
        }

        /// <summary>
        /// Scale this Vec3 by the specified amount.
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public Vec3 Scale(float amount)
        {
            return this * amount;
        }

        /// <summary>
        /// Scale this Vec3 by the specified vector.
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public Vec3 Scale(Vec3 vector)
        {
            return new Vec3(x * vector.x, y * vector.y, z * vector.z);
        }

        /// <summary>
        /// The dot product of the vectors. Given by the * operand.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public float Dot(Vec3 other)
        {
            return this * other;
        }

        /// <summary>
        /// The cross product of the vectors. Given by the ^ (XOR) operand.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public Vec3 Cross(Vec3 other)
        {
            return this ^ other;
        }

        /// <summary>
        /// If true, both objects are equal.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(Vec3))
                return false;

            Vec3 v = (Vec3)obj;
            return v.x == x && v.y == y;
        }

        /// <summary>
        /// Get the hash code of this Vec3.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            // Big primes for good distributions
            ulong px = 0x35AA0D41,
                  py = 0x647B2241,
                  pz = 0x6827C5DF;

            // Convert components to uints by taking the same bytes
            ulong nx = BitConverter.ToUInt32(BitConverter.GetBytes(x), 0),
                  ny = BitConverter.ToUInt32(BitConverter.GetBytes(y), 0),
                  nz = BitConverter.ToUInt32(BitConverter.GetBytes(x), 0);

            // XOR of products
            ulong res = (px * nx) ^ (py * ny) ^ (pz * nz);
            return unchecked((int)res); // Cast to an int with overflow
        }

        /// <summary>
        /// A string representation of this vector.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Vec3({0}, {1}, {2})", x, y, z);
        }

        // Operator section
        #pragma warning disable 1591

        public static Vec3 operator +(Vec3 a, Vec3 b)
        {
            return new Vec3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vec3 operator -(Vec3 a, Vec3 b)
        {
            return new Vec3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static Vec3 operator -(Vec3 a)
        {
            return new Vec3(-a.x, -a.y, -a.z);
        }

        public static Vec3 operator *(Vec3 a, float b)
        {
            return new Vec3(a.x * b, a.y * b, a.z * b);
        }

        public static Vec3 operator *(float a, Vec3 b)
        {
            return new Vec3(b.x * a, b.y * a, b.z * a);
        }

        public static float operator *(Vec3 a, Vec3 b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        public static Vec3 operator /(Vec3 a, float b)
        {
            return new Vec3(a.x / b, a.y / b, a.z / b);
        }

        public static Vec3 operator ^(Vec3 a, Vec3 b)
        {
            return new Vec3(
                a.y * b.z - a.z * b.y,
                a.z * b.x - a.x * b.z,
                a.x * b.y - a.y * b.x
            );
        }

        public static bool operator ==(Vec3 a, object b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Vec3 a, object b)
        {
            return !a.Equals(b);
        }

        #pragma warning restore 1591
    }
}
