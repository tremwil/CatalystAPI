using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalyst.Mathf;
using System.Diagnostics;

namespace Catalyst.Memory
{
    /// <summary>
    /// A few useful values collected from game memory.
    /// </summary>
    public class GameInfo : IDisposable
    {
        /// <summary>
        /// A value to test if the game is loading.
        /// </summary>
        protected DeepPointer<byte> isLoading;

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

            isLoading = new DeepPointer<byte>(
                MemManager.ProcHandle,
                "MirrorsEdgeCatalyst.exe",
                0x240C2B8, 0x4C1
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

        private bool disposed = false;

        /// <summary>
        /// Free resources used by this object.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !disposed)
            {
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
