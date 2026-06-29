using System.Collections.Generic;

using NUnit.Framework;

using UnityEngine;

namespace SideXP.Core.Tests
{

    public class VectorUtilityTests
    {

        private const float Tolerance = 1e-5f;

        private static void AssertApproximatelyEqual(Vector3 expected, Vector3 actual)
        {
            Assert.AreEqual(expected.x, actual.x, Tolerance, "x component");
            Assert.AreEqual(expected.y, actual.y, Tolerance, "y component");
            Assert.AreEqual(expected.z, actual.z, Tolerance, "z component");
        }

        private static void AssertApproximatelyEqual(Vector2 expected, Vector2 actual)
        {
            Assert.AreEqual(expected.x, actual.x, Tolerance, "x component");
            Assert.AreEqual(expected.y, actual.y, Tolerance, "y component");
        }

        #region Barycentre - Vector3

        [Test]
        public void Barycentre_Vector3_Empty_ReturnsZero()
        {
            AssertApproximatelyEqual(Vector3.zero, VectorUtility.Barycentre(new List<Vector3>()));
        }

        [Test]
        public void Barycentre_Vector3_SinglePoint_ReturnsThatPoint()
        {
            var point = new Vector3(1f, 2f, 3f);
            AssertApproximatelyEqual(point, VectorUtility.Barycentre(new List<Vector3> { point }));
        }

        [Test]
        public void Barycentre_Vector3_MultiplePoints_ReturnsAverage()
        {
            var points = new List<Vector3>
            {
                new Vector3(0f, 0f, 0f),
                new Vector3(2f, 0f, 0f),
                new Vector3(0f, 6f, 0f),
            };
            AssertApproximatelyEqual(new Vector3(2f / 3f, 2f, 0f), VectorUtility.Barycentre(points));
        }

        #endregion


        #region Barycentre - Vector2

        [Test]
        public void Barycentre_Vector2_Empty_ReturnsZero()
        {
            AssertApproximatelyEqual(Vector2.zero, VectorUtility.Barycentre(new List<Vector2>()));
        }

        [Test]
        public void Barycentre_Vector2_MultiplePoints_ReturnsAverage()
        {
            var points = new List<Vector2>
            {
                new Vector2(0f, 0f),
                new Vector2(4f, 0f),
                new Vector2(0f, 2f),
            };
            AssertApproximatelyEqual(new Vector2(4f / 3f, 2f / 3f), VectorUtility.Barycentre(points));
        }

        #endregion


        #region Barycentre - weighted Vector3

        [Test]
        public void Barycentre_WeightedVector3_EqualWeights_MatchPlainAverage()
        {
            var weighted = new List<(Vector3, float)>
            {
                (new Vector3(0f, 0f, 0f), 1f),
                (new Vector3(2f, 0f, 0f), 1f),
                (new Vector3(0f, 6f, 0f), 1f),
            };
            AssertApproximatelyEqual(new Vector3(2f / 3f, 2f, 0f), VectorUtility.Barycentre(weighted));
        }

        [Test]
        public void Barycentre_WeightedVector3_HeavierPointPullsResult()
        {
            // Point B is weighted 3x, so the barycentre sits at (0*1 + 4*3) / (1 + 3) = 3.
            var weighted = new List<(Vector3, float)>
            {
                (new Vector3(0f, 0f, 0f), 1f),
                (new Vector3(4f, 0f, 0f), 3f),
            };
            AssertApproximatelyEqual(new Vector3(3f, 0f, 0f), VectorUtility.Barycentre(weighted));
        }

        [Test]
        public void Barycentre_WeightedVector3_ZeroTotalWeight_ReturnsZero()
        {
            // Weights cancel out, so the total weight is 0 and the method falls back to Vector3.zero.
            var weighted = new List<(Vector3, float)>
            {
                (new Vector3(5f, 5f, 5f), 1f),
                (new Vector3(1f, 1f, 1f), -1f),
            };
            AssertApproximatelyEqual(Vector3.zero, VectorUtility.Barycentre(weighted));
        }

        #endregion


        #region Barycentre - weighted Vector2

        [Test]
        public void Barycentre_WeightedVector2_HeavierPointPullsResult()
        {
            var weighted = new List<(Vector2, float)>
            {
                (new Vector2(0f, 0f), 1f),
                (new Vector2(4f, 0f), 3f),
            };
            AssertApproximatelyEqual(new Vector2(3f, 0f), VectorUtility.Barycentre(weighted));
        }

        [Test]
        public void Barycentre_WeightedVector2_ZeroTotalWeight_ReturnsZero()
        {
            var weighted = new List<(Vector2, float)>
            {
                (new Vector2(5f, 5f), 2f),
                (new Vector2(1f, 1f), -2f),
            };
            AssertApproximatelyEqual(Vector2.zero, VectorUtility.Barycentre(weighted));
        }

        #endregion

    }

}
