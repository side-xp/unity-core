using System.Collections.Generic;

using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Extension functions for <see cref="Vector2"/> values.
    /// </summary>
    public static class Vector2Extensions
    {

        /// <summary>
        /// Returns a new <see cref="Vector2"/> with its values superior or equal to the given minimum value.
        /// </summary>
        /// <inheritdoc cref="Vector3Extensions.Min(Vector3, float)"/>
        public static Vector2 Min(this Vector2 vector, float min)
        {
            return new Vector2
            (
                Mathf.Max(vector.x, min),
                Mathf.Max(vector.y, min)
            );
        }

        /// <summary>
        /// Returns a new <see cref="Vector2"/> instance with its values inferior or equal to the given maximum value.
        /// </summary>
        /// <inheritdoc cref="Vector3Extensions.Max(Vector3, float)"/>
        public static Vector2 Max(this Vector2 vector, float max)
        {
            return new Vector2
            (
                Mathf.Min(vector.x, max),
                Mathf.Min(vector.y, max)
            );
        }

        /// <inheritdoc cref="VectorUtility.Barycentre(IList{Vector2})"/>
        public static Vector2 Barycentre(this IList<Vector2> points)
        {
            return VectorUtility.Barycentre(points);
        }

        /// <inheritdoc cref="VectorUtility.Barycentre(IList{ValueTuple{Vector2, float}})"/>
        public static Vector2 Barycentre(this IList<(Vector2, float)> weightedPoints)
        {
            return VectorUtility.Barycentre(weightedPoints);
        }

    }

}