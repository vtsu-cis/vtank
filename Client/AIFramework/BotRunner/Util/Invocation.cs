using System;
using System.Collections.Generic;
using System.Text;

namespace VTankBotRunner.Util
{
    public struct Invocation : IDisposable
    {
        public delegate void InvocationTarget(object param);

        private long lastTime;
        private long elapsed;

        public InvocationTarget Target;
        public object Parameter;
        public long DelayTimeMs;

        public Invocation(InvocationTarget target)
            : this(target, null) { }

        public Invocation(InvocationTarget target, object param)
            : this(target, param, 0)
        {
        }

        public Invocation(InvocationTarget target, object param, long delayTimeMs)
        {
            this.Target = target;
            this.Parameter = param;
            this.DelayTimeMs = delayTimeMs;

            elapsed = 0;
            lastTime = Network.Util.Clock.GetTimeMilliseconds();
        }

        #region Methods
        public void Invoke()
        {
            Target(Parameter);
        }

        public bool Ready()
        {
            if (elapsed >= DelayTimeMs)
                return true;

            long now = Network.Util.Clock.GetTimeMilliseconds();
            elapsed += now - lastTime;
            lastTime = now;

            return elapsed >= DelayTimeMs;
        }

        #region IDisposable Members
        public void Dispose()
        {
            Target = null;
            Parameter = null;
        }
        #endregion
        #endregion
    }
}
