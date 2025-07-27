using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Draws a text field with a "browse" button to select a folder path.
    /// </summary>
    public class FolderPathAttribute : PropertyAttribute
    {

        /// <summary>
        /// The title of the "open folder" panel.
        /// </summary>
        public string Title { get; set; } = "Select Folder";

        /// <summary>
        /// The default name of the directory to select.
        /// </summary>
        public string DefaultName { get; set; } = string.Empty;

        /// <summary>
        /// If enabled, allow the user to select a folder outside of the current project's directory.
        /// </summary>
        public bool AllowExternal { get; set; } = false;

        /// <inheritdoc cref="FolderPathAttribute(string, bool)"/>
        public FolderPathAttribute() { }

        /// <inheritdoc cref="FolderPathAttribute" />
        /// <param name="title"><inheritdoc cref="Title" path="/summary"/></param>
        /// <param name="allowExternal"><inheritdoc cref="AllowExternal" path="/summary"/></param>
        public FolderPathAttribute(string title, bool allowExternal = false)
        {
            Title = title;
            AllowExternal = allowExternal;
        }

    }

}