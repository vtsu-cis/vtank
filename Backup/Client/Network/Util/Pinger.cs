using System;
using System.Collections.Generic;
using System.Threading;

namespace Network.Util
{
    /// <summary>
    /// The Pinger class, just how it sounds, is responsible for pinging a targeted
    /// server repeatedly to calculate an average response time. The average ping is
    /// recorded, and can be used to tell the client how long it takes to send and
    /// receive messages to and from the server.
    /// </summary>
    public class Pinger
    {
        #region Members

        private Thread internalThread;
        private bool running = false;
        private double averageLatency = -1;
        private bool newValue = false;
        private Ice.ObjectPrx session;

        /// <summary>
        /// Constant which defines how many times to retrieve a latency time before
        /// calculating the average.
        /// </summary>
        public const int RequestBufferSize = 6;

        /// <summary>
        /// How long to wait in-between requests (in milliseconds).
        /// </summary>
        public const int RequestInterval = 1000;

        /// <summary>
        /// How long to wait in-between recording average latencies (in milliseconds).
        /// </summary>
        public const int RequestWait = 10000;

        #endregion

        #region Properties

        /// <summary>
        /// Get whether the pinger is still running. Setting this attribute to false
        /// will stop the thread.
        /// </summary>
        public bool Running
        {
            get
            {
                lock (this)
                {
                    return running;
                }
            }

            set
            {
                lock (this)
                {
                    running = value;
                    if (!running)
                    {
                        internalThread.Interrupt();
                    }
                }
            }
        }

        /// <summary>
        /// Get the average latency to the target server.
        /// </summary>
        public double AverageLatency
        {
            get
            {
                lock (this)
                {
                    return averageLatency;
                }
            }

            private set
            {
                lock (this)
                {
                    averageLatency = value;
                }
                
                NewValueAvailable = true;
            }
        }

        /// <summary>
        /// The Pinger is nice enough to keep track of when a new ping is available, so that
        /// GUI updates can only be made when necessary.
        /// Note: When called, if the result is 'true', this property is set to 'false' until
        /// the next average latency calculation.
        /// </summary>
        public bool NewValueAvailable
        {
            get
            {
                lock (this)
                {
                    if (newValue)
                    {
                        newValue = false;
                        return true;
                    }

                    return false;
                }
            }

            private set
            {
                lock (this)
                {
                    newValue = value;
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Start the pinger. It runs in a thread and returns the average response time.
        /// </summary>
        /// <param name="session">Session to ping. Note that this cannot be a UDP or
        /// one-way session, because it requires a response from the server to be
        /// effective.</param>
        public Pinger(Ice.ObjectPrx _session)
        {
            session = _session;
            Running = true;
            internalThread = new Thread(new ThreadStart(Run));
            internalThread.Start();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Perform the threaded operation. This runs infinitely (until the thread is
        /// interrupted by setting Running equal to false).
        /// </summary>
        private void Run()
        {
            try
            {
                List<double> averageBuffer = new List<double>();
                while (Running)
                {
                    for (int requests = 0; requests < RequestBufferSize; requests++)
                    {
                        double startTime = Clock.GetTimeMilliseconds();
                        session.ice_ping();
                        double endTime = Clock.GetTimeMilliseconds();
                        double elapsed = endTime - startTime;

                        averageBuffer.Add(elapsed);
                        if (AverageLatency < 0)
                        {
                            // There is no average latency set, so set it for the programmer's benefit.
                            AverageLatency = elapsed;
                        }
                         
                        Thread.Sleep(RequestInterval);
                    }

                    double sum = 0;
                    for (int i = 0; i < averageBuffer.Count; i++)
                    {
                        sum += averageBuffer[i];
                    }

                    AverageLatency = sum / averageBuffer.Count;

                    averageBuffer.Clear();

                    Thread.Sleep(RequestWait);
                }
            }
            catch (ThreadInterruptedException e) { Console.WriteLine(e); } // Ignore thread interruption.
            catch (Ice.Exception) 
            {
                try
                {
                    Running = false;
                }
                catch (ThreadInterruptedException e) { Console.WriteLine(e); }
            }
        }

        /// <summary>
        /// The Pinger is nice enough to provide a formatted version of the ping,
        /// which is more pleasing for the end-user to see than 8 decimal places.
        /// </summary>
        /// <returns>User-friendly formatted string.</returns>
        public string GetFormattedAverageLatency()
        {
            return String.Format("{0} ms", (int)Math.Ceiling(AverageLatency));
        }

        #endregion
    }
}
