using UnityEditor;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Miscellaneous functions for working with path strings.
    /// </summary>
    [InitializeOnLoad]
    public static class PathEditorUtility
    {

        #region Fields

        /// <summary>
        /// Name of the /ProjectSettings directory.
        /// </summary>
        public const string ProjectSettingsDirectory = "ProjectSettings";

        /// <summary>
        /// Name of the /UserSettings directory.
        /// </summary>
        public const string UserSettingsDirectory = "UserSettings";

        /// <summary>
        /// The absolute path to the project's /ProjectSettings directory.
        /// </summary>
        public static readonly string ProjectSettingsPath = null;

        /// <summary>
        /// The absolute path to the project's /UserSettings directory.
        /// </summary>
        public static readonly string UserSettingsPath = null;

        #endregion


        #region Lifecycle

        /// <summary>
        /// Static constructor.
        /// </summary>
        static PathEditorUtility()
        {
            ProjectSettingsPath = $"{PathUtility.ProjectPath}/{ProjectSettingsDirectory}";
            UserSettingsPath = $"{PathUtility.ProjectPath}/{UserSettingsDirectory}";
        }

        #endregion

    }

}
