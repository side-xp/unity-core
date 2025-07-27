using System.Collections.Generic;

namespace SideXP.Core
{

    /// <summary>
    /// Miscellaneous utility functions for calculating statistics.
    /// </summary>
    public static class StatisticsUtility
    {

        /// <summary>
        /// Calculates the sum from given values.
        /// </summary>
        /// <param name="values">The values to sum.</param>
        /// <returns>Returns the calculated value.</returns>
        public static float Sum(this IEnumerable<float> values)
        {
            float sum = 0;
            foreach (float v in values)
                sum += v;
            return sum;
        }
        
        /// <inheritdoc cref="Sum(IEnumerable{float})"/>
        public static int Sum(this IEnumerable<int> values)
        {
            int sum = 0;
            foreach (int v in values)
                sum += v;
            return sum;
        }

        /// <summary>
        /// Calculates the average value from given ones.
        /// </summary>
        /// <param name="values">The values of which to calculate the average.</param>
        /// <returns>Returns the calculated value.</returns>
        public static float Average(this IEnumerable<float> values)
        {
            float sum = 0;
            float count = 0;
            foreach (float v in values)
            {
                sum += v;
                count++;
            }
            return sum / count;
        }

        /// <inheritdoc cref="Average(IEnumerable{float})"/>
        public static float Average(this IEnumerable<int> values)
        {
            int sum = 0;
            int count = 0;
            foreach (int v in values)
            {
                sum += v;
                count++;
            }
            return sum / (float)count;
        }

    }

}