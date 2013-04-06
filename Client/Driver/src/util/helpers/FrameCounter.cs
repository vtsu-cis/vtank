using System;
using System.Collections.Generic;
using System.Text;
using Client.src.service;

namespace Client.src.util.game
{
    /// <summary>
    /// Helper class for calcuating the number of frames that go by per minute. A 'frame'
    /// passes when the Update function is called.
    /// </summary>
    public class FrameCounter
    {
        #region Members
        private int frameCounter;
        private double frameTime;
        private double updateInterval;

        private static readonly double ONE_SECOND = 1.0;
        #endregion

        #region Properties
        /// <summary>
        /// Gets an instant snapshot of the FPS.
        /// </summary>
        public long FPS
        {
            get;
            private set;
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Create the frame counter. The update interval is set to a default value of 
        /// one second.
        /// </summary>
        public FrameCounter()
            : this(ONE_SECOND)
        {
        }

        /// <summary>
        /// Create the frame counter.
        /// </summary>
        /// <param name="updateInterval">How long (in seconds) 
        /// to wait before updating the FPS display.</param>
        public FrameCounter(double updateInterval)
        {
            this.updateInterval = updateInterval;
            Reset();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Reset the frame counter, re-initializing all values.
        /// </summary>
        public void Reset()
        {
            frameCounter = 0;
            frameTime = 0;
            FPS = 0;
        }

        /// <summary>
        /// Update the frame counter, indicating that one frame has passed.
        /// </summary>
        public void Update()
        {
            double elapsed = ServiceManager.Game.DeltaTime;

            frameTime += elapsed;
            if (frameTime >= updateInterval)
            {
                FPS = frameCounter;
                frameTime = 0;
                frameCounter = 0;
            }
        }

        /// <summary>
        /// Increment the frame counter. This should be called from the main "Draw()" function.
        /// </summary>
        public void IncrementFrames()
        {
            frameCounter++;
        }

        /// <summary>
        /// Get the FPS formatted in a user-friendly string, using the format:
        /// X FPS (e.g. 60 FPS).
        /// </summary>
        /// <returns>Formatted string.</returns>
        public string GetFormattedFPS()
        {
            return String.Format("{0} FPS", FPS);
        }
        #endregion
    }
}
