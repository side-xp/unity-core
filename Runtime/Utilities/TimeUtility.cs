using System;

namespace SideXP.Core
{

    /// <summary>
    /// Miscellaneous functions for working with time and date values.
    /// </summary>
    public static class TimeUtility
    {

        /// <summary>
        /// Gets the number of milliseconds since <see cref="DateTime.UnixEpoch"/> to <see cref="DateTime.UtcNow"/>.
        /// </summary>
        /// <param name="local">If enabled, get the timestamp to the local time instead of UTC (basically use <see cref="DateTime.Now"/>
        /// instead of <see cref="DateTime.UtcNow"/>).</param>
        /// <returns>Returns the number of milliseconds since <see cref="DateTime.UnixEpoch"/> to <see cref="DateTime.UtcNow"/>.</returns>
        public static long GetTimestamp(bool local = false)
        {
            return local
                ? ToTimestamp(DateTime.UtcNow)
                : ToTimestamp(DateTime.Now);
        }

        /// <summary>
        /// Gets the number of seconds since <see cref="DateTime.UnixEpoch"/> to <see cref="DateTime.UtcNow"/>.
        /// </summary>
        /// <returns>Returns the number of seconds since <see cref="DateTime.UnixEpoch"/> to <see cref="DateTime.UtcNow"/>.</returns>
        /// <inheritdoc cref="GetTimestamp(bool)"/>
        public static long GetTimestampSeconds(bool local = false)
        {
            return local
                ? ToTimestampSeconds(DateTime.UtcNow)
                : ToTimestampSeconds(DateTime.Now);
        }

        /// <summary>
        /// Converts a given time into a number of elapsed milliseconds since <see cref="DateTime.UnixEpoch"/>.
        /// </summary>
        /// <param name="time">The time to convert.</param>
        /// <returns>Returns the number of elapsed milliseconds since <see cref="DateTime.UnixEpoch"/>.</returns>
        public static long ToTimestamp(DateTime time)
        {
            return (long)time.Subtract(DateTime.UnixEpoch).TotalMilliseconds;
        }

        /// <summary>
        /// Converts a given time into a number of elapsed seconds since <see cref="DateTime.UnixEpoch"/>.
        /// </summary>
        /// <returns>Returns the number of elapsed seconds since <see cref="DateTime.UnixEpoch"/>.</returns>
        /// <inheritdoc cref="ToTimestamp(DateTime)"/>
        public static long ToTimestampSeconds(DateTime time)
        {
            return (long)time.Subtract(DateTime.UnixEpoch).TotalSeconds;
        }

    }

}