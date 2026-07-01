using System.Collections.Generic;

using NUnit.Framework;

using UnityEngine;

namespace SideXP.Core.Tests
{

    public class VectorExtensionsTests
    {

        private const float Tolerance = 1e-5f;

        // Note on semantics: Min(v, m) floors each component to at least m (component-wise Mathf.Max),
        // and Max(v, m) ceils each component to at most m (component-wise Mathf.Min). The names refer to
        // the bound being applied, not the operation.

        #region Vector2Extensions

        [Test]
        public void Vector2_Min_RaisesComponentsBelowBound()
        {
            Vector2 result = new Vector2(-5f, 10f).Min(0f);
            Assert.AreEqual(0f, result.x, Tolerance);
            Assert.AreEqual(10f, result.y, Tolerance);
        }

        [Test]
        public void Vector2_Max_LowersComponentsAboveBound()
        {
            Vector2 result = new Vector2(-5f, 10f).Max(0f);
            Assert.AreEqual(-5f, result.x, Tolerance);
            Assert.AreEqual(0f, result.y, Tolerance);
        }

        [Test]
        public void Vector2_Barycentre_ForwardsToVectorUtility()
        {
            var points = new List<Vector2> { new Vector2(0f, 0f), new Vector2(2f, 4f) };
            Vector2 result = points.Barycentre();
            Assert.AreEqual(1f, result.x, Tolerance);
            Assert.AreEqual(2f, result.y, Tolerance);
        }

        [Test]
        public void Vector2_WeightedBarycentre_ForwardsToVectorUtility()
        {
            var points = new List<(Vector2, float)>
            {
                (new Vector2(0f, 0f), 1f),
                (new Vector2(4f, 4f), 3f),
            };
            Vector2 result = points.Barycentre();
            Assert.AreEqual(3f, result.x, Tolerance);
            Assert.AreEqual(3f, result.y, Tolerance);
        }

        #endregion


        #region Vector2IntExtensions

        [Test]
        public void Vector2Int_Min_RaisesComponentsBelowBound()
        {
            Vector2Int result = new Vector2Int(-5, 10).Min(0);
            Assert.AreEqual(new Vector2Int(0, 10), result);
        }

        [Test]
        public void Vector2Int_Max_LowersComponentsAboveBound()
        {
            Vector2Int result = new Vector2Int(-5, 10).Max(0);
            Assert.AreEqual(new Vector2Int(-5, 0), result);
        }

        #endregion


        #region Vector3Extensions

        [Test]
        public void Vector3_Min_RaisesComponentsBelowBound()
        {
            Vector3 result = new Vector3(-5f, 3f, 10f).Min(0f);
            Assert.AreEqual(new Vector3(0f, 3f, 10f), result);
        }

        [Test]
        public void Vector3_Max_LowersComponentsAboveBound()
        {
            Vector3 result = new Vector3(-5f, 3f, 10f).Max(0f);
            Assert.AreEqual(new Vector3(-5f, 0f, 0f), result);
        }

        [Test]
        public void Vector3_Translate_Copy_ReturnsNewArrayAndLeavesSourceUntouched()
        {
            Vector3[] points = { new Vector3(1f, 1f, 1f), new Vector3(2f, 2f, 2f) };
            Vector3[] result = points.Translate(new Vector3(1f, 0f, -1f));

            Assert.AreNotSame(points, result);
            // Source is unchanged.
            Assert.AreEqual(new Vector3(1f, 1f, 1f), points[0]);
            Assert.AreEqual(new Vector3(2f, 2f, 2f), points[1]);
            // Result is translated.
            Assert.AreEqual(new Vector3(2f, 1f, 0f), result[0]);
            Assert.AreEqual(new Vector3(3f, 2f, 1f), result[1]);
        }

        [Test]
        public void Vector3_Translate_InPlace_MutatesAndReturnsSameArray()
        {
            Vector3[] points = { new Vector3(1f, 1f, 1f), new Vector3(2f, 2f, 2f) };
            Vector3[] result = points.Translate(new Vector3(1f, 0f, -1f), inPlace: true);

            Assert.AreSame(points, result);
            Assert.AreEqual(new Vector3(2f, 1f, 0f), points[0]);
            Assert.AreEqual(new Vector3(3f, 2f, 1f), points[1]);
        }

        [Test]
        public void Vector3_To2D_DropsZAndPreservesOrder()
        {
            var points = new List<Vector3>
            {
                new Vector3(1f, 2f, 99f),
                new Vector3(3f, 4f, -99f),
            };
            Vector2[] result = points.To2D();

            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(new Vector2(1f, 2f), result[0]);
            Assert.AreEqual(new Vector2(3f, 4f), result[1]);
        }

        // Regression: To2D used to advance its index by 2 per element, throwing IndexOutOfRangeException
        // for any list of 2+ points (and skipping slots). Verify a larger list converts cleanly.
        [Test]
        public void Vector3_To2D_MultipleElements_ConvertsAllWithoutThrowing()
        {
            var points = new List<Vector3>();
            for (int i = 0; i < 5; i++)
                points.Add(new Vector3(i, i * 2, i * 3));

            Vector2[] result = points.To2D();

            Assert.AreEqual(5, result.Length);
            for (int i = 0; i < 5; i++)
                Assert.AreEqual(new Vector2(i, i * 2), result[i]);
        }

        [Test]
        public void Vector3_Barycentre_ForwardsToVectorUtility()
        {
            var points = new List<Vector3> { new Vector3(0f, 0f, 0f), new Vector3(2f, 4f, 6f) };
            Vector3 result = points.Barycentre();
            Assert.AreEqual(new Vector3(1f, 2f, 3f), result);
        }

        #endregion


        #region Vector3IntExtensions

        [Test]
        public void Vector3Int_Min_RaisesComponentsBelowBound()
        {
            Vector3Int result = new Vector3Int(-5, 3, 10).Min(0);
            Assert.AreEqual(new Vector3Int(0, 3, 10), result);
        }

        [Test]
        public void Vector3Int_Max_LowersComponentsAboveBound()
        {
            Vector3Int result = new Vector3Int(-5, 3, 10).Max(0);
            Assert.AreEqual(new Vector3Int(-5, 0, 0), result);
        }

        #endregion


        #region Vector4Extensions

        [Test]
        public void Vector4_Min_RaisesComponentsBelowBound()
        {
            Vector4 result = new Vector4(-5f, 3f, 10f, -1f).Min(0f);
            Assert.AreEqual(new Vector4(0f, 3f, 10f, 0f), result);
        }

        [Test]
        public void Vector4_Max_LowersComponentsAboveBound()
        {
            Vector4 result = new Vector4(-5f, 3f, 10f, -1f).Max(0f);
            Assert.AreEqual(new Vector4(-5f, 0f, 0f, -1f), result);
        }

        #endregion

    }

}
