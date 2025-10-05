using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Extension functions for <see cref="Collider2D"/> components. See <see cref="ColliderExtensions"/> for 3D colliders.
    /// </summary>
    public static class Collider2DExtensions
    {

        /// <inheritdoc cref="ColliderExtensions.GetColliderBounds(Collider)"/>
        public static Bounds GetColliderBounds(this Collider2D collider)
        {
            return GetColliderBounds(collider, out Bounds bounds) ? bounds : default;
        }

        /// <inheritdoc cref="ColliderExtensions.GetColliderBounds(Collider, out Bounds)"/>
        public static bool GetColliderBounds(this Collider2D collider, out Bounds bounds)
        {
            if (Application.isPlaying && !collider.gameObject.IsPrefab())
            {
                bounds = collider.bounds;
                return true;
            }

            bool success = true;
            bounds = default;

            if (collider is CircleCollider2D circle)
                bounds = new Bounds(circle.offset, new Vector2(circle.radius * 2, circle.radius * 2));
            else if (collider is BoxCollider2D box)
                bounds = new Bounds(box.offset, box.size);
            else if (collider is CapsuleCollider2D capsule)
                bounds = new Bounds(capsule.offset, capsule.size);
            else if (collider is PolygonCollider2D poly)
                // @todo
                bounds = poly.bounds;
            else if (collider is EdgeCollider2D edge)
                // @todo
                bounds = edge.bounds;
            else if (collider is CompositeCollider2D composite)
                // @todo
                bounds = composite.bounds;
#if UNITY_2021_2_OR_NEWER
            else if (collider is CustomCollider2D custom)
                // @todo
                bounds = custom.bounds;
#endif
            else
                success = false;

            if (!success)
                Debug.LogWarning($"The bounds of the collider {collider.name} can't be queried: invalid collider type ({collider.GetType()}).", collider);

            return success;
        }

        /// <returns>Returns the found collider offset.</returns>
        /// <inheritdoc cref="GetColliderOffset(Collider2D, out Vector2)"/>
        public static Vector2 GetColliderOffset(this Collider2D collider)
        {
            return GetColliderOffset(collider, out Vector2 offset) ? offset : Vector2.zero;
        }

        /// <summary>
        /// Gets the offset of a <see cref="Collider2D"/>.
        /// </summary>
        /// <param name="collider">The game object to check.</param>
        /// <param name="offset">Outputs the found collider offset.</param>
        /// <returns>Returns true if the offset has been queried successfully.</returns>
        public static bool GetColliderOffset(this Collider2D collider, out Vector2 offset)
        {
            // @todo This requires testing for new Unity versions to check if the behavior for offsets is the same as for collider bounds.
            offset = collider.offset;
            return true;
        }

    }

}