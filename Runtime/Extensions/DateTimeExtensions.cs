using System;

namespace SideXP.Core
{

    /// <summary>
    /// Extension functions for <see cref="DateTime"/> values.
    /// </summary>
    public static class DateTimeExtensions
    {

        /// <inheritdoc cref="TimeUtility.ToTimestamp(DateTime)"/>
        public static long ToTimestamp(this DateTime time)
        {
            return TimeUtility.ToTimestamp(time);
        }

        /// <inheritdoc cref="TimeUtility.ToTimestampSeconds(DateTime)"/>
        public static long ToTimestampSeconds(this DateTime time)
        {
            return TimeUtility.ToTimestampSeconds(time);
        }

    }

}