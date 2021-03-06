﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Catalyst.Memory
{
    /// <summary>
    /// A few useful values collected from game memory.
    /// </summary>
    public class GameInfo
    {
        /// <summary>
        /// The memory manager.
        /// </summary>
        public MemoryManager MemManager { get; protected set; }

        // TODO : Better summary
        /// <summary>
        /// Initialize a new instance of the GameInfo class.
        /// </summary>
        public GameInfo(MemoryManager manager)
        {
            // Pointer for loading was discovered
            // by https://github.com/Psp4804. All credits goes to
            // him for it.

            MemManager = manager;
        }

        /// <summary>
        /// Returns true when the game is loading.
        /// </summary>
        /// <returns></returns>
        public bool IsLoading()
        {
            return MemManager.ReadGeneric<int>(0x142576fdc) == -1;
        }

        /// <summary>
        /// Get the current timescale (game speed).
        /// </summary>
        /// <returns></returns>
        public float GetTimescale()
        {
            return MemManager.ReadGenericPtr<float>(0, 0x142142A68, 0x48);
        }

        /// <summary>
        /// Set the current time scale (game speed). Beware of extreme values.
        /// </summary>
        /// <param name="newValue">The new timescale.</param>
        public void SetTimescale(float newValue)
        {
            if (newValue < 0)
                throw new ArgumentException("value must be bigger or equal to 0", "newValue");

            MemManager.WriteGenericPtr(newValue, 0, 0x142142A68, 0x48);
        }

        /// <summary>
        /// Get the current time of day, in seconds.
        /// </summary>
        /// <returns></returns>
        public int GetTimeOfDay()
        {
            return MemManager.ReadGenericPtr<int>(0, 0x14255C2F8, 0x8, 0x28, 0x30);
        }

        /// <summary>
        /// Set the current time of day.
        /// </summary>
        /// <param name="newValue">The day time, in seconds.</param>
        public void SetTimeOfDay(int newValue)
        {
            if (newValue < 0 || newValue > 86400)
                throw new ArgumentException("Value must be in range [0, 86400]", "newValue");

            MemManager.WriteGenericPtr(newValue, 0, 0x14255C2F8, 0x8, 0x28, 0x30);
        }

        /// <summary>
        /// Gets the coordinates of the in-game waypoint.
        /// </summary>
        /// <returns></returns>
        public Vec3 GetWaypointPos()
        {
            return MemManager.ReadGenericPtr<Vec3>(0, 0x1425789B0, 0x70, 0x128, 0x2270);
        }

        /// <summary>
        /// Sets the coordinates of the waypoint to a specific value.
        /// </summary>
        /// <param name="position"></param>
        public void SetWaypointPos(Vec3 position)
        {
            MemManager.WriteGenericPtr(position, 0, 0x1425789B0, 0x70, 0x128, 0x2270);
        }
    }
}
