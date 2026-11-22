using System;
using System.Collections.Generic;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Filters the icons of the index by search text, category and style, as in the editor icons window.
    /// </summary>
    internal static class IconFilter
    {

        #region Fields

        /// <summary>
        /// Characters that separate the words of a search.
        /// </summary>
        private static readonly char[] WordSeparators = { ' ', '\t' };

        #endregion


        #region Public API

        /// <summary>
        /// Gets the icons that match a search.
        /// </summary>
        /// <param name="icons">The icons to filter.</param>
        /// <param name="search">The search text. See <see cref="Matches"/> for how it's matched.</param>
        /// <param name="category">The category of the icons to keep, or null or empty for all categories.</param>
        /// <param name="isFilledOnly">If enabled, only keeps the icons that exist in the filled style.</param>
        /// <returns>Returns the matching icons, in the same order.</returns>
        internal static List<IconInfo> Filter(IReadOnlyList<IconInfo> icons, string search, string category, bool isFilledOnly)
        {
            string[] words = GetSearchWords(search);
            var matchingIcons = new List<IconInfo>();
            foreach (IconInfo icon in icons)
            {
                if (Matches(icon, words, category, isFilledOnly))
                    matchingIcons.Add(icon);
            }
            return matchingIcons;
        }

        /// <summary>
        /// Splits a search text into words.
        /// </summary>
        /// <param name="search">The search text.</param>
        /// <returns>Returns the words of the search, without empty ones.</returns>
        internal static string[] GetSearchWords(string search)
        {
            return string.IsNullOrWhiteSpace(search)
                ? Array.Empty<string>()
                : search.Split(WordSeparators, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Checks if an icon matches a search.
        /// </summary>
        /// <remarks>
        /// Every word must be found, ignoring case, in the icon name or in one of its tags. So <c>arrow left</c> matches
        /// <c>arrow-left</c>, and <c>house</c> matches <c>home</c> through its tags.
        /// </remarks>
        /// <param name="icon">The icon to check.</param>
        /// <param name="words">The words of the search (see <see cref="GetSearchWords"/>).</param>
        /// <param name="category">The category of the icons to keep, or null or empty for all categories.</param>
        /// <param name="isFilledOnly">If enabled, only keeps the icons that exist in the filled style.</param>
        /// <returns>Returns true if the icon matches.</returns>
        internal static bool Matches(IconInfo icon, string[] words, string category, bool isFilledOnly)
        {
            if (isFilledOnly && !icon.HasFilled)
                return false;

            if (!string.IsNullOrEmpty(category) && icon.Category != category)
                return false;

            foreach (string word in words)
            {
                if (!Contains(icon.Name, word) && !HasTagContaining(icon.Tags, word))
                    return false;
            }
            return true;
        }

        #endregion


        #region Private API

        /// <summary>
        /// Checks if a text contains a word, ignoring case.
        /// </summary>
        /// <param name="text">The text in which to search.</param>
        /// <param name="word">The word to find.</param>
        /// <returns>Returns true if the text contains the word.</returns>
        private static bool Contains(string text, string word)
        {
            return text != null && text.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Checks if one of the tags of an icon contains a word, ignoring case.
        /// </summary>
        /// <param name="tags">The tags of the icon.</param>
        /// <param name="word">The word to find.</param>
        /// <returns>Returns true if a tag contains the word.</returns>
        private static bool HasTagContaining(IReadOnlyList<string> tags, string word)
        {
            if (tags == null)
                return false;

            foreach (string tag in tags)
            {
                if (Contains(tag, word))
                    return true;
            }
            return false;
        }

        #endregion

    }

}
