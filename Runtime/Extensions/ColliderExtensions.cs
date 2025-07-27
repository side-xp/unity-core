using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Extension functions for <see cref="Collider"/> components. See <see cref="Collider2DExtensions"/> for 2D colliders.
    /// </summary>
    public static class ColliderExtensions
    {

        /// <returns>Returns the collider bounds.</returns>
        /// <inheritdoc cref="GetColliderBounds(Collider, out Bounds)"/>
        public static Bounds GetColliderBounds(this Collider collider)
        {
            return GetColliderBounds(collider, out Bounds bounds) ? bounds : default;
        }

        /// <summary>
        /// Gets the bounds of a collider.
        /// </summary>
        /// <remarks>The bounds can technically be queried by using <see cref="Collider.bounds"/>, but if the game is not running or the
        /// object is a prefab, the collider won't be setup. The only way to query the bounds then is to use that property from the "real"
        /// type instead of <see cref="Collider"/> directly.</remarks>
        /// <param name="collider">The collider of which to query the bounds.</param>
        /// <param name="bounds">Outputs the collider bounds.</param>
        /// <returns>Returns true if the collider type is supported.</returns>
        public static bool GetColliderBounds(this Collider collider, out Bounds bounds)
        {
            if (Application.isPlaying && !collider.gameObject.IsPrefab())
            {
                bounds = collider.bounds;
                return true;
            }

            bool success = true;
            bounds = default;

            if (collider is SphereCollider sphere)
                bounds = sphere.bounds;
            else if (collider is BoxCollider box)
                bounds = box.bounds;
            else if (collider is CapsuleCollider capsule)
                bounds = capsule.bounds;
            else if (collider is MeshCollider mesh)
                bounds = mesh.bounds;
            else
                success = false;

            if (!success)
                Debug.LogWarning($"The bounds of the collider {collider} ({collider.GetType()}) can't be queried: invalid collider type.", collider);

            return success;
        }

    }

}