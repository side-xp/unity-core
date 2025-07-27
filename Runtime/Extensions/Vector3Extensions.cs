using System.Collections.Generic;

using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Extension functions for <see cref="Vector3"/> values.
    /// </summary>
    public static class Vector3Extensions
    {

        /// <summary>
        /// Returns a new <see cref="Vector3"/> with its values superior or equal to the given minimum value.
        /// </summary>
        /// <param name="vector">The input vector to compute.</param>
        /// <param name="min">The minimum value of the given vector.</param>
        /// <returns>Returns the computed vector.</returns>
        public static Vector3 Min(this Vector3 vector, float min)
        {
            return new Vector3
            (
                Mathf.Max(vector.x, min),
                Mathf.Max(vector.y, min),
                Mathf.Max(vector.z, min)
            );
        }

        /// <summary>
        /// Returns a new <see cref="Vector3"/> instance with its values inferior or equal to the given maximum value.
        /// </summary>
        /// <inheritdoc cref="Min(Vector3, float)"/>
        /// <param name="max">The maximum value of the given vector.</param>
        public static Vector3 Max(this Vector3 vector, float max)
        {
            return new Vector3
            (
                Mathf.Min(vector.x, max),
                Mathf.Min(vector.y, max),
                Mathf.Min(vector.z, max)
            );
        }

        /// <summary>
        /// Translates the given points.
        /// </summary>
        /// <param name="points">The points you want to translate.</param>
        /// <param name="translation">The traslation vector.</param>
        /// <param name="inPlace">If enabled, the input array is modified in-place, instead of returning a new array.</param>
        /// <returns>Returns the translated points.</returns>
        public static Vector3[] Translate(this Vector3[] points, Vector3 translation, bool inPlace = false)
        {
            // If the array is meant to be modified in-place
            if (inPlace)
            {
                // For each point
                for (int i = 0; i < points.Length; i++)
                {
                    // Translate the position in-place
                    points[i].Set
                    (
                        points[i].x + translation.x,
                        points[i].y + translation.y,
                        points[i].z + translation.z
                    );
                }
                return points;
            }

            Vector3[] newPoints = new Vector3[points.Length];
            // For each point
            for (int i = 0; i < points.Length; i++)
            {
                // Create a new point with translation applied
                newPoints[i] = new Vector3
                (
                    points[i].x + translation.x,
                    points[i].y + translation.y,
                    points[i].z + translation.z
                );
            }
            return newPoints;
        }

        /// <summary>
        /// Converts this list into a <see cref="Vector2"/> array.
        /// </summary>
        /// <param name="points">The points you want to convert.</param>
        /// <returns>Returns the processed array.</returns>
        public static Vector2[] To2D(this IList<Vector3> points)
        {
            Vector2[] points2D = new Vector2[points.Count];
            int i = 0;
            foreach (Vector3 p in points)
            {
                points2D[i++] = p;
                i++;
            }
            return points2D;
        }

        /// <inheritdoc cref="VectorUtility.Barycentre(IList{Vector3})"/>
        public static Vector3 Barycentre(this IList<Vector3> points)
        {
            return VectorUtility.Barycentre(points);
        }

        /// <inheritdoc cref="VectorUtility.Barycentre(IList{ValueTuple{Vector3, float}})"/>
        public static Vector3 Barycentre(this IList<(Vector3, float)> weightedPoints)
        {
            return VectorUtility.Barycentre(weightedPoints);
        }

    }

}