using System.Collections;
using System.Collections.Generic;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// PlayMode tests for behaviour that only differs at run time — chiefly the deferred <see cref="Object.Destroy(Object)"/>
    /// branch of <see cref="TransformUtility"/>. Edit-mode coverage (the immediate <see cref="Object.DestroyImmediate(Object)"/>
    /// branch) lives in <see cref="TransformUtilityTests"/>.
    /// </summary>
    public class TransformUtilityPlayModeTests
    {

        private class Marker : MonoBehaviour { }

        // Root objects created during a test, destroyed in TearDown.
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

        private Transform NewRoot(string name = "Root")
        {
            var go = new GameObject(name);
            _roots.Add(go);
            return go.transform;
        }

        private Transform NewChild(Transform parent, string name = "Child")
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        [UnityTest]
        public IEnumerator ClearHierarchy_PlayMode_DeferredDestroy_TakesEffectNextFrame()
        {
            Transform root = NewRoot();
            NewChild(root, "A");
            NewChild(root, "B");

            root.ClearHierarchy();

            // In play mode Object.Destroy is queued to end-of-frame, so the children still exist during this frame.
            Assert.AreEqual(2, root.childCount);

            yield return null; // let the queued destruction run

            Assert.AreEqual(0, root.childCount);
        }

        [UnityTest]
        public IEnumerator RemoveChildrenOfType_PlayMode_NestedMatches_CountsEveryMatch()
        {
            Transform root = NewRoot();
            Transform parent = NewChild(root, "Parent");
            parent.gameObject.AddComponent<Marker>();
            NewChild(parent, "Child").gameObject.AddComponent<Marker>();

            // Deferred destruction means nothing is gone while the recursive scan runs, so both the parent and its child are
            // counted. This is the counterpart to the EditMode test RemoveChildrenOfType_MatchingParentWithMatchingChild_
            // CountsParentOnce, where DestroyImmediate removes the child before the scan reaches it (count == 1).
            int removed = root.RemoveChildrenOfType<Marker>();
            Assert.AreEqual(2, removed);

            yield return null;

            Assert.AreEqual(0, root.childCount);
        }

    }

}
