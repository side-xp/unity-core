using UnityEditor;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Extension functions for <see cref="SceneAsset"/> objects.
    /// </summary>
    public static class SceneAssetExtensions
    {

        /// <inheritdoc cref="SceneEditorUtility.IsIncluded(SceneAsset)"/>
        public static bool IsIncluded(this SceneAsset sceneAsset)
        {
            return SceneEditorUtility.IsIncluded(sceneAsset);
        }

        /// <inheritdoc cref="SceneEditorUtility.IsEnabled(SceneAsset)"/>
        public static bool IsEnabled(this SceneAsset sceneAsset)
        {
            return SceneEditorUtility.IsEnabled(sceneAsset);
        }

        /// <inheritdoc cref="SceneEditorUtility.AddToBuildSettings(SceneAsset)"/>
        public static bool AddToBuildSettings(this SceneAsset sceneAsset)
        {
            return SceneEditorUtility.AddToBuildSettings(sceneAsset);
        }

        /// <inheritdoc cref="SceneEditorUtility.Enable(SceneAsset)"/>
        public static bool Enable(this SceneAsset sceneAsset)
        {
            return SceneEditorUtility.Enable(sceneAsset);
        }

        /// <inheritdoc cref="SceneEditorUtility.Disable(SceneAsset)"/>
        public static bool Disable(this SceneAsset sceneAsset)
        {
            return SceneEditorUtility.Disable(sceneAsset);
        }

        /// <inheritdoc cref="SceneEditorUtility.RemoveFromBuildSettings(SceneAsset)"/>
        public static bool RemoveFromBuildSettings(this SceneAsset sceneAsset)
        {
            return SceneEditorUtility.RemoveFromBuildSettings(sceneAsset);
        }

    }

}