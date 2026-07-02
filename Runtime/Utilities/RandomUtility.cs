using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Utility functions for random behaviors.
    /// </summary>
    public static class RandomUtility
    {

        /// <summary>
        /// Gets a random value among the given ones.
        /// </summary>
        /// <param name="values">The values that can be picked at random.</param>
        /// <returns>Returns the randomly picked value, or null if no value is provided.</returns>
        public static object RandomAmong(params object[] values)
        {
            return RandomAmong<object>(values);
        }

        /// <typeparam name="T">The type of the values to pick at random.</typeparam>
        /// <returns>Returns the randomly picked value, or the default value of <typeparamref name="T"/> if no value is provided.</returns>
        /// <inheritdoc cref="RandomAmong(object[])"/>
        public static T RandomAmong<T>(params T[] values)
        {
            if (values == null || values.Length == 0)
                return default;

            return values[Random.Range(0, values.Length)];
        }

    }

}
