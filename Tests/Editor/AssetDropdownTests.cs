using System.Collections.Generic;

using NUnit.Framework;

using UnityEngine;
using UnityEditor;

using SideXP.Core.EditorOnly;

using Object = UnityEngine.Object;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for the menu of <see cref="AssetDropdownAttribute"/> (<see cref="AssetDropdownMenu"/>): the supported types, and the texts
    /// of the options and of the values.
    /// </summary>
    /// <remarks>
    /// The tests of the texts of the options use real assets, in a temporary folder created in <see cref="SetUp"/> and removed in
    /// <see cref="TearDown"/>.
    /// </remarks>
    public class AssetDropdownTests
    {

        #region Fields

        /// <summary>
        /// The name of the temporary folder.
        /// </summary>
        private const string TempFolderName = "AssetDropdownTests_Temp";

        /// <summary>
        /// The path of the temporary folder.
        /// </summary>
        private const string TempFolder = "Assets/" + TempFolderName;

        #endregion


        #region Lifecycle

        /// <summary>
        /// Creates the temporary folder.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            if (AssetDatabase.IsValidFolder(TempFolder))
                AssetDatabase.DeleteAsset(TempFolder);

            AssetDatabase.CreateFolder("Assets", TempFolderName);
        }

        /// <summary>
        /// Deletes the temporary folder and its assets.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            if (AssetDatabase.IsValidFolder(TempFolder))
                AssetDatabase.DeleteAsset(TempFolder);
        }

        #endregion


        #region Asset type

        /// <summary>
        /// The type of the assets of a single reference is the type of the field.
        /// </summary>
        [Test]
        public void GetAssetType_SingleReference_ReturnsFieldType()
        {
            Assert.AreEqual(typeof(SampleScriptableObject), AssetDropdownMenu.GetAssetType(typeof(SampleScriptableObject)));
        }

        /// <summary>
        /// The type of the assets of an array or a list is the type of its elements, since Unity applies the attribute to each element.
        /// </summary>
        [Test]
        public void GetAssetType_Collections_ReturnsElementType()
        {
            Assert.AreEqual(typeof(SampleScriptableObject), AssetDropdownMenu.GetAssetType(typeof(SampleScriptableObject[])));
            Assert.AreEqual(typeof(SampleScriptableObject), AssetDropdownMenu.GetAssetType(typeof(List<SampleScriptableObject>)));
        }

        /// <summary>
        /// A null field type gives no asset type.
        /// </summary>
        [Test]
        public void GetAssetType_Null_ReturnsNull()
        {
            Assert.IsNull(AssetDropdownMenu.GetAssetType(null));
        }

        /// <summary>
        /// Scriptable objects and the other types of assets are supported.
        /// </summary>
        [Test]
        public void IsSupported_AssetTypes_True()
        {
            Assert.IsTrue(AssetDropdownMenu.IsSupported(typeof(SampleScriptableObject)));
            Assert.IsTrue(AssetDropdownMenu.IsSupported(typeof(Material)));
            Assert.IsTrue(AssetDropdownMenu.IsSupported(typeof(GameObject)));
        }

        /// <summary>
        /// Components, the types that would list every asset of the project, and the types that are not Unity objects are not
        /// supported.
        /// </summary>
        [Test]
        public void IsSupported_OtherTypes_False()
        {
            Assert.IsFalse(AssetDropdownMenu.IsSupported(typeof(Transform)));
            Assert.IsFalse(AssetDropdownMenu.IsSupported(typeof(MonoBehaviour)));
            Assert.IsFalse(AssetDropdownMenu.IsSupported(typeof(Object)));
            Assert.IsFalse(AssetDropdownMenu.IsSupported(typeof(ScriptableObject)));
            Assert.IsFalse(AssetDropdownMenu.IsSupported(typeof(int)));
            Assert.IsFalse(AssetDropdownMenu.IsSupported(null));
        }

        #endregion


        #region Texts

        /// <summary>
        /// A main asset is listed by its name.
        /// </summary>
        [Test]
        public void GetOptionText_MainAsset_ReturnsName()
        {
            SampleScriptableObject asset = CreateAsset("Main");

            Assert.AreEqual("Main", AssetDropdownMenu.GetOptionText(asset));
        }

        /// <summary>
        /// A sub-asset is listed in a submenu named after its main asset, and a sub-asset without name gets a placeholder.
        /// </summary>
        [Test]
        public void GetOptionText_Subasset_InSubmenuOfMainAsset()
        {
            SampleScriptableObject mainAsset = CreateAsset("Main");
            SampleScriptableObject subasset = AddSubasset(mainAsset, "Sub");
            SampleScriptableObject unnamedSubasset = AddSubasset(mainAsset, string.Empty);
            // Saved and imported again, like the assets listed by a menu, which are loaded from their files
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(mainAsset), ImportAssetOptions.ForceUpdate);

            Assert.AreEqual("Main/Sub", AssetDropdownMenu.GetOptionText(subasset));
            Assert.AreEqual("Main/(Unnamed)", AssetDropdownMenu.GetOptionText(unnamedSubasset));
        }

        /// <summary>
        /// The value of a dropdown is the name of the referenced asset with its icon, or <i>None</i> without icon.
        /// </summary>
        [Test]
        public void GetValueText_ReferenceAndNone_LikeOptions()
        {
            SampleScriptableObject asset = ScriptableObject.CreateInstance<SampleScriptableObject>();
            asset.name = "Sword";
            try
            {
                Assert.AreEqual("Sword", AssetDropdownMenu.GetValueText(asset));
                Assert.AreEqual("None", AssetDropdownMenu.GetValueText(null));
                Assert.IsNull(AssetDropdownMenu.GetValueIcon(null));
            }
            finally
            {
                Object.DestroyImmediate(asset);
            }
        }

        /// <summary>
        /// A reference to a destroyed object is shown as missing, without icon.
        /// </summary>
        [Test]
        public void GetValueText_DestroyedObject_Missing()
        {
            SampleScriptableObject asset = ScriptableObject.CreateInstance<SampleScriptableObject>();
            Object.DestroyImmediate(asset);

            Assert.AreEqual("Missing", AssetDropdownMenu.GetValueText(asset));
            Assert.IsNull(AssetDropdownMenu.GetValueIcon(asset));
        }

        #endregion


        #region Private API

        /// <summary>
        /// Creates an asset in the temporary folder.
        /// </summary>
        /// <param name="name">The name of the asset file, without extension.</param>
        /// <returns>Returns the created asset.</returns>
        private static SampleScriptableObject CreateAsset(string name)
        {
            SampleScriptableObject asset = ScriptableObject.CreateInstance<SampleScriptableObject>();
            AssetDatabase.CreateAsset(asset, $"{TempFolder}/{name}.asset");
            return asset;
        }

        /// <summary>
        /// Adds a sub-asset to the file of an asset.
        /// </summary>
        /// <param name="mainAsset">The main asset of the file.</param>
        /// <param name="name">The name of the sub-asset.</param>
        /// <returns>Returns the created sub-asset.</returns>
        private static SampleScriptableObject AddSubasset(Object mainAsset, string name)
        {
            SampleScriptableObject subasset = ScriptableObject.CreateInstance<SampleScriptableObject>();
            subasset.name = name;
            AssetDatabase.AddObjectToAsset(subasset, mainAsset);
            return subasset;
        }

        #endregion

    }

}
