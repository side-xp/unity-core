using System.Collections.Generic;

using UnityEngine.SceneManagement;
using UnityEditor;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Miscellaneous utility functions for working with <see cref="Scene"/> and <see cref="SceneAsset"/> in the editor.
    /// </summary>
    public static class SceneEditorUtility
    {

        /// <param name="scene">The scene to check.</param>
        /// <inheritdoc cref="IsIncluded(string)"/>
        public static bool IsIncluded(Scene scene)
        {
            return IsIncluded(scene.path);
        }

        /// <param name="sceneAsset">The scene to check.</param>
        /// <inheritdoc cref="IsIncluded(string)"/>
        public static bool IsIncluded(SceneAsset sceneAsset)
        {
            return IsIncluded(AssetDatabase.GetAssetPath(sceneAsset));
        }

        /// <summary>
        /// Checks if a given scene is included in build settings.
        /// </summary>
        /// <param name="scenePath">The path of the scene to check.</param>
        /// <returns>Returns true if the given scene is included in build settings, whether it's enabled or not.</returns>
        public static bool IsIncluded(string scenePath)
        {
            foreach (EditorBuildSettingsScene buildSettingsScene in EditorBuildSettings.scenes)
            {
                if (buildSettingsScene.path == scenePath)
                    return true;
            }
            return false;
        }

        /// <param name="scene">The scene to check.</param>
        /// <inheritdoc cref="IsEnabled(string)"/>
        public static bool IsEnabled(Scene scene)
        {
            return IsEnabled(scene.path);
        }

        /// <param name="sceneAsset">The scene to check.</param>
        /// <inheritdoc cref="IsEnabled(string)"/>
        public static bool IsEnabled(SceneAsset sceneAsset)
        {
            return IsEnabled(AssetDatabase.GetAssetPath(sceneAsset));
        }

        /// <summary>
        /// Checks if a given scene is included and enabled in build settings.
        /// </summary>
        /// <param name="scenePath">The path of the scene to check.</param>
        /// <returns>Returns true if the given scene is included and enabled in build settings.</returns>
        public static bool IsEnabled(string scenePath)
        {
            foreach (EditorBuildSettingsScene buildSettingsScene in EditorBuildSettings.scenes)
            {
                if (buildSettingsScene.path == scenePath && buildSettingsScene.enabled)
                    return true;
            }
            return false;
        }

        /// <param name="scene">The scene to add.</param>
        /// <inheritdoc cref="AddToBuildSettings(string)"/>
        public static bool AddToBuildSettings(Scene scene)
        {
            return AddToBuildSettings(scene.path);
        }

        /// <param name="sceneAsset">The scene to add.</param>
        /// <inheritdoc cref="AddToBuildSettings(string)"/>
        public static bool AddToBuildSettings(SceneAsset sceneAsset)
        {
            return AddToBuildSettings(AssetDatabase.GetAssetPath(sceneAsset));
        }

        /// <summary>
        /// Adds a given scene to the build settings, if it's not already registered.
        /// </summary>
        /// <param name="scenePath">The path of the scene to add.</param>
        /// <returns>Returns true if the scene has been added to build settings successfully.</returns>
        public static bool AddToBuildSettings(string scenePath)
        {
            if (IsIncluded(scenePath))
                return false;

            List<EditorBuildSettingsScene> scenesList = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes)
            {
                new EditorBuildSettingsScene(scenePath, true)
            };
            EditorBuildSettings.scenes = scenesList.ToArray();
            return true;
        }

        /// <param name="scene">The scene to enable.</param>
        /// <inheritdoc cref="Enable(string)"/>
        public static bool Enable(Scene scene)
        {
            return Enable(scene.path);
        }

        /// <param name="sceneAsset">The scene to enable.</param>
        /// <inheritdoc cref="Enable(string)"/>
        public static bool Enable(SceneAsset sceneAsset)
        {
            return Enable(AssetDatabase.GetAssetPath(sceneAsset));
        }

        /// <summary>
        /// Enables a given scene in the build settings.
        /// </summary>
        /// <param name="scenePath">The path of the scene to enable.</param>
        /// <returns>Returns true if the scene has been enabled successfully or was already enabled.</returns>
        public static bool Enable(string scenePath)
        {
            List<EditorBuildSettingsScene> sceneInfoList = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            int index = sceneInfoList.FindIndex(i => i.path == scenePath);

            if (index < 0)
                return false;

            sceneInfoList[index] = new EditorBuildSettingsScene(scenePath, true);
            EditorBuildSettings.scenes = sceneInfoList.ToArray();
            return true;
        }

        /// <param name="scene">The scene to disable.</param>
        /// <inheritdoc cref="Disable(string)"/>
        public static bool Disable(Scene scene)
        {
            return Disable(scene.path);
        }

        /// <param name="sceneAsset">The scene to disable.</param>
        /// <inheritdoc cref="Disable(string)"/>
        public static bool Disable(SceneAsset sceneAsset)
        {
            return Disable(AssetDatabase.GetAssetPath(sceneAsset));
        }

        /// <summary>
        /// Disables a given scene in the build settings.
        /// </summary>
        /// <param name="scenePath">The path of the scene to disable.</param>
        /// <returns>Returns true if the scene has been disabled successfully or was already disabled.</returns>
        public static bool Disable(string scenePath)
        {
            List<EditorBuildSettingsScene> sceneInfoList = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            int index = sceneInfoList.FindIndex(i => i.path == scenePath);
            
            if (index < 0)
                return false;

            sceneInfoList[index] = new EditorBuildSettingsScene(scenePath, false);
            EditorBuildSettings.scenes = sceneInfoList.ToArray();
            return true;
        }

        /// <param name="scene">The scene to remove.</param>
        /// <inheritdoc cref="RemoveFromBuildSettings(string)"/>
        public static bool RemoveFromBuildSettings(Scene scene)
        {
            return RemoveFromBuildSettings(scene.path);
        }

        /// <param name="sceneAsset">The scene to remove.</param>
        /// <inheritdoc cref="RemoveFromBuildSettings(string)"/>
        public static bool RemoveFromBuildSettings(SceneAsset sceneAsset)
        {
            return RemoveFromBuildSettings(AssetDatabase.GetAssetPath(sceneAsset));
        }

        /// <summary>
        /// Removes a given scene from the build settings.
        /// </summary>
        /// <param name="scenePath">The path of the scene to remove.</param>
        /// <returns>Returns true if the scene has been removed from the build settings successfully.</returns>
        public static bool RemoveFromBuildSettings(string scenePath)
        {
            List<EditorBuildSettingsScene> scenesList = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            int index = scenesList.FindIndex(i => i.path == scenePath);

            if (index < 0)
                return false;

            scenesList.RemoveAt(index);
            EditorBuildSettings.scenes = scenesList.ToArray();
            return true;
        }

    }

}