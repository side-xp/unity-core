namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Converts icon sizes on screen into stored file sizes, and resolves icon colors.
    /// </summary>
    internal static class IconSizes
    {

        #region Fields

        /// <summary>
        /// Size on screen of <see cref="EIconSize.Small"/> icons, in points.
        /// </summary>
        internal const float SmallPointSize = 16f;

        /// <summary>
        /// Size on screen of <see cref="EIconSize.Medium"/> icons, in points.
        /// </summary>
        internal const float MediumPointSize = 24f;

        /// <summary>
        /// Size on screen of <see cref="EIconSize.Large"/> icons, in points.
        /// </summary>
        internal const float LargePointSize = 32f;

        /// <summary>
        /// Margin under which a stored size is considered large enough, in pixels, so float imprecision on the display scale doesn't
        /// select the next stored size.
        /// </summary>
        private const float PixelSizeTolerance = 0.01f;

        #endregion


        #region Public API

        /// <summary>
        /// Gets the size on screen of an icon, in points.
        /// </summary>
        /// <param name="size">The size of the icon.</param>
        /// <returns>Returns the size on screen, in points.</returns>
        internal static float GetPointSize(EIconSize size)
        {
            switch (size)
            {
                case EIconSize.Medium:
                    return MediumPointSize;

                case EIconSize.Large:
                    return LargePointSize;

                default:
                    return SmallPointSize;
            }
        }

        /// <summary>
        /// Gets the size of the stored file to draw an icon sharply.
        /// </summary>
        /// <remarks>
        /// The file whose size matches the point size multiplied by the display scale is used. When none matches, the smallest larger
        /// file is used (and drawn smaller), or the largest file if none is large enough.
        /// </remarks>
        /// <example>
        /// At 150% display scale, <c>GetPixelSize(EIconSize.Small, 1.5f)</c> returns <c>24</c>, and
        /// <c>GetPixelSize(EIconSize.Medium, 1.5f)</c> returns <c>48</c>.
        /// </example>
        /// <param name="size">The size of the icon on screen.</param>
        /// <param name="pixelsPerPoint">The display scale, as <see cref="UnityEditor.EditorGUIUtility.pixelsPerPoint"/>.</param>
        /// <returns>Returns the size of the stored file to use, in pixels.</returns>
        internal static int GetPixelSize(EIconSize size, float pixelsPerPoint)
        {
            if (pixelsPerPoint <= 0f)
                pixelsPerPoint = 1f;

            float requiredPixels = GetPointSize(size) * pixelsPerPoint;
            int[] storedSizes = IconStorage.StoredSizes;
            foreach (int storedSize in storedSizes)
            {
                if (storedSize >= requiredPixels - PixelSizeTolerance)
                    return storedSize;
            }

            return storedSizes[storedSizes.Length - 1];
        }

        /// <summary>
        /// Checks if an icon must be black, rather than white.
        /// </summary>
        /// <param name="color">The color of the icon.</param>
        /// <param name="isDarkTheme">
        /// Whether the editor uses the dark theme, as <see cref="UnityEditor.EditorGUIUtility.isProSkin"/>.
        /// </param>
        /// <returns>Returns true if the icon must be black.</returns>
        internal static bool IsBlack(EIconColor color, bool isDarkTheme)
        {
            return color == EIconColor.Black || (color == EIconColor.Auto && !isDarkTheme);
        }

        #endregion

    }

}
