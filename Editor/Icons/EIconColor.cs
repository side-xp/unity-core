namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Defines the color of an editor icon.
    /// </summary>
    public enum EIconColor
    {

        /// <summary>
        /// White on the dark editor theme, black on the light one.
        /// </summary>
        Auto,

        /// <summary>
        /// White icon, to draw as is or tinted.
        /// </summary>
        White,

        /// <summary>
        /// Black icon, mostly for IMGUI controls with text on the light theme, since tinting with <c>GUI.color</c> also tints the text.
        /// </summary>
        Black,

    }

}
