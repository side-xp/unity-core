using System.Collections.Generic;

using NUnit.Framework;

using UnityEngine;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="ComponentExtensions"/>.
    /// </summary>
    public class ComponentExtensionsTests
    {

        private const float Tolerance = 1e-3f;

        // Marker components used to test instantiation and type-based queries.
        private class Marker : MonoBehaviour { }
        private class OtherMarker : MonoBehaviour { }

        // Objects created during a test (including instantiated clones), destroyed in TearDown.
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

        private GameObject Track(GameObject go)
        {
            _objects.Add(go);
            return go;
        }

        private GameObject NewObject(string name = "Object")
        {
            return Track(new GameObject(name));
        }

        private GameObject NewChild(GameObject parent, string name = "Child", bool active = true)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.SetActive(active);
            return Track(go);
        }

        #region InstantiateGameObject

        [Test]
        public void InstantiateGameObject_PositionRotationParent_ClonesAtPoseUnderParent()
        {
            Marker source = NewObject("Source").AddComponent<Marker>();
            GameObject parent = NewObject("Parent");
            var position = new Vector3(1f, 2f, 3f);
            Quaternion rotation = Quaternion.Euler(0f, 90f, 0f);

            Marker clone = source.InstantiateGameObject(position, rotation, parent.transform, out GameObject cloneObject);
            Track(cloneObject);

            Assert.IsNotNull(clone);
            Assert.AreNotSame(source.gameObject, cloneObject, "The clone must be a distinct GameObject.");
            Assert.AreSame(cloneObject, clone.gameObject, "The returned component must belong to the out clone.");
            Assert.AreSame(parent.transform, cloneObject.transform.parent);
            Assert.Less(Vector3.Distance(position, cloneObject.transform.position), Tolerance);
            Assert.Less(Quaternion.Angle(rotation, cloneObject.transform.rotation), Tolerance);
        }

        [Test]
        public void InstantiateGameObject_Parent_ClonesUnderParent()
        {
            Marker source = NewObject("Source").AddComponent<Marker>();
            GameObject parent = NewObject("Parent");

            Marker clone = source.InstantiateGameObject(parent.transform, out GameObject cloneObject);
            Track(cloneObject);

            Assert.IsNotNull(clone);
            Assert.AreSame(parent.transform, cloneObject.transform.parent);
        }

        [Test]
        public void InstantiateGameObject_Parameterless_ClonesWithoutParent()
        {
            Marker source = NewObject("Source").AddComponent<Marker>();

            Marker clone = source.InstantiateGameObject(out GameObject cloneObject);
            Track(cloneObject);

            Assert.IsNotNull(clone);
            Assert.AreNotSame(source.gameObject, cloneObject);
            Assert.IsNull(cloneObject.transform.parent);
            Assert.AreSame(cloneObject, clone.gameObject);
        }

        [Test]
        public void InstantiateGameObject_NoOutOverload_ReturnsClonedComponent()
        {
            Marker source = NewObject("Source").AddComponent<Marker>();

            Marker clone = source.InstantiateGameObject();
            Track(clone.gameObject);

            Assert.IsNotNull(clone);
            Assert.AreNotSame(source, clone);
        }

        #endregion


        #region GetComponentsInChildren (includeInactive, excludeSelf)

        [Test]
        public void GetComponentsInChildren_ExcludeSelf_OmitsParentComponent()
        {
            GameObject parent = NewObject("Parent");
            Marker parentMarker = parent.AddComponent<Marker>();
            Marker childMarker = NewChild(parent, "Child").AddComponent<Marker>();

            Marker[] result = parent.transform.GetComponentsInChildren<Marker>(true, excludeSelf: true);

            CollectionAssert.Contains(result, childMarker);
            CollectionAssert.DoesNotContain(result, parentMarker);
        }

        [Test]
        public void GetComponentsInChildren_IncludeSelf_KeepsParentComponent()
        {
            GameObject parent = NewObject("Parent");
            Marker parentMarker = parent.AddComponent<Marker>();
            Marker childMarker = NewChild(parent, "Child").AddComponent<Marker>();

            Marker[] result = parent.transform.GetComponentsInChildren<Marker>(true, excludeSelf: false);

            CollectionAssert.AreEquivalent(new[] { parentMarker, childMarker }, result);
        }

        [Test]
        public void GetComponentsInChildren_ExcludeInactive_OmitsInactiveChildren()
        {
            GameObject parent = NewObject("Parent");
            Marker activeChild = NewChild(parent, "Active").AddComponent<Marker>();
            NewChild(parent, "Inactive", active: false).AddComponent<Marker>();

            Marker[] result = parent.transform.GetComponentsInChildren<Marker>(false, excludeSelf: true);

            CollectionAssert.AreEqual(new[] { activeChild }, result);
        }

        #endregion


        #region TryGetComponentInParent / TryGetComponentInChildren

        [Test]
        public void TryGetComponentInParent_Generic_FoundOnAncestor()
        {
            GameObject parent = NewObject("Parent");
            Marker marker = parent.AddComponent<Marker>();
            GameObject child = NewChild(parent);

            Assert.IsTrue(child.transform.TryGetComponentInParent(out Marker output));
            Assert.AreSame(marker, output);
        }

        [Test]
        public void TryGetComponentInParent_TypeOverload_NotFound()
        {
            GameObject child = NewChild(NewObject());
            Assert.IsFalse(child.transform.TryGetComponentInParent(typeof(OtherMarker), out Component output));
            Assert.IsNull(output);
        }

        [Test]
        public void TryGetComponentInChildren_Generic_FoundOnDescendant()
        {
            GameObject parent = NewObject("Parent");
            Marker marker = NewChild(parent).AddComponent<Marker>();

            Assert.IsTrue(parent.transform.TryGetComponentInChildren(out Marker output));
            Assert.AreSame(marker, output);
        }

        [Test]
        public void TryGetComponentInChildren_IncludeInactive_TogglesInactiveDescendants()
        {
            GameObject parent = NewObject("Parent");
            NewChild(parent, "Inactive", active: false).AddComponent<Marker>();

            Assert.IsTrue(parent.transform.TryGetComponentInChildren(out Marker _, includeInactive: true));
            Assert.IsFalse(parent.transform.TryGetComponentInChildren(out Marker _, includeInactive: false));
        }

        #endregion

    }

}
