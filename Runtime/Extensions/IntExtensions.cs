using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Extension functions for int values.
    /// </summary>
    public static class IntExtensions
    {

        /// <inheritdoc cref="MathUtility.Ratio(float, float, float)"/>
        public static float Ratio(this int value, int min, int max)
        {
            return MathUtility.Ratio(value, min, max);
        }
        
        /// <inheritdoc cref="MathUtility.Ratio(float, float, float)"/>
        public static float Percents(this int value, int min, int max)
        {
            return MathUtility.Percents(value, min, max);
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