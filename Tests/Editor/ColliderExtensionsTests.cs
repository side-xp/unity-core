using System.Collections.Generic;
using System.Text.RegularExpressions;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="ColliderExtensions"/>. These run in edit mode, where bounds are computed from the collider's geometry
    /// (the play-mode path that reads <see cref="Collider.bounds"/> directly is trivial and not re-tested here).
    /// </summary>
    public class ColliderExtensionsTests
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
            var go = new GameObject("Collider");
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

        #region BoxCollider

        [Test]
        public void GetColliderBounds_UnitBox_ReturnsUnitBoundsAtPosition()
        {
            GameObject go = NewObject(position: new Vector3(2f, -1f, 3f));
            go.AddComponent<BoxCollider>(); // default center 0, size (1,1,1)

            Assert.IsTrue(go.GetComponent<BoxCollider>().GetColliderBounds(out Bounds bounds));
            AssertApproximately(new Vector3(2f, -1f, 3f), bounds.center);
            AssertApproximately(Vector3.one, bounds.size);
        }

        [Test]
        public void GetColliderBounds_BoxRotated90AroundY_SwapsExtents()
        {
            GameObject go = NewObject(rotation: Quaternion.Euler(0f, 90f, 0f));
            BoxCollider box = go.AddComponent<BoxCollider>();
            box.size = new Vector3(1f, 2f, 3f);

            box.GetColliderBounds(out Bounds bounds);

            // A 90° yaw swaps the X and Z extents: (1,2,3) => (3,2,1). A box AABB is exact under rotation.
            AssertApproximately(Vector3.zero, bounds.center);
            AssertApproximately(new Vector3(3f, 2f, 1f), bounds.size);
        }

        [Test]
        public void GetColliderBounds_BoxScaled_ReflectsLossyScale()
        {
            GameObject go = NewObject(scale: new Vector3(2f, 3f, 4f));
            go.AddComponent<BoxCollider>(); // unit size

            go.GetComponent<BoxCollider>().GetColliderBounds(out Bounds bounds);

            AssertApproximately(new Vector3(2f, 3f, 4f), bounds.size);
        }

        #endregion


        #region SphereCollider

        [Test]
        public void GetColliderBounds_Sphere_ReturnsDiameterSizedBoundsAtCenter()
        {
            GameObject go = NewObject();
            SphereCollider sphere = go.AddComponent<SphereCollider>();
            sphere.radius = 1f;
            sphere.center = new Vector3(1f, 0f, 0f);

            sphere.GetColliderBounds(out Bounds bounds);

            AssertApproximately(new Vector3(1f, 0f, 0f), bounds.center);
            AssertApproximately(new Vector3(2f, 2f, 2f), bounds.size); // diameter = radius * 2
        }

        #endregion


        #region CapsuleCollider

        [Test]
        public void GetColliderBounds_CapsuleAlongY_HeightAlongYDiameterAcross()
        {
            GameObject go = NewObject();
            CapsuleCollider capsule = go.AddComponent<CapsuleCollider>();
            capsule.radius = 0.5f;   // diameter 1
            capsule.height = 2f;
            capsule.direction = 1;   // Y axis

            capsule.GetColliderBounds(out Bounds bounds);

            AssertApproximately(new Vector3(1f, 2f, 1f), bounds.size);
        }

        [Test]
        public void GetColliderBounds_CapsuleAlongX_HeightAlongX()
        {
            GameObject go = NewObject();
            CapsuleCollider capsule = go.AddComponent<CapsuleCollider>();
            capsule.radius = 0.5f;   // diameter 1
            capsule.height = 2f;
            capsule.direction = 0;   // X axis

            capsule.GetColliderBounds(out Bounds bounds);

            AssertApproximately(new Vector3(2f, 1f, 1f), bounds.size);
        }

        #endregion


        #region MeshCollider

        [Test]
        public void GetColliderBounds_MeshColliderWithSharedMesh_ReturnsTransformedMeshBounds()
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = new Vector3(5f, 0f, 0f);
            _objects.Add(cube);
            Object.DestroyImmediate(cube.GetComponent<BoxCollider>());

            MeshCollider mesh = cube.AddComponent<MeshCollider>();
            mesh.sharedMesh = cube.GetComponent<MeshFilter>().sharedMesh; // unit cube mesh

            Assert.IsTrue(mesh.GetColliderBounds(out Bounds bounds));
            AssertApproximately(new Vector3(5f, 0f, 0f), bounds.center);
            AssertApproximately(Vector3.one, bounds.size);
        }

        [Test]
        public void GetColliderBounds_MeshColliderWithoutSharedMesh_ReturnsFalseAndWarns()
        {
            GameObject go = NewObject();
            MeshCollider mesh = go.AddComponent<MeshCollider>(); // no MeshFilter => sharedMesh stays null

            LogAssert.Expect(LogType.Warning, new Regex("can't be queried"));
            Assert.IsFalse(mesh.GetColliderBounds(out Bounds bounds));
            Assert.AreEqual(default(Bounds), bounds);
        }

        #endregion


        #region Overloads

        [Test]
        public void GetColliderBounds_NonOutOverload_ReturnsBounds()
        {
            GameObject go = NewObject(position: new Vector3(0f, 4f, 0f));
            BoxCollider box = go.AddComponent<BoxCollider>();

            Bounds bounds = box.GetColliderBounds();

            AssertApproximately(new Vector3(0f, 4f, 0f), bounds.center);
            AssertApproximately(Vector3.one, bounds.size);
        }

        #endregion

    }

}
