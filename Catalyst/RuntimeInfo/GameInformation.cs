using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalyst.Mathf;
using System.Diagnostics;

using Catalyst.Unmanaged;
namespace Catalyst.RuntimeInfo
{
    /// <summary>
    /// A few useful values collected from game memory.
    /// </summary>
    public class GameInformation : IDisposable
    {
        /// <summary>
        /// A pointer to the cosine of the camera's yaw divided by two.
        /// No idea why the game stores rotation like that.
        /// <para></para>NOTE: the sign of the value is incorrect, so you
        /// must also use sinYawOver2 to get the yaw.
        /// </summary>
        protected DeepPointer<float> cosYawOver2;
        /// <summary>
        /// A pointer to the sine of the camera's yaw divided by two.
        /// No idea why the game stores rotation like that.
        /// <para></para>NOTE: the sign of the value is incorrect, so you
        /// must also use cosYawOver2 to get the yaw.
        /// </summary>
        protected DeepPointer<float> sinYawOver2;

        /// <summary>
        /// Faith's position. The coordinate system is right-handed.
        /// </summary>
        protected DeepPointer<Vec3> position;

        /// <summary>
        /// A value to test if the game is loading.
        /// </summary>
        protected DeepPointer<byte> isLoading;

        /// <summary>
        /// A value representing Faith's different movements.
        /// </summary>
        protected DeepPointer<MovementState> movement;

        /// <summary>
        /// The memory manager.
        /// </summary>
        public MemoryManager Memory { get; protected set; }

        // TODO : Better summary
        /// <summary>
        /// Initialize a new instance of the GameInformation class.
        /// </summary>
        public GameInformation()
        {
            Memory = new MemoryManager();
            Memory.OpenProcess("MirrorsEdgeCatalyst");

            // Pointers for position and loading were discovered
            // by https://github.com/Psp4804. All credits goes to
            // him for these.

            position = new DeepPointer<Vec3>(
                Memory.ProcHandle,
                "MirrorsEdgeCatalyst.exe",
                0x02578A68, 0x70, 0x98, 0x238, 0x20, 0x22d0
            );
            cosYawOver2 = new DeepPointer<float>(
                Memory.ProcHandle,
                "MirrorsEdgeCatalyst.exe",
                0x02578A68, 0x70, 0x98, 0x238, 0x20, 0x22cc
            );
            sinYawOver2 = new DeepPointer<float>(
                Memory.ProcHandle,
                "MirrorsEdgeCatalyst.exe",
                0x02578A68, 0x70, 0x98, 0x238, 0x20, 0x22c4
            );
            isLoading = new DeepPointer<byte>(
                Memory.ProcHandle,
                "MirrorsEdgeCatalyst.exe",
                0x240C2B8, 0x4C1
            );
            movement = new DeepPointer<MovementState>(
                Memory.ProcHandle,
                "MirrorsEdgeCatalyst.exe",
                0x2576FDC
            );
        }

        /// <summary>
        /// Returns true when the game is loading.
        /// </summary>
        /// <returns></returns>
        public bool IsLoading()
        {
            return Convert.ToBoolean(isLoading.GetValue());
        }

        /// <summary>
        /// Get Faith's current position as a Vec3.
        /// </summary>
        /// <returns></returns>
        public Vec3 GetPosition()
        {
            return position.GetValue();
        }

        /// <summary>
        /// Get the current orientation of the camera, as radians.
        /// </summary>
        /// <returns></returns>
        public float GetYaw()
        {
            var sin = sinYawOver2.GetValue();
            var cos = cosYawOver2.GetValue();

            if (sin < 0 || cos < 0) // cos and sin have a random sign shift at cos = -0.5
                return 2 * (float)(Math.PI - Math.Acos(cos));

            return 2 * (float)Math.Acos(cos);
        }

        /// <summary>
        /// Returns Faith's movement state.
        /// </summary>
        /// <returns></returns>
        public MovementState GetMovementState()
        {
            return movement.GetValue();
        }


        private bool disposed = false;

        /// <summary>
        /// Free resources used by this object.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !disposed)
            {
                Memory.Dispose();
                position.Dispose();
                sinYawOver2.Dispose();
                cosYawOver2.Dispose();
                isLoading.Dispose();
                disposed = true;
            }
        }

        /// <summary>
        /// Free resources used by this object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
