using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalyst.Mathf;

namespace Catalyst.Memory
{
    /// <summary>
    /// Information regarding the player.
    /// </summary>
    public class PlayerInfo
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
        /// A value representing Faith's different movements.
        /// </summary>
        protected DeepPointer<MovementState> movement;

        /// <summary>
        /// The memory manager.
        /// </summary>
        public MemoryManager MemManager { get; protected set; }

        // TODO : Better summary
        /// <summary>
        /// Initialize a new instance of the PlayerInfo class.
        /// </summary>
        public PlayerInfo(MemoryManager manager)
        {
            // Pointer for position was discovered
            // by https://github.com/Psp4804. All credits goes to
            // him for it.

            MemManager = manager;

            position = new DeepPointer<Vec3>(
                MemManager.ProcHandle,
                "MirrorsEdgeCatalyst.exe",
                0x02578A68, 0x70, 0x98, 0x238, 0x20, 0x22d0
            );
            cosYawOver2 = new DeepPointer<float>(
                MemManager.ProcHandle,
                "MirrorsEdgeCatalyst.exe",
                0x02578A68, 0x70, 0x98, 0x238, 0x20, 0x22cc
            );
            sinYawOver2 = new DeepPointer<float>(
                MemManager.ProcHandle,
                "MirrorsEdgeCatalyst.exe",
                0x02578A68, 0x70, 0x98, 0x238, 0x20, 0x22c4
            );
            movement = new DeepPointer<MovementState>(
                MemManager.ProcHandle,
                "MirrorsEdgeCatalyst.exe",
                0x2576FDC
            );
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
                position.Dispose();
                sinYawOver2.Dispose();
                cosYawOver2.Dispose();
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
