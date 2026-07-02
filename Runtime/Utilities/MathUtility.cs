using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Miscellaneous functions for mathematics.
    /// </summary>
    public static class MathUtility
    {

        /// <summary>
        /// Checks the two float values are close enough to be considered equal. This is meant to prevent float imprecisions.
        /// </summary>
        /// <remarks>
        /// This overload relies on <see cref="Mathf.Approximately(float, float)"/>, which uses a <i>relative</i> tolerance that
        /// scales with the magnitude of the compared values. For an explicit, absolute tolerance, use
        /// <see cref="Approximately(float, float, float)"/>.
        /// </remarks>
        /// <param name="a">The first value to compare.</param>
        /// <param name="b">The second value to compare.</param>
        /// <returns>Returns true if b is equal (or very close) to a.</returns>
        public static bool Approximately(float a, float b)
        {
            return Mathf.Approximately(a, b);
        }

        /// <summary>
        /// Checks the two float values are within a given absolute tolerance of each other.
        /// </summary>
        /// <remarks>
        /// Unlike <see cref="Approximately(float, float)"/>, this overload uses an <i>absolute</i> tolerance: it returns true if
        /// <paramref name="a"/> is within <paramref name="epsilon"/> of <paramref name="b"/>, bounds included. The sign of
        /// <paramref name="epsilon"/> is ignored, and an epsilon of 0 still considers strictly equal values as approximately equal.
        /// </remarks>
        /// <param name="a">The first value to compare.</param>
        /// <param name="b">The second value to compare.</param>
        /// <param name="epsilon">The absolute tolerance. Its absolute value is used, so its sign doesn't matter.</param>
        /// <returns>Returns true if a is within epsilon of b (bounds included).</returns>
        public static bool Approximately(float a, float b, float epsilon)
        {
            epsilon = Mathf.Abs(epsilon);
            return a >= b - epsilon && a <= b + epsilon;
        }

        /// <summary>
        /// Remaps the given value from one range to another.
        /// </summary>
        /// <remarks>
        /// If the input value is out of the <paramref name="fromMin"/>..<paramref name="fromMax"/> range, the remapped value will
        /// also be out of the target range (the value is not clamped).<br/>
        /// The output range may be inverted (<paramref name="toMin"/> greater than <paramref name="toMax"/>) to produce a
        /// descending mapping. However, if the <i>input</i> range is empty or inverted (<paramref name="fromMax"/> is not greater
        /// than <paramref name="fromMin"/>), this function returns 0.<br/>
        /// This integer overload uses integer division, so the result is truncated toward zero.
        /// </remarks>
        /// <example>
        /// <code>
        /// MathUtility.Remap(5, 0, 10, 100, 200);      // 150
        /// MathUtility.Remap(0, -10, 10, -100, 100);   // 0
        /// MathUtility.Remap(-10, -10, 10, -100, 100); // -100
        /// MathUtility.Remap(10, -10, 10, -100, 100);  // 100
        /// MathUtility.Remap(15, 0, 10, 100, 200);     // 250 (out of range in, out of range out)
        /// </code>
        /// </example>
        /// <param name="value">The value to remap.</param>
        /// <param name="fromMin">The lower bound of the value's current range.</param>
        /// <param name="fromMax">The upper bound of the value's current range.</param>
        /// <param name="toMin">The lower bound of the value's target range.</param>
        /// <param name="toMax">The upper bound of the value's target range.</param>
        /// <returns>Returns the remapped value, or 0 if the input range is empty or inverted.</returns>
        public static int Remap(int value, int fromMin, int fromMax, int toMin, int toMax)
        {
            // The intermediate multiplication is computed as a long to avoid integer overflow on large ranges.
            return fromMax - fromMin > 0
                ? (int)((long)(value - fromMin) * (toMax - toMin) / (fromMax - fromMin) + toMin)
                : 0;
        }

        /// <summary>
        /// Remaps the given value from one range to another, preserving fractional precision.
        /// </summary>
        /// <remarks>
        /// Behaves like <see cref="Remap(int, int, int, int, int)"/>, but keeps the fractional part of the result. As with the
        /// integer overload, an empty or inverted input range returns 0.
        /// </remarks>
        /// <inheritdoc cref="Remap(int, int, int, int, int)" path="/param"/>
        /// <returns>Returns the remapped value, or 0 if the input range is empty or inverted.</returns>
        public static float Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            return fromMax - fromMin > 0
                ? (value - fromMin) * (toMax - toMin) / (fromMax - fromMin) + toMin
                : 0;
        }

        /// <summary>
        /// Remaps the given value from its range to a range between 0 and 1. If the input value is out of the given range, it's
        /// clamped between 0 and 1.
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
        /// Remaps the given value from its range to a range between 0 and 100. Note that, unlike <see cref="Ratio(float, float,
        /// float)"/>, the result is not clamped: if the input value is out of range, the remapped value will also be out of range.
        /// </summary>
        /// <inheritdoc cref="Ratio(float, float, float)" path="/param"/>
        /// <returns>Returns the remapped value, between 0 and 100 if the input value is within range.</returns>
        public static float Percents(float value, float min, float max)
        {
            return Remap(value, min, max, 0, 100);
        }

    }

}
