using UnityEngine;
using UnityEditor;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Miscellaneous functions for working with path strings.
    /// </summary>
    [InitializeOnLoad]
    public class PathEditorUtility : MonoBehaviour
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
        /// The absolute path to the /ProjectSettings directory
        /// </summary>
        public static readonly string ProjectSettingsPath = null;

        /// <summary>
        /// The absolute path to the /UserSettings directory
        /// </summary>
        public static readonly string UserSettingsPath = null;

        #endregion


        #region Lifecycle

        /// <summary>
        /// Static constructor.
        /// </summary>
        static PathEditorUtility()
        {
            ProjectSettingsPath = PathUtility.ToPath(ProjectSettingsDirectory);
            UserSettingsPath = PathUtility.ToPath(UserSettingsDirectory);
        }

        #endregion

    }

}
