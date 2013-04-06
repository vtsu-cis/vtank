/*!
    \file   ClockSync.cs
    \brief  Definition of the ClockSync callback.
    \author Copyright (C) 2009 by Vermont Technical College.
*/
using System;

namespace Client.src.callbacks
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
