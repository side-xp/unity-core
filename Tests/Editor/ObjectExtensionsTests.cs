using System.Collections.Generic;

using NUnit.Framework;

using UnityEngine;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="ObjectExtensions"/> (thin forwarders to <see cref="RuntimeObjectUtility"/>).
    /// </summary>
    public class ObjectExtensionsTests
    {

        private class Marker : MonoBehaviour { }
        private class DummyAsset : ScriptableObject { }

        // Objects created during a test, destroyed in TearDown.
        private readonly List<Object> _objects = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object obj in _objects)
            {
                if (obj != null)
                    Object.DestroyImmediate(obj);
            }
            _objects.Clear();
        }

        private GameObject NewObject()
        {
            var go = new GameObject("Object");
            _objects.Add(go);
            return go;
        }

        private DummyAsset NewAsset()
        {
            var asset = ScriptableObject.CreateInstance<DummyAsset>(); // an Object not attached to any GameObject
            _objects.Add(asset);
            return asset;
        }

        #region GetTransform / TryGetTransform

        [Test]
        public void GetTransform_GameObject_ReturnsItsTransform()
        {
            GameObject go = NewObject();
            Assert.AreSame(go.transform, go.GetTransform());
        }

        [Test]
        public void GetTransform_Component_ReturnsItsTransform()
        {
            GameObject go = NewObject();
            Marker marker = go.AddComponent<Marker>();
            Assert.AreSame(go.transform, marker.GetTransform());
        }

        [Test]
        public void GetTransform_NonSceneObject_ReturnsNull()
        {
            Assert.IsNull(NewAsset().GetTransform());
        }

        [Test]
        public void TryGetTransform_GameObject_ReturnsTrue()
        {
            GameObject go = NewObject();
            Assert.IsTrue(go.TryGetTransform(out Transform transform));
            Assert.AreSame(go.transform, transform);
        }

        [Test]
        public void TryGetTransform_NonSceneObject_ReturnsFalse()
        {
            Assert.IsFalse(NewAsset().TryGetTransform(out Transform transform));
            Assert.IsNull(transform);
        }

        #endregion


        #region GetGameObject / TryGetGameObject

        [Test]
        public void GetGameObject_GameObject_ReturnsItself()
        {
            GameObject go = NewObject();
            Assert.AreSame(go, go.GetGameObject());
        }

        [Test]
        public void GetGameObject_Component_ReturnsOwner()
        {
            GameObject go = NewObject();
            Marker marker = go.AddComponent<Marker>();
            Assert.AreSame(go, marker.GetGameObject());
        }

        [Test]
        public void GetGameObject_NonSceneObject_ReturnsNull()
        {
            Assert.IsNull(NewAsset().GetGameObject());
        }

        [Test]
        public void TryGetGameObject_Component_ReturnsTrue()
        {
            GameObject go = NewObject();
            Marker marker = go.AddComponent<Marker>();

            Assert.IsTrue(marker.TryGetGameObject(out GameObject gameObject));
            Assert.AreSame(go, gameObject);
        }

        [Test]
        public void TryGetGameObject_NonSceneObject_ReturnsFalse()
        {
            Assert.IsFalse(NewAsset().TryGetGameObject(out GameObject gameObject));
            Assert.IsNull(gameObject);
        }

        #endregion

    }

}
