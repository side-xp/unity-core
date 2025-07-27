using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Extension functions for <see cref="Vector3Int"/> values.
    /// </summary>
    public static class Vector3IntExtensions
    {

        /// <summary>
        /// Returns a new <see cref="Vector3Int"/> with its values superior or equal to the given minimum value.
        /// </summary>
        /// <inheritdoc cref="Vector3Extensions.Min(Vector3, float)"/>
        public static Vector3Int Min(this Vector3Int vector, int min)
        {
            return new Vector3Int
            (
                Mathf.Max(vector.x, min),
                Mathf.Max(vector.y, min),
                Mathf.Max(vector.z, min)
            );
        }

        /// <summary>
        /// Returns a new <see cref="Vector3Int"/> instance with its values inferior or equal to the given maximum value.
        /// </summary>
        /// <inheritdoc cref="Vector3Extensions.Max(Vector3, float)"/>
        public static Vector3Int Max(this Vector3Int vector, int max)
        {
            return new Vector3Int
            (
                Mathf.Min(vector.x, max),
                Mathf.Min(vector.y, max),
                Mathf.Min(vector.z, max)
            );
        }

    }

}