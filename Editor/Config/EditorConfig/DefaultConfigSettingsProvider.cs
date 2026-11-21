using System;

using UnityEditor;

using Object = UnityEngine.Object;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Generate menus to edit a specific asset type by drawing its default inspector.
    /// </summary>
    [Obsolete("Use " + nameof(EditorConfigSettingsProvider) + ".Create<T>() instead.")]
    public abstract class DefaultConfigSettingsProvider
    {

        /// <summary>
        /// Creates a <see cref="SettingsProvider"/> instance that displays the given settings asset.
        /// </summary>
        /// <param name="settingsAsset">The asset that contains the settings to display.</param>
        /// <param name="menu">
        /// The menu from which these settings can be edited, relative to the appropriate window depending on the scope.
        /// </param>
        /// <param name="scope">Defines when the settings are displayed.</param>
        /// <param name="keywords">The keywords to search for these settings to be filtered.</param>
        /// <returns>Returns the created <see cref="SettingsProvider"/>.</returns>
        protected static SettingsProvider MakeSettingsProvider(Object settingsAsset, string menu, SettingsScope scope, string[] keywords = null)
        {
            return new EditorConfigSettingsProvider(menu, scope, () => settingsAsset, keywords);
        }

    }

}
