using UnityEditor;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Generate menus to edit the Core package editor config.
    /// </summary>
    public class CoreEditorConfigProvider : DefaultConfigSettingsProvider
    {

        [SettingsProvider]
        private static SettingsProvider RegisterProjectSettingsMenu()
        {
            return MakeSettingsProvider(CoreEditorConfig.I, EditorConstants.ProjectSettings + "/General", SettingsScope.Project);
        }

        [SettingsProvider]
        private static SettingsProvider RegisterUserSettingsMenu()
        {
            return MakeSettingsProvider(CoreEditorConfig.I, EditorConstants.Preferences + "/General", SettingsScope.User);
        }

    }

}
