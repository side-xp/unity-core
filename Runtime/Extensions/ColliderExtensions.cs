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
        /// <remarks>
        /// While the game is running, the physics system provides accurate world bounds through <see cref="Collider.bounds"/>. At edit
        /// time (or for a prefab) that representation isn't baked, and reading it — even through the concrete collider type — returns a
        /// zeroed value. In that case the bounds are computed from the collider's geometry (center/size/radius/mesh) transformed into
        /// world space instead. Note: for a rotated non-box collider the computed AABB is slightly looser than the physics one.
        /// </remarks>
        /// <param name="collider">The collider of which to query the bounds.</param>
        /// <param name="bounds">Outputs the collider bounds.</param>
        /// <returns>Returns true if the collider type is supported (box, sphere, capsule, or mesh with a shared mesh).</returns>
        public static bool GetColliderBounds(this Collider collider, out Bounds bounds)
        {
            // When the game is running, the physics representation is baked, so Collider.bounds is accurate.
            if (Application.isPlaying && !collider.gameObject.IsPrefab())
            {
                bounds = collider.bounds;
                return true;
            }

            // Otherwise, compute the bounds from the collider's geometry and transform them into world space.
            if (GetLocalColliderBounds(collider, out Bounds localBounds))
            {
                bounds = TransformBounds(collider.transform, localBounds);
                return true;
            }

            Debug.LogWarning($"The bounds of the collider {collider} ({collider.GetType()}) can't be queried: unsupported collider type.", collider);
            bounds = default;
            return false;
        }

        /// <summary>
        /// Gets the bounds of a supported collider in its own local space (before any transform is applied).
        /// </summary>
        /// <param name="collider">The collider of which to query the local bounds.</param>
        /// <param name="bounds">Outputs the local-space bounds.</param>
        /// <returns>Returns true if the collider type is supported.</returns>
        private static bool GetLocalColliderBounds(Collider collider, out Bounds bounds)
        {
            switch (collider)
            {
                case BoxCollider box:
                    bounds = new Bounds(box.center, box.size);
                    return true;

                case SphereCollider sphere:
                    bounds = new Bounds(sphere.center, Vector3.one * (sphere.radius * 2f));
                    return true;

                case CapsuleCollider capsule:
                    float diameter = capsule.radius * 2f;
                    // The height includes the hemispherical caps, so it can't be smaller than the diameter.
                    float height = Mathf.Max(capsule.height, diameter);
                    Vector3 size = capsule.direction switch
                    {
                        0 => new Vector3(height, diameter, diameter),   // X axis
                        1 => new Vector3(diameter, height, diameter),   // Y axis
                        _ => new Vector3(diameter, diameter, height),   // Z axis
                    };
                    bounds = new Bounds(capsule.center, size);
                    return true;

                case MeshCollider mesh when mesh.sharedMesh != null:
                    bounds = mesh.sharedMesh.bounds;
                    return true;

                default:
                    bounds = default;
                    return false;
            }
        }

        /// <summary>
        /// Transforms a local-space bounds into world space by encapsulating its eight transformed corners (so rotation and scale are
        /// taken into account).
        /// </summary>
        /// <param name="transform">The transform to apply.</param>
        /// <param name="localBounds">The local-space bounds to transform.</param>
        /// <returns>Returns the world-space axis-aligned bounds.</returns>
        private static Bounds TransformBounds(Transform transform, Bounds localBounds)
        {
            Vector3 center = localBounds.center;
            Vector3 ext = localBounds.extents;
            Matrix4x4 matrix = transform.localToWorldMatrix;

            var result = new Bounds(matrix.MultiplyPoint3x4(center), Vector3.zero);
            for (int x = -1; x <= 1; x += 2)
            {
                for (int y = -1; y <= 1; y += 2)
                {
                    for (int z = -1; z <= 1; z += 2)
                    {
                        Vector3 corner = center + new Vector3(ext.x * x, ext.y * y, ext.z * z);
                        result.Encapsulate(matrix.MultiplyPoint3x4(corner));
                    }
                }
            }

            return result;
        }

    }

}