using UnityEngine.SceneManagement;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Extension functions for <see cref="Scene"/> values.
    /// </summary>
    public static class SceneExtensions
    {

        /// <inheritdoc cref="SceneEditorUtility.IsIncluded(Scene)"/>
        public static bool IsIncluded(this Scene scene)
        {
            return SceneEditorUtility.IsIncluded(scene);
        }

        /// <inheritdoc cref="SceneEditorUtility.IsEnabled(Scene)"/>
        public static bool IsEnabled(this Scene scene)
        {
            return SceneEditorUtility.IsEnabled(scene);
        }

        /// <inheritdoc cref="SceneEditorUtility.AddToBuildSettings(Scene)"/>
        public static bool AddToBuildSettings(this Scene scene)
        {
            return SceneEditorUtility.AddToBuildSettings(scene);
        }

        /// <inheritdoc cref="SceneEditorUtility.Enable(Scene)"/>
        public static bool Enable(this Scene scene)
        {
            return SceneEditorUtility.Enable(scene);
        }

        /// <inheritdoc cref="SceneEditorUtility.Disable(Scene)"/>
        public static bool Disable(this Scene scene)
        {
            return SceneEditorUtility.Disable(scene);
        }

        /// <inheritdoc cref="SceneEditorUtility.RemoveFromBuildSettings(Scene)"/>
        public static bool RemoveFromBuildSettings(this Scene scene)
        {
            return SceneEditorUtility.RemoveFromBuildSettings(scene);
        }

    }

}