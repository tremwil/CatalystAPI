using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalyst.Memory;

namespace Catalyst.Scripting
{
    public class GameInfoWrapper
    {
        internal GameInfo ginfo;

        public ValueHolder<bool> IsLoading;

        public GameInfoWrapper(MemoryManager manager)
        {
            ginfo = new GameInfo(manager);
            IsLoading = new ValueHolder<bool>();
        }
    }
}
