using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Extension functions for <see cref="Object"/> instances.
    /// </summary>
    public static class ObjectExtensions
    {

        /// <inheritdoc cref="RuntimeObjectUtility.GetTransform(Object)"/>
        public static Transform GetTransform(this Object obj)
        {
            return RuntimeObjectUtility.GetTransform(obj);
        }

        /// <inheritdoc cref="RuntimeObjectUtility.GetTransform(Object, out Transform)"/>
        public static bool TryGetTransform(this Object obj, out Transform transform)
        {
            return RuntimeObjectUtility.GetTransform(obj, out transform);
        }

        /// <inheritdoc cref="RuntimeObjectUtility.GetGameObject(Object)"/>
        public static GameObject GetGameObject(this Object obj)
        {
            return RuntimeObjectUtility.GetGameObject(obj);
        }

        /// <inheritdoc cref="RuntimeObjectUtility.GetGameObject(Object, out GameObject)"/>
        public static bool TryGetGameObject(this Object obj, out GameObject gameObject)
        {
            return RuntimeObjectUtility.GetGameObject(obj, out gameObject);
        }

    }

}