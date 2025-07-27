using System;
using System.Collections.Generic;

using UnityEngine;

using Object = UnityEngine.Object;

namespace SideXP.Core
{

    /// <summary>
    /// Miscellaneous functions for working with <see cref="Object"/>s and assets at runtime.<br/>
    /// </summary>
    public static class RuntimeObjectUtility
    {

        #region Delegates

        /// <summary>
        /// Called when an asset is double-clicked in the editor.
        /// </summary>
        /// <param name="asset">The asset to open.</param>
        /// <param name="line">The line at which the file should be opened.</param>
        /// <returns>Returns true if the asset has been opened successfully (and so other registered callbacks won't be invoked.</returns>
        public delegate bool OpenAssetDelegate(Object asset, int line);

        #endregion


        #region Fields

        /// <summary>
        /// The list of all the registered handlers for an asset being opened.
        /// </summary>
        private static List<OpenAssetDelegate> s_openAssetHandlers = new List<OpenAssetDelegate>();

        #endregion


        #region Utilities

        /// <summary>
        /// Gets the <see cref="Transform"/> component of an object that can be placed in a scene.
        /// </summary>
        /// <param name="obj">The object from which you want to get the <see cref="Transform"/> component.</param>
        /// <returns>Returns the found <see cref="Transform"/> component.</returns>
        public static Transform GetTransform(Object obj)
        {
            if (obj is GameObject go)
            {
                return go.transform;
            }
            else if (obj is Component comp)
            {
                return comp.transform;
            }
            else
            {
                return null;
            }
        }

        /// <inheritdoc cref="GetTransform(Object)"/>
        /// <param name="transform">Outputs the found <see cref="Transform"/> component.</param>
        /// <returns>Returns true if the given object has a <see cref="Transform"/> component.</returns>
        public static bool GetTransform(Object obj, out Transform transform)
        {
            transform = GetTransform(obj);
            return transform != null;
        }

        /// <summary>
        /// Gets the <see cref="GameObject"/> to which the given object is attached.
        /// </summary>
        /// <param name="obj">The object of which you want to get the attached <see cref="GameObject"/>.</param>
        /// <returns>Returns the found attached <see cref="GameObject"/>.</returns>
        public static GameObject GetGameObject(Object obj)
        {
            if (obj is GameObject go)
            {
                return go;
            }
            else if (obj is Component comp)
            {
                return comp.gameObject;
            }
            else
            {
                return null;
            }
        }

        /// <inheritdoc cref="GetGameObject(Object)"/>
        /// <param name="gameObject">Outputs the found attached <see cref="GameObject"/>.</param>
        /// <returns>Returns true if the given object is attached to a <see cref="GameObject"/>.</returns>
        public static bool GetGameObject(Object obj, out GameObject gameObject)
        {
            gameObject = GetGameObject(obj);
            return gameObject != null;
        }

        /// <summary>
        /// Checks if a <see cref="GameObject"/> is a prefab at runtime.
        /// </summary>
        /// <param name="gameObject">The <see cref="GameObject"/> to check.</param>
        /// <returns>Returns true if the given object is a prefab.</returns>
        public static bool IsPrefab(GameObject gameObject)
        {
            return gameObject.scene.name == null;
        }

        /// <summary>
        /// Finds an <see cref="UnityEngine.Object"/>.
        /// </summary>
        /// <param name="origin">The object from which the expected one is queried.</param>
        /// <param name="type">The type of the expected object.</param>
        /// <param name="name">If provided, this function will consider only the objects with that exact name.</param>
        /// <param name="strategy">The strategy to use to find the expected object.</param>
        /// <returns>Returns the found object.</returns>
        /// /// <remarks>
        /// By default the strategy <see cref="FFindObjectStrategy.None"/> is used. This means the reference is queried using the following
        /// rules.<br/><br/>
        /// If this attribute is used inside a <see cref="UnityEngine.MonoBehaviour"/> script:<br/>
        /// - If the decorated property is of type <see cref="UnityEngine.GameObject"/> or <see cref="UnityEngine.Component"/>, the
        /// reference will be queried in the following order: this game object, its direct children, its direct parent, or the whole
        /// scene.<br/>
        /// - Else, in any other case, the reference will be queried from the whole scene.<br/><br/>
        /// If this attribute is used inside a <see cref="UnityEngine.ScriptableObject"/> script, the reference will be queried from
        /// subassets, or the loaded resources.<br/>
        /// This default behavior is equivalent as using the value
        /// <see cref="FFindObjectStrategy.DirectHierarchy"/>|<see cref="FFindObjectStrategy.Scene"/>.<br/>
        /// <br/>
        /// Special case: if <paramref name="type"/> is <see cref="UnityEngine.GameObject"/> or <see cref="UnityEngine.Component"/> and this
        /// function tries to find the object in the scene (using <see cref="FFindObjectStrategy.Scene"/>, the name is in fact the expected
        /// tag name of the objects. This is counter-intuitive, but using tags in Unity is the expected way to "group" game objects, instead
        /// of naming them the same.
        /// </remarks>
        public static Object FindObjectFrom(object origin, Type type, string name = null, FFindObjectStrategy strategy = FFindObjectStrategy.None)
        {
            // Cancel if the expected object type is not an Object
            if (!typeof(Object).IsAssignableFrom(type))
            {
                Debug.LogWarning($"Invalid object type {type}. Only objects that inherit from {nameof(UnityEngine)}.{nameof(Object)} can be found using {nameof(RuntimeObjectUtility)}.{nameof(FindObjectFrom)}()");
                return null;
            }

            // Use default strategy if undefined
            if (strategy == FFindObjectStrategy.None)
                strategy = FFindObjectStrategy.DirectHierarchy | FFindObjectStrategy.Scene;

            // If the expected type is Component or GameObject
            if (typeof(Component).IsAssignableFrom(type) || typeof(GameObject).IsAssignableFrom(type))
            {
                // Query origin GameObject
                GameObject originGameObject = origin as GameObject;
                if (originGameObject == null)
                {
                    if (origin is Component originComponent)
                        originGameObject = originComponent.gameObject;
                }

                // If the origin is a GameObject
                if (originGameObject != null)
                {
                    // If the strategy targets "self"
                    if (strategy.HasFlag(FFindObjectStrategy.Self))
                    {
                        // If no name is defined or the origin object has the expected name
                        if (string.IsNullOrEmpty(name) || originGameObject.name == name)
                        {
                            // If the expected object type is a Component, try to get the component from the origin
                            if (typeof(Component).IsAssignableFrom(type))
                            {
                                if (originGameObject.TryGetComponent(type, out Component expectedComponent))
                                    return expectedComponent;
                            }
                            // Else (if the expected type is a GameObject), return self
                            else
                            {
                                return originGameObject;
                            }
                        }
                    }

                    // If the strategy targets "children"
                    if (strategy.HasFlag(FFindObjectStrategy.Children))
                    {
                        GameObject[] children = originGameObject.GetChildren(strategy.HasFlag(FFindObjectStrategy.Recursive));
                        foreach (GameObject child in children)
                        {
                            // If no name is defined or the current child has the expected name
                            if (string.IsNullOrEmpty(name) || child.name == name)
                            {
                                // If the expected object type is a Component, try to get the component from the current child
                                if (typeof(Component).IsAssignableFrom(type))
                                {
                                    if (child.TryGetComponent(type, out Component expectedComponent))
                                        return expectedComponent;
                                }
                                // Else (if the expected type is a GameObject), return the current child
                                else
                                {
                                    return child;
                                }
                            }
                        }
                    }

                    // If the strategy targets "parent"
                    if (strategy.HasFlag(FFindObjectStrategy.Parent))
                    {
                        Transform parent = originGameObject.transform;
                        do
                        {
                            parent = parent.parent;
                            if (parent != null)
                            {
                                // If no name is defined or the current child has the expected name
                                if (string.IsNullOrEmpty(name) || parent.name == name)
                                {
                                    // If the expected object type is a Component, try to get the component from the current child
                                    if (typeof(Component).IsAssignableFrom(type))
                                    {
                                        if (parent.TryGetComponent(type, out Component expectedComponent))
                                            return expectedComponent;
                                    }
                                    // Else if the expected type is a GameObject, return the current parent's Game Object
                                    else
                                    {
                                        return parent.gameObject;
                                    }
                                }
                            }
                        }
                        while (parent != null && strategy.HasFlag(FFindObjectStrategy.Recursive));
                    }
                }

                // If the strategy targets "scene" (and so no need to start searching from a specific object)
                if (strategy.HasFlag(FFindObjectStrategy.Scene))
                {
                    // If no name is defined and the expected object is a specifically a component, simply use Object.FindObjectOfType()
                    if (string.IsNullOrEmpty(name) && typeof(Component).IsAssignableFrom(type))
#if UNITY_6000
                        return Object.FindFirstObjectByType(type);
#else
                        return Object.FindObjectOfType(type);
#endif

                    // For each Game Object in the scene with the given tag
                    foreach (GameObject obj in GameObject.FindGameObjectsWithTag(name))
                    {
                        // If no name is defined or the current object has the expected name
                        if (string.IsNullOrEmpty(name) || obj.name == name)
                        {
                            // If the expected object type is a Component, try to get the component from the current object
                            if (typeof(Component).IsAssignableFrom(type))
                            {
                                if (obj.TryGetComponent(type, out Component expectedComponent))
                                    return expectedComponent;
                            }
                            // Else (if the expected type is a GameObject), return the current object
                            else
                            {
                                return obj;
                            }
                        }
                    }
                }
            }

            // Else (if the expected type is not a Component or a GameObject)
            else
            {
                Object[] objs = Resources.FindObjectsOfTypeAll(type);
                
                // If no name is defined and, return the first object found with the expected type
                if (string.IsNullOrEmpty(name) && objs.Length > 0)
                    return objs[0];

                // Get the first object in the list with the expected name
                foreach (Object obj in objs)
                {
                    if (obj.name == name)
                        return obj;
                }
            }

            return null;
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


        #region Runtime Callbacks

        /// <summary>
        /// Invokes registered <see cref="OpenAssetDelegate"/> handlers to check if one of them can open the asset.<br/>
        /// WARNING: Since this function resolves the asset by using <see cref="UnityEditor.EditorUtility.InstanceIDToObject(int)"/>, it
        /// won't do anything outside the editor context.
        /// </summary>
        /// <remarks>
        /// This is mostly meant to use <see cref="UnityEditor.Callbacks.OnOpenAssetAttribute"/> without having to implement editor code in
        /// your assets, and allowing the custom editors to handle this event easily.
        /// </remarks>
        /// <param name="instanceId">The instance id of the asset being opened.</param>
        /// <param name="line">The target line in the file being opened.</param>
        /// <returns>Returns true if the asset can be opened.</returns>
        public static bool TryOpenAsset(int instanceId, int line)
        {
#if UNITY_EDITOR
            Object asset = UnityEditor.EditorUtility.InstanceIDToObject(instanceId);
            foreach (OpenAssetDelegate handler in s_openAssetHandlers)
            {
                if (handler.Invoke(asset, line))
                    return true;
            }
#endif
            return false;
        }

        /// <summary>
        /// Registers a handler for an "open asset" operation.
        /// </summary>
        /// <remarks>
        /// <inheritdoc cref="TryOpenAsset(int, int)"/>
        /// </remarks>
        /// <param name="handler">The handler of this operation.</param>
        public static void AddOpenAssetHandler(OpenAssetDelegate handler)
        {
            s_openAssetHandlers.Add(handler);
        }

        #endregion

    }

}