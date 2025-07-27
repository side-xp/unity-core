namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Miscellaneous constant values for editor features.
    /// </summary>
    public static class EditorConstants
    {

        /// <summary>
        /// Prefix used for main toolbar menus. Ends with "/".
        /// </summary>
        public const string ToolbarMenuPrefix = "Tools/";

        /// <summary>
        /// Base path used for custom menus in the main toolbar. You must concatenate your own path starting with "/".
        /// </summary>
        public const string ToolbarMenu = ToolbarMenuPrefix + Constants.CompanyName;

        /// <summary>
        /// Base path used for custom menus in the main toolbar for opening custom editor windows. You must concatenate your own path
        /// starting with "/".
        /// </summary>
        public const string EditorWindowMenu = ToolbarMenu;

        /// <summary>
        /// Base path used for custom menus in the main toolbar for opening demo custom editor windows. You must concatenate your own path
        /// starting with "/".
        /// </summary>
        public const string EditorWindowMenuDemos = EditorWindowMenu + Constants.DemosSubmenu;

        /// <summary>
        /// Base path used for custom viewers that display informations about the engine or a core system on GUI.. You must concatenate your
        /// own path starting with "/".
        /// </summary>
        public const string EditorWindowMenuViewers = EditorWindowMenu + "/Viewers";

        /// <summary>
        /// Base path used for custom menus in the main toolbar for opening core library demo custom editor windows. You must concatenate
        /// your own path starting with "/".
        /// </summary>
        public const string EditorWindowMenuDemosCore = EditorWindowMenuDemos + "/Core";

        /// <summary>
        /// Base path used for custom Preferences menus. You must concatenate your own path starting with "/".
        /// </summary>
        public const string Preferences = Constants.CompanyName;

        /// <summary>
        /// Base path used for custom Project Settings menus. You must concatenate your own path starting with "/".
        /// </summary>
        public const string ProjectSettings = Constants.CompanyName;

    }

}