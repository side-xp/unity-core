using System;
using System.Reflection;

using UnityEngine;
using UnityEditor;

using Object = UnityEngine.Object;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Processes the asset scripts that use the <see cref="UniqueIdAttribute"/> to assign their id as expected.
    /// </summary>
    public class UniqueIdProcessor : AssetPostprocessor
    {

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            // Foreach field marked with [UniqueId] attribute in the project
            foreach (FieldInfo fieldInfo in TypeCache.GetFieldsWithAttribute<UniqueIdAttribute>())
            {
                // If the current field is declared in a ScriptableObject class
                if (typeof(ScriptableObject).IsAssignableFrom(fieldInfo.DeclaringType))
                {
                    // For each asset in the project of that type
                    foreach (Object asset in ObjectUtility.FindAssets(fieldInfo.DeclaringType))
                    {
                        string guid = null;
                        // Try to get the GUID from file
                        if(!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out guid, out long localId) && !string.IsNullOrEmpty(guid) && localId != 0)
                        {
                            Debug.LogWarning($"Failed to get the GUID of the asset of type {asset.GetType()}: as a fallback, {nameof(UniqueIdProcessor)} will assign a native C# GUID to its field {fieldInfo.DeclaringType}.{fieldInfo.Name} marked with {nameof(UniqueIdAttribute)}.", asset);
                            guid = Guid.NewGuid().ToString();
                        }

                        // Assign the id to the found field
                        SerializedObject assetObj = new SerializedObject(asset);
                        assetObj.FindProperty(fieldInfo.Name).stringValue = guid;
                        assetObj.ApplyModifiedPropertiesWithoutUndo();
                    }
                }
                // Else, if the current field is a serialized property declared in an asset
                else if (fieldInfo.DeclaringType.Is(typeof(Object)))
                {
                    // For each asset in the project of that type
                    foreach (Object asset in ObjectUtility.FindAssets(fieldInfo.DeclaringType))
                    {
                        SerializedObject assetObj = new SerializedObject(asset);
                        // Try to get the named serialized property
                        SerializedProperty prop = assetObj.FindProperty(fieldInfo.Name);
                        if (prop == null)
                            continue;

                        // Assign a GUID depending on the property type
                        if (prop.propertyType == SerializedPropertyType.String)
                            prop.stringValue = Guid.NewGuid().ToString();
                        else if (prop.propertyType == SerializedPropertyType.Integer)
                            prop.intValue = Guid.NewGuid().ToString().GetHashCode();
                        else if (prop.propertyType == SerializedPropertyType.Float)
                            prop.floatValue = Guid.NewGuid().ToString().GetHashCode();
                        else
                            continue;

                        prop.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    }
                }
                else
                {
                    Debug.LogWarning($"The field {fieldInfo.DeclaringType}.{fieldInfo.Name} uses {nameof(UniqueIdAttribute)}, but that attribute can be used only in {nameof(ScriptableObject)} classes.");
                }

                /**
                 *  @todo
                 *  We want to make [UniqueId] supported for MonoBehaviour classes.
                 *  There's no function to get a unique identifier for a component though, so we must do it by hand.
                 *  The problem with components is that there is several use cases:
                 *      - Usage in a scene, not related to a prefab
                 *      - Usage in a scene, related to a prefab
                 *      - Usage in a prefab
                 *      - Special care needed to handle prefab variants
                 *  The unique identifier of a component can be computed by combining the scene or prefab GUID, the script GUID, the
                 *  componet id and the prefab instance id in the scene if applicable.
                 *  It will require to read and directly edit scene assets (*.unity) and prefab assets (*.prefab) to do so. Here is some
                 *  utility pattern Regex:
                 *      - @"--- !u!\d+\s+\D*(?<componentId>\d+)" -> Find the component id in a scene or prefab asset
                 *      - @"m_Script:\s*{.*(?:guid:\s*(?<guid>\w+)).*?}" -> Find the GUID of a script attached to a MonoBehaviour entry in a
                 *      scene or prefab asset
                 */
            }
        }

    }

}