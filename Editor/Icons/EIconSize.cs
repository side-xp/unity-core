namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Defines the size of an editor icon on screen, in points.
    /// </summary>
    /// <remarks>
    /// Editor layouts are measured in points. The icon file is chosen from that size and the display scale, so the icon stays sharp.
    /// </remarks>
    public enum EIconSize
    {

        /// <summary>
        /// 16 points. Same size as Unity's own icons: fits in a single inspector line and in toolbars.
        /// </summary>
        Small,

        /// <summary>
        /// 24 points, for larger buttons.
        /// </summary>
        Medium,

        /// <summary>
        /// 32 points, for headers and icon lists.
        /// </summary>
        Large,

    }

}
