using System.Reflection;

using UnityEngine;
using UnityEditor;

namespace SideXP.Core.EditorOnly
{

    /// <inheritdoc cref="TypesMigration.Reload"/>
    public class TypeRefProcessor : AssetPostprocessor
    {

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths,
            bool didDomainReload)
        {
            // Log message if types migrations are not tracked
            foreach (FieldInfo type in TypeCache.GetFieldsWithAttribute<TypeRefAttribute>())
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