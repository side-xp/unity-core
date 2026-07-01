using System.Collections.Generic;

using NUnit.Framework;

using UnityEngine;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="CameraExtensions"/>.
    /// </summary>
    public class CameraExtensionsTests
    {

        private const float Tolerance = 1e-3f;

        // Objects created during a test, destroyed in TearDown.
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

        /// <summary>
        /// Creates a camera with a fixed aspect so the extents math is deterministic (independent of the current screen size).
        /// </summary>
        private Camera NewCamera(bool orthographic, float orthographicSize, float aspect)
        {
            var go = new GameObject("Camera");
            _objects.Add(go);
            Camera camera = go.AddComponent<Camera>();
            camera.orthographic = orthographic;
            camera.orthographicSize = orthographicSize;
            camera.aspect = aspect; // explicit override; stays until ResetAspect()
            return camera;
        }

        #region GetExtentsOrthographic

        [Test]
        public void GetExtentsOrthographic_Orthographic_ReturnsSizeScaledByAspectAndSize()
        {
            Camera camera = NewCamera(true, 5f, 2f);

            Vector2 extents = camera.GetExtentsOrthographic();

            // (orthographicSize * aspect, orthographicSize) => (10, 5)
            Assert.Less(Vector2.Distance(new Vector2(10f, 5f), extents), Tolerance);
        }

        [Test]
        public void GetExtentsOrthographic_Perspective_ReturnsZero()
        {
            // A perspective camera logs an informational message (Debug.Log, which does not fail the test) and returns zero.
            Camera camera = NewCamera(false, 5f, 2f);

            Assert.AreEqual(Vector2.zero, camera.GetExtentsOrthographic());
        }

        #endregion


        #region GetBoundsOrthographic

        [Test]
        public void GetBoundsOrthographic_Orthographic_ReturnsDoubleTheExtents()
        {
            Camera camera = NewCamera(true, 5f, 2f);

            Vector2 bounds = camera.GetBoundsOrthographic();

            // extents (10, 5) doubled => (20, 10)
            Assert.Less(Vector2.Distance(new Vector2(20f, 10f), bounds), Tolerance);
        }

        [Test]
        public void GetBoundsOrthographic_Perspective_ReturnsZero()
        {
            Camera camera = NewCamera(false, 5f, 2f);

            Assert.AreEqual(Vector2.zero, camera.GetBoundsOrthographic());
        }

        #endregion

    }

}
