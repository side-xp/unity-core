using System.Reflection;

using UnityEngine;
using UnityEditor;

namespace SideXP.Core.EditorOnly
{

    /// <inheritdoc cref="TypesMigration.Reload"/>
    public class TypeRefProcessor : AssetPostprocessor
    {

#if UNITY_2021_2_OR_NEWER
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths,
            bool didDomainReload)
#else
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
#endif
        {
            // Log message if types migrations are not tracked
#if UNITY_2020_OR_NEWER
            foreach (FieldInfo fieldInfo in TypeCache.GetFieldsWithAttribute<TypeRefAttribute>())
#else
            foreach (FieldInfo fieldInfo in TypeCachePolyfill.GetFieldsWithAttribute<TypeRefAttribute>())
#endif
            {
                if (!CoreEditorConfig.I.TrackTypesMigrations)
                    Debug.Log($"A field marked with the [TypeRef] attribute has been found in your project, but type migrations are not tracked. You can enable this tracking from Edit > Project Settings > {Constants.CompanyName} > General > Track Types Migrations. Without this option enabled, renamed types won't be tracked, so the type stored in a [TypeRef] property may not be valid at some point.");

                break;
            }

            if (CoreEditorConfig.I.TrackTypesMigrations)
                TypesMigration.Reload();
        }

    }

}