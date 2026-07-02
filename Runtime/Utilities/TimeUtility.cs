using System;

namespace SideXP.Core
{

    /// <summary>
    /// Miscellaneous functions for working with time and date values.
    /// </summary>
    public static class TimeUtility
    {

#if UNITY_6000_0_OR_NEWER
        // DateTime.UnixEpoch is only available on the newer .NET profile shipped with Unity 6+.
        private static DateTime Epoch => DateTime.UnixEpoch;
#else
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
#endif

        /// <summary>
        /// Gets the number of milliseconds elapsed since the Unix epoch (1970-01-01 UTC) for the current time.
        /// </summary>
        /// <param name="local">If enabled, computes the timestamp from local wall-clock time (<see cref="DateTime.Now"/>)
        /// instead of UTC (<see cref="DateTime.UtcNow"/>).</param>
        /// <returns>Returns the number of elapsed milliseconds.</returns>
        public static long GetTimestamp(bool local = false)
        {
            return local
                ? ToTimestamp(DateTime.Now)
                : ToTimestamp(DateTime.UtcNow);
        }

        /// <summary>
        /// Gets the number of seconds elapsed since the Unix epoch (1970-01-01 UTC) for the current time.
        /// </summary>
        /// <returns>Returns the number of elapsed seconds.</returns>
        /// <inheritdoc cref="GetTimestamp(bool)"/>
        public static long GetTimestampSeconds(bool local = false)
        {
            return local
                ? ToTimestampSeconds(DateTime.Now)
                : ToTimestampSeconds(DateTime.UtcNow);
        }

        /// <summary>
        /// Converts a given time into the number of elapsed milliseconds since the Unix epoch (1970-01-01 UTC).<br/>
        /// The <see cref="DateTime"/>'s wall-clock value is used as-is (its <see cref="DateTimeKind"/> is not normalized),
        /// so pass a UTC value (e.g. <see cref="DateTime.UtcNow"/>) for a true Unix timestamp.
        /// </summary>
        /// <param name="time">The time to convert.</param>
        /// <returns>Returns the number of elapsed milliseconds.</returns>
        public static long ToTimestamp(DateTime time)
        {
            return (long)(time - Epoch).TotalMilliseconds;
        }

        /// <summary>
        /// Converts a given time into the number of elapsed seconds since the Unix epoch (1970-01-01 UTC).
        /// </summary>
        /// <returns>Returns the number of elapsed seconds.</returns>
        /// <inheritdoc cref="ToTimestamp(DateTime)"/>
        public static long ToTimestampSeconds(DateTime time)
        {
            return (long)(time - Epoch).TotalSeconds;
        }

    }

}
