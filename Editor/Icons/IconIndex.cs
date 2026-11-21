using System;

namespace SideXP.Core.EditorOnly
{

    // Field names are the keys of the index file, read and written with JsonUtility, so they don't follow the naming rules
#pragma warning disable IDE1006

    /// <summary>
    /// The index of the available icons, as stored in the shared folder.
    /// </summary>
    /// <remarks>
    /// Converted from the Tabler catalog by <see cref="TablerCatalogConverter"/>, and read and written with
    /// <see cref="UnityEngine.JsonUtility"/>.
    /// </remarks>
    [Serializable]
    internal class IconIndex
    {

        /// <summary>
        /// The version of Tabler the index comes from. Icons are always downloaded with this version, so their files match the index.
        /// </summary>
        public string version = null;

        /// <summary>
        /// The available icons.
        /// </summary>
        public IconIndexEntry[] icons = { };

    }

    /// <summary>
    /// Informations about an icon, as stored in the index.
    /// </summary>
    [Serializable]
    internal class IconIndexEntry
    {

        /// <summary>
        /// The name of the icon, which only uses lowercase letters, digits and hyphens.
        /// </summary>
        public string name = null;

        /// <summary>
        /// The category of the icon, as written in the Tabler catalog (for example <c>Version control</c>).
        /// </summary>
        public string category = null;

        /// <summary>
        /// The tags of the icon, used to search icons by keywords.
        /// </summary>
        public string[] tags = { };

        /// <summary>
        /// Whether the icon exists in the filled style. Every icon exists in the outline style.
        /// </summary>
        public bool hasFilled = false;

    }

#pragma warning restore IDE1006

}
