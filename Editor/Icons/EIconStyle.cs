namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Defines the style of an editor icon.
    /// </summary>
    public enum EIconStyle
    {

        /// <summary>
        /// Shapes drawn with strokes only. Available for every icon.
        /// </summary>
        Outline,

        /// <summary>
        /// Filled shapes. Available for some icons only.
        /// </summary>
        /// <remarks>
        /// Falls back to <see cref="Outline"/> when the icon has no filled version.
        /// </remarks>
        Filled,

    }

}
