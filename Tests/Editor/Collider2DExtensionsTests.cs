using System.Collections.Generic;

using NUnit.Framework;

using UnityEngine;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="Collider2DExtensions"/>. Only the primitive colliders (circle/box/capsule) compute world bounds from
    /// geometry at edit time; the complex types (polygon/edge/composite/custom) still fall back to <see cref="Collider2D.bounds"/>
    /// and are not covered here.
    /// </summary>
    public class Collider2DExtensionsTests
    {

        private const float Tolerance = 1e-3f;

        private readonly List<GameObject> _objects = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject go in _objects)
            {
                if (go != null)
                    Object.DestroyImmediate(go);
            }
            _objects.Clear();
        }

        private GameObject NewObject(Vector3 position = default, Quaternion rotation = default, Vector3 scale = default)
        {
            var go = new GameObject("Collider2D");
            go.transform.position = position;
            go.transform.rotation = rotation == default ? Quaternion.identity : rotation;
            go.transform.localScale = scale == default ? Vector3.one : scale;
            _objects.Add(go);
            return go;
        }

        private static void AssertApproximately(Vector3 expected, Vector3 actual, string message = null)
        {
            Assert.Less(Vector3.Distance(expected, actual), Tolerance, message ?? $"Expected {expected}, got {actual}.");
        }

        #region BoxCollider2D

        [Test]
        public void GetColliderBounds_Box_ReturnsWorldBoundsAtPosition()
        {
            GameObject go = NewObject(position: new Vector3(100f, 50f, 0f));
            BoxCollider2D box = go.AddComponent<BoxCollider2D>();
            box.size = new Vector2(2f, 4f);

            Assert.IsTrue(box.GetColliderBounds(out Bounds bounds));
            AssertApproximately(new Vector3(100f, 50f, 0f), bounds.center, "Bounds must follow the object's world position.");
            AssertApproximately(new Vector3(2f, 4f, 0f), bounds.size);
        }

        [Test]
        public void GetColliderBounds_BoxRotated90AroundZ_SwapsExtents()
        {
            GameObject go = NewObject(rotation: Quaternion.Euler(0f, 0f, 90f));
            BoxCollider2D box = go.AddComponent<BoxCollider2D>();
            box.size = new Vector2(1f, 3f);

            box.GetColliderBounds(out Bounds bounds);

            AssertApproximately(Vector3.zero, bounds.center);
            AssertApproximately(new Vector3(3f, 1f, 0f), bounds.size); // (1,3) rotated 90° => (3,1)
        }

        [Test]
        public void GetColliderBounds_BoxScaled_ReflectsLossyScale()
        {
            GameObject go = NewObject(scale: new Vector3(2f, 3f, 1f));
            BoxCollider2D box = go.AddComponent<BoxCollider2D>();
            box.size = new Vector2(1f, 1f);

            box.GetColliderBounds(out Bounds bounds);

            AssertApproximately(new Vector3(2f, 3f, 0f), bounds.size);
        }

        #endregion


        #region CircleCollider2D

        [Test]
        public void GetColliderBounds_Circle_ReturnsDiameterSizedBoundsAtOffset()
        {
            GameObject go = NewObject();
            CircleCollider2D circle = go.AddComponent<CircleCollider2D>();
            circle.radius = 1f;
            circle.offset = new Vector2(1f, 0f);

            circle.GetColliderBounds(out Bounds bounds);

            AssertApproximately(new Vector3(1f, 0f, 0f), bounds.center);
            AssertApproximately(new Vector3(2f, 2f, 0f), bounds.size); // diameter = radius * 2
        }

        #endregion


        #region CapsuleCollider2D

        [Test]
        public void GetColliderBounds_Capsule_ReturnsSizeSizedBounds()
        {
            GameObject go = NewObject();
            CapsuleCollider2D capsule = go.AddComponent<CapsuleCollider2D>();
            capsule.size = new Vector2(2f, 5f); // size is already the capsule's bounding box

            capsule.GetColliderBounds(out Bounds bounds);

            AssertApproximately(new Vector3(2f, 5f, 0f), bounds.size);
        }

        #endregion


        #region GetColliderOffset

        [Test]
        public void GetColliderOffset_ReturnsColliderOffset()
        {
            GameObject go = NewObject();
            BoxCollider2D box = go.AddComponent<BoxCollider2D>();
            box.offset = new Vector2(1f, 2f);

            Assert.IsTrue(box.GetColliderOffset(out Vector2 offset));
            Assert.Less(Vector2.Distance(new Vector2(1f, 2f), offset), Tolerance);
            Assert.Less(Vector2.Distance(new Vector2(1f, 2f), box.GetColliderOffset()), Tolerance);
        }

        #endregion

    }

}
