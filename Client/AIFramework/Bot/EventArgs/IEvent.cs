using System;

namespace AIFramework.Bot.EventArgs
{
    public class IBotEvent : IDisposable
    {
        protected IBotEvent()
        {
        }

        #region IDisposable Members
        public virtual void Dispose()
        {
        }
        #endregion
    }
}
