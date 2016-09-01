using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalyst.Input
{
    /// <summary>
    /// A class uniting both key and mouse bindings.
    /// </summary>
    public class InputBinding
    {
        /// <summary>
        /// The key binding.
        /// </summary>
        public DIKCode KeyBinding
        {
            get;
            internal set;
        }

        /// <summary>
        /// The mouse binding.
        /// </summary>
        public MouseCode MouseBinding
        {
            get;
            internal set;
        }

        /// <summary>
        /// Create an input binding from a key and a mouse button.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="btn"></param>
        public InputBinding(DIKCode key, MouseCode btn)
        {
            KeyBinding = key;
            MouseBinding = btn;
        }

        /// <summary>
        /// Create an input binding from a key.
        /// </summary>
        /// <param name="key"></param>
        public InputBinding(DIKCode key)
        {
            KeyBinding = key;
            MouseBinding = MouseCode.None;
        }

        /// <summary>
        /// Create an input binding from a mouse button.
        /// </summary>
        /// <param name="btn"></param>
        public InputBinding(MouseCode btn)
        {
            KeyBinding = DIKCode.NONE;
            MouseBinding = btn;
        }
 
        /// <summary>
        /// An empty input binding.
        /// </summary>
        public InputBinding()
        {
            KeyBinding = DIKCode.NONE;
            MouseBinding = MouseCode.None;
        }
    }
}
