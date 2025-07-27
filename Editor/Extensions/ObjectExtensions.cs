using System;

using UnityEngine;

using Object = UnityEngine.Object;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Miscellaneous extension functions for <see cref="Object"/> and assets.
    /// </summary>
    public static class ObjectExtensions
    {

        /// <inheritdoc cref="ObjectUtility.GetGUID(Object)"/>
        public static string GetGUID(this Object obj)
        {
            return ObjectUtility.GetGUID(obj);
        }

        /// <inheritdoc cref="ObjectUtility.IsAsset(Object)"/>
        public static bool IsAsset(this Object obj)
        {
            return ObjectUtility.IsAsset(obj);
        }

        /// <inheritdoc cref="ObjectUtility.GetUniqueAssetName(Object, string)"/>
        public static string GetUniqueAssetName(this Object asset, string expectedName)
        {
            return ObjectUtility.GetUniqueAssetName(asset, expectedName);
        }

        /// <inheritdoc cref="ObjectUtility.GetAssetPath(Object)"/>
        public static string GetAssetPath(this Object asset)
        {
            return ObjectUtility.GetAssetPath(asset);
        }

        /// <inheritdoc cref="ObjectUtility.GetAbsoluteAssetPath(Object)"/>
        public static string GetAbsoluteAssetPath(this Object asset)
        {
            return ObjectUtility.GetAbsoluteAssetPath(asset);
        }

        /// <inheritdoc cref="ObjectUtility.FindSubassetOfType(Object, Type, bool)"/>
        public static Object FindSubassetOfType(this Object asset, Type type, bool includeMainAsset = false)
        {
            return ObjectUtility.FindSubassetOfType(asset, type, includeMainAsset);
        }

        /// <inheritdoc cref="ObjectUtility.FindSubassetOfType{T}(Object, bool)"/>
        public static T FindSubassetOfType<T>(this Object asset, bool includeMainAsset = false)
            where T : Object
        {
            return ObjectUtility.FindSubassetOfType<T>(asset, includeMainAsset);
        }

        /// <inheritdoc cref="ObjectUtility.FindAllSubassetsOfType(Object, Type, bool)"/>
        public static Object[] FindAllSubassetsOfType(this Object asset, Type type, bool includeMainAsset = false)
        {
            return ObjectUtility.FindAllSubassetsOfType(asset, type, includeMainAsset);
        }

        /// <inheritdoc cref="ObjectUtility.FindAllSubassetsOfType{T}(Object, bool)"/>
        public static T[] FindAllSubassetsOfType<T>(this Object asset, bool includeMainAsset = false)
            where T : Object
        {
            return ObjectUtility.FindAllSubassetsOfType<T>(asset, includeMainAsset);
        }

        /// <inheritdoc cref="ObjectUtility.GetMainAsset(Object)"/>
        public static Object GetMainAsset(this Object asset)
        {
            return ObjectUtility.GetMainAsset(asset);
        }

        /// <inheritdoc cref="ObjectUtility.FindAssetInHierarchy(Object, Type)"/>
        public static Object FindAssetInHierarchy(this Object asset, Type assetType)
        {
            return ObjectUtility.FindAssetInHierarchy(asset, assetType);
        }

        /// <inheritdoc cref="ObjectUtility.FindAssetInHierarchy(Object, Type, out Object)"/>
        public static bool FindAssetInHierarchy(this Object asset, Type assetType, out Object foundAsset)
        {
            return ObjectUtility.FindAssetInHierarchy(asset, assetType, out foundAsset);
        }

        /// <inheritdoc cref="ObjectUtility.FindAssetInHierarchy{T}(Object)"/>
        public static T FindAssetInHierarchy<T>(this Object asset)
            where T : Object
        {
            return ObjectUtility.FindAssetInHierarchy<T>(asset);
        }

        /// <inheritdoc cref="ObjectUtility.FindAssetInHierarchy{T}(Object, out T)"/>
        public static bool FindAssetInHierarchy<T>(this Object asset, out T foundAsset)
            where T : Object
        {
            return ObjectUtility.FindAssetInHierarchy(asset, out foundAsset);
        }

        /// <inheritdoc cref="ObjectUtility.AttachObject(Object, Object, bool)"/>
        public static bool AttachObject(this Object asset, Object objectToAttach, bool skipReimport = false)
        {
            return ObjectUtility.AttachObject(asset, objectToAttach, skipReimport);
        }

        /// <inheritdoc cref="ObjectUtility.CreateSubasset(Type, Object, string)"/>
        public static ScriptableObject CreateSubasset(this Object asset, Type subAssetType, string name = null)
        {
            return ObjectUtility.CreateSubasset(subAssetType, asset, name);
        }

        /// <inheritdoc cref="ObjectUtility.CreateSubasset(Type, Object, out ScriptableObject, string)"/>
        public static bool CreateSubasset(this Object asset, Type subAssetType, out ScriptableObject subAsset, string name = null)
        {
            return ObjectUtility.CreateSubasset(subAssetType, asset, out subAsset, name);
        }

        /// <inheritdoc cref="ObjectUtility.CreateSubasset{TSubasset}(Object, string)"/>
        public static TSubasset CreateSubasset<TSubasset>(this Object asset, string name = null)
            where TSubasset : ScriptableObject
        {
            return ObjectUtility.CreateSubasset<TSubasset>(asset, name);
        }

        /// <inheritdoc cref="ObjectUtility.CreateSubasset{TSubasset}(Object, out TSubasset, string)"/>
        public static bool CreateSubasset<TSubasset>(this Object asset, out TSubasset subAsset, string name = null)
            where TSubasset : ScriptableObject
        {
            return ObjectUtility.CreateSubasset(asset, out subAsset, name);
        }

        /// <inheritdoc cref="ObjectUtility.IsSubasset(Object)"/>
        public static bool IsSubasset(this Object obj)
        {
            return ObjectUtility.IsSubasset(obj);
        }

        /// <inheritdoc cref="ObjectUtility.IsSubassetOf(Object, Object)"/>
        public static bool IsSubassetOf(this Object obj, Object container)
        {
            return ObjectUtility.IsSubassetOf(obj, container);
        }

        /// <inheritdoc cref="ObjectUtility.SaveAndReimport(Object)"/>
        public static bool SaveAndReimport(this Object asset)
        {
            return ObjectUtility.SaveAndReimport(asset);
        }

        /// <inheritdoc cref="ObjectUtility.Rename(Object, string, bool)"/>
        public static void Rename(this Object obj, string name, bool skipReimport = false)
        {
            ObjectUtility.Rename(obj, name, skipReimport);
        }

    }

}