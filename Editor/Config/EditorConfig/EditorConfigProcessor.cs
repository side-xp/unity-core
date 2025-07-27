using UnityEditor;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Asset processor for editor settings.
    /// </summary>
    [InitializeOnLoad]
    public class EditorConfigProcessor : AssetModificationProcessor
    {

        /// <summary>
        /// Static constructor.
        /// </summary>
        static EditorConfigProcessor()
        {
            AssemblyReloadEvents.beforeAssemblyReload += HandleBeforeAssemblyReload;
        }

        /// <summary>
        /// Called when Unity is about to write serialized assets on disk.
        /// </summary>
        private static string[] OnWillSaveAssets(string[] paths)
        {
            EditorConfigUtility.SaveAll();
            return paths;
        }

        /// <inheritdoc cref="AssemblyReloadEvents.beforeAssemblyReload"/>
        private static void HandleBeforeAssemblyReload()
        {
            EditorConfigUtility.SaveAll();
        }

    }

}
