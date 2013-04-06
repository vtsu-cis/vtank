using System;

namespace Network.Util
{
    /// <summary>
    /// Provides utility functions related to time.
    /// </summary>
    public static class Clock
    {
        private static DateTime beginning = new DateTime(1970, 1, 1);

        /// <summary>
        /// Get the total time since January 1, 1970, in milliseconds.
        /// </summary>
        /// <returns>UTC timestamp.</returns>
        public static long GetTimeMilliseconds()
        {
            return (long)(DateTime.UtcNow - beginning).TotalMilliseconds;
        }

        /// <summary>
        /// Formats a time (in seconds) to appear as what one would see on a clock
        /// (e.g. M:SS) except in minutes:seconds, not hours:minutes..
        /// </summary>
        /// <returns>formatted string</returns>
        public static string FormatTime()
        {
            double time = (double)GetTimeMilliseconds() / 1000.0f;
            int minutes = (int)(time / 60);
            int seconds = (int)(time % 60);

            if (minutes <= 0 && seconds <= 0)
            {
                return "0:00";
            }

            return String.Format("{0}:{1:00}", minutes, seconds);
        }

        /// <summary>
        /// Formats a time (in seconds) to appear as what one would see on a clock
        /// (e.g. M:SS) except in minutes:seconds, not hours:minutes..
        /// </summary>
        /// <param name="time">Time in seconds.</param>
        /// <returns>formatted string</returns>
        public static string FormatTime(double time)
        {
            int minutes = (int)(time / 60);
            int seconds = (int)(time % 60);

            if (minutes <= 0 && seconds <= 0)
            {
                return "0:00";
            }

            return String.Format("{0}:{1:00}", minutes, seconds);
        }
    }
}
