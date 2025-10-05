using System;

namespace SideXP.Core
{

    /// <summary>
    /// Miscellaneous functions for working with time and date values.
    /// </summary>
    public static class TimeUtility
    {

#if !UNITY_6000_0_OR_NEWER
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
#endif

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
#if UNITY_6000_0_OR_NEWER
            return (long)time.Subtract(DateTime.UnixEpoch).TotalMilliseconds;
#else
            return (long)(time.ToUniversalTime() - UnixEpoch).TotalMilliseconds;
#endif
        }

        /// <summary>
        /// Converts a given time into a number of elapsed seconds since <see cref="DateTime.UnixEpoch"/>.
        /// </summary>
        /// <returns>Returns the number of elapsed seconds since <see cref="DateTime.UnixEpoch"/>.</returns>
        /// <inheritdoc cref="ToTimestamp(DateTime)"/>
        public static long ToTimestampSeconds(DateTime time)
        {
#if UNITY_6000_0_OR_NEWER
            return (long)time.Subtract(DateTime.UnixEpoch).TotalSeconds;
#else
            return (long)(time.ToUniversalTime() - UnixEpoch).TotalSeconds;
#endif
        }

    }

}