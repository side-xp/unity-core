using UnityEditor;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Generate menus to edit the Core package editor config.
    /// </summary>
    public static class CoreEditorConfigProvider
    {

        /// <summary>
        /// Registers the menu to edit the Core package settings from the Project Settings window.
        /// </summary>
        /// <returns>Returns the created settings provider.</returns>
        [SettingsProvider]
        private static SettingsProvider RegisterProjectSettingsMenu()
        {
            return EditorConfigSettingsProvider.Create<CoreEditorConfig>(EditorConstants.ProjectSettings + "/General", SettingsScope.Project);
        }

        /// <summary>
        /// Registers the menu to edit the Core package settings from the Preferences window.
        /// </summary>
        /// <returns>Returns the created settings provider.</returns>
        [SettingsProvider]
        private static SettingsProvider RegisterUserSettingsMenu()
        {
            return EditorConfigSettingsProvider.Create<CoreEditorConfig>(EditorConstants.Preferences + "/General", SettingsScope.User);
        }

    }

}
