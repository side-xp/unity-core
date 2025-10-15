using System;

using UnityEngine;

using Object = UnityEngine.Object;

namespace SideXP.Core
{

    /// <summary>
    /// Extension functions for <see cref="Component"/> objects.
    /// </summary>
    public static class ComponentExtensions
    {

        /// <summary>
        /// Creates an instance of the <see cref="GameObject"/> to which the component is attached.
        /// </summary>
        /// <typeparam name="T">The type of the attached component.</typeparam>
        /// <param name="component">The attached component.</param>
        /// <param name="gameObject">Outputs the new <see cref="GameObject"/> instance.</param>
        /// <returns>Returns the component</returns>
        /// <inheritdoc cref="Object.Instantiate(Object, Vector3, Quaternion, Transform)"/>
        public static T InstantiateGameObject<T>(this T component, Vector3 position, Quaternion rotation, Transform parent, out GameObject gameObject)
            where T : Component
        {
            gameObject = Object.Instantiate(component.gameObject, position, rotation, parent);
            return gameObject.GetComponent<T>();
        }

        /// <inheritdoc cref="InstantiateGameObject{T}(T, Vector3, Quaternion, Transform, out GameObject)"/>
        public static T InstantiateGameObject<T>(this T component, Vector3 position, Quaternion rotation, Transform parentt)
            where T : Component
        {
            return InstantiateGameObject(component, position, rotation, parentt, out _);
        }

        /// <inheritdoc cref="InstantiateGameObject{T}(T, Vector3, Quaternion, Transform, out GameObject)"/>
        /// <inheritdoc cref="Object.Instantiate(Object, Transform, bool)"/>
        public static T InstantiateGameObject<T>(this T component, Transform parent, out GameObject gameObject, bool instantiateInWorldSpace = false)
            where T : Component
        {
            gameObject = Object.Instantiate(component.gameObject, parent, instantiateInWorldSpace);
            return gameObject.GetComponent<T>();
        }

        /// <inheritdoc cref="InstantiateGameObject{T}(T, Transform, out GameObject, bool)"/>
        public static T InstantiateGameObject<T>(this T component, Transform parent, bool instantiateInWorldSpace = false)
            where T : Component
        {
            return InstantiateGameObject(component, parent, out _, instantiateInWorldSpace);
        }

        /// <inheritdoc cref="InstantiateGameObject{T}(T, Vector3, Quaternion, Transform, out GameObject)"/>
        /// <inheritdoc cref="Object.Instantiate(Object)"/>
        public static T InstantiateGameObject<T>(this T component, out GameObject gameObject)
            where T : Component
        {
            gameObject = Object.Instantiate(component.gameObject);
            return gameObject.GetComponent<T>();
        }

        /// <inheritdoc cref="InstantiateGameObject{T}(T, out GameObject)"/>
        public static T InstantiateGameObject<T>(this T component)
            where T : Component
        {
            return InstantiateGameObject(component, out _);
        }

        /// <inheritdoc cref="Component.GetComponentInParent(Type, bool)"/>
        /// <param name="component">The component from which to get the target one.</param>
        /// <param name="componentType"><inheritdoc cref="Component.GetComponentInParent(Type, bool)" path="/param[@name='t']"/></param>
        /// <param name="output">Outputs the found component of the given type.</param>
        /// <returns>Returns true if a component of the expected type has been found on the given component's Game Object, or its
        /// parent.</returns>
        public static bool TryGetComponentInParent(this Component component, Type componentType, out Component output, bool includeInactive)
        {
#if UNITY_2020_3_OR_NEWER
            output = component.GetComponentInParent(componentType, includeInactive);
#else
            output = component.GetComponentInParent(componentType);
#endif
            return output != null;
        }

        /// <inheritdoc cref="TryGetComponentInParent(Component, Type, out Component, bool)"/>
        public static bool TryGetComponentInParent(this Component component, Type componentType, out Component output)
        {
            return TryGetComponentInParent(component, componentType, out output, false);
        }

        /// <inheritdoc cref="Component.GetComponentInParent{T}(bool)"/>
        /// <inheritdoc cref="TryGetComponentInParent(Component, Type, out Component, bool)"/>
        public static bool TryGetComponentInParent<T>(this Component component, out T output, bool includeInactive)
            where T : Component
        {
#if UNITY_2020_3_OR_NEWER
            output = component.GetComponentInParent<T>(includeInactive);
#else
            output = component.GetComponentInParent<T>();
#endif
            return output != null;
        }

        /// <inheritdoc cref="TryGetComponentInParent{T}(Component, out T, bool)"/>
        public static bool TryGetComponentInParent<T>(this Component component, out T output)
            where T : Component
        {
            return TryGetComponentInParent(component, out output, false);
        }

        /// <inheritdoc cref="Component.GetComponentInChildren(Type, bool)"/>
        /// <param name="component">The component from which to get the target one.</param>
        /// <param name="componentType"><inheritdoc cref="Component.GetComponentInChildren(Type, bool)" path="/param[@name='t']"/></param>
        /// <param name="output">Outputs the found component of the given type.</param>
        /// <returns>Returns true if a component of the expected type has been found on the given component's Game Object, or its
        /// children.</returns>
        public static bool TryGetComponentInChildren(this Component component, Type componentType, out Component output, bool includeInactive)
        {
            output = component.GetComponentInChildren(componentType, includeInactive);
            return output != null;
        }

        /// <inheritdoc cref="TryGetComponentInChildren(Component, Type, out Component, bool)"/>
        public static bool TryGetComponentInChildren(this Component component, Type componentType, out Component output)
        {
            return TryGetComponentInChildren(component, componentType, out output, false);
        }

        /// <inheritdoc cref="Component.GetComponentInChildren{T}(bool)"/>
        /// <inheritdoc cref="TryGetComponentInChildren(Component, Type, out Component, bool)"/>
        public static bool TryGetComponentInChildren<T>(this Component component, out T output, bool includeInactive)
            where T : Component
        {
            output = component.GetComponentInChildren<T>(includeInactive);
            return output != null;
        }

        /// <inheritdoc cref="TryGetComponentInChildren{T}(Component, out T, bool)"/>
        public static bool TryGetComponentInChildren<T>(this Component component, out T output)
            where T : Component
        {
            return TryGetComponentInChildren(component, out output, false);
        }

        /// <summary>
        /// Custom version of <see cref="Component.GetComponentsInChildren{T}(bool)"/> that allows you to exclude the parent object.
        /// </summary>
        /// <typeparam name="T">The type of components to find.</typeparam>
        /// <param name="includeInactive">If enabled, inactive objects will be queried too.</param>
        /// <param name="excludeSelf">If enabled and the given parent has a component of the given type, that component will be
        /// ignored.</param>
        /// <returns>Returns the queried components.</returns>
        public static T[] GetComponentsInChildren<T>(this Component component, bool includeInactive, bool excludeSelf)
            where T : Component
        {
            using (var children = new ListPoolScope<T>())
            {
                component.GetComponentsInChildren(includeInactive, children.List);
                if (excludeSelf && component.TryGetComponent(out T parentComp))
                    children.Remove(parentComp);

                return children.ToArray();
            }
        }

    }

}