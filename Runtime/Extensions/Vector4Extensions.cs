using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Extension functions for <see cref="Vector4"/> values.
    /// </summary>
    public static class Vector4Extensions
    {

        /// <summary>
        /// Returns a new <see cref="Vector4"/> with its values superior or equal to the given minimum value.
        /// </summary>
        /// <inheritdoc cref="Vector3Extensions.Min(Vector3, float)"/>
        public static Vector4 Min(this Vector4 vector, float min)
        {
            return new Vector4
            (
                Mathf.Max(vector.x, min),
                Mathf.Max(vector.y, min),
                Mathf.Max(vector.z, min),
                Mathf.Max(vector.w, min)
            );
        }

        /// <summary>
        /// Returns a new <see cref="Vector4"/> instance with its values inferior or equal to the given maximum value.
        /// </summary>
        /// <inheritdoc cref="Vector3Extensions.Max(Vector3, float)"/>
        public static Vector4 Max(this Vector4 vector, float max)
        {
            return new Vector4
            (
                Mathf.Min(vector.x, max),
                Mathf.Min(vector.y, max),
                Mathf.Min(vector.z, max),
                Mathf.Min(vector.w, max)
            );
        }

    }

}