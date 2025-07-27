using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Extension functions for <see cref="Vector2Int"/> values.
    /// </summary>
    public static class Vector2IntExtensions
    {

        /// <summary>
        /// Returns a new <see cref="Vector2Int"/> with its values superior or equal to the given minimum value.
        /// </summary>
        /// <inheritdoc cref="Vector3Extensions.Min(Vector3, float)"/>
        public static Vector2Int Min(this Vector2Int vector, int min)
        {
            return new Vector2Int
            (
                Mathf.Max(vector.x, min),
                Mathf.Max(vector.y, min)
            );
        }

        /// <summary>
        /// Returns a new <see cref="Vector2Int"/> instance with its values inferior or equal to the given maximum value.
        /// </summary>
        /// <inheritdoc cref="Vector3Extensions.Max(Vector3, float)"/>
        public static Vector2Int Max(this Vector2Int vector, int max)
        {
            return new Vector2Int
            (
                Mathf.Min(vector.x, max),
                Mathf.Min(vector.y, max)
            );
        }

    }

}