using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Utility functions for random behaviors.
    /// </summary>
    public static class RandomUtility
    {

        /// <summary>
        /// Gets a ramdom value among the given ones.
        /// </summary>
        /// <param name="values">The values that can be picked at random.</param>
        /// <returns>Returns the randomly picked value.</returns>
        public static object RandomAmong(params object[] values)
        {
            return values[Random.Range(0, values.Length)];
        }

        /// <typeparam name="T">The type of the values to picked at random.</typeparam>
        /// <inheritdoc cref="RandomAmong(object[])"/>
        public static T RandomAmong<T>(params T[] values)
        {
            return values[Random.Range(0, values.Length)];
        }

    }

}