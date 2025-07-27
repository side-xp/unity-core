using System;

using UnityEngine;
using UnityEditor;

using Object = UnityEngine.Object;
using System.IO;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Miscellaneous functions for working with <see cref="ScriptableObject"/> and assets in the editor.
    /// </summary>
    public class ScriptableObjectUtility
    {

        /// <summary>
        /// Gets the path to the source script of the given type of <see cref="ScriptableObject"/>.
        /// </summary>
        /// <param name="scriptableObjectType">The type of the <see cref="ScriptableObject"/> you want to get the script file path.</param>
        public static string GetScriptPath(Type scriptableObjectType)
        {
            ScriptableObject tmpInstance = ScriptableObject.CreateInstance(scriptableObjectType);
            MonoScript sourceScriptAsset = MonoScript.FromScriptableObject(tmpInstance);
            Object.DestroyImmediate(tmpInstance);
            return AssetDatabase.GetAssetPath(sourceScriptAsset);
        }

        /// <inheritdoc cref="GetScriptPath(Type)"/>
        /// <param name="obj">The <see cref="ScriptableObject"/> you want to get the script path.</param>
        /// <returns>Returns the found script path, or null if the path to the script file can't be found.</returns>
        public static string GetScriptPath(ScriptableObject obj)
        {
            return GetScriptPath(obj.GetType());
        }

        /// <inheritdoc cref="GetScriptPath(Type)"/>
        /// <typeparam name="TScriptableObject">The type of the <see cref="ScriptableObject"/> you want to get the script file path.</typeparam>
        public static string GetScriptPath<TScriptableObject>()
            where TScriptableObject : ScriptableObject
        {
            return GetScriptPath(typeof(TScriptableObject));
        }

        /// <summary>
        /// Reset the given object to its default values. This operation is undoable.
        /// </summary>
        /// <remarks>Since Unity doesn't provide a utility function to do this, this function will create an asset of the same type, and
        /// copy its serialized values to the given object.</remarks>
        /// <param name="obj">The object to reset.</param>
        public static void ResetToDefaults(ScriptableObject obj)
        {
            ScriptableObject defaultObj = ScriptableObject.CreateInstance(obj.GetType());
            string json = JsonUtility.ToJson(defaultObj);
            Undo.RecordObject(obj, "Reset Object To Defaults");
            JsonUtility.FromJsonOverwrite(json, obj);
        }

        // The following part has been removed, as Resources are not meant to be used anymore.
        //
        ///// <summary>
        ///// Gets a resource of a given type if it exists, or creates it.
        ///// </summary>
        ///// <param name="assetType">The expected type of the resource asset.</param>
        ///// <param name="isNew">Outputs true if the asset has just been created.</param>
        ///// <param name="path">The path of the resource asset, from /Resources directory (so excluding the "Resources/" part). You must also
        ///// omit the file extension.</param>
        ///// <returns>Returns the found or created resource asset.</returns>
        //public static ScriptableObject GetOrCreateResource(Type assetType, out bool isNew, string path = null)
        //{
        //    // Use [Asset Type Name].asset as default path
        //    if (string.IsNullOrEmpty(path))
        //        path = $"{assetType.Name}";

        //    // If an asset exists at the given type
        //    Object existingAsset = Resources.Load(path, assetType);
        //    if (existingAsset != null)
        //    {
        //        isNew = false;
        //        // Warning if the types doesn't match
        //        if (!assetType.IsAssignableFrom(existingAsset.GetType()))
        //        {
        //            Debug.LogWarning($"A resource already exists at {PathUtility.BaseResourcesPath}/{path}, but the found object is of type {existingAsset.GetType().FullName}, expected {assetType.FullName}.", existingAsset);
        //            return null;
        //        }
        //        return existingAsset as ScriptableObject;
        //    }

        //    // Fix resource path
        //    if (!path.StartsWith(PathUtility.BaseResourcesPath))
        //        path = $"{PathUtility.BaseResourcesPath}/{path}";
        //    if (Path.GetExtension(path) != ".asset")
        //        path += ".asset";

        //    // Create directory if it doesn't exist yet
        //    string absPath = Path.GetDirectoryName(path.ToAbsolutePath());
        //    try
        //    {
        //        if (!Directory.Exists(absPath))
        //            Directory.CreateDirectory(absPath);
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogException(e);
        //    }

        //    // Create the asset
        //    ScriptableObject asset = ScriptableObject.CreateInstance(assetType);
        //    AssetDatabase.CreateAsset(asset, path);
        //    Debug.Log($"Resource of type {assetType.FullName} created at {PathUtility.BaseResourcesPath}/{path}", asset);
        //    isNew = true;
        //    return asset;
        //}

        ///// <typeparam name="T"><inheritdoc cref="GetOrCreateResource(Type, out bool, string)" path="/param[@name='assetType']"/></typeparam>
        ///// <inheritdoc cref="GetOrCreateResource(Type, out bool, string)"/>
        //public static T GetOrCreateResource<T>(out bool isNew, string path = null)
        //    where T : ScriptableObject
        //{
        //    ScriptableObject asset = GetOrCreateResource(typeof(T), out isNew, path);
        //    return asset as T;
        //}

    }

}