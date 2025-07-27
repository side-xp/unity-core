using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Extension functions for float values.
    /// </summary>
    public static class IntExtensions
    {

        /// <inheritdoc cref="Math.Ratio(float, float, float)"/>
        public static float Ratio(this int value, int min, int max)
        {
            return Math.Ratio(value, min, max);
        }
        
        /// <inheritdoc cref="Math.Ratio(float, float, float)"/>
        public static float Percents(this int value, int min, int max)
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
        public static int Clamp(this int value, int min, int max)
        {
            return Mathf.Clamp(value, min, max);
        }

        /// <summary>
        /// Pads a value as string with leading characters.
        /// </summary>
        /// <param name="value">The value to pad.</param>
        /// <param name="length">The expected length of the string.</param>
        /// <param name="padChar">The character used to pad the value.</param>
        /// <returns>Returns the padded value as string.</returns>
        public static string Pad(this int value, int length, char padChar = '0')
        {
            return value.ToString().PadLeft(length, padChar);
        }

    }

}