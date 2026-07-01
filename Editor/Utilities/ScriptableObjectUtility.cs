using System;

using UnityEngine;
using UnityEditor;

using Object = UnityEngine.Object;

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
            Object.DestroyImmediate(defaultObj);
            Undo.RecordObject(obj, "Reset Object To Defaults");
            JsonUtility.FromJsonOverwrite(json, obj);
        }

    }

}