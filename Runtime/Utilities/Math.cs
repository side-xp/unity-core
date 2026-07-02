namespace SideXP.Core
{

    /// <summary>
    /// Miscellaneous functions for mathematics.
    /// </summary>
    [System.Obsolete("Use " + nameof(MathUtility) + " instead. This class will be removed in a future update.")]
    public static class Math
    {

        /// <inheritdoc cref="MathUtility.Approximately(float, float)"/>
        public static bool Approximately(float a, float b)
        {
            return MathUtility.Approximately(a, b);
        }

        /// <inheritdoc cref="MathUtility.Approximately(float, float, float)"/>
        public static bool Approximately(float a, float b, float epsilon)
        {
            return MathUtility.Approximately(a, b, epsilon);
        }

        /// <inheritdoc cref="MathUtility.Remap(int, int, int, int, int)"/>
        public static int Remap(int value, int fromMin, int fromMax, int toMin, int toMax)
        {
            return MathUtility.Remap(value, fromMin, fromMax, toMin, toMax);
        }

        /// <inheritdoc cref="MathUtility.Remap(float, float, float, float, float)"/>
        public static float Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            return MathUtility.Remap(value, fromMin, fromMax, toMin, toMax);
        }

        /// <inheritdoc cref="MathUtility.Ratio(float, float, float)"/>
        public static float Ratio(float value, float min, float max)
        {
            return MathUtility.Ratio(value, min, max);
        }

        /// <inheritdoc cref="MathUtility.Percents(float, float, float)"/>
        public static float Percents(float value, float min, float max)
        {
            return MathUtility.Percents(value, min, max);
        }

    }

}