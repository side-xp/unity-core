using UnityEngine;
using UnityEditor;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Generate menus to edit a specific asset type by drawing its default inspector.
    /// </summary>
    public abstract class DefaultConfigSettingsProvider
    {

        private static Editor s_editor = null;

        /// <summary>
        /// Creates a <see cref="SettingsProvider"/> instance from
        /// </summary>
        /// <param name="settingsAsset">The asset that contains the settings to display.</param>
        /// <param name="menu">The menu from which these settings can be edited, relative to the appropriate window depending on the
        /// scope.</param>
        /// <param name="scope">Defines when the settings are displayed.</param>
        /// <param name="keywords">The keywords to search for these settings to be filtered.</param>
        /// <returns>Returns the created <see cref="SettingsProvider"/>.</returns>
        protected static SettingsProvider MakeSettingsProvider(Object settingsAsset, string menu, SettingsScope scope, string[] keywords = null)
        {
            return new SettingsProvider(menu, scope, keywords)
            {
                activateHandler = (search, ui) =>
                {
                    if (s_editor == null)
                        s_editor = Editor.CreateEditor(settingsAsset);
                },
                guiHandler = str =>
                {
                    if (s_editor == null)
                    {
                        EditorGUILayout.HelpBox($"Failed to create the Editor instance for {(settingsAsset != null ? settingsAsset.name : "NULL")}. Recompile ", MessageType.Warning);
                        return;
                    }

                    s_editor.OnInspectorGUI();
                },
                deactivateHandler = () =>
                {
                    if (s_editor != null)
                    {
                        if (EditorApplication.isPlaying)
                            Object.Destroy(s_editor);
                        else
                            Object.DestroyImmediate(s_editor);
                    }
                }
            };
        }

    }

}
