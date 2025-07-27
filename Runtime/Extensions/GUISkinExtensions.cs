using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Extension functions for <see cref="GUISkin"/> instances.
    /// </summary>
    public static class GUISkinExtensions
    {

        /// <summary>
        /// Finds a style on this <see cref="GUISkin"/> asset.
        /// </summary>
        /// <param name="skin">The <see cref="GUISkin"/> on which you want to find the style.</param>
        /// <param name="styleName">The name of the style you want to find.</param>
        /// <param name="fallbackStyle">The default style to use if the named style can't be found in the <see cref="GUISkin"/> asset.</param>
        /// <returns>Returns the found style, or the given fallback if the named style doesn't exist on the <see cref="GUISkin"/> asset.</returns>
        public static GUIStyle FindStyle(this GUISkin skin, string styleName, GUIStyle fallbackStyle)
        {
            GUIStyle style = skin.FindStyle(styleName);
            return style != null ? style : fallbackStyle;
        }

    }

}