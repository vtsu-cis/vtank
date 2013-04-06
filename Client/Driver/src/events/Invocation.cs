using System;
using System.Collections.Generic;
using System.Text;

namespace Client.src.events
{
    public struct Invocation
    {
        public delegate void InvocationTarget(object param);

        public InvocationTarget Target;
        public object Parameter;

        public Invocation(InvocationTarget target)
            : this(target, null) { }

        public Invocation(InvocationTarget target, object param)
        {
            this.Target = target;
            this.Parameter = param;
        }
    }
}
