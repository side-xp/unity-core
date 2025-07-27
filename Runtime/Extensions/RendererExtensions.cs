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
        /// <remarks>The bounds can technically be queried by using <see cref="Renderer.bounds"/>, but if the game is not running or the
        /// object is a prefab, the renderer won't be setup. The only way to query the bounds then is to use that property from the "real"
        /// type instead of <see cref="Renderer"/> directly.</remarks>
        /// <param name="renderer">The renderer of which to query the bounds.</param>
        /// <param name="bounds">Outputs the renderer bounds.</param>
        /// <returns>Returns true if the renderer type is supported.</returns>
        public static bool GetRendererBounds(this Renderer renderer, out Bounds bounds)
        {
            // @todo This requires testing for new Unity versions to check if the behavior for renderers is still the same as for colliders.
            bounds = renderer.bounds;
            return true;
        }

    }

}