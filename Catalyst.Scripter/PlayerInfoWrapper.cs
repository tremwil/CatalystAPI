using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalyst.Memory;


namespace Catalyst.Scripting
{
    public class PlayerInfoWrapper
    {
        internal PlayerInfo pinfo;

        public ValueHolder<Vec3> Position;
        public ValueHolder<Vec3> Velocity;

        public ValueHolder<float> Yaw;
        public ValueHolder<MovementState> MovementState;

        public PlayerInfoWrapper(MemoryManager manager)
        {
            pinfo = new PlayerInfo(manager);
            Position = new ValueHolder<Vec3>();
            Velocity = new ValueHolder<Vec3>();
            Yaw = new ValueHolder<float>();
            MovementState = new ValueHolder<MovementState>();
        }
    }
}
