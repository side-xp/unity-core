using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Extension functions for float values.
    /// </summary>
    public static class FloatExtensions
    {

        /// <inheritdoc cref="Math.Approximately(float, float)"/>
        public static bool Approximately(this float a, float b)
        {
            return Mathf.Approximately(a, b);
        }

        /// <inheritdoc cref="Math.Approximately(float, float, float)"/>
        public static bool Approximately(this float a, float b, float epsilon)
        {
            return a > b - epsilon && a < b + epsilon;
        }

        /// <inheritdoc cref="Math.Ratio(float, float, float)"/>
        public static float Ratio(this float value, float min, float max)
        {
            return Math.Ratio(value, min, max);
        }
        
        /// <inheritdoc cref="Math.Ratio(float, float, float)"/>
        public static float Percents(this float value, float min, float max)
        {
            return Math.Percents(value, min, max);
        }

        /// <summary>
        /// Clamps this value between given min and max.
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <param name="min">The lower bound of the value.</param>
        /// <param name="max">The upper bound of the value.</param>
        /// <returns>Returns the clamped value.</returns>
        public static float Clamp(this float value, float min, float max)
        {
            return Mathf.Clamp(value, min, max);
        }

        /// <summary>
        /// Pads a value as string with leading characters.
        /// </summary>
        /// <param name="value">The value to pad.</param>
        /// <param name="length">The expected length of the string.</param>
        /// <param name="decimals">The number of decimals to display.</param>
        /// <param name="padChar">The character used to pad the value.</param>
        /// <returns>Returns the padded value as string.</returns>
        public static string Pad(this float value, int length, int decimals = 0, char padChar = '0')
        {
            return value.ToString($"F{Mathf.Max(0, decimals)}").PadLeft(length, padChar);
        }

    }

}