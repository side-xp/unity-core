using UnityEditor;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Registers the pages to edit the Core package settings from the Project Settings and Preferences windows.
    /// </summary>
    public static class CoreSettingsProvider
    {

        /// <summary>
        /// Registers the page to edit <see cref="CoreProjectSettings"/> from the Project Settings window.
        /// </summary>
        /// <returns>Returns the created settings provider.</returns>
        [SettingsProvider]
        private static SettingsProvider RegisterProjectSettingsMenu()
        {
            return EditorConfigSettingsProvider.Create<CoreProjectSettings>(
                EditorConstants.ProjectSettings + "/General",
                SettingsScope.Project
            );
        }

        /// <summary>
        /// Registers the page to edit <see cref="CorePreferences"/> from the Preferences window.
        /// </summary>
        /// <returns>Returns the created settings provider.</returns>
        [SettingsProvider]
        private static SettingsProvider RegisterPreferencesMenu()
        {
            return EditorConfigSettingsProvider.Create<CorePreferences>(EditorConstants.Preferences + "/General", SettingsScope.User);
        }

    }

}
