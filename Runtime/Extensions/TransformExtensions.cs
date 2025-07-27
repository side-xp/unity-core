using System;

using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Extension functions for <see cref="Transform"/> instances.
    /// </summary>
    public static class TransformExtensions
    {

        /// <inheritdoc cref="TransformUtility.ForEachChild(Transform, Action{Transform}, bool)"/>
        public static void ForEachChild(this Transform transform, Action<Transform> action, bool recursive = false)
        {
            TransformUtility.ForEachChild(transform, action, recursive);
        }

        /// <inheritdoc cref="TransformUtility.GetChildren(Transform, bool)"/>
        public static Transform[] GetChildren(this Transform transform, bool recursive = false)
        {
            return TransformUtility.GetChildren(transform, recursive);
        }

        /// <inheritdoc cref="TransformUtility.ClearHierarchy(Transform)"/>
        public static void ClearHierarchy(this Transform transform)
        {
            TransformUtility.ClearHierarchy(transform);
        }

        /// <inheritdoc cref="TransformUtility.RemoveChildrenOfType(Transform, Type)"/>
        public static int RemoveChildrenOfType(this Transform transform, Type componentType)
        {
            return TransformUtility.RemoveChildrenOfType(transform, componentType);
        }

        /// <inheritdoc cref="TransformUtility.RemoveChildrenOfType{T}(Transform)"/>
        public static int RemoveChildrenOfType<T>(this Transform transform)
            where T : Component
        {
            return TransformUtility.RemoveChildrenOfType<T>(transform);
        }

        /// <inheritdoc cref="TransformUtility.Pool{T}(Transform, int, Func{Transform, T}, Action{T, int}, Action{T, int})"/>
        public static void Pool<T>(this Transform container, int expectedCount, Func<Transform, T> onInstantiate, Action<T, int> onInit, Action<T, int> onDiscard = null)
            where T : Component
        {
            TransformUtility.Pool(container, expectedCount, onInstantiate, onInit, onDiscard);
        }

    }

}