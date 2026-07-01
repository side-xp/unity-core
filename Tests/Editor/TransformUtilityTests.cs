using System.Collections.Generic;

using NUnit.Framework;

using UnityEngine;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="TransformUtility"/> and its <see cref="TransformExtensions"/> forwarders.
    /// </summary>
    public class TransformUtilityTests
    {

        // Marker components used to test type-based queries/removal.
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

        /// <summary>
        /// Creates a root <see cref="GameObject"/> tracked for teardown.
        /// </summary>
        private Transform NewRoot(string name = "Root")
        {
            var go = new GameObject(name);
            _roots.Add(go);
            return go.transform;
        }

        /// <summary>
        /// Creates a child <see cref="GameObject"/> under the given parent.
        /// </summary>
        private Transform NewChild(Transform parent, string name = "Child", bool active = true)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.SetActive(active);
            return go.transform;
        }

        #region GetChildren

        [Test]
        public void GetChildren_DirectOnly_ReturnsDirectChildrenExcludingSelf()
        {
            Transform root = NewRoot();
            Transform a = NewChild(root, "A");
            Transform b = NewChild(root, "B");
            NewChild(a, "A_Nested"); // grandchild must not appear in a direct query

            Transform[] children = root.GetChildren();

            CollectionAssert.AreEquivalent(new[] { a, b }, children);
        }

        [Test]
        public void GetChildren_Recursive_ReturnsAllDescendantsExcludingSelf()
        {
            Transform root = NewRoot();
            Transform a = NewChild(root, "A");
            Transform b = NewChild(root, "B");
            Transform nested = NewChild(a, "A_Nested");

            Transform[] children = root.GetChildren(true);

            CollectionAssert.AreEquivalent(new[] { a, b, nested }, children);
            CollectionAssert.DoesNotContain(children, root);
        }

        [Test]
        public void GetChildren_Recursive_IncludesInactiveDescendants()
        {
            Transform root = NewRoot();
            Transform a = NewChild(root, "A");
            Transform inactive = NewChild(a, "Inactive", active: false);

            Transform[] children = root.GetChildren(true);

            CollectionAssert.Contains(children, inactive);
        }

        [Test]
        public void GetChildren_NoChildren_ReturnsEmpty()
        {
            Transform root = NewRoot();
            Assert.IsEmpty(root.GetChildren());
            Assert.IsEmpty(root.GetChildren(true));
        }

        #endregion


        #region ForEachChild

        [Test]
        public void ForEachChild_DirectOnly_InvokesActionForEachDirectChild()
        {
            Transform root = NewRoot();
            NewChild(root, "A");
            Transform b = NewChild(root, "B");
            NewChild(b, "B_Nested"); // not visited in a direct pass

            var visited = new List<Transform>();
            root.ForEachChild(visited.Add);

            Assert.AreEqual(2, visited.Count);
        }

        [Test]
        public void ForEachChild_Recursive_InvokesActionForEveryDescendant()
        {
            Transform root = NewRoot();
            Transform a = NewChild(root, "A");
            NewChild(root, "B");
            NewChild(a, "A_Nested");

            var visited = new List<Transform>();
            root.ForEachChild(visited.Add, true);

            Assert.AreEqual(3, visited.Count);
        }

        #endregion


        #region ClearHierarchy

        [Test]
        public void ClearHierarchy_DestroysEveryChildSubtree()
        {
            Transform root = NewRoot();
            Transform a = NewChild(root, "A");
            NewChild(root, "B");
            NewChild(a, "A_Nested");

            root.ClearHierarchy();

            // DestroyImmediate is used in edit mode, so the hierarchy is empty synchronously.
            Assert.AreEqual(0, root.childCount);
        }

        #endregion


        #region RemoveChildrenOfType

        [Test]
        public void RemoveChildrenOfType_Generic_RemovesMatchingChildrenAndReturnsCount()
        {
            Transform root = NewRoot();
            NewChild(root, "Match1").gameObject.AddComponent<Marker>();
            NewChild(root, "Plain");
            NewChild(root, "Match2").gameObject.AddComponent<Marker>();

            int removed = root.RemoveChildrenOfType<Marker>();

            Assert.AreEqual(2, removed);
            Assert.AreEqual(1, root.childCount); // only the plain child remains
        }

        [Test]
        public void RemoveChildrenOfType_NonMatchingType_RemovesNothing()
        {
            Transform root = NewRoot();
            NewChild(root, "Marked").gameObject.AddComponent<Marker>();

            int removed = root.RemoveChildrenOfType<OtherMarker>();

            Assert.AreEqual(0, removed);
            Assert.AreEqual(1, root.childCount);
        }

        [Test]
        public void RemoveChildrenOfType_TypeOverload_MatchesGenericOverload()
        {
            Transform root = NewRoot();
            NewChild(root, "Match").gameObject.AddComponent<Marker>();
            NewChild(root, "Plain");

            int removed = root.RemoveChildrenOfType(typeof(Marker));

            Assert.AreEqual(1, removed);
            Assert.AreEqual(1, root.childCount);
        }

        [Test]
        public void RemoveChildrenOfType_Recursive_RemovesNestedMatch()
        {
            Transform root = NewRoot();
            Transform plain = NewChild(root, "Plain"); // no marker
            NewChild(plain, "NestedMatch").gameObject.AddComponent<Marker>();

            int removed = root.RemoveChildrenOfType<Marker>();

            // The match is a grandchild, so removal must reach it recursively.
            Assert.AreEqual(1, removed);
            Assert.AreEqual(0, plain.childCount);
        }

        [Test]
        public void RemoveChildrenOfType_MatchingParentWithMatchingChild_CountsParentOnce()
        {
            Transform root = NewRoot();
            Transform parent = NewChild(root, "Parent");
            parent.gameObject.AddComponent<Marker>();
            NewChild(parent, "Child").gameObject.AddComponent<Marker>();

            // The parent is destroyed first; its (also-matching) child is gone by the time the scan reaches it,
            // and the null guard skips it — so it is counted once, without throwing.
            int removed = root.RemoveChildrenOfType<Marker>();

            Assert.AreEqual(1, removed);
            Assert.AreEqual(0, root.childCount);
        }

        #endregion


        #region Pool

        [Test]
        public void Pool_FewerExistingThanExpected_InstantiatesToReachCount()
        {
            Transform container = NewRoot("Container");

            var initIndices = new List<int>();

            container.Pool<Marker>(3,
                c => NewChild(c, "Pooled").gameObject.AddComponent<Marker>(),
                (marker, index) => initIndices.Add(index));

            Assert.AreEqual(3, container.GetComponentsInChildren<Marker>(true, true).Length);
            CollectionAssert.AreEqual(new[] { 0, 1, 2 }, initIndices);
        }

        [Test]
        public void Pool_MoreExistingThanExpected_DiscardsAndDeactivatesExtras()
        {
            Transform container = NewRoot("Container");
            for (int i = 0; i < 5; i++)
                NewChild(container, "Existing" + i).gameObject.AddComponent<Marker>();

            var initIndices = new List<int>();
            var discardIndices = new List<int>();

            container.Pool<Marker>(2,
                c => NewChild(c, "New").gameObject.AddComponent<Marker>(),
                (marker, index) => initIndices.Add(index),
                (marker, index) => discardIndices.Add(index));

            CollectionAssert.AreEqual(new[] { 0, 1 }, initIndices);
            CollectionAssert.AreEqual(new[] { 2, 3, 4 }, discardIndices);

            int active = 0, inactive = 0;
            foreach (Marker m in container.GetComponentsInChildren<Marker>(true, true))
            {
                if (m.gameObject.activeSelf) active++;
                else inactive++;
            }
            Assert.AreEqual(2, active);
            Assert.AreEqual(3, inactive);
        }

        [Test]
        public void Pool_ExactCount_ReusesAllWithoutInstantiatingOrDiscarding()
        {
            Transform container = NewRoot("Container");
            for (int i = 0; i < 3; i++)
                NewChild(container, "Existing" + i, active: false).gameObject.AddComponent<Marker>();

            int instantiateCalls = 0;
            int discardCalls = 0;
            var initIndices = new List<int>();

            container.Pool<Marker>(3,
                c => { instantiateCalls++; return NewChild(c, "New").gameObject.AddComponent<Marker>(); },
                (marker, index) => initIndices.Add(index),
                (marker, index) => discardCalls++);

            Assert.AreEqual(0, instantiateCalls);
            Assert.AreEqual(0, discardCalls);
            CollectionAssert.AreEqual(new[] { 0, 1, 2 }, initIndices);

            // Reused instances are (re)activated even though they started inactive.
            foreach (Marker m in container.GetComponentsInChildren<Marker>(true, true))
                Assert.IsTrue(m.gameObject.activeSelf);
        }

        #endregion

    }

}
