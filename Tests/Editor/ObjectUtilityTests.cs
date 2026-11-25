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
        private const string InnerFolderName = "Inner";
        private const string InnerDir = TempDir + "/" + InnerFolderName;

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


        #region LoadAllAssetsOfType

        /// <summary>
        /// The assets of a type include the assets of its derived types and the sub-assets, ordered by path, with the sub-assets after
        /// their main asset, ordered by name. Hidden sub-assets are skipped.
        /// </summary>
        [Test]
        public void LoadAllAssetsOfType_BaseType_IncludesDerivedTypesAndSubassetsInOrder()
        {
            CreateSearchedAssets();

            CollectionAssert.AreEqual(new[] { "Derived", "Main", "Sub A", "Sub Z" }, LoadNamesInTempDir(typeof(SampleScriptableObject)));
        }

        /// <summary>
        /// The assets of a derived type skip the assets of its base type, including the main asset of a file that contains one of its
        /// sub-assets.
        /// </summary>
        [Test]
        public void LoadAllAssetsOfType_DerivedType_SkipsBaseType()
        {
            CreateSearchedAssets();

            CollectionAssert.AreEqual(new[] { "Derived", "Sub Z" }, LoadNamesInTempDir(typeof(SampleDerivedScriptableObject)));
        }

        /// <summary>
        /// The generic version loads the same assets, in the same order, typed.
        /// </summary>
        [Test]
        public void LoadAllAssetsOfType_Generic_SameAssetsTyped()
        {
            CreateSearchedAssets();

            List<SampleDerivedScriptableObject> assets = ObjectUtility.LoadAllAssetsOfType<SampleDerivedScriptableObject>();

            CollectionAssert.AreEqual(ObjectUtility.LoadAllAssetsOfType(typeof(SampleDerivedScriptableObject)), assets);
            CollectionAssert.IsNotEmpty(assets);
        }

        /// <summary>
        /// The game objects are the prefabs, without the children of their hierarchy.
        /// </summary>
        [Test]
        public void LoadAllAssetsOfType_GameObject_SkipsChildrenOfPrefabs()
        {
            GameObject root = new GameObject("Prefab");
            new GameObject("Child").transform.SetParent(root.transform);
            try
            {
                PrefabUtility.SaveAsPrefabAsset(root, $"{TempDir}/Prefab.prefab");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }

            CollectionAssert.AreEqual(new[] { "Prefab" }, LoadNamesInTempDir(typeof(GameObject)));
        }

        /// <summary>
        /// The type must be a type of Unity object.
        /// </summary>
        [Test]
        public void LoadAllAssetsOfType_InvalidType_Throws()
        {
            Assert.Throws<System.ArgumentException>(() => ObjectUtility.LoadAllAssetsOfType(null));
            Assert.Throws<System.ArgumentException>(() => ObjectUtility.LoadAllAssetsOfType(typeof(string)));
            Assert.Throws<System.ArgumentException>(() => ObjectUtility.LoadAllAssetsOfType(typeof(string), new[] { TempDir }));
        }

        /// <summary>
        /// With folders, only the assets of these folders are loaded, subfolders included, in the order of their paths.
        /// </summary>
        [Test]
        public void LoadAllAssetsOfType_Folders_OnlyAssetsInFoldersAndSubfolders()
        {
            CreateInnerAsset();

            CollectionAssert.AreEqual(new[] { "Inner" }, LoadNames(typeof(SampleScriptableObject), new[] { InnerDir }));
            CollectionAssert.AreEqual(new[] { "Inner", "Main" }, LoadNames(typeof(SampleScriptableObject), new[] { TempDir }));
        }

        /// <summary>
        /// The folders that don't exist are ignored, and no asset is found if none of them exists.
        /// </summary>
        [Test]
        public void LoadAllAssetsOfType_MissingFolders_Ignored()
        {
            CreateInnerAsset();
            string missingDir = $"{TempDir}/Missing";

            CollectionAssert.AreEqual(new[] { "Inner" }, LoadNames(typeof(SampleScriptableObject), new[] { missingDir, InnerDir, null }));
            Assert.IsEmpty(ObjectUtility.LoadAllAssetsOfType(typeof(SampleScriptableObject), new[] { missingDir }));
        }

        /// <summary>
        /// The folders can use backslashes and end with a slash, and overlapping folders load each asset once.
        /// </summary>
        [Test]
        public void LoadAllAssetsOfType_FolderFormats_SameAssetsOnce()
        {
            CreateInnerAsset();

            CollectionAssert.AreEqual(new[] { "Inner" },
                LoadNames(typeof(SampleScriptableObject), new[] { $"{InnerDir.Replace('/', '\\')}\\" }));
            CollectionAssert.AreEqual(new[] { "Inner", "Main" },
                LoadNames(typeof(SampleScriptableObject), new[] { $"{TempDir}/", InnerDir }));
        }

        /// <summary>
        /// Without folders (null or empty), the whole project is searched.
        /// </summary>
        [Test]
        public void LoadAllAssetsOfType_NoFolders_WholeProject()
        {
            CreateInnerAsset();
            List<Object> allAssets = ObjectUtility.LoadAllAssetsOfType(typeof(SampleScriptableObject));

            CollectionAssert.AreEqual(allAssets, ObjectUtility.LoadAllAssetsOfType(typeof(SampleScriptableObject), null));
            CollectionAssert.AreEqual(allAssets, ObjectUtility.LoadAllAssetsOfType(typeof(SampleScriptableObject), new string[0]));
        }

        /// <summary>
        /// The generic version with folders loads the same assets, in the same order, typed.
        /// </summary>
        [Test]
        public void LoadAllAssetsOfType_GenericWithFolders_SameAssetsTyped()
        {
            CreateInnerAsset();

            List<SampleScriptableObject> assets = ObjectUtility.LoadAllAssetsOfType<SampleScriptableObject>(new[] { TempDir });

            CollectionAssert.AreEqual(ObjectUtility.LoadAllAssetsOfType(typeof(SampleScriptableObject), new[] { TempDir }), assets);
            Assert.AreEqual(2, assets.Count);
        }

        /// <summary>
        /// Creates the <see cref="InnerDir"/> folder, with the asset <c>Inner</c> (<see cref="SampleScriptableObject"/>).
        /// </summary>
        private static void CreateInnerAsset()
        {
            AssetDatabase.CreateFolder(TempDir, InnerFolderName);
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<SampleScriptableObject>(), $"{InnerDir}/Inner.asset");
        }

        /// <summary>
        /// Loads the assets of a type in some folders with <see cref="ObjectUtility.LoadAllAssetsOfType(System.Type, string[])"/>.
        /// </summary>
        /// <param name="type">The type of the assets to load.</param>
        /// <param name="folders">The folders where to search.</param>
        /// <returns>Returns the names of the loaded assets, in order.</returns>
        private static string[] LoadNames(System.Type type, string[] folders)
        {
            List<string> names = new List<string>();
            foreach (Object asset in ObjectUtility.LoadAllAssetsOfType(type, folders))
                names.Add(asset.name);

            return names.ToArray();
        }

        /// <summary>
        /// Creates the assets searched by the tests of <see cref="ObjectUtility.LoadAllAssetsOfType(System.Type)"/>, next to the main
        /// asset: <c>Derived</c> (<see cref="SampleDerivedScriptableObject"/>), and the sub-assets of the main asset <c>Sub Z</c>
        /// (<see cref="SampleDerivedScriptableObject"/>), <c>Sub A</c> and <c>Hidden</c> (hidden from the Project window).
        /// </summary>
        private void CreateSearchedAssets()
        {
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<SampleDerivedScriptableObject>(), $"{TempDir}/Derived.asset");

            SampleDerivedScriptableObject subZ = ScriptableObject.CreateInstance<SampleDerivedScriptableObject>();
            subZ.name = "Sub Z";
            AssetDatabase.AddObjectToAsset(subZ, _mainAsset);

            SampleScriptableObject subA = ScriptableObject.CreateInstance<SampleScriptableObject>();
            subA.name = "Sub A";
            AssetDatabase.AddObjectToAsset(subA, _mainAsset);

            SampleScriptableObject hidden = ScriptableObject.CreateInstance<SampleScriptableObject>();
            hidden.name = "Hidden";
            hidden.hideFlags = HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(hidden, _mainAsset);

            // Import the file again, so the search index knows the types of its sub-assets
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(MainAssetPath, ImportAssetOptions.ForceUpdate);
        }

        /// <summary>
        /// Loads the assets of a type with <see cref="ObjectUtility.LoadAllAssetsOfType(System.Type)"/>, limited to the temporary
        /// folder, so the other assets of the project don't affect the tests.
        /// </summary>
        /// <param name="type">The type of the assets to load.</param>
        /// <returns>Returns the names of the loaded assets, in order.</returns>
        private static string[] LoadNamesInTempDir(System.Type type)
        {
            List<string> names = new List<string>();
            foreach (Object asset in ObjectUtility.LoadAllAssetsOfType(type))
            {
                if (AssetDatabase.GetAssetPath(asset).StartsWith($"{TempDir}/"))
                    names.Add(asset.name);
            }
            return names.ToArray();
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
