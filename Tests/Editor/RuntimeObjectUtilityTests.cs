using System.Collections.Generic;
using System.Text.RegularExpressions;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="RuntimeObjectUtility"/> — partial: <see cref="RuntimeObjectUtility.IsPrefab"/> (scene-object case) and the
    /// hierarchy-based strategies of <see cref="RuntimeObjectUtility.FindObjectFrom(object, System.Type, string, FFindObjectStrategy)"/>.
    /// The prefab-asset case, the tag-based scene search, and the editor-only open-asset handlers are out of scope here.
    /// (<see cref="RuntimeObjectUtility.GetTransform"/>/<see cref="RuntimeObjectUtility.GetGameObject"/> are covered via ObjectExtensionsTests.)
    /// </summary>
    public class RuntimeObjectUtilityTests
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

        private GameObject New(string name, GameObject parent = null, bool marker = false)
        {
            var go = new GameObject(name);
            if (parent != null)
                go.transform.SetParent(parent.transform, false);
            if (marker)
                go.AddComponent<Marker>();
            _objects.Add(go);
            return go;
        }

        private DummyAsset NewAsset(string name)
        {
            var asset = ScriptableObject.CreateInstance<DummyAsset>();
            asset.name = name;
            _objects.Add(asset);
            return asset;
        }

        #region IsPrefab

        [Test]
        public void IsPrefab_SceneObject_ReturnsFalse()
        {
            // A runtime-created object belongs to a scene (scene.name != null), so it's not a prefab.
            Assert.IsFalse(RuntimeObjectUtility.IsPrefab(New("Object")));
        }

        #endregion


        #region FindObjectFrom - invalid type

        [Test]
        public void FindObjectFrom_NonObjectType_LogsWarningAndReturnsNull()
        {
            LogAssert.Expect(LogType.Warning, new Regex("Invalid object type"));
            Assert.IsNull(RuntimeObjectUtility.FindObjectFrom(null, typeof(int)));
        }

        #endregion


        #region FindObjectFrom - Self

        [Test]
        public void FindObjectFrom_Self_FindsComponentOnOrigin()
        {
            GameObject origin = New("Origin", marker: true);

            var found = RuntimeObjectUtility.FindObjectFrom<Marker>(origin, strategy: FFindObjectStrategy.Self);

            Assert.AreSame(origin.GetComponent<Marker>(), found);
        }

        [Test]
        public void FindObjectFrom_Self_NameMismatch_ReturnsNull()
        {
            GameObject origin = New("Origin", marker: true);

            var found = RuntimeObjectUtility.FindObjectFrom<Marker>(origin, name: "SomethingElse", strategy: FFindObjectStrategy.Self);

            Assert.IsNull(found);
        }

        #endregion


        #region FindObjectFrom - Children

        [Test]
        public void FindObjectFrom_Children_FindsComponentOnDirectChild()
        {
            GameObject origin = New("Origin");
            GameObject child = New("Child", origin, marker: true);

            var found = RuntimeObjectUtility.FindObjectFrom<Marker>(origin, strategy: FFindObjectStrategy.Children);

            Assert.AreSame(child.GetComponent<Marker>(), found);
        }

        [Test]
        public void FindObjectFrom_Children_WithoutRecursive_IgnoresGrandchild()
        {
            GameObject origin = New("Origin");
            GameObject child = New("Child", origin);
            New("Grandchild", child, marker: true);

            var found = RuntimeObjectUtility.FindObjectFrom<Marker>(origin, strategy: FFindObjectStrategy.Children);

            Assert.IsNull(found);
        }

        [Test]
        public void FindObjectFrom_ChildrenRecursive_FindsComponentOnGrandchild()
        {
            GameObject origin = New("Origin");
            GameObject child = New("Child", origin);
            GameObject grandchild = New("Grandchild", child, marker: true);

            var found = RuntimeObjectUtility.FindObjectFrom<Marker>(origin, strategy: FFindObjectStrategy.Children | FFindObjectStrategy.Recursive);

            Assert.AreSame(grandchild.GetComponent<Marker>(), found);
        }

        #endregion


        #region FindObjectFrom - Parent

        [Test]
        public void FindObjectFrom_Parent_FindsComponentOnDirectParent()
        {
            GameObject parent = New("Parent", marker: true);
            GameObject origin = New("Origin", parent);

            var found = RuntimeObjectUtility.FindObjectFrom<Marker>(origin, strategy: FFindObjectStrategy.Parent);

            Assert.AreSame(parent.GetComponent<Marker>(), found);
        }

        [Test]
        public void FindObjectFrom_Parent_WithoutRecursive_IgnoresGrandparent()
        {
            GameObject grandparent = New("Grandparent", marker: true);
            GameObject parent = New("Parent", grandparent);
            GameObject origin = New("Origin", parent);

            var found = RuntimeObjectUtility.FindObjectFrom<Marker>(origin, strategy: FFindObjectStrategy.Parent);

            Assert.IsNull(found); // only the direct parent is checked, and it has no Marker
        }

        [Test]
        public void FindObjectFrom_ParentRecursive_FindsComponentOnAncestor()
        {
            GameObject grandparent = New("Grandparent", marker: true);
            GameObject parent = New("Parent", grandparent);
            GameObject origin = New("Origin", parent);

            var found = RuntimeObjectUtility.FindObjectFrom<Marker>(origin, strategy: FFindObjectStrategy.Parent | FFindObjectStrategy.Recursive);

            Assert.AreSame(grandparent.GetComponent<Marker>(), found);
        }

        #endregion


        #region FindObjectFrom - GameObject type and misc overloads

        [Test]
        public void FindObjectFrom_GameObjectType_ReturnsGameObjectNotComponent()
        {
            GameObject origin = New("Origin");
            GameObject target = New("Target", origin);

            var found = RuntimeObjectUtility.FindObjectFrom<GameObject>(origin, name: "Target", strategy: FFindObjectStrategy.Children);

            Assert.AreSame(target, found);
        }

        [Test]
        public void FindObjectFrom_NotFound_OutOverloadReturnsFalse()
        {
            GameObject origin = New("Origin"); // no Marker anywhere

            bool result = RuntimeObjectUtility.FindObjectFrom<Marker>(origin, out Marker found, strategy: FFindObjectStrategy.Self);

            Assert.IsFalse(result);
            Assert.IsNull(found);
        }

        [Test]
        public void FindObjectFrom_Scene_FindsComponentInScene()
        {
            Marker marker = New("InScene", marker: true).GetComponent<Marker>();

            // origin is null: skips the hierarchy passes and searches the scene by type.
            var found = RuntimeObjectUtility.FindObjectFrom<Marker>(null, strategy: FFindObjectStrategy.Scene);

            Assert.AreSame(marker, found);
        }

        #endregion


        #region FindObjectFrom - non-Component/GameObject type

        [Test]
        public void FindObjectFrom_NonComponentType_FindsLoadedObjectByName()
        {
            DummyAsset asset = NewAsset("Unique_Asset_Name_12345");

            var found = RuntimeObjectUtility.FindObjectFrom<DummyAsset>(null, name: "Unique_Asset_Name_12345");

            Assert.AreSame(asset, found);
        }

        #endregion

    }

}
