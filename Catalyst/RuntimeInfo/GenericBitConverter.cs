using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Catalyst.RuntimeInfo
{
    /// <summary>
    /// A set of method for converting generic objects to bytes and vice-versa.
    /// </summary>
    public static class GenericBitConverter
    {
        /// <summary>
        /// Get the size, in bytes, of a type in a way that supports enums.
        /// </summary>
        /// <param name="t">The type.</param>
        /// <returns></returns>
        public static int GetTypeSize(Type t)
        {
            if (t.IsEnum)
            {
                return Marshal.SizeOf(Enum.GetUnderlyingType(t));
            }
            if (t.IsValueType)
            {
                return Marshal.SizeOf(t);
            }
            throw new ArgumentException("Cannot determine size of a non value-type object, got " + t.FullName);
        }

        /// <summary>
        /// Convert bytes to a specific struct.
        /// </summary>
        /// <typeparam name="T">The struct to convert to.</typeparam>
        /// <param name="data">The data to convert.</param>
        /// <param name="safe">If true, the method will throw an error if T's size does not match the data.</param>
        /// <returns></returns>
        public static T ToStruct<T>(byte[] data, bool safe) where T : struct
        {
            if (safe)
                if (GetTypeSize(typeof(T)) != data.Length)
                    throw new ArgumentException("Not enough data to convert", "data");

            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
        }

        /// <summary>
        /// Convert bytes to a sequential or explicit struct.
        /// </summary>
        /// <typeparam name="T">The struct to convert to.</typeparam>
        /// <param name="data">The data to convert.</param>
        /// <returns></returns>
        public static T ToStruct<T>(byte[] data) where T : struct
        {
            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
        }
    }
}
