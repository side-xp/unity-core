using NUnit.Framework;

using UnityEngine.SceneManagement;
using UnityEditor;

using SideXP.Core.EditorOnly;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="SceneEditorUtility"/> and the <see cref="SceneExtensions"/>/<see cref="SceneAssetExtensions"/> forwarders.
    /// </summary>
    /// <remarks>
    /// All the logic mutates the global <see cref="EditorBuildSettings.scenes"/>, so the original array is captured and restored around
    /// every test (net-zero change to <c>ProjectSettings/EditorBuildSettings.asset</c>). The string overloads carry all the logic and
    /// run against a synthetic path (the list logic needs no real file). The <see cref="Scene"/>/<see cref="SceneAsset"/> overloads and
    /// the extension forwarders are one-liners over that logic; rather than fabricate a real <c>.unity</c> (which would require opening a
    /// scene), they're exercised through their degenerate inputs — <c>default(Scene)</c> and a <c>null</c> <see cref="SceneAsset"/> — to
    /// confirm the path is forwarded and no overload throws on a missing path.
    /// </remarks>
    public class SceneEditorUtilityTests
    {

        private const string SyntheticPath = "Assets/__SceneEditorUtility_Synthetic__.unity";

        private EditorBuildSettingsScene[] _originalScenes;

        [SetUp]
        public void SetUp()
        {
            _originalScenes = EditorBuildSettings.scenes;
        }

        [TearDown]
        public void TearDown()
        {
            EditorBuildSettings.scenes = _originalScenes;
        }

        #region String overloads

        [Test]
        public void IsIncluded_NotIncluded_ReturnsFalse()
        {
            Assert.IsFalse(SceneEditorUtility.IsIncluded(SyntheticPath));
        }

        [Test]
        public void AddToBuildSettings_NewPath_AddsAsEnabled()
        {
            Assert.IsTrue(SceneEditorUtility.AddToBuildSettings(SyntheticPath));
            Assert.IsTrue(SceneEditorUtility.IsIncluded(SyntheticPath));
            Assert.IsTrue(SceneEditorUtility.IsEnabled(SyntheticPath), "A newly added scene should be enabled by default.");
        }

        [Test]
        public void AddToBuildSettings_AlreadyIncluded_ReturnsFalse()
        {
            SceneEditorUtility.AddToBuildSettings(SyntheticPath);
            Assert.IsFalse(SceneEditorUtility.AddToBuildSettings(SyntheticPath));
        }

        [Test]
        public void AddToBuildSettings_EmptyPath_ReturnsFalseAndDoesNotAdd()
        {
            // Guard regression: an empty path must not create a bogus ("", enabled) build-settings entry.
            Assert.IsFalse(SceneEditorUtility.AddToBuildSettings(string.Empty));
            Assert.IsFalse(SceneEditorUtility.IsIncluded(string.Empty));
        }

        [Test]
        public void AddToBuildSettings_NullPath_ReturnsFalse()
        {
            Assert.IsFalse(SceneEditorUtility.AddToBuildSettings((string)null));
        }

        [Test]
        public void Enable_NotIncluded_ReturnsFalse()
        {
            Assert.IsFalse(SceneEditorUtility.Enable(SyntheticPath));
        }

        [Test]
        public void Disable_NotIncluded_ReturnsFalse()
        {
            Assert.IsFalse(SceneEditorUtility.Disable(SyntheticPath));
        }

        [Test]
        public void DisableThenEnable_IncludedScene_TogglesEnabledState()
        {
            SceneEditorUtility.AddToBuildSettings(SyntheticPath);

            Assert.IsTrue(SceneEditorUtility.Disable(SyntheticPath));
            Assert.IsFalse(SceneEditorUtility.IsEnabled(SyntheticPath));
            Assert.IsTrue(SceneEditorUtility.IsIncluded(SyntheticPath), "Disabling should keep the scene included.");

            Assert.IsTrue(SceneEditorUtility.Enable(SyntheticPath));
            Assert.IsTrue(SceneEditorUtility.IsEnabled(SyntheticPath));
        }

        [Test]
        public void RemoveFromBuildSettings_Included_RemovesAndReturnsTrue()
        {
            SceneEditorUtility.AddToBuildSettings(SyntheticPath);

            Assert.IsTrue(SceneEditorUtility.RemoveFromBuildSettings(SyntheticPath));
            Assert.IsFalse(SceneEditorUtility.IsIncluded(SyntheticPath));
        }

        [Test]
        public void RemoveFromBuildSettings_NotIncluded_ReturnsFalse()
        {
            Assert.IsFalse(SceneEditorUtility.RemoveFromBuildSettings(SyntheticPath));
        }

        #endregion


        #region Scene / SceneAsset overloads (degenerate inputs — forwarding + null-safety)

        [Test]
        public void SceneOverloads_DefaultScene_ResolveEmptyPathWithoutThrowing()
        {
            Scene scene = default;

            Assert.IsFalse(SceneEditorUtility.IsIncluded(scene));
            Assert.IsFalse(SceneEditorUtility.IsEnabled(scene));
            Assert.IsFalse(SceneEditorUtility.AddToBuildSettings(scene), "An empty scene path must not be added (guard).");
            Assert.IsFalse(SceneEditorUtility.Enable(scene));
            Assert.IsFalse(SceneEditorUtility.Disable(scene));
            Assert.IsFalse(SceneEditorUtility.RemoveFromBuildSettings(scene));
        }

        [Test]
        public void SceneAssetOverloads_NullAsset_ResolveEmptyPathWithoutThrowing()
        {
            SceneAsset sceneAsset = null;

            Assert.IsFalse(SceneEditorUtility.IsIncluded(sceneAsset));
            Assert.IsFalse(SceneEditorUtility.IsEnabled(sceneAsset));
            Assert.IsFalse(SceneEditorUtility.AddToBuildSettings(sceneAsset), "A null scene asset resolves to an empty path (guard).");
            Assert.IsFalse(SceneEditorUtility.Enable(sceneAsset));
            Assert.IsFalse(SceneEditorUtility.Disable(sceneAsset));
            Assert.IsFalse(SceneEditorUtility.RemoveFromBuildSettings(sceneAsset));
        }

        [Test]
        public void SceneExtensions_ForwardToUtility()
        {
            Scene scene = default;

            Assert.IsFalse(scene.IsIncluded());
            Assert.IsFalse(scene.IsEnabled());
            Assert.IsFalse(scene.AddToBuildSettings());
            Assert.IsFalse(scene.Enable());
            Assert.IsFalse(scene.Disable());
            Assert.IsFalse(scene.RemoveFromBuildSettings());
        }

        [Test]
        public void SceneAssetExtensions_ForwardToUtility()
        {
            SceneAsset sceneAsset = null;

            Assert.IsFalse(sceneAsset.IsIncluded());
            Assert.IsFalse(sceneAsset.IsEnabled());
            Assert.IsFalse(sceneAsset.AddToBuildSettings());
            Assert.IsFalse(sceneAsset.Enable());
            Assert.IsFalse(sceneAsset.Disable());
            Assert.IsFalse(sceneAsset.RemoveFromBuildSettings());
        }

        #endregion

    }

}
