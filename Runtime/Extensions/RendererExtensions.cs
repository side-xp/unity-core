using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Extension functions for <see cref="Renderer"/> components.
    /// </summary>
    public static class RendererExtensions
    {

        /// <returns>Returns the renderer bounds.</returns>
        /// <inheritdoc cref="GetRendererBounds(Renderer, out Bounds)"/>
        public static Bounds GetRendererBounds(this Renderer renderer)
        {
            return GetRendererBounds(renderer, out Bounds bounds) ? bounds : default;
        }

        /// <summary>
        /// Gets the bounds of a renderer.
        /// </summary>
        /// <remarks>
        /// Contrary to colliders (see <see cref="ColliderExtensions.GetColliderBounds(Collider, out Bounds)"/>), a mesh renderer's
        /// <see cref="Renderer.bounds"/> is populated even when the game is not running, so it can be read directlyhere.
        /// </remarks>
        /// <param name="renderer">The renderer of which to query the bounds.</param>
        /// <param name="bounds">Outputs the renderer bounds.</param>
        /// <returns>Always returns true (every <see cref="Renderer"/> exposes bounds).</returns>
        public static bool GetRendererBounds(this Renderer renderer, out Bounds bounds)
        {
            // Renderer.bounds is valid at edit time for mesh renderers (unlike Collider.bounds), so it's read directly.
            // @todo Verify this still holds for other renderer types (skinned mesh, sprite, line, ...) if the need arises.
            bounds = renderer.bounds;
            return true;
        }

    }

}