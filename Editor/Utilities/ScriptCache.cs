using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Cache data for <see cref="ScriptUtility"/>.
    /// </summary>
    [EditorConfig(EEditorConfigScope.User)]
    internal class ScriptCache : ScriptableObject, IEditorConfig
    {

        #region Subclasses

        /// <summary>
        /// Groups informations about a script in cached data.
        /// </summary>
        [System.Serializable]
        private class ScriptInfo
        {

            /// <summary>
            /// The path to the script asset, relative from the project's root directory.
            /// </summary>
            [SerializeField]
            private string _path = string.Empty;

            /// <summary>
            /// The <see cref="Type.AssemblyQualifiedName"/> value of the script's type.
            /// </summary>
            [SerializeField]
            private string _typeName = string.Empty;

            /// <summary>
            /// The type declared in the script.
            /// </summary>
            private Type _type = null;

            /// <summary>
            /// The script asset in the project.
            /// </summary>
            private MonoScript _script = null;

            /// <inheritdoc cref="ScriptInfo"/>
            /// <param name="path">The path to the script asset.</param>
            /// <param name="type"><inheritdoc cref="_type" path="/summary"/></param>
            public ScriptInfo(string path, Type type)
            {
                _path = path.ToRelativePath();
                _typeName = type.AssemblyQualifiedName;
                _type = type;
            }

            /// <inheritdoc cref="_path"/>
            public string Path => _path;

            /// <inheritdoc cref="_typeName"/>
            public string TypeName => _typeName;

            /// <inheritdoc cref="_type"/>
            public Type Type
            {
                get
                {
                    if (_type == null)
                        _type = System.Type.GetType(_typeName);
                    return _type;
                }
            }

            /// <inheritdoc cref="_script"/>
            public MonoScript ScriptAsset
            {
                get
                {
                    if (_script == null)
                        _script = AssetDatabase.LoadAssetAtPath<MonoScript>(_path);
                    return _script;
                }
            }

            /// <summary>
            /// Checks if this entry is still valid.
            /// </summary>
            /// <remarks>This function only checks if the stored type name can be converted into <see cref="System.Type"/>, and the asset
            /// still exists at the same path.</remarks>
            public bool IsValid => ScriptAsset != null && Type != null;

        }

        #endregion


        #region Fields

        /// <summary>
        /// Cached data about scripts in the project.
        /// </summary>
        [SerializeField]
        private List<ScriptInfo> _cacheData = new List<ScriptInfo>();

        /// <summary>
        /// Flag enabled after the cached data have been cleaned.
        /// </summary>
        private static bool s_didCleanCache = false;

        #endregion


        #region Public API

        /// <summary>
        /// Gets the loaded cache or load them from disk if not already.
        /// </summary>
        public static ScriptCache Instance => EditorConfigUtility.Get<ScriptCache>();

        /// <inheritdoc cref="Instance"/>
        public static ScriptCache I => Instance;

        /// <summary>
        /// Gets the cached data for the script at the given path.
        /// </summary>
        /// <param name="path">The path of the script.</param>
        /// <param name="type">Outputs the type declared in the script.</param>
        /// <param name="script">Outputs the script asset.</param>
        /// <returns>Returns true if a valid cached data has been found.</returns>
        public static bool Get(string path, out Type type, out MonoScript script)
        {
            CleanCache();
            path = path.ToRelativePath();

            ScriptInfo entry = I._cacheData.Find(i => i.Path == path);
            type = entry?.Type;
            script = entry?.ScriptAsset;
            return entry != null && entry.IsValid;
        }

        /// <summary>
        /// Gets the cached data for a given script asset.
        /// </summary>
        /// <param name="script">The script asset.</param>
        /// <param name="path">Outputs the path of the script.</param>
        /// <inheritdoc cref="Get(string, out Type, out MonoScript)"/>
        public static bool Get(MonoScript script, out string path, out Type type)
        {
            CleanCache();

            ScriptInfo entry = I._cacheData.Find(i => i.ScriptAsset == script);
            path = entry?.Path;
            type = entry?.Type;
            return entry != null && entry.IsValid;
        }

        /// <summary>
        /// Sets the cached data of a script at the given path.
        /// </summary>
        /// <param name="path">The path of the script asset.</param>
        /// <param name="type">The type declared in the script.</param>
        public static void Set(string path, Type type)
        {
            // Delete potential existing entry for the given type
            for (int i = I._cacheData.Count - 1; i >= 0; i--)
            {
                if (I._cacheData[i].Type == type)
                {
                    I._cacheData.RemoveAt(i);
                    break;
                }
            }

            I._cacheData.Add(new ScriptInfo(path.ToRelativePath(), type));
        }

        /// <summary>
        /// Sets the cached data of given script asset.
        /// </summary>
        /// <param name="script">The script asset.</param>
        /// <inheritdoc cref="Set(string, Type)"/>
        public static void Set(MonoScript script, Type type)
        {
            Set(AssetDatabase.GetAssetPath(script), type);
        }

        /// <inheritdoc cref="IEditorConfig.PostLoad"/>
        public void PostLoad() { }

        #endregion


        #region Privtae API

        /// <summary>
        /// Removes all invalid entries in cached data.
        /// </summary>
        /// <returns>Returns true if the cache has been cleaned successfully.</returns>
        private static bool CleanCache()
        {
            if (s_didCleanCache)
                return false;

            for (int i = I._cacheData.Count - 1; i >= 0; i--)
            {
                if (!I._cacheData[i].IsValid)
                    I._cacheData.RemoveAt(i);
            }

            s_didCleanCache = true;
            return true;
        }

        #endregion

    }

}