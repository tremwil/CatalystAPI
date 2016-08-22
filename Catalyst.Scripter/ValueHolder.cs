using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalyst.Scripting
{
    public class ValueHolder<T> where T : struct
    {
        public T Old { get; private set; }
        public T Current { get; private set; }

        public ValueHolder()
        {
            Old = default(T);
            Current = default(T);    
        }

        internal void Update(T newValue)
        {
            Old = Current;
            Current = newValue;
        }
    }
}
