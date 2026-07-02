using System.Collections.Generic;
using System.Text.RegularExpressions;

using NUnit.Framework;

using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;

using SideXP.Core.EditorOnly;

using Object = UnityEngine.Object;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for the editor <see cref="ObjectUtility"/>.
    /// </summary>
    /// <remarks>
    /// A temporary folder + main <c>.asset</c> is created under <c>Assets/</c> in <see cref="SetUp"/> and removed in
    /// <see cref="TearDown"/>, so the mutating methods (create/attach/rename subassets) can run against a real asset without leaving
    /// anything behind. Read-only methods are exercised against that asset and against in-memory (non-asset) objects.
    /// </remarks>
    public class ObjectUtilityTests
    {

        private const string TempFolderName = "ObjectUtilityTests_Temp";
        private const string TempDir = "Assets/" + TempFolderName;
        private const string MainAssetPath = TempDir + "/Main.asset";

        private SampleScriptableObject _mainAsset;
        private readonly List<Object> _toDestroy = new List<Object>();

        [SetUp]
        public void SetUp()
        {
            if (AssetDatabase.IsValidFolder(TempDir))
                AssetDatabase.DeleteAsset(TempDir);
            AssetDatabase.CreateFolder("Assets", TempFolderName);

            _mainAsset = ScriptableObject.CreateInstance<SampleScriptableObject>();
            AssetDatabase.CreateAsset(_mainAsset, MainAssetPath);
        }

        [TearDown]
        public void TearDown()
        {
            if (AssetDatabase.IsValidFolder(TempDir))
                AssetDatabase.DeleteAsset(TempDir);

            foreach (Object obj in _toDestroy)
            {
                if (obj != null)
                    Object.DestroyImmediate(obj);
            }
            _toDestroy.Clear();
        }

        /// <summary>
        /// Creates a tracked in-memory (non-asset) ScriptableObject, destroyed in teardown.
        /// </summary>
        private SampleScriptableObject NewInMemory(string name = "InMemory")
        {
            SampleScriptableObject obj = ScriptableObject.CreateInstance<SampleScriptableObject>();
            obj.name = name;
            _toDestroy.Add(obj);
            return obj;
        }

        #region Paths / IsAsset / GUID

        [Test]
        public void GetAssetPath_RealAsset_ReturnsPath()
        {
            Assert.AreEqual(MainAssetPath, ObjectUtility.GetAssetPath(_mainAsset));
        }

        [Test]
        public void GetAssetPath_InMemoryObject_ReturnsNull()
        {
            Assert.IsNull(ObjectUtility.GetAssetPath(NewInMemory()));
        }

        [Test]
        public void GetAbsoluteAssetPath_RealAsset_ReturnsAbsolutePath()
        {
            string abs = ObjectUtility.GetAbsoluteAssetPath(_mainAsset);
            Assert.IsFalse(string.IsNullOrEmpty(abs));
            StringAssert.Contains("Main.asset", abs);
        }

        [Test]
        public void GetAbsoluteAssetPath_InMemoryObject_ReturnsNull()
        {
            Assert.IsNull(ObjectUtility.GetAbsoluteAssetPath(NewInMemory()));
        }

        [Test]
        public void IsAsset_RealAsset_ReturnsTrue()
        {
            Assert.IsTrue(ObjectUtility.IsAsset(_mainAsset));
        }

        [Test]
        public void IsAsset_InMemoryObject_ReturnsFalse()
        {
            Assert.IsFalse(ObjectUtility.IsAsset(NewInMemory()));
        }

        [Test]
        public void GetGUID_RealAsset_ReturnsAssetGuid()
        {
            Assert.AreEqual(AssetDatabase.AssetPathToGUID(MainAssetPath), ObjectUtility.GetGUID(_mainAsset));
        }

        [Test]
        public void GetGUID_InMemoryObject_ReturnsNull()
        {
            Assert.IsNull(ObjectUtility.GetGUID(NewInMemory()));
        }

        [Test]
        public void GetMainAsset_MainAsset_ReturnsItself()
        {
            Assert.AreEqual(_mainAsset, ObjectUtility.GetMainAsset(_mainAsset));
        }

        #endregion


        #region FindAssets

        [Test]
        public void FindAsset_ByTypeAndName_ReturnsMatch()
        {
            SampleScriptableObject found = ObjectUtility.FindAsset<SampleScriptableObject>("Main");
            Assert.AreEqual(_mainAsset, found);
        }

        [Test]
        public void FindAssets_ByTypeAndName_ContainsMatch()
        {
            SampleScriptableObject[] found = ObjectUtility.FindAssets<SampleScriptableObject>("Main");
            CollectionAssert.Contains(found, _mainAsset);
        }

        [Test]
        public void FindAssets_NoMatch_ReturnsEmpty()
        {
            SampleScriptableObject[] found = ObjectUtility.FindAssets<SampleScriptableObject>("ZZZ_NoSuchAsset_12345");
            Assert.IsEmpty(found);
        }

        #endregion


        #region GetUniqueAssetName

        [Test]
        public void GetUniqueAssetName_CollidingName_ReturnsDifferentName()
        {
            // "Main.asset" already exists in the temp folder, so the requested name must be made unique.
            string unique = ObjectUtility.GetUniqueAssetName(_mainAsset, "Main");
            Assert.AreNotEqual("Main", unique);
            StringAssert.StartsWith("Main", unique);
        }

        [Test]
        public void GetUniqueAssetName_FreeName_ReturnsSameName()
        {
            Assert.AreEqual("Fresh_9f3a", ObjectUtility.GetUniqueAssetName(_mainAsset, "Fresh_9f3a"));
        }

        [Test]
        public void GetUniqueAssetName_InMemoryObject_ReturnsNull()
        {
            Assert.IsNull(ObjectUtility.GetUniqueAssetName(NewInMemory(), "Whatever"));
        }

        #endregion


        #region FindAssetInHierarchy

        [Test]
        public void FindAssetInHierarchy_MainAssetType_ReturnsMainAsset()
        {
            SampleScriptableObject found = ObjectUtility.FindAssetInHierarchy<SampleScriptableObject>(_mainAsset);
            Assert.AreEqual(_mainAsset, found);
        }

        [Test]
        public void FindAssetInHierarchy_InMemoryObject_ReturnsNullWithoutThrowing()
        {
            // Regression: a non-asset input used to NRE (LoadAssetAtPath(null).GetType()).
            SampleScriptableObject inMemory = NewInMemory();
            Object found = null;
            Assert.DoesNotThrow(() => found = ObjectUtility.FindAssetInHierarchy(inMemory, typeof(SampleScriptableObject)));
            Assert.IsNull(found);
        }

        [Test]
        public void FindAssetInHierarchy_TypeNotInHierarchy_ReturnsFalse()
        {
            bool result = ObjectUtility.FindAssetInHierarchy<Material>(_mainAsset, out Material found);
            Assert.IsFalse(result);
            Assert.IsNull(found);
        }

        #endregion


        #region Subassets

        [Test]
        public void FindAllSubassetsOfType_NoSubassets_ReturnsEmpty()
        {
            SampleScriptableObject[] subassets = ObjectUtility.FindAllSubassetsOfType<SampleScriptableObject>(_mainAsset);
            Assert.IsEmpty(subassets);
        }

        [Test]
        public void CreateSubasset_AttachesSubassetToContainer()
        {
            SampleScriptableObject sub = ObjectUtility.CreateSubasset<SampleScriptableObject>(_mainAsset, "Sub");

            Assert.IsNotNull(sub);
            Assert.IsTrue(ObjectUtility.IsSubasset(sub), "The created object should be a sub-asset.");
            Assert.IsTrue(ObjectUtility.IsSubassetOf(sub, _mainAsset), "The sub-asset should belong to the container.");
            CollectionAssert.Contains(ObjectUtility.FindAllSubassetsOfType<SampleScriptableObject>(_mainAsset), sub);
        }

        [Test]
        public void CreateSubasset_AutoName_IgnoresUnrelatedProjectAssets()
        {
            // Regression for the unique-name fix: uniqueness is checked against the CONTAINER's sub-assets, not every project asset.
            // An unrelated asset already named "New{TypeName}" must NOT force the new sub-asset's auto name to be bumped.
            SampleScriptableObject unrelated = ScriptableObject.CreateInstance<SampleScriptableObject>();
            AssetDatabase.CreateAsset(unrelated, TempDir + "/New" + nameof(SampleScriptableObject) + ".asset");

            SampleScriptableObject sub = ObjectUtility.CreateSubasset<SampleScriptableObject>(_mainAsset);

            Assert.IsNotNull(sub);
            Assert.AreEqual("New" + nameof(SampleScriptableObject), sub.name,
                "The container has no sub-asset with that name, so the auto name should not be bumped by an unrelated project asset.");
        }

        [Test]
        public void CreateSubasset_NonAssetContainer_ReturnsNull()
        {
            LogAssert.Expect(LogType.Warning, new Regex("can't create a sub-asset"));
            SampleScriptableObject sub = ObjectUtility.CreateSubasset<SampleScriptableObject>(NewInMemory());
            Assert.IsNull(sub);
        }

        [Test]
        public void FindSubassetOfType_ReturnsAttachedSubasset()
        {
            SampleScriptableObject sub = ObjectUtility.CreateSubasset<SampleScriptableObject>(_mainAsset, "Sub");
            SampleScriptableObject found = ObjectUtility.FindSubassetOfType<SampleScriptableObject>(_mainAsset);
            Assert.AreEqual(sub, found);
        }

        [Test]
        public void AttachObject_InMemoryObject_AttachesAsSubasset()
        {
            SampleScriptableObject toAttach = NewInMemory("Attached");

            bool result = ObjectUtility.AttachObject(_mainAsset, toAttach);

            Assert.IsTrue(result);
            Assert.IsTrue(ObjectUtility.IsSubassetOf(toAttach, _mainAsset));
        }

        [Test]
        public void AttachObject_AlreadyAnAsset_ReturnsFalse()
        {
            SampleScriptableObject otherAsset = ScriptableObject.CreateInstance<SampleScriptableObject>();
            AssetDatabase.CreateAsset(otherAsset, TempDir + "/Other.asset");

            LogAssert.Expect(LogType.Warning, new Regex("can't attach an asset that is already saved"));
            bool result = ObjectUtility.AttachObject(_mainAsset, otherAsset);
            Assert.IsFalse(result);
        }

        [Test]
        public void AttachObject_NonAssetContainer_ReturnsFalse()
        {
            LogAssert.Expect(LogType.Warning, new Regex("can't attach an object to another that is not an asset"));
            bool result = ObjectUtility.AttachObject(NewInMemory("Container"), NewInMemory("Child"));
            Assert.IsFalse(result);
        }

        [Test]
        public void IsSubasset_MainAsset_ReturnsFalse()
        {
            Assert.IsFalse(ObjectUtility.IsSubasset(_mainAsset));
        }

        #endregion


        #region SaveAndReimport / Rename

        [Test]
        public void SaveAndReimport_RealAsset_ReturnsTrue()
        {
            Assert.IsTrue(ObjectUtility.SaveAndReimport(_mainAsset));
        }

        [Test]
        public void SaveAndReimport_InMemoryObject_ReturnsFalse()
        {
            Assert.IsFalse(ObjectUtility.SaveAndReimport(NewInMemory()));
        }

        [Test]
        public void Rename_MainAsset_ChangesName()
        {
            ObjectUtility.Rename(_mainAsset, "Renamed");
            Assert.AreEqual("Renamed", _mainAsset.name);
        }

        [Test]
        public void Rename_Subasset_ChangesName()
        {
            SampleScriptableObject sub = ObjectUtility.CreateSubasset<SampleScriptableObject>(_mainAsset, "Sub");
            ObjectUtility.Rename(sub, "SubRenamed");
            Assert.AreEqual("SubRenamed", sub.name);
        }

        [Test]
        public void Rename_InMemoryObject_ChangesNameOnly()
        {
            SampleScriptableObject inMemory = NewInMemory("Original");
            ObjectUtility.Rename(inMemory, "NewName");
            Assert.AreEqual("NewName", inMemory.name);
        }

        #endregion

    }

}
