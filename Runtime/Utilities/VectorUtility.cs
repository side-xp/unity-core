using System.Collections.Generic;

using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Miscellaeous functions for working with vectors.
    /// </summary>
    public static class VectorUtility
    {

        /// <summary>
        /// Computes the barycentre of the given points.<br/>
        /// Barycentre formula:<br/>
        /// <code>
        /// .               ->                ->
        /// ->    weightA * OA + weightN... * ON...
        /// OG = ___________________________________
        /// .            weightA + weightN...
        /// </code>
        /// </summary>
        /// <param name="points">The points from which you want to compute the barycentre.</param>
        /// <returns>Returns the computed barycentre.</returns>
        public static Vector3 Barycentre(IList<Vector3> points)
        {
            Vector3 sum = Vector3.zero;
            foreach (Vector3 p in points)
                sum += p;
            return points.Count > 0 ? sum / points.Count : Vector3.zero;
        }

        /// <inheritdoc cref="Barycentre(IList{Vector2})"/>
        public static Vector2 Barycentre(IList<Vector2> points)
        {
            Vector2 sum = Vector2.zero;
            foreach (Vector2 p in points)
                sum += p;
            return points.Count > 0 ? sum / points.Count : Vector2.zero;
        }

        /// <inheritdoc cref="Barycentre(IList{Vector3})"/>
        /// <param name="weightedPoints">The points from which you want to compute the barycentre, and their associated weight. The more the weight value, the closer the barycentre to this point.</param>
        public static Vector3 Barycentre(IList<(Vector3, float)> weightedPoints)
        {
            Vector3 sum = Vector3.zero;
            float weightSum = 0f;
            foreach ((Vector3, float) p in weightedPoints)
            {
                sum += p.Item1 * p.Item2;
                weightSum += p.Item2;
            }
            return weightSum != 0f ? sum / weightSum : Vector3.zero;
        }

        /// <inheritdoc cref="Barycentre(IList{ValueTuple{Vector3, float}})"/>
        public static Vector2 Barycentre(IList<(Vector2, float)> weightedPoints)
        {
            Vector2 sum = Vector2.zero;
            float weightSum = 0f;
            foreach ((Vector2, float) p in weightedPoints)
            {
                sum += p.Item1 * p.Item2;
                weightSum += p.Item2;
            }
            return weightSum != 0f ? sum / weightSum : Vector2.zero;
        }

    }

}