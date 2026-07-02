using System.Collections.Generic;

using NUnit.Framework;

using UnityEngine;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="GameObjectExtensions"/>.
    /// </summary>
    public class GameObjectExtensionsTests
    {

        private const float Tolerance = 1e-3f;

        // Marker components used to test type-based component queries.
        private class Marker : MonoBehaviour { }
        private class OtherMarker : MonoBehaviour { }

        // Root objects created during a test, destroyed in TearDown (destroying a root destroys its whole subtree).
        private readonly List<GameObject> _roots = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject root in _roots)
            {
                if (root != null)
                    Object.DestroyImmediate(root);
            }
            _roots.Clear();
        }

        private GameObject NewObject(string name = "Object")
        {
            var go = new GameObject(name);
            _roots.Add(go);
            return go;
        }

        private GameObject NewChild(GameObject parent, string name = "Child", bool active = true)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.SetActive(active);
            return go;
        }

        private GameObject NewPrimitive(Vector3 position)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube); // MeshRenderer + BoxCollider, unit-sized mesh
            go.transform.position = position;
            _roots.Add(go);
            return go;
        }

        // NUnit's Assert.AreEqual compares Vector3 via exact Equals, so use a tolerance for bounds math.
        private static void AssertApproximately(Vector3 expected, Vector3 actual, string message = null)
        {
            Assert.Less(Vector3.Distance(expected, actual), Tolerance, message ?? $"Expected {expected}, got {actual}.");
        }

        #region GetChildren

        [Test]
        public void GetChildren_DirectOnly_ReturnsDirectChildGameObjects()
        {
            GameObject root = NewObject();
            GameObject a = NewChild(root, "A");
            GameObject b = NewChild(root, "B");
            NewChild(a, "A_Nested"); // grandchild excluded from a direct query

            GameObject[] children = root.GetChildren();

            CollectionAssert.AreEquivalent(new[] { a, b }, children);
        }

        [Test]
        public void GetChildren_Recursive_ReturnsAllDescendantGameObjects()
        {
            GameObject root = NewObject();
            GameObject a = NewChild(root, "A");
            GameObject b = NewChild(root, "B");
            GameObject nested = NewChild(a, "A_Nested");

            GameObject[] children = root.GetChildren(true);

            CollectionAssert.AreEquivalent(new[] { a, b, nested }, children);
        }

        [Test]
        public void GetChildren_NoChildren_ReturnsEmpty()
        {
            Assert.IsEmpty(NewObject().GetChildren());
        }

        #endregion


        #region IsPrefab

        [Test]
        public void IsPrefab_SceneObject_ReturnsFalse()
        {
            // A runtime-created object belongs to a scene, so it is never considered a prefab.
            // (The prefab-asset case is covered with RuntimeObjectUtility.)
            Assert.IsFalse(NewObject().IsPrefab());
        }

        #endregion


        #region GetBounds

        [Test]
        public void GetBounds_RendererObject_ReturnsTrueWithMatchingBounds()
        {
            GameObject cube = NewPrimitive(new Vector3(5f, 0f, 0f));

            Assert.IsTrue(cube.GetBounds(out Bounds bounds));
            AssertApproximately(cube.transform.position, bounds.center, "Bounds should be centered on the cube.");
            AssertApproximately(Vector3.one, bounds.size, "A unit cube should have unit-sized bounds.");
        }

        [Test]
        public void GetBounds_PreferCollider_ReturnsColliderBounds()
        {
            // A primitive cube has both a renderer and a collider; preferCollider takes the collider path first.
            // ColliderExtensions computes collider bounds from geometry at edit time, so the value is meaningful here.
            GameObject cube = NewPrimitive(new Vector3(-2f, 1f, 0f));

            Assert.IsTrue(cube.GetBounds(out Bounds bounds, preferCollider: true));
            AssertApproximately(cube.transform.position, bounds.center);
            AssertApproximately(Vector3.one, bounds.size);
        }

        [Test]
        public void GetBounds_NoBoundsComponent_ReturnsFalse()
        {
            Assert.IsFalse(NewObject().GetBounds(out Bounds bounds));
            Assert.AreEqual(default(Bounds), bounds);
        }

        [Test]
        public void GetBounds_IncludeChildren_FindsBoundsOnChildRenderer()
        {
            GameObject root = NewObject();
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(root.transform, false);
            cube.transform.localPosition = Vector3.zero;

            Assert.IsTrue(root.GetBounds(out Bounds bounds, includeChildren: true));
            AssertApproximately(Vector3.one, bounds.size);
        }

        [Test]
        public void GetBounds_IncludeChildrenDisabled_IgnoresChildBounds()
        {
            GameObject root = NewObject();
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(root.transform, false);

            Assert.IsFalse(root.GetBounds(out _, includeChildren: false));
        }

        [Test]
        public void GetBounds_NonOutOverload_ReturnsDefaultWhenNotFound()
        {
            Assert.AreEqual(default(Bounds), NewObject().GetBounds());
        }

        #endregion


        #region TryGetComponentInParent

        [Test]
        public void TryGetComponentInParent_Generic_FoundOnAncestor()
        {
            GameObject parent = NewObject();
            Marker marker = parent.AddComponent<Marker>();
            GameObject child = NewChild(parent);

            Assert.IsTrue(child.TryGetComponentInParent(out Marker output));
            Assert.AreSame(marker, output);
        }

        [Test]
        public void TryGetComponentInParent_Generic_NotFound()
        {
            GameObject child = NewChild(NewObject());
            Assert.IsFalse(child.TryGetComponentInParent(out OtherMarker output));
            Assert.IsNull(output);
        }

        [Test]
        public void TryGetComponentInParent_TypeOverload_FoundOnAncestor()
        {
            GameObject parent = NewObject();
            parent.AddComponent<Marker>();
            GameObject child = NewChild(parent);

            Assert.IsTrue(child.TryGetComponentInParent(typeof(Marker), out Component output));
            Assert.IsInstanceOf<Marker>(output);
        }

        [Test]
        public void TryGetComponentInParent_IncludeInactive_TogglesInactiveAncestors()
        {
            GameObject parent = NewObject();
            parent.AddComponent<Marker>();
            parent.SetActive(false); // the whole branch is now inactive
            GameObject child = NewChild(parent);

            Assert.IsTrue(child.TryGetComponentInParent(out Marker _, includeInactive: true));
            Assert.IsFalse(child.TryGetComponentInParent(out Marker _, includeInactive: false));
        }

        #endregion


        #region TryGetComponentInChildren

        [Test]
        public void TryGetComponentInChildren_Generic_FoundOnDescendant()
        {
            GameObject root = NewObject();
            GameObject child = NewChild(root);
            Marker marker = child.AddComponent<Marker>();

            Assert.IsTrue(root.TryGetComponentInChildren(out Marker output));
            Assert.AreSame(marker, output);
        }

        [Test]
        public void TryGetComponentInChildren_Generic_NotFound()
        {
            GameObject root = NewObject();
            NewChild(root);
            Assert.IsFalse(root.TryGetComponentInChildren(out OtherMarker output));
            Assert.IsNull(output);
        }

        [Test]
        public void TryGetComponentInChildren_TypeOverload_FoundOnDescendant()
        {
            GameObject root = NewObject();
            NewChild(root).AddComponent<Marker>();

            Assert.IsTrue(root.TryGetComponentInChildren(typeof(Marker), out Component output));
            Assert.IsInstanceOf<Marker>(output);
        }

        [Test]
        public void TryGetComponentInChildren_IncludeInactive_TogglesInactiveDescendants()
        {
            GameObject root = NewObject();
            GameObject child = NewChild(root, active: false);
            child.AddComponent<Marker>();

            Assert.IsTrue(root.TryGetComponentInChildren(out Marker _, includeInactive: true));
            Assert.IsFalse(root.TryGetComponentInChildren(out Marker _, includeInactive: false));
        }

        #endregion

    }

}
