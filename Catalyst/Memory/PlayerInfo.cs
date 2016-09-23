using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalyst.Memory
{
    /// <summary>
    /// Information regarding the player.
    /// </summary>
    public class PlayerInfo
    {
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
        }

        /// <summary>
        /// Get Faith's current position as a Vec3.
        /// </summary>
        /// <returns></returns>
        public Vec3 GetPosition()
        {
            return MemManager.ReadGenericPtr<Vec3>(0, 0x142578A68, 0x70, 0x98, 0x238, 0x18, 0x22d0);
        }

        /// <summary>
        /// Set Faith's current position as a Vec3.
        /// </summary>
        /// <param name="newPos"></param>
        public void SetPosition(Vec3 newPos)
        {
            MemManager.WriteGenericPtr(newPos, 0, 0x142578A68, 0x70, 0x98, 0x238, 0x18, 0x22d0);
            MemManager.WriteGenericPtr(newPos, 0, 0x142578A68, 0x70, 0x98, 0x238, 0x20, 0x22d0);
        }

        /// <summary>
        /// Get the last position of Faith which resides on the ground.
        /// </summary>
        /// <returns></returns>
        public Vec3 GetLastGroundPos()
        {
            return MemManager.ReadGenericPtr<Vec3>(0, 0x1423DA028, 0x20, 0x20, 0x40, 0x20, 0x00);
        }

        /// <summary>
        /// Set the last position of Faith which resides on the ground.
        /// </summary>
        /// <param name="newPos">The new position value.</param>
        /// <returns></returns>
        public void SetLastGroundPos(Vec3 newPos)
        {
            MemManager.WriteGenericPtr(newPos, 0, 0x1423DA028, 0x20, 0x20, 0x40, 0x20, 0x00);
        }

        /// <summary>
        /// Set the last Y coord of Faith which resides on the ground.
        /// </summary>
        /// <param name="newY">The new Y coordinate.</param>
        /// <returns></returns>
        public void SetLastGroundYCoord(float newY)
        {
            MemManager.WriteGenericPtr(newY, 0, 0x1423DA028, 0x20, 0x20, 0x40, 0x20, 0x04);
        }

        /// <summary>
        /// Get the current orientation of the camera, as radians.
        /// </summary>
        /// <returns></returns>
        public float GetCameraYaw()
        {
            var sin = MemManager.ReadGenericPtr<float>(0, 0x142578A68, 0x70, 0x98, 0x238, 0x18, 0x22c4);
            var cos = MemManager.ReadGenericPtr<float>(0, 0x142578A68, 0x70, 0x98, 0x238, 0x18, 0x22cc);
            var sinabs = Math.Abs(sin);

            if (sin < 0 || cos < 0) // cos and sin have a random sign shift at cos = -0.5, sin ~ 0.864
                return Mathf.PI2 - 2 * (float)Math.Asin(sinabs);

            return 2 * (float)Math.Asin(sinabs);
        }

        /// <summary>
        /// Sets the camera's yaw.
        /// </summary>
        /// <param name="angle">The new angle.</param>
        public void SetCameraYaw(float angle)
        {
            // Set the angle to the range [0, 2PI)
            angle = Mathf.DivMod(angle, Mathf.PI2);

            float cos = (float)Math.Cos(angle * 0.5f);
            float sin = (float)Math.Sin(angle * 0.5f);

            if (angle > 4.1887902f) // 240 degrees
            {
                cos = -cos;
                sin = -sin;
            }

            MemManager.WriteGenericPtr(sin, 0, 0x142578A68, 0x70, 0x98, 0x238, 0x18, 0x22c4);
            MemManager.WriteGenericPtr(sin, 0, 0x142578A68, 0x70, 0x98, 0x238, 0x20, 0x22c4);
            MemManager.WriteGenericPtr(cos, 0, 0x142578A68, 0x70, 0x98, 0x238, 0x18, 0x22cc);
            MemManager.WriteGenericPtr(cos, 0, 0x142578A68, 0x70, 0x98, 0x238, 0x20, 0x22cc);
        }

        /// <summary>
        /// Get the camera yaw as a unit vector. Faster than getting it as an angle.
        /// </summary>
        /// <returns></returns>
        public Vec3 GetCameraYawVector()
        {
            var sin = MemManager.ReadGenericPtr<float>(0, 0x142578A68, 0x70, 0x98, 0x238, 0x18, 0x22c4);
            var cos = MemManager.ReadGenericPtr<float>(0, 0x142578A68, 0x70, 0x98, 0x238, 0x18, 0x22cc);

            if (sin < 0)
            {
                sin = -sin;
                cos = -cos;
            }

            // Use double angle identities to multiply angle
            return new Vec3(cos * cos - sin * sin, 0, -2 * sin * cos);
        }

        /// <summary>
        /// Set the camera yaw from a unit vector. Faster than using an angle.
        /// </summary>
        /// <param name="unitVector">The unit vector to convert.</param>
        public void SetCameraYawVector(Vec3 unitVector)
        {
            // Use half-angle identities
            float xover2 = unitVector.x * 0.5f;
            float sin = -(float)Math.Sqrt(0.5f - xover2);
            float cos =  (float)Math.Sqrt(0.5f + xover2);

            if (unitVector.z < 0)
            {
                if (cos > 0.5f) sin = -sin;
                else cos = -cos;
            }

            MemManager.WriteGenericPtr(sin, 0, 0x142578A68, 0x70, 0x98, 0x238, 0x18, 0x22c4);
            MemManager.WriteGenericPtr(sin, 0, 0x142578A68, 0x70, 0x98, 0x238, 0x20, 0x22c4);
            MemManager.WriteGenericPtr(cos, 0, 0x142578A68, 0x70, 0x98, 0x238, 0x18, 0x22cc);
            MemManager.WriteGenericPtr(cos, 0, 0x142578A68, 0x70, 0x98, 0x238, 0x20, 0x22cc);
        }

        /// <summary>
        /// Returns Faith's movement state.
        /// </summary>
        /// <returns></returns>
        public MovementState GetMovementState()
        {
            return MemManager.ReadGenericPtr<MovementState>(0, 0x142576fdc);
        }
    }
}
