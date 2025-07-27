using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Miscellaneous functions for mathematics.
    /// </summary>
    public static class Math
    {

        /// <summary>
        /// Checks the two float values are close enough to be considered equals. This is meant to prevent float imprecisions.
        /// </summary>
        /// <returns>Returns true if b is equals (or very close) to a.</returns>
        public static bool Approximately(float a, float b)
        {
            return Mathf.Approximately(a, b);
        }

        /// <inheritdoc cref="Approximately(float, float)"/>
        /// <param name="epsilon">The approximation value. Basically, this function returns true if a is higher than (b - epsilon) and a is lower than (b + epsilon).</param>
        public static bool Approximately(float a, float b, float epsilon)
        {
            return a > b - epsilon && a < b + epsilon;
        }

        /// <summary>
        /// Remaps the given value between the given range. Note that if the input value is out of range, the remapped value will also be out of range.
        /// </summary>
        /// <example>
        /// Remap(5, 0, 10, 100, 200) // Outputs 150
        /// Remap(0, -10, 10, -100, 100) // Outputs 0
        /// Remap(-10, -10, 10, -100, 100) // Outputs -100
        /// Remap(10, -10, 10, -100, 100) // Outputs 100
        /// Remap(0, -10, 10, 100, 200) // Outputs 150
        /// </example>
        /// <param name="value">The value to remap.</param>
        /// <param name="fromMin">The lower bound of the value's current range.</param>
        /// <param name="fromMax">The upper bound of the value's current range.</param>
        /// <param name="toMin">The lower bound of the value's target range.</param>
        /// <param name="toMax">The upper bound of the value's target range.</param>
        /// <returns>Returns the remapped value.</returns>
        public static int Remap(int value, int fromMin, int fromMax, int toMin, int toMax)
        {
            return fromMax - fromMin > 0
                ? (value - fromMin) * (toMax - toMin) / (fromMax - fromMin) + toMin
                : 0;
        }

        /// <inheritdoc cref="Remap(int, int, int, int, int)"/>
        public static float Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            return fromMax - fromMin > 0
                ? (value - fromMin) * (toMax - toMin) / (fromMax - fromMin) + toMin
                : 0;
        }

        /// <summary>
        /// Remaps the given value from its range to a range between 0 and 1. If the input value is out of the given range, it's clamped between 0 and 1.
        /// </summary>
        /// <param name="value">The value to remap.</param>
        /// <param name="min">The lower bound of the value's current range.</param>
        /// <param name="max">The upper bound of the value's current range.</param>
        /// <returns>Returns the remapped value, clamped between 0 and 1.</returns>
        public static float Ratio(float value, float min, float max)
        {
            return Mathf.Clamp01(Remap(value, min, max, 0, 1));
        }

        /// <summary>
        /// Remaps the given value from its range to a range between 0 and 100. Note that if the input value is out of range, the remapped value will also be out of range.
        /// </summary>
        /// <returns>Returns the remapped value.</returns>
        /// <inheritdoc cref="Ratio(float, float, float)"/>
        public static float Percents(float value, float min, float max)
        {
            return Remap(value, min, max, 0, 100);
        }

    }

}