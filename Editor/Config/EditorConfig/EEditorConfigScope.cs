namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Defines the scope of the data contained in an editor settings object.
    /// </summary>
    public enum EEditorConfigScope
    {

        /// <summary>
        /// The settings apply to the project, and should be stored in /ProjectSettings.
        /// </summary>
        Project,

        /// <summary>
        /// The settings apply to the user, and should be stored in /UserSettings
        /// </summary>
        User,

        /// <summary>
        /// The settings apply to the user, and should be stored in <see cref="UnityEditor.EditorPrefs"/> registry.
        /// </summary>
        /// <remarks>In this case, the settings will be shared by all the projects and all Unity versions for the user's machine.</remarks>
        Editor,

    }

}
