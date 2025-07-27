using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Extension functions for <see cref="Camera"/> instances.
    /// </summary>
    public static class CameraExtensions
    {

        /// <summary>
        /// Calculates the camera render extents, using orthographic mode.<br/>
        /// The extents are Vector2(orthographic size * aspect ratio, orthographic size). You can use this value to automatically define screen limits.
        /// </summary>
        /// <param name="camera">The <see cref="Camera"/> of which you want to get the extents.</param>
        /// <returns>Returns the computed extents.</returns>
        public static Vector2 GetExtentsOrthographic(this Camera camera)
        {
            if (camera.orthographic)
            {
                return new Vector2
                (
                    camera.orthographicSize * camera.aspect,
                    camera.orthographicSize
                );
            }
            else
            {
                Debug.Log($"This camera ({camera.name}) must be turned to orthographic mode in order to use {nameof(GetExtentsOrthographic)}() function.");
                return Vector2.zero;
            }
        }

        /// <summary>
        /// Calculates the camera render extends using <see cref="GetExtentsOrthographic(Camera)"/> and double it to get bounds, using orthographic mode.
        /// </summary>
        /// <param name="camera">The <see cref="Camera"/> of which you want to get the bounds.</param>
        /// <returns>Returns the computed bounds.</returns>
        public static Vector2 GetBoundsOrthographic(this Camera camera)
        {
            return camera.GetExtentsOrthographic() * 2;
        }

    }

}