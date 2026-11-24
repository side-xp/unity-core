using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using Object = UnityEngine.Object;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// The menu of the dropdowns of <see cref="AssetDropdownAttribute"/>: a <i>None</i> option, then every asset of a type found in the
    /// project. Use it to show the same dropdown in other editor tools.
    /// </summary>
    /// <remarks>
    /// The options are found each time a menu is created, so new assets show up right away. Sub-assets are listed in a submenu named
    /// after their main asset, since the names of sub-assets are only unique within their file. A <c>/</c> in the name of a sub-asset
    /// creates a submenu too.
    /// </remarks>
    public static class AssetDropdownMenu
    {

        #region Fields

        private const string NoneText = "None";
        private const string MissingText = "Missing";
        private const string UnnamedText = "(Unnamed)";

        /// <summary>
        /// The character that separates a submenu from its items in the paths of the items of a <see cref="GenericMenu"/>.
        /// </summary>
        private const char SubmenuSeparator = '/';

        /// <summary>
        /// The types that can't be listed by a dropdown, since every asset of the project would be an option.
        /// </summary>
        private static readonly Type[] TooGenericTypes = { typeof(Object), typeof(ScriptableObject) };

        #endregion


        #region Public API

        /// <summary>
        /// The message shown instead of the dropdown when <see cref="AssetDropdownAttribute"/> is used on a field of a type it doesn't
        /// support.
        /// </summary>
        public const string UnsupportedTypeMessage = "Use AssetDropdown with a reference to a type of asset (not a component).";

        /// <summary>
        /// Gets the type of the assets referenced by a field: the type of the field itself, or the type of the elements of an array or
        /// a list, since Unity applies property attributes to each element.
        /// </summary>
        /// <param name="fieldType">The type of the field.</param>
        /// <returns>Returns the type of the referenced assets, or null if the field type is null.</returns>
        public static Type GetAssetType(Type fieldType)
        {
            if (fieldType == null)
                return null;

            if (fieldType.IsArray)
                return fieldType.GetElementType();

            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
                return fieldType.GetGenericArguments()[0];

            return fieldType;
        }

        /// <summary>
        /// Checks if a dropdown can list the assets of a type.
        /// </summary>
        /// <remarks>
        /// Components are not supported: they are not assets of their own, but parts of prefabs. <see cref="Object"/> and
        /// <see cref="ScriptableObject"/> are not supported either, since every asset of the project would be an option.
        /// </remarks>
        /// <param name="assetType">The type of the assets.</param>
        /// <returns>Returns true if the type is a type of asset that a dropdown can list.</returns>
        public static bool IsSupported(Type assetType)
        {
            return assetType != null
                && typeof(Object).IsAssignableFrom(assetType)
                && !typeof(Component).IsAssignableFrom(assetType)
                && Array.IndexOf(TooGenericTypes, assetType) < 0;
        }

        /// <summary>
        /// Creates the menu of a dropdown: <i>None</i>, then every asset of a type found in the project, ordered by path, the current
        /// value checked.
        /// </summary>
        /// <param name="assetType">The type of the assets to list, which must be supported (see <see cref="IsSupported(Type)"/>).</param>
        /// <param name="currentValue">The current value of the dropdown, checked in the menu. Null to check <i>None</i>.</param>
        /// <param name="onPicked">Called with the picked asset when an option is picked, or with null for <i>None</i>.</param>
        /// <returns>Returns the created menu, to show with <see cref="GenericMenu.DropDown(Rect)"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the type or the callback is null.</exception>
        public static GenericMenu Create(Type assetType, Object currentValue, Action<Object> onPicked)
        {
            if (assetType == null)
                throw new ArgumentNullException(nameof(assetType));

            if (onPicked == null)
                throw new ArgumentNullException(nameof(onPicked));

            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent(NoneText), currentValue == null, () => onPicked(null));
            menu.AddSeparator(string.Empty);

            List<Object> assets = ObjectUtility.LoadAllAssetsOfType(assetType);
            if (assets.Count == 0)
            {
                menu.AddDisabledItem(new GUIContent($"No {assetType.Name} in the project"));
                return menu;
            }

            foreach (Object asset in assets)
            {
                Object option = asset;
                menu.AddItem(new GUIContent(GetOptionText(option)), option == currentValue, () => onPicked(option));
            }

            return menu;
        }

        /// <summary>
        /// Gets the text of an asset in the menu of a dropdown: its name, in a submenu named after its main asset if it's a sub-asset.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <returns>Returns the text of the option, whose <c>/</c> create submenus.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the asset is null.</exception>
        public static string GetOptionText(Object asset)
        {
            if (asset == null)
                throw new ArgumentNullException(nameof(asset));

            string name = GetName(asset);
            string path = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(path))
                return name;

            // Compared with the main asset of the file instead of AssetDatabase.IsSubAsset(), which is false for a sub-asset added to a
            // file that hasn't been imported again since
            Object mainAsset = AssetDatabase.LoadMainAssetAtPath(path);
            return mainAsset != null && mainAsset != asset
                ? $"{GetName(mainAsset)}{SubmenuSeparator}{name}"
                : name;
        }

        /// <summary>
        /// Gets the text of the value of a dropdown: the name of the referenced asset, <i>None</i> or <i>Missing</i>, like the options
        /// of the menu.
        /// </summary>
        /// <param name="value">The value of the dropdown.</param>
        /// <returns>Returns the text to display.</returns>
        public static string GetValueText(Object value)
        {
            if (value != null)
                return GetName(value);

            // A missing reference (deleted asset) is not null for C#, but is null for Unity
            return !ReferenceEquals(value, null) ? MissingText : NoneText;
        }

        /// <summary>
        /// Gets the icon of the value of a dropdown: the icon of the referenced asset, or no icon for <i>None</i> and <i>Missing</i>.
        /// </summary>
        /// <param name="value">The value of the dropdown.</param>
        /// <returns>Returns the icon to display, or null if there is none.</returns>
        public static Texture2D GetValueIcon(Object value)
        {
            return value != null ? AssetPreview.GetMiniThumbnail(value) : null;
        }

        #endregion


        #region Private API

        /// <summary>
        /// Gets the name of an asset, or a placeholder if it has none, so its option is never an empty line.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <returns>Returns the name to display.</returns>
        private static string GetName(Object asset)
        {
            return string.IsNullOrWhiteSpace(asset.name) ? UnnamedText : asset.name;
        }

        #endregion

    }

}
