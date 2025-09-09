using System;
using System.Collections.Generic;

using UnityEngine;

using Object = UnityEngine.Object;

namespace SideXP.Core
{

    /// <summary>
    /// Miscellaneous functions for working with <see cref="Transform"/> component.
    /// </summary>
    public static class TransformUtility
    {

        /// <summary>
        /// Performs an operation for each child of a given <see cref="Transform"/> (excluding itself).
        /// </summary>
        /// <param name="action">The operation to perform for each child.</param>
        /// <inheritdoc cref="GetChildren(Transform, bool)"/>
        public static void ForEachChild(Transform transform, Action<Transform> action, bool recursive = false)
        {
            foreach (Transform child in GetChildren(transform, recursive))
                action(child);
        }

        /// <summary>
        /// Gets all the children of a given <see cref="Transform"/> (excluding itself).
        /// </summary>
        /// <param name="recursive">By default, this function processes only direct children. If enabled, gets the children recursively in the hierarchy.</param>
        /// <param name="transform">The <see cref="Transform"/> component to process.</param>
        /// <returns>Returns the found children.</returns>
        public static Transform[] GetChildren(Transform transform, bool recursive = false)
        {
            List<Transform> children = new List<Transform>();

            // Finds children recursively if required
            if (recursive)
            {
                children.AddRange(transform.GetComponentsInChildren<Transform>(true));
                children.Remove(transform);
            }
            // Else, get only direct children
            else
            {
                foreach (Transform child in transform)
                {
                    if (child != transform)
                        children.Add(child);
                }
            }

            return children.ToArray();
        }

        /// <summary>
        /// Destroy all transform in the given one's hierarchy.
        /// </summary>
        /// <inheritdoc cref="ForEachChild(Transform, Action{Transform}, bool)"/>
        public static void ClearHierarchy(Transform transform)
        {
            ForEachChild(transform, child =>
            {
                if (Application.isPlaying)
                    Object.Destroy(child.gameObject);
                else
                    Object.DestroyImmediate(child.gameObject);
            });
        }

        /// <summary>
        /// Removes all children in the hierarchy if they have a given component attached.
        /// </summary>
        /// <param name="transform">The transform of which to clean the hierarchy.</param>
        /// <param name="componentType">The component type of the children to remove.</param>
        /// <returns>Returns the number of removed children.</returns>
        public static int RemoveChildrenOfType(Transform transform, Type componentType)
        {
            int count = 0;
            ForEachChild(transform, child =>
            {
                if (child.TryGetComponent(componentType, out Component _))
                {
                    Object.Destroy(child.gameObject);
                    count++;
                }
            });
            return count;
        }

        /// <typeparam name="T">The component type of the children to remove.</typeparam>
        /// <inheritdoc cref="RemoveChildrenOfType(Transform, Type)"/>
        public static int RemoveChildrenOfType<T>(Transform transform)
            where T : Component
        {
            return RemoveChildrenOfType(transform, typeof(T));
        }

        /// <summary>
        /// Reuse existing instances of a given type of component from a given conntainer, or create new instances to match a given count.
        /// <example>
        /// <code>
        /// Transform container = GetUIContainer();
        /// UI_Item uiItemPrefab = GetUIItemPrefab();
        /// 
        /// container.Pool(5,
        ///     uiContainer =>
        ///     {
        ///         GameObject uiItem = Instantiate(GetUIItemPrefab().gameObject, uiContainer, false);
        ///         return uiItem.GetComponent{UI_Item}();
        ///     },
        ///     (uiItem, index) =>
        ///     {
        ///         uiItem.SetIndex(index);
        ///     });
        /// </code>
        /// </example>
        /// </summary>
        /// <typeparam name="T">The type of the objects to find or instantiate in the container.</typeparam>
        /// <param name="container">The object that contains the pooled objects, or which will contain the created instances.</param>
        /// <param name="expectedCount">The expected number of objects in the container.</param>
        /// <param name="onInstantiate">Called when a new object instance is required.</param>
        /// <param name="onInit">Called when a pooled or a created instance is reused. First parameter is the reused instance, second parameter
        /// is its index in the container.</param>
        /// <param name="onDiscard">Called when an existing instance won't be reused. First parameter is the instance, seconds parameter is its
        /// index in the container.</param>
        public static void Pool<T>(Transform container, int expectedCount, Func<Transform, T> onInstantiate, Action<T, int> onInit, Action<T, int> onDiscard = null)
            where T : Component
        {
            T[] existingInstances = container.GetComponentsInChildren<T>(true, true);
            int count = Mathf.Max(0, expectedCount, existingInstances.Length);
            for (int i = 0; i < count; i++)
            {
                if (i < expectedCount)
                {
                    T instance = i < existingInstances.Length
                        ? existingInstances[i]
                        : onInstantiate(container);

                    onInit(instance, i);
                    instance.gameObject.SetActive(true);
                }
                else
                {
                    if (onDiscard != null)
                        onDiscard(existingInstances[i], i);

                    existingInstances[i].gameObject.SetActive(false);
                }
            }
        }

    }

}