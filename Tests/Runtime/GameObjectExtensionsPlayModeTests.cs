using System.Collections;
using System.Collections.Generic;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// PlayMode tests for <see cref="GameObjectExtensions"/> behaviour that can't be verified at edit time — chiefly collider
    /// bounds, which are only populated while the game is running. Edit-mode coverage lives in <see cref="GameObjectExtensionsTests"/>.
    /// </summary>
    public class GameObjectExtensionsPlayModeTests
    {

        private const float Tolerance = 1e-3f;

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

        private GameObject NewObject(string name = "Object")
        {
            var go = new GameObject(name);
            _roots.Add(go);
            return go;
        }

        [UnityTest]
        public IEnumerator GetBounds_PreferCollider_PlayMode_ReturnsColliderBounds()
        {
            GameObject go = NewObject("Collider");
            go.transform.position = new Vector3(-2f, 1f, 0f);
            go.AddComponent<BoxCollider>(); // default size (1,1,1), centered on the object

            yield return new WaitForFixedUpdate(); // let physics register the collider so collider.bounds is populated

            // Unlike edit mode (where collider.bounds is zeroed), the collider path yields the real world-space bounds here.
            Assert.IsTrue(go.GetBounds(out Bounds bounds, preferCollider: true));
            Assert.Less(Vector3.Distance(go.transform.position, bounds.center), Tolerance, "Bounds should be centered on the object.");
            Assert.Less(Vector3.Distance(Vector3.one, bounds.size), Tolerance, "A unit BoxCollider should have unit-sized bounds.");
        }

    }

}
