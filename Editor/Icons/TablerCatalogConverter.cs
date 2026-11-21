using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using UnityEngine;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Converts the Tabler catalog (<c>icons.json</c>) into an <see cref="IconIndex"/>.
    /// </summary>
    /// <remarks>
    /// The catalog is a dictionary keyed by icon name, which <see cref="JsonUtility"/> can't read. Since each entry starts with a
    /// <c>name</c> field equal to its key, the keys are removed to turn the dictionary into an array. The conversion fails instead of
    /// producing a broken index if the catalog doesn't follow that format.
    /// </remarks>
    internal static class TablerCatalogConverter
    {

        #region Subclasses

        // Field names are the keys of the Tabler catalog, read with JsonUtility, so they don't follow the naming rules
#pragma warning disable IDE1006

        /// <summary>
        /// The Tabler catalog, once converted from a dictionary into an array.
        /// </summary>
        [Serializable]
        internal class Catalog
        {

            /// <summary>
            /// The icons of the catalog.
            /// </summary>
            public CatalogIcon[] icons = { };

        }

        /// <summary>
        /// An icon of the Tabler catalog.
        /// </summary>
        [Serializable]
        internal class CatalogIcon
        {

            /// <summary>
            /// The name of the icon.
            /// </summary>
            public string name = null;

            /// <summary>
            /// The category of the icon.
            /// </summary>
            public string category = null;

            /// <summary>
            /// The tags of the icon.
            /// </summary>
            public string[] tags = { };

            /// <summary>
            /// The styles in which the icon exists.
            /// </summary>
            public CatalogStyles styles = null;

        }

        /// <summary>
        /// The styles in which an icon of the Tabler catalog exists.
        /// </summary>
        [Serializable]
        internal class CatalogStyles
        {

            /// <summary>
            /// The outline style of the icon.
            /// </summary>
            public CatalogStyle outline = null;

            /// <summary>
            /// The filled style of the icon.
            /// </summary>
            public CatalogStyle filled = null;

        }

        /// <summary>
        /// A style of an icon of the Tabler catalog.
        /// </summary>
        [Serializable]
        internal class CatalogStyle
        {

            /// <summary>
            /// The Tabler version in which the icon has been added or last changed in this style.
            /// </summary>
            public string version = null;

            /// <summary>
            /// The code point of the icon in this style, in the Tabler icon font.
            /// </summary>
            public string unicode = null;

        }

#pragma warning restore IDE1006

        #endregion


        #region Fields

        /// <summary>
        /// Matches the key of a catalog entry, followed by the start of the entry up to its first field, which must be <c>name</c>.
        /// </summary>
        private static readonly Regex EntryKeyRegex = new Regex(@"""[^""]+""\s*:\s*\{\s*""name""");

        /// <summary>
        /// Matches a <c>tags</c> array, capturing its start, its content and its end. Strings are matched as a whole, so a closing
        /// bracket inside a tag doesn't end the array.
        /// </summary>
        private static readonly Regex TagsArrayRegex = new Regex(@"(""tags""\s*:\s*\[)((?:""(?:[^""\\]|\\.)*""|[^\]""])*)(\])");

        /// <summary>
        /// Matches a value in a <c>tags</c> array: either a string, a bare number, or a bare <c>true</c>, <c>false</c> or <c>null</c>.
        /// </summary>
        /// <remarks>
        /// Bare literals are matched since some tags are words that have been converted to JSON literals upstream (in Tabler 3.52.0,
        /// <c>number-0</c> has a <c>null</c> tag next to <c>nil</c> and <c>zero</c>).
        /// </remarks>
        private static readonly Regex TagValueRegex = new Regex(@"""(?:[^""\\]|\\.)*""|-?\d+(?:\.\d+)?(?:[eE][+-]?\d+)?|true|false|null");

        #endregion


        #region Public API

        /// <summary>
        /// Converts the Tabler catalog into an index.
        /// </summary>
        /// <remarks>
        /// Icons with a name that is not a valid icon name (see <see cref="IconStorage.IsValidIconName(string)"/>) are skipped with a
        /// warning, since their files couldn't be stored safely.
        /// </remarks>
        /// <param name="catalogJson">The content of the Tabler catalog file.</param>
        /// <param name="version">The version of Tabler the catalog comes from.</param>
        /// <param name="index">Outputs the converted index, or null if the conversion failed.</param>
        /// <param name="error">Outputs the reason why the conversion failed, or null if it succeeded.</param>
        /// <returns>Returns true if the catalog has been converted successfully.</returns>
        internal static bool TryConvert(string catalogJson, string version, out IconIndex index, out string error)
        {
            index = null;

            if (string.IsNullOrWhiteSpace(version))
            {
                error = "The Tabler version of the catalog is missing.";
                return false;
            }

            int entryCount = string.IsNullOrWhiteSpace(catalogJson) ? -1 : CountRootObjects(catalogJson);
            if (entryCount < 0)
            {
                error = "The Tabler catalog is not a valid JSON object.";
                return false;
            }

            if (entryCount == 0)
            {
                error = "The Tabler catalog doesn't contain any icon.";
                return false;
            }

            // Turn each "{key}": {"name"... into {"name"...
            int matchCount = 0;
            string json = EntryKeyRegex.Replace(catalogJson, match =>
            {
                matchCount++;
                return "{\"name\"";
            });

            // Checked in both directions: an entry that doesn't start with "name", or a nested object that does
            if (matchCount != entryCount)
            {
                error = $"Unexpected Tabler catalog format: it contains {entryCount} entries, but {matchCount} of them start with a "
                    + "\"name\" field. The conversion expects every entry to start with it.";
                return false;
            }

            json = TagsArrayRegex.Replace(json, QuoteNumericTags);

            // Turn the root object into an array, wrapped in an object since JsonUtility can't read a root array
            int start = json.IndexOf('{');
            int end = json.LastIndexOf('}');
            json = $"{{\"icons\":[{json.Substring(start + 1, end - start - 1)}]}}";

            Catalog catalog;
            try
            {
                catalog = JsonUtility.FromJson<Catalog>(json);
            }
            catch (ArgumentException exception)
            {
                error = $"The Tabler catalog couldn't be read: {exception.Message}";
                return false;
            }

            if (catalog == null || catalog.icons == null || catalog.icons.Length != entryCount)
            {
                int readCount = catalog != null && catalog.icons != null ? catalog.icons.Length : 0;
                error = $"The Tabler catalog contains {entryCount} entries, but {readCount} have been read.";
                return false;
            }

            index = new IconIndex { version = version, icons = MakeEntries(catalog.icons) };
            error = null;
            return true;
        }

        #endregion


        #region Private API

        /// <summary>
        /// Counts the objects directly in the root object of a JSON text, which are the entries of the catalog.
        /// </summary>
        /// <param name="json">The JSON text.</param>
        /// <returns>Returns the number of objects in the root object, or -1 if the root is not a single object.</returns>
        private static int CountRootObjects(string json)
        {
            int depth = 0;
            int count = 0;
            bool hasRoot = false;
            bool isInString = false;

            for (int i = 0; i < json.Length; i++)
            {
                char c = json[i];
                if (isInString)
                {
                    // Skip the escaped character, which may be a quote
                    if (c == '\\')
                        i++;
                    else if (c == '"')
                        isInString = false;

                    continue;
                }

                switch (c)
                {
                    case '"':
                        isInString = true;
                        break;

                    case '{':
                    case '[':
                        if (depth == 0)
                        {
                            if (c != '{' || hasRoot)
                                return -1;

                            hasRoot = true;
                        }
                        else if (depth == 1 && c == '{')
                        {
                            count++;
                        }
                        depth++;
                        break;

                    case '}':
                    case ']':
                        depth--;
                        if (depth < 0)
                            return -1;
                        break;
                }
            }

            return hasRoot && depth == 0 && !isInString ? count : -1;
        }

        /// <summary>
        /// Puts quotes around the bare values (numbers and literals) of a matched <c>tags</c> array, since <see cref="JsonUtility"/>
        /// reads tags as strings.
        /// </summary>
        /// <param name="match">The match of <see cref="TagsArrayRegex"/>.</param>
        /// <returns>Returns the <c>tags</c> array, with only string values.</returns>
        private static string QuoteNumericTags(Match match)
        {
            string values = TagValueRegex.Replace(
                match.Groups[2].Value,
                value => value.Value[0] == '"' ? value.Value : $"\"{value.Value}\""
            );
            return $"{match.Groups[1].Value}{values}{match.Groups[3].Value}";
        }

        /// <summary>
        /// Makes the index entries from the icons of the catalog, skipping the icons with an invalid name.
        /// </summary>
        /// <param name="icons">The icons of the catalog.</param>
        /// <returns>Returns the index entries.</returns>
        private static IconIndexEntry[] MakeEntries(CatalogIcon[] icons)
        {
            var entries = new List<IconIndexEntry>(icons.Length);
            var invalidNames = new List<string>();

            foreach (CatalogIcon icon in icons)
            {
                if (!IconStorage.IsValidIconName(icon.name))
                {
                    invalidNames.Add(icon.name);
                    continue;
                }

                entries.Add(new IconIndexEntry
                {
                    name = icon.name,
                    category = icon.category ?? string.Empty,
                    tags = icon.tags ?? Array.Empty<string>(),
                    hasFilled = icon.styles != null && HasStyle(icon.styles.filled),
                });
            }

            if (invalidNames.Count > 0)
            {
                Debug.LogWarning($"{invalidNames.Count} icons have been skipped from the Tabler catalog, since their names use other "
                    + $"characters than lowercase letters, digits and hyphens: {string.Join(", ", invalidNames)}");
            }

            return entries.ToArray();
        }

        /// <summary>
        /// Checks if a style is defined for an icon of the catalog.
        /// </summary>
        /// <remarks>
        /// <see cref="JsonUtility"/> creates an empty instance for a missing field of a serializable class type, so a missing style is
        /// not null, but has no data.
        /// </remarks>
        /// <param name="style">The style to check.</param>
        /// <returns>Returns true if the style is defined.</returns>
        private static bool HasStyle(CatalogStyle style)
        {
            return style != null && (!string.IsNullOrEmpty(style.unicode) || !string.IsNullOrEmpty(style.version));
        }

        #endregion

    }

}
