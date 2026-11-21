using UnityEditor;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Saves the loaded editor configs when Unity saves the project, before scripts are reloaded, and when the editor quits.
    /// </summary>
    [InitializeOnLoad]
    public class EditorConfigProcessor
#if UNITY_6000_0_OR_NEWER
        : AssetModificationProcessor
#else
        : UnityEditor.AssetModificationProcessor
#endif
    {

        /// <summary>
        /// Static constructor.
        /// </summary>
        static EditorConfigProcessor()
        {
            AssemblyReloadEvents.beforeAssemblyReload += HandleBeforeAssemblyReload;
            EditorApplication.quitting += HandleQuitting;
        }

        /// <summary>
        /// Called when Unity is about to write serialized assets on disk.
        /// </summary>
        /// <param name="paths">The paths of the assets about to be saved.</param>
        /// <returns>Returns the paths of the assets to save, unchanged.</returns>
        private static string[] OnWillSaveAssets(string[] paths)
        {
            EditorConfigUtility.SaveAll();
            return paths;
        }

        /// <inheritdoc cref="AssemblyReloadEvents.beforeAssemblyReload"/>
        private static void HandleBeforeAssemblyReload()
        {
            EditorConfigUtility.SaveAll();
            // The loaded configs would survive the reload and leak, since they use HideFlags.DontSave
            EditorConfigUtility.UnloadAll();
        }

        /// <inheritdoc cref="EditorApplication.quitting"/>
        private static void HandleQuitting()
        {
            EditorConfigUtility.SaveAll();
        }

    }

}
