namespace SideXP.Core
{

    /// <summary>
    /// Miscellaneous constant values.
    /// </summary>
    public static class Constants
    {

        // Common

        /// <summary>
        /// Name of the company.
        /// </summary>
        public const string CompanyName = "Sideways Experiments";

        /// <summary>
        /// The URL to the company's website.
        /// </summary>
        public const string CompanyWebsite = "https://sideways-experiments.com";

        /// <summary>
        /// Base URL for online documentation of the SideXP Library. You must concatenate your own path starting with "/".
        /// </summary>
        public const string BaseHelpUrl = "https://gitlab.com/sideways-experiments/frameworks/unity/library";

        // Menus

        /// <summary>
        /// Base path used for <see cref="UnityEngine.AddComponentMenu"/>. You must concatenate your own path starting with "/".
        /// </summary>
        public const string AddComponentMenu = CompanyName;

        /// <summary>
        /// Base path used for <see cref="UnityEngine.CreateAssetMenuAttribute"/>. You must concatenate your own path starting with "/".
        /// </summary>
        public const string CreateAssetMenu = CompanyName;

        // Demos

        /// <summary>
        /// The submenu for demo features (from "Add Component" button, "Asset > Create" and the main toolbar menus). Starts with "/". You
        /// must concatenate your own path starting with "/".
        /// </summary>
        public const string DemosSubmenu = "/Demos";

        /// <summary>
        /// The define symbol used to enable demos of the framework.
        /// </summary>
        public const string DemosDefine = "SIDEXP_DEMOS";

        /// <summary>
        /// Base path used for <see cref="UnityEngine.AddComponentMenu"/> with demo components. You must concatenate your own path starting
        /// with "/".
        /// </summary>
        public const string AddComponentMenuDemos = CompanyName + DemosSubmenu;

        /// <summary>
        /// Base path used for <see cref="UnityEngine.AddComponentMenu"/> with demo components related to the core package features. You
        /// must concatenate your own path starting with "/".
        /// </summary>
        public const string AddComponentMenuDemosCore = AddComponentMenuDemos + "/Core";

        /// <summary>
        /// Base path used for <see cref="UnityEngine.CreateAssetMenuAttribute"/> with demo assets. You must concatenate your own path
        /// starting with "/".
        /// </summary>
        public const string CreateAssetMenuDemos = CompanyName + DemosSubmenu;

        /// <summary>
        /// Base path used for <see cref="UnityEngine.CreateAssetMenuAttribute"/> with demo assets related to the core package features. You
        /// must concatenate your own path starting with "/".
        /// </summary>
        public const string CreateAssetMenuDemosCore = CompanyName + DemosSubmenu + "/Core";

    }

}
