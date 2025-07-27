using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Extension functions for <see cref="Rect"/> values.
    /// </summary>
    public static class RectExtensions
    {

        /// <summary>
        /// Holds this <see cref="Rect"/> inside the given area.
        /// </summary>
        /// <param name="rect">The <see cref="Rect"/> you want to hold in the given area.</param>
        /// <param name="area">The available area in which the given rect can be placed.</param>
		public static void HoldInArea(this ref Rect rect, Rect area)
        {
            if (rect.y + rect.height > area.y + area.height)
                rect.y = area.y + area.height - rect.height;
            if (rect.x + rect.width > area.x + area.width)
                rect.x = area.x + area.width - rect.width;
            if (rect.x < area.x)
                rect.x = area.x;
            if (rect.y < area.y)
                rect.y = area.y;
        }

        /// <summary>
        /// Holds this <see cref="Rect"/> inside the screen space.
        /// </summary>
        /// <param name="rect">The <see cref="Rect"/> you want to hold in the screen space.</param>
        public static void HoldInScreenSpace(this ref Rect rect)
        {
            Vector2 screeSize = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
            rect.HoldInArea(new Rect(Vector2.zero, screeSize));
        }

    }

}