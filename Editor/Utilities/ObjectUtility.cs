using System;
using System.IO;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using Object = UnityEngine.Object;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Miscellaneous utility functions for working with <see cref="Object"/> instances and assets.
    /// </summary>
    public static class ObjectUtility
    {

        #region Paths

        /// <summary>
        /// Gets the relative path to the given asset from the root directory of the current Unity project.
        /// </summary>
        /// <param name="asset">The asset of which you want to get the path.</param>
        /// <returns>Returns the found relative asset path, or null if the given asset is not an asset.</returns>
        public static string GetAssetPath(Object asset)
        {
            string path = AssetDatabase.GetAssetPath(asset);
            return string.IsNullOrEmpty(path) ? null : path;
        }

        /// <summary>
        /// Gets the absolute path to the given asset.
        /// </summary>
        /// <inheritdoc cref="GetAssetPath(Object)"/>
        /// <returns>Returns the found absolute asset path, or null if the asset is not an asset.</returns>
        public static string GetAbsoluteAssetPath(Object asset)
        {
            string path = GetAssetPath(asset);
            return !string.IsNullOrEmpty(path) ? path.ToAbsolutePath() : null;
        }

        #endregion


        #region Queries

        /// <summary>
        /// Finds all the assets in the project that match the given name and type.
        /// </summary>
        /// <param name="name">The expected name of the assets.<br/>
        /// Words separated by spaces are considered as separate filters. These are used as search strings, so typing just "image" will
        /// query all assets that include this word in their name.</param>
        /// <param name="type">The expected type of the assets.</param>
        /// <param name="querySubassets">If enabled, this function will also search in the assets attached to main ones.</param>
        /// <returns>Returns all the found assets that match the given informations.</returns>
        /// <seealso cref="AssetDatabase.FindAssets(string)"/>
        public static Object[] FindAssets(string name, Type type, bool querySubassets = false)
        {
            // Remove file extension if applicable
            if (!string.IsNullOrEmpty(name))
                name = Path.GetFileNameWithoutExtension(name);

            // Prepare search string
            string search = !string.IsNullOrEmpty(name) ? name : "";

            // Add type filter if applicable
            if (type != null)
            {
                if (!string.IsNullOrEmpty(search))
                    search += " ";
                search += $"t:{type.Name}";
            }
            else
            {
                type = typeof(Object);
            }

            List<Object> assets = new List<Object>();
            string[] guids = AssetDatabase.FindAssets(search);

            // For each found assets of the expected type
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Object mainAsset = AssetDatabase.LoadAssetAtPath<Object>(path);

                // Add the main asset to the list if it has the expected type.
                // This check must be done because AssetDatabase.FindAssets() will also query assets that contains attached assets of the expected type.
                if (type.IsAssignableFrom(mainAsset.GetType()))
                    assets.Add(mainAsset);

                // If the query is extended to attached assets
                if (querySubassets)
                {
                    // For each subasset
                    foreach (Object subasset in AssetDatabase.LoadAllAssetsAtPath(path))
                    {
                        // Skkip if the subasset is broken
                        if (subasset == null)
                            continue;

                        // Skip if the current sub object is the main one
                        if (subasset == mainAsset)
                            continue;

                        // Add the asset to the list if it has the expected type
                        if (type.IsAssignableFrom(subasset.GetType()))
                            assets.Add(subasset);
                    }
                }
            }

            return assets.ToArray();
        }

        /// <typeparam name="T">The expected type of the assets.</typeparam>
        /// <inheritdoc cref="FindAssets(string, Type, bool)"/>
        public static T[] FindAssets<T>(string name, bool querySubassets = false)
            where T : Object
        {
            List<T> assets = new List<T>();
            foreach (Object asset in FindAssets(name, typeof(T), querySubassets))
            {
                T typedAsset = asset as T;
                if (typedAsset != null)
                    assets.Add(typedAsset);
            }
            return assets.ToArray();
        }

        /// <inheritdoc cref="FindAssets(string, Type, bool)"/>
        public static Object[] FindAssets(Type type, bool querySubassets = false)
        {
            return FindAssets(null, type, querySubassets);
        }

        /// <inheritdoc cref="FindAssets{T}(string, bool)"/>
        public static T[] FindAssets<T>(bool querySubassets = false)
            where T : Object
        {
            return FindAssets<T>(null, querySubassets);
        }

        /// <inheritdoc cref="FindAssets(string, Type, bool)"/>
        public static Object[] FindAssets(string name, bool querySubassets = false)
        {
            return FindAssets(name, null, querySubassets);
        }

        /// <summary>
        /// Find an asset in the project that match the given name and type.
        /// </summary>
        /// <param name="name">The expected name of the asset.<br/>
        /// Words separated by spaces are considered as separate filters. These are used as search strings, so typing just "image" will
        /// query all assets that include this word in their name.</param>
        /// <param name="type">The expected type of the asset.</param>
        /// <returns>Returns the first found asset that match the given filters.</returns>
        /// <inheritdoc cref="FindAssets(string, Type, bool)"/>
        public static Object FindAsset(string name, Type type, bool querySubassets = false)
        {
            Object[] assets = FindAssets(name, type, querySubassets);
            return assets.Length > 0 ? assets[0] : null;
        }

        /// <typeparam name="T">The expected type of the asset.</typeparam>
        /// <inheritdoc cref="FindAsset(string, Type, bool)"/>
        public static T FindAsset<T>(string name, bool querySubassets = false)
            where T : Object
        {
            return FindAsset(name, typeof(T), querySubassets) as T;
        }

        /// <inheritdoc cref="FindAsset(string, Type, bool)"/>
        public static Object FindAsset(Type type, bool querySubassets = false)
        {
            return FindAsset(null, type, querySubassets);
        }

        /// <inheritdoc cref="FindAsset{T}(string, bool)"/>
        public static T FindAsset<T>(bool querySubassets = false)
            where T : Object
        {
            return FindAsset(null, typeof(T), querySubassets) as T;
        }

        /// <inheritdoc cref="FindAsset(string, Type, bool)"/>
        public static Object FindAsset(string name, bool querySubassets = false)
        {
            return FindAsset(name, null, querySubassets);
        }

        /// <summary>
        /// Gets the first subasset of the given type in the given main asset.
        /// </summary>
        /// <param name="asset">The main asset of which you want to get the subasset.</param>
        /// <param name="type">The expected type of the subasset.</param>
        /// <param name="includeMainAsset">If enabled, if the main asset has the expected type, it's the one being returned.</param>
        /// <returns>Returns the found subasset with the expected type.</returns>
        public static Object FindSubassetOfType(Object asset, Type type, bool includeMainAsset = false)
        {
            Object[] subassets = FindAllSubassetsOfType(asset, type, includeMainAsset);
            return subassets.Length > 0 ? subassets[0] : null;
        }

        /// <inheritdoc cref="FindSubassetOfType(Object, Type, bool)"/>
        /// <typeparam name="T">The expected type of the subasset.</typeparam>
        public static T FindSubassetOfType<T>(Object asset, bool includeMainAsset = false)
            where T : Object
        {
            return FindSubassetOfType(asset, typeof(T), includeMainAsset) as T;
        }

        /// <summary>
        /// Gets the subassets of the given type in the given main asset.
        /// </summary>
        /// <param name="asset">The main asset of which you want to get the subassets.</param>
        /// <param name="type">The expected type of the subassets.</param>
        /// <param name="includeMainAsset">If enabled, if the main asset has the expected type, it's included in the returned array.</param>
        /// <returns>Returns the found subassets with the expected type.</returns>
        public static Object[] FindAllSubassetsOfType(Object asset, Type type, bool includeMainAsset = false)
        {
            string path = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(path))
                return null;

            List<Object> subassets = new List<Object>();

            // For each sub object
            foreach (Object subasset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                // Skkip if the subasset is broken
                if (subasset == null)
                    continue;

                // Skip if the current sub object is the main one
                if (subasset == asset && !includeMainAsset)
                    continue;

                // Add the asset to the list if it has the expected type
                if (type.IsAssignableFrom(subasset.GetType()))
                    subassets.Add(subasset);
            }

            return subassets.ToArray();
        }

        /// <inheritdoc cref="FindAllSubassetsOfType(Object, Type, bool)"/>
        /// <typeparam name="T">The expected type of the subassets.</typeparam>
        public static T[] FindAllSubassetsOfType<T>(Object asset, bool includeMainAsset = false)
            where T : Object
        {
            Object[] subassets = FindAllSubassetsOfType(asset, typeof(T), includeMainAsset);
            List<Object> assets = new List<Object>(subassets);
            return assets.ConvertAll(a => a as T).ToArray();
        }

        /// <summary>
        /// Finds an asset of a given type in the hierarchy of a given asset, whether it's the main asset or a subasset.
        /// </summary>
        /// <param name="asset">The asset of which you want to inspect the hierarchy.</param>
        /// <param name="assetType">The type of the asset to find.</param>
        /// <returns>Returns the found asset.</returns>
        public static Object FindAssetInHierarchy(Object asset, Type assetType)
        {
            string path = GetAssetPath(asset);
            Object mainAsset = AssetDatabase.LoadAssetAtPath<Object>(path);
            // Stop if the expected asset is the main one
            if (mainAsset.GetType() == assetType)
                return mainAsset;

            // For each subasset
            foreach (Object subasset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                // Skkip if the subasset is broken
                if (subasset == null)
                    continue;

                if (subasset.GetType() == assetType)
                    return subasset;
            }

            return null;
        }

        /// <param name="foundAsset">Outputs the found asset.</param>
        /// <returns>Returns true if an asset has been found.</returns>
        /// <inheritdoc cref="FindAssetInHierarchy(Object, Type)"/>
        public static bool FindAssetInHierarchy(Object asset, Type assetType, out Object foundAsset)
        {
            foundAsset = FindAssetInHierarchy(asset, assetType);
            return foundAsset != null;
        }

        /// <typeparam name="T">The type of the asset to find.</typeparam>
        /// <inheritdoc cref="FindAssetInHierarchy(Object, Type)"/>
        public static T FindAssetInHierarchy<T>(Object asset)
            where T : Object
        {
            return FindAssetInHierarchy(asset, typeof(T)) as T;
        }

        /// <inheritdoc cref="FindAssetInHierarchy(Object, Type, out Object)"/>
        /// <inheritdoc cref="FindAssetInHierarchy{T}(Object)"/>
        public static bool FindAssetInHierarchy<T>(Object asset, out T foundAsset)
            where T : Object
        {
            foundAsset = FindAssetInHierarchy<T>(asset);
            return foundAsset != null;
        }

        /// <inheritdoc cref="RuntimeObjectUtility.FindObjectFrom(object, Type, string, FFindObjectStrategy)"/>
        public static Object FindObjectFrom(object origin, Type type, string name = null, FFindObjectStrategy strategy = FFindObjectStrategy.None)
        {
            // Try to find the object
            Object obj = RuntimeObjectUtility.FindObjectFrom(origin, type, name, strategy);
            if (obj != null)
                return obj;

            return typeof(Object).IsAssignableFrom(type) ? FindAsset(name, type, true) : null;
        }

        /// <param name="obj">Outputs the found object.</param>
        /// <returns>Returns true if an object has been found.</returns>
        /// <inheritdoc cref="FindObjectFrom(object, Type, string, FFindObjectStrategy)"/>
        public static bool FindObjectFrom(object origin, Type type, out Object obj, string name = null, FFindObjectStrategy strategy = FFindObjectStrategy.None)
        {
            obj = FindObjectFrom(origin, type, name, strategy);
            return obj != null;
        }

        /// <typeparam name="T">The type of the expected object.</typeparam>
        /// <inheritdoc cref="FindObjectFrom(object, Type, string, FFindObjectStrategy)"/>
        public static T FindObjectFrom<T>(object origin, string name = null, FFindObjectStrategy strategy = FFindObjectStrategy.None)
            where T : Object
        {
            return FindObjectFrom(origin, typeof(T), name, strategy) as T;
        }

        /// <inheritdoc cref="FindObjectFrom(object, Type, out Object, string, FFindObjectStrategy)"/>
        /// <inheritdoc cref="FindObjectFrom{T}(object, string, FFindObjectStrategy)"/>
        public static bool FindObjectFrom<T>(object origin, out T obj, string name = null, FFindObjectStrategy strategy = FFindObjectStrategy.None)
            where T : Object
        {
            obj = FindObjectFrom<T>(origin, name, strategy);
            return obj != null;
        }

        #endregion


        #region Subassets

        /// <summary>
        /// Creates and attaches a subasset to an existing asset.
        /// </summary>
        /// <param name="subAssetType">The type of the subasset to create.</param>
        /// <param name="asset">The asset to which the created subasset will be attached.</param>
        /// <param name="name">The name of the subasset to create. If undefined, uses New[subasset type name].</param>
        /// <returns>Returns the created subasset.</returns>
        public static ScriptableObject CreateSubasset(Type subAssetType, Object asset, string name = null)
        {
            // Cancel if the given object is not an asset.
            if (!IsAsset(asset))
            {
                Debug.LogWarning("You can't create a sub-asset to an object that is not saved in the project.");
                return null;
            }

            // Create the subasset instance
            ScriptableObject subasset = ScriptableObject.CreateInstance(subAssetType);

            // Create unique name if no one has been set
            if (string.IsNullOrEmpty(name))
            {
                string basename = $"New{subAssetType.Name}";
                name = basename;

                int iteration = 0;
                foreach (Object existingSubAsset in FindAssets(subAssetType, asset))
                {
                    if (existingSubAsset.name == name)
                    {
                        iteration++;
                        name = basename + iteration;
                    }
                }
            }
            subasset.name = name;

            // Attach and save item asset
            AssetDatabase.AddObjectToAsset(subasset, asset);
            SaveAndReimport(asset);

            return subasset;
        }

        /// <param name="subAsset">Outputs the created subasset.</param>
        /// <returns>Returns true if the subasset has been created and attached successfully.</returns>
        /// <inheritdoc cref="CreateSubasset(Type, Object, string)"/>
        public static bool CreateSubasset(Type subAssetType, Object asset, out ScriptableObject subAsset, string name = null)
        {
            subAsset = CreateSubasset(subAssetType, asset, name);
            return subAsset != null;
        }

        /// <inheritdoc cref="CreateSubasset(Type, Object, string)"/>
        /// <typeparam name="TSubasset">The type of the subasset to create.</typeparam>
        public static TSubasset CreateSubasset<TSubasset>(Object asset, string name = null)
            where TSubasset : ScriptableObject
        {
            return CreateSubasset(typeof(TSubasset), asset, name) as TSubasset;
        }

        /// <inheritdoc cref="CreateSubasset(Type, Object, out ScriptableObject, string)"/>
        /// <inheritdoc cref="CreateSubasset{TSubasset}(Object, string)"/>
        public static bool CreateSubasset<TSubasset>(Object asset, out TSubasset subAsset, string name = null)
            where TSubasset : ScriptableObject
        {
            subAsset = CreateSubasset(typeof(TSubasset), asset, name) as TSubasset;
            return subAsset != null;
        }

        /// <summary>
        /// Checks if a given object is a sub-asset.
        /// </summary>
        /// <param name="obj">The object you want to check.</param>
        /// <returns>Returns true if the given object is a sub-asset.</returns>
        public static bool IsSubasset(Object obj)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path))
                return false;

            return AssetDatabase.LoadAssetAtPath<Object>(path) != obj;
        }

        /// <summary>
        /// Checks if a given object is a sub-asset of a given asset.
        /// </summary>
        /// <param name="container">The expected main asset.</param>
        /// <returns>Returns true if the given object is a sub-asset of the given container.</returns>
        /// <inheritdoc cref="IsSubasset(Object)"/>
        public static bool IsSubassetOf(Object obj, Object container)
        {
            foreach (Object subAsset in FindAllSubassetsOfType<Object>(container))
            {
                if (subAsset == obj)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Attaches an object to an existing asset, and perform the appropriate operations to avoid inconsistent states and refresh views accordingly.
        /// </summary>
        /// <param name="asset">The asset to which you want to attach the object.</param>
        /// <param name="objectToAttach">The object to attach to the asset. Note that this object must not be an asset of the project, only an instance yet not serialized.</param>
        /// <param name="skipReimport">By default, the modified asset is reimported to save changes on disk. If disabled, you must save and
        /// reimport the asset by yourself.</param>
        /// <returns>Returns true if the object has been attached successfully to the asset.</returns>
        public static bool AttachObject(Object asset, Object objectToAttach, bool skipReimport = false)
        {
            if (!IsAsset(asset))
            {
                Debug.LogWarning("You can't attach an object to another that is not an asset saved in the project.");
                return false;
            }

            if (IsAsset(objectToAttach))
            {
                Debug.LogWarning("You can't attach an asset that is already saved in your project to another. As an alternative, you can attach a copy of the asset, and destroy the original one.");
                return false;
            }

            AssetDatabase.AddObjectToAsset(objectToAttach, asset);

            if (!skipReimport)
                SaveAndReimport(asset);

            return true;
        }

        #endregion


        #region Utilities

        /// <summary>
        /// Gets the GUID string of the given <see cref="Object"/>.<br/>
        /// Note that GUIDs are assigned only to assets, not to scene objects. So if this <see cref="Object"/> is a scene object or an object loaded only in memory, this function will return null.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> whose GUID you want to get.</param>
        /// <returns>Returns the GUID string of this <see cref="Object"/>, or null if this <see cref="Object"/> is not an asset (meaning it's a scene object or an object loaded only i memory).</returns>
        public static string GetGUID(Object obj)
        {
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out string guid, out long localId);
            return localId != 0 ? guid : null;
        }

        /// <summary>
        /// Checks if the given <see cref="Object"/> is an asset (and not a scene object).
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> you want to check.</param>
        /// <returns>Returns true if the given <see cref="Object"/> is an asset.</returns>
        public static bool IsAsset(Object obj)
        {
            return GetGUID(obj) != null;
        }

        /// <summary>
        /// Gets the unique name of an asset to create in the same directory than the given one.
        /// </summary>
        /// <param name="asset">The asset you want to process.</param>
        /// <param name="expectedName">The name you want to set on the asset.</param>
        /// <returns>Returns the processed unique asset name.</returns>
        public static string GetUniqueAssetName(Object asset, string expectedName)
        {
            string path = AssetDatabase.GetAssetPath(asset);
            // Cancel if the given asset is not an asset
            if (string.IsNullOrEmpty(path))
                return null;

            string basePath = System.IO.Path.GetDirectoryName(path);
            string uniquePath = AssetDatabase.GenerateUniqueAssetPath(basePath + System.IO.Path.DirectorySeparatorChar + expectedName + System.IO.Path.GetExtension(path));
            return System.IO.Path.GetFileName(uniquePath.Substring(0, uniquePath.Length - System.IO.Path.GetExtension(path).Length));
        }

        /// <summary>
        /// Gets the main asset of a given one.
        /// </summary>
        /// <param name="asset">The asset from which you want to get the main assset.</param>
        /// <returns>Returns the found main asset, or the given asset itself if it's not a subasset.</returns>
        public static Object GetMainAsset(Object asset)
        {
            return AssetDatabase.LoadAssetAtPath<Object>(GetAssetPath(asset));
        }

        /// <summary>
        /// Reimports the given asset. You should call this function after adding, updating or removing a subasset.
        /// </summary>
        /// <param name="asset">The asset you want to save and reimport.</param>
        /// <returns>Returns true if the given object is truly an asset in the project.</returns>
        public static bool SaveAndReimport(Object asset)
        {
            AssetDatabase.SaveAssets();
            string path = AssetDatabase.GetAssetPath(asset);
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.ImportAsset(path);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Renames a given asset.
        /// </summary>
        /// <remarks>This function also works fine with subassets, unlike <see cref="AssetDatabase.RenameAsset(string, string)"/>, which only works with main assets.</remarks>
        /// <param name="obj">The asset to rename. If the given object is not an asset saved on disk, this function still renames it but it won't save it on disk.</param>
        /// <param name="name">The new name of the asset.</param>
        /// <inheritdoc cref="AttachObject(Object, Object, bool)"/>
        public static void Rename(Object obj, string name, bool skipReimport = false)
        {
            // If the given object is not an asset saved on disk
            if (!IsAsset(obj))
            {
                obj.name = name;
            }
            // Else, if the given object is a subasset
            else if (obj.IsSubasset())
            {
                obj.name = name;
                if (!skipReimport)
                    SaveAndReimport(obj);
            }
            // Else, if the given object is a main asset
            else
            {
                string error = AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(obj), name);
                if (!string.IsNullOrEmpty(error))
                    Debug.LogWarning("Failed to rename asset: " + error);
            }
        }

        #endregion

    }

}