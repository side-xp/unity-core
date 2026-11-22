using System.Collections.Generic;

using UnityEditor;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Registers the pages to edit the Core package settings from the Project Settings and Preferences windows.
    /// </summary>
    public static class CoreSettingsProvider
    {

        /// <summary>
        /// Keywords to find the Preferences page from the search field, added to the ones generated from the fields of
        /// <see cref="CorePreferences"/>. They cover the sections drawn by custom code, as the editor icons section.
        /// </summary>
        private static readonly string[] PreferencesKeywords = { "Icons", "Editor Icons", "Tabler", "Icons Folder" };

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
            EditorConfigSettingsProvider provider = EditorConfigSettingsProvider.Create<CorePreferences>(
                EditorConstants.Preferences + "/General",
                SettingsScope.User
            );

            var keywords = new HashSet<string>(PreferencesKeywords);
            if (provider.keywords != null)
                keywords.UnionWith(provider.keywords);

            provider.keywords = keywords;
            return provider;
        }

    }

}
