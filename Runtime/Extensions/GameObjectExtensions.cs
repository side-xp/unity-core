using System;
using System.Collections.Generic;

using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Extension functions for <see cref="GameObject"/> instances.
    /// </summary>
    public static class GameObjectExtensions
    {

        /// <summary>
        /// Gets the children from a given Game Object (excluding itself).
        /// </summary>
        /// <param name="gameObject">The Game Object from which you want to get the children (excluding itself).</param>
        /// <returns>Returns the found children.</returns>
        /// <inheritdoc cref="TransformUtility.GetChildren(Transform, bool)"/>
        public static GameObject[] GetChildren(this GameObject gameObject, bool recursive = false)
        {
            List<GameObject> children = new List<GameObject>();
            foreach (Transform child in gameObject.transform.GetChildren(recursive))
                children.Add(child.gameObject);
            return children.ToArray();
        }

        /// <inheritdoc cref="RuntimeObjectUtility.IsPrefab(GameObject)"/>
        public static bool IsPrefab(this GameObject gameObject)
        {
            return RuntimeObjectUtility.IsPrefab(gameObject);
        }

        /// <returns>Returns the object bounds.</returns>
        /// <inheritdoc cref="GetBounds(GameObject, out Bounds, bool, bool)"/>
        public static Bounds GetBounds(this GameObject gameObject, bool preferCollider = false, bool includeChildren = true)
        {
            return GetBounds(gameObject, out Bounds bounds, preferCollider, includeChildren) ? bounds : default;
        }

        /// <summary>
        /// Gets the bounds of an object, using its renderer or collider.
        /// </summary>
        /// <param name="gameObject">The object to get the bounds.</param>
        /// <param name="bounds">Outputs the object bounds.</param>
        /// <param name="preferCollider">If enabled, try to get the object bounds from the collider first, instead of the renderer
        /// first.</param>
        /// <param name="includeChildren">If enabled and the given object doesn't have a component that may contain bounds, this function
        /// will search for that component in children.</param>
        /// <returns>Returns true if the object bounds have been found.</returns>
        public static bool GetBounds(this GameObject gameObject, out Bounds bounds, bool preferCollider = false, bool includeChildren = true)
        {
            if (preferCollider)
            {
                if (gameObject.TryGetComponent(out Collider collider) && ColliderExtensions.GetColliderBounds(collider, out bounds))
                    return true;
                else if (gameObject.TryGetComponent(out Collider2D collider2D) && Collider2DExtensions.GetColliderBounds(collider2D, out bounds))
                    return true;
                else if (gameObject.TryGetComponent(out Renderer renderer) && RendererExtensions.GetRendererBounds(renderer, out bounds))
                    return true;
            }
            else
            {
                if (gameObject.TryGetComponent(out Renderer renderer) && RendererExtensions.GetRendererBounds(renderer, out bounds))
                    return true;
                else if (gameObject.TryGetComponent(out Collider collider) && ColliderExtensions.GetColliderBounds(collider, out bounds))
                    return true;
                else if (gameObject.TryGetComponent(out Collider2D collider2D) && Collider2DExtensions.GetColliderBounds(collider2D, out bounds))
                    return true;
            }

            if (includeChildren)
            {
                if (preferCollider)
                {
                    Collider collider = gameObject.GetComponentInChildren<Collider>();
                    if (collider != null && ColliderExtensions.GetColliderBounds(collider, out bounds))
                        return true;

                    Collider2D collider2D = gameObject.GetComponentInChildren<Collider2D>();
                    if (collider2D != null && Collider2DExtensions.GetColliderBounds(collider2D, out bounds))
                        return true;

                    Renderer renderer = gameObject.GetComponentInChildren<Renderer>();
                    if (renderer != null && RendererExtensions.GetRendererBounds(renderer, out bounds))
                        return true;
                }
               else
                {
                    Renderer renderer = gameObject.GetComponentInChildren<Renderer>();
                    if (renderer != null && RendererExtensions.GetRendererBounds(renderer, out bounds))
                        return true;

                    Collider collider = gameObject.GetComponentInChildren<Collider>();
                    if (collider != null && ColliderExtensions.GetColliderBounds(collider, out bounds))
                        return true;

                    Collider2D collider2D = gameObject.GetComponentInChildren<Collider2D>();
                    if (collider2D != null && Collider2DExtensions.GetColliderBounds(collider2D, out bounds))
                        return true;
                }
            }

            bounds = default;
            return false;
        }

        /// <inheritdoc cref="GameObject.GetComponentInParent(Type, bool)"/>
        /// <param name="component">The component from which to get the target one.</param>
        /// <param name="componentType"><inheritdoc cref="GameObject.GetComponentInParent(Type, bool)" path="/param[@name='t']"/></param>
        /// <param name="output">Outputs the found component of the given type.</param>
        /// <returns>Returns true if a component of the expected type has been found on the given Game Object, or its parent.</returns>
        public static bool TryGetComponentInParent(this GameObject component, Type componentType, out Component output, bool includeInactive)
        {
#if UNITY_2020_3_OR_NEWER
            output = component.GetComponentInParent(componentType, includeInactive);
#else
            output = component.GetComponentInParent(componentType);
#endif
            return output != null;
        }

        /// <inheritdoc cref="TryGetComponentInParent(GameObject, Type, out Component, bool)"/>
        public static bool TryGetComponentInParent(this GameObject component, Type componentType, out Component output)
        {
            return TryGetComponentInParent(component, componentType, out output, false);
        }

        /// <inheritdoc cref="Component.GetComponentInParent{T}(bool)"/>
        /// <inheritdoc cref="TryGetComponentInParent(GameObject, Type, out Component, bool)"/>
        public static bool TryGetComponentInParent<T>(this GameObject component, out T output, bool includeInactive)
            where T : Component
        {
#if UNITY_2020_3_OR_NEWER
            output = component.GetComponentInParent<T>(includeInactive);
#else
            output = component.GetComponentInParent<T>();
#endif
            return output != null;
        }

        /// <inheritdoc cref="TryGetComponentInParent{T}(GameObject, out T, bool)"/>
        public static bool TryGetComponentInParent<T>(this GameObject component, out T output)
            where T : Component
        {
            return TryGetComponentInParent(component, out output, false);
        }

        /// <inheritdoc cref="Component.GetComponentInChildren(Type, bool)"/>
        /// <param name="component">The component from which to get the target one.</param>
        /// <param name="componentType"><inheritdoc cref="Component.GetComponentInChildren(Type, bool)" path="/param[@name='t']"/></param>
        /// <param name="output">Outputs the found component of the given type.</param>
        /// <returns>Returns true if a component of the expected type has been found on the given Game Object, or its children.</returns>
        public static bool TryGetComponentInChildren(this GameObject component, Type componentType, out Component output, bool includeInactive)
        {
            output = component.GetComponentInChildren(componentType, includeInactive);
            return output != null;
        }

        /// <inheritdoc cref="TryGetComponentInChildren(GameObject, Type, out Component, bool)"/>
        public static bool TryGetComponentInChildren(this GameObject component, Type componentType, out Component output)
        {
            return TryGetComponentInChildren(component, componentType, out output, false);
        }

        /// <inheritdoc cref="Component.GetComponentInChildren{T}(bool)"/>
        /// <inheritdoc cref="TryGetComponentInChildren(GameObject, Type, out Component, bool)"/>
        public static bool TryGetComponentInChildren<T>(this GameObject component, out T output, bool includeInactive)
            where T : Component
        {
            output = component.GetComponentInChildren<T>(includeInactive);
            return output != null;
        }

        /// <inheritdoc cref="TryGetComponentInChildren{T}(GameObject, out T, bool)"/>
        public static bool TryGetComponentInChildren<T>(this GameObject component, out T output)
            where T : Component
        {
            return TryGetComponentInChildren(component, out output, false);
        }

    }

}