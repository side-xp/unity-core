using NUnit.Framework;

using UnityEngine;
using UnityEditor;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Integration test for <see cref="EditorOnly.UniqueIdProcessor"/>.
    /// </summary>
    /// <remarks>
    /// The processor is a private <see cref="AssetPostprocessor.OnPostprocessAllAssets"/> callback with no public entry point, so this
    /// drives it through the real import pipeline: creating a <see cref="UniqueSO"/> asset makes Unity run the processor, which should
    /// stamp the asset's GUID into its <c>[UniqueId]</c> field. The temp asset is created under <c>Assets/</c> and deleted afterwards.
    /// </remarks>
    public class UniqueIdProcessorTests
    {

        private const string s_tempFolderName = "__UniqueIdProcessorTest__";
        private const string s_tempDir = "Assets/" + s_tempFolderName;

        [Test]
        public void OnImport_UniqueSO_AssignsAssetGuidToUniqueIdField()
        {
            if (AssetDatabase.IsValidFolder(s_tempDir))
                AssetDatabase.DeleteAsset(s_tempDir);
            AssetDatabase.CreateFolder("Assets", s_tempFolderName);

            string path = s_tempDir + "/Unique.asset";
            try
            {
                UniqueSO instance = ScriptableObject.CreateInstance<UniqueSO>();
                AssetDatabase.CreateAsset(instance, path);
                // Force a reimport so the UniqueIdProcessor postprocessor runs deterministically for this asset.
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

                string expectedGuid = AssetDatabase.AssetPathToGUID(path);
                UniqueSO loaded = AssetDatabase.LoadAssetAtPath<UniqueSO>(path);

                Assert.IsFalse(string.IsNullOrEmpty(loaded.UniqueId), "The [UniqueId] field should have been assigned on import.");
                Assert.AreEqual(expectedGuid, loaded.UniqueId, "The [UniqueId] field should hold the asset's GUID.");
            }
            finally
            {
                AssetDatabase.DeleteAsset(s_tempDir);
            }
        }

    }

}
