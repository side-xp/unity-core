namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Defines a change to apply to the stored icon files after the index has been refreshed.
    /// </summary>
    internal enum EIconIndexChange
    {

        /// <summary>
        /// The category of the icon changed, so its folder must be moved to the new category folder.
        /// </summary>
        MoveFolder,

        /// <summary>
        /// The icon has been removed from the catalog, so its folder must be deleted.
        /// </summary>
        DeleteFolder,

        /// <summary>
        /// A style has been removed from the icon, so its files for that style must be deleted.
        /// </summary>
        DeleteStyleFiles,

    }

}
