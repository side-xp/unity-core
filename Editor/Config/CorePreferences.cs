using UnityEngine;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// User preferences for the Core package features.
    /// </summary>
    /// <remarks>
    /// These preferences are stored in <see cref="UnityEditor.EditorPrefs"/>, so they're shared by all the projects (and all the Unity
    /// versions) on the user's machine.
    /// </remarks>
    [EditorConfig(EEditorConfigScope.Editor, Key = "SideXP.Core.EditorOnly.CorePreferences")]
    public class CorePreferences : EditorConfig<CorePreferences>
    {

        #region Fields

        /// <summary>
        /// Name of the serialized property of <see cref="_iconsFolder"/>, drawn separately by <see cref="CorePreferencesEditor"/>.
        /// </summary>
        internal const string IconsFolderPropertyName = nameof(_iconsFolder);

        /// <summary>
        /// Absolute path to the folder where the editor icons are stored. If empty, the default folder for this OS is used.
        /// </summary>
        /// <remarks>
        /// Drawn by <see cref="CorePreferencesEditor"/>, since changing it must move the icons first.
        /// </remarks>
        [SerializeField]
        [FolderPath("Choose the icons folder", true, IsDelayed = true)]
        [Tooltip("Folder where the editor icons are stored, shared by every project and Unity version on this machine." +
            "\nChanging it moves the icons.")]
        private string _iconsFolder = string.Empty;

        #endregion


        #region Public API

        /// <inheritdoc cref="_iconsFolder"/>
        public string IconsFolder => _iconsFolder;

        #endregion


        #region Internal API

        /// <summary>
        /// Sets the folder where the editor icons are stored, and saves the preferences.
        /// </summary>
        /// <remarks>
        /// This doesn't move the icons. Use <see cref="IconFolderMover"/> to do so before calling this function.
        /// </remarks>
        /// <param name="folder">The absolute path to the folder, or null or empty to use the default folder.</param>
        /// <returns>Returns true if the preferences have been saved successfully.</returns>
        internal bool SetIconsFolder(string folder)
        {
            _iconsFolder = folder ?? string.Empty;
            return Save();
        }

        #endregion

    }

}
