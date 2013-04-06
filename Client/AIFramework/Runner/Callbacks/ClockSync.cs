using System;
using System.Collections.Generic;
using System.Text;

namespace AIFramework.Runner.Callbacks
{
    /// <summary>
    /// ClockSync callback returns the clients current time in milliseconds to the server
    /// This synchronizes the server and the clients machine to the same time and tries to
    /// corrrect for latency issues and clients clock not the same as the servers clock
    /// </summary>
    public class ClockSync : GameSession.ClockSynchronizerDisp_
    {
        /// <summary>
        /// Request the current time 
        /// </summary>
        /// <param name="current__"></param>
        /// <returns>Clients current time in milliseconds</returns>
        public override long Request(Ice.Current current__)
        {
            return Network.Util.Clock.GetTimeMilliseconds();
        }
    }
}
