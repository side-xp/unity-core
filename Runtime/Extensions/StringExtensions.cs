using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Extension functions for string values.
    /// </summary>
    public static class StringExtensions
    {

        #region Fields

        public const byte Ascii0 = (byte)'0';
        public const byte Ascii9 = (byte)'9';
        public const byte AsciiA = (byte)'A';
        public const byte AsciiZ = (byte)'Z';
        public const byte Asciia = (byte)'a';
        public const byte Asciiz = (byte)'z';

        #endregion


        #region Public API

        /// <summary>
        /// Shortcut for using <see cref="string.Split(string[], StringSplitOptions)"/> with one string as a separator.
        /// </summary>
        /// <param name="str">The string to split.</param>
        /// <param name="separator">The separator to use for splitting the string.</param>
        /// <param name="splitOptions">Eventual split options.</param>
        /// <returns>Returns the splitted string.</returns>
        public static string[] Split(this string str, string separator, StringSplitOptions splitOptions = StringSplitOptions.RemoveEmptyEntries)
        {
            return str.Split(new string[1] { separator }, splitOptions);
        }

        /// <summary>
        /// Splits a string by newline characters.
        /// </summary>
        /// <param name="str">The string to split.</param>
        /// <param name="splitOptions">Eventual split options.</param>
        /// <returns>Returns the splitted string.</returns>
        public static string[] SplitLines(this string str, StringSplitOptions splitOptions = StringSplitOptions.RemoveEmptyEntries)
        {
            return str.Split(new string[] { "\n", "\r\n" }, splitOptions);
        }

        /// <summary>
		/// Creates a string that contains several iterations of the given input string.
		/// </summary>
		/// <param name="str">The string to repeat.</param>
		/// <param name="iterations">The number of times the input string is repeated in the output.</param>
		/// <returns>Returns the input string repeated the given number of times.</returns>
		public static string Repeat(this string str, int iterations)
        {
            string output = "";
            for (int i = 0; i < System.Math.Max(0, iterations); i++)
                output += str;

            return output;
        }

        /// <summary>
        /// Counts the number of occurrences of a given string into another.
        /// </summary>
        /// <param name="str">The string in which you want to count the occurrences.</param>
        /// <param name="pattern">The pattern you want to count the occurrences.</param>
        /// <returns>Returns the number of occurrences found.</returns>
        public static int Occurrences(this string str, string pattern)
        {
            return Regex.Matches(str, pattern).Count;
        }

        /// <inheritdoc cref="Slice(string, int, int)"/>
        /// <returns>Returns the substring, from the given start index to the end of the string.</returns>
        public static string Slice(this string str, int start)
        {
            return Slice(str, start, str.Length);
        }

        /// <summary>
        /// Extracts a substring, from an index to another.
        /// </summary>
        /// <param name="str">The string you want to slice.</param>
        /// <param name="start">The start index of the string to extract. If negative, it's used as (str.Length - start).</param>
        /// <param name="end">The end index of the string to extract (exclusive). If negative, it's used as (str.Length - end). If the end
        /// index is lower than the start index, start and end are inverted.</param>
        /// <returns>Returns the substring, from the given start index to the given end one.</returns>
        public static string Slice(this string str, int start, int end)
        {
            if (start < 0)
                start = str.Length + start;
            if (start < 0)
                start = 0;

            if (end < 0)
                end = str.Length + end;
            if (end < 0)
                end = 0;

            if (start > end)
            {
                int tmp = start;
                start = end;
                end = tmp;
            }

            if (end > str.Length)
                end = str.Length;

            return str.Substring(start, end - start);
        }

        /// <summary>
        /// Remove all diacritic characters (accents, combined letters, emojis, ...), keeping their base character in the output string.
        /// </summary>
        /// <param name="str">The string from which you want to remove the diacritic characters.</param>
        /// <returns>Returns the processed string.</returns>
        public static string RemoveDiacritics(this string str)
        {
            str = str.Normalize(NormalizationForm.FormD);
            string output = string.Empty;

            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(c);
                if (category != UnicodeCategory.NonSpacingMark && category != UnicodeCategory.Surrogate)
                    output += c;
            }
            return output;
        }

        /// <summary>
        /// Removes all characters that are not letters or digits in the given string.
        /// </summary>
        /// <param name="str">The string from which you want to remove the characters.</param>
        /// <param name="allowedChars">The allowed characters in the output string that won't be removed.</param>
        /// <returns>Returns the processed string.</returns>
        public static string RemoveSpecialChars(this string str, string allowedChars = null)
        {
            string output = string.Empty;
            if (allowedChars == null)
                allowedChars = "";

            for (int i = 0; i < str.Length; i++)
            {
                if (!IsLetterOrDigit(str[i]))
                {
                    bool allowed = false;
                    for (int j = 0; j < allowedChars.Length; j++)
                    {
                        if (str[i] == allowedChars[j])
                        {
                            allowed = true;
                            break;
                        }
                    }

                    if (!allowed)
                        continue;
                }

                output += str[i];
            }

            return output;
        }

        /// <inheritdoc cref="PathUtility.ToPath(string, bool, char)"/>
        public static string ToPath(this string str, bool normalize = false, char separator = PathUtility.DefaultDirectorySeparator)
        {
            return PathUtility.ToPath(str, normalize, separator);
        }

        /// <inheritdoc cref="PathUtility.ToAbsolutePath(string)"/>
        public static string ToAbsolutePath(this string str)
        {
            return PathUtility.ToAbsolutePath(str);
        }

        /// <inheritdoc cref="PathUtility.ToRelativePath(string, bool)"/>
        public static string ToRelativePath(this string str, bool normalize = false)
        {
            return PathUtility.ToRelativePath(str, normalize);
        }

        /// <inheritdoc cref="PathUtility.IsProjectPath(string)"/>
        public static bool IsProjectPath(this string str)
        {
            return PathUtility.IsProjectPath(str);
        }

        /// <summary>
        /// Converts this string into a <see cref="GUIContent"/> instance.
        /// </summary>
        /// <param name="text">The text of the <see cref="GUIContent"/>.</param>
        /// <param name="icon">The icon of the <see cref="GUIContent"/>.</param>
        /// <param name="tooltip">The tooltip of the <see cref="GUIContent"/>.</param>
        /// <returns>Returns the converted <see cref="GUIContent"/>.</returns>
        public static GUIContent ToGUIContent(this string text, Texture2D icon, string tooltip)
        {
            return new GUIContent(text, icon, tooltip);
        }

        /// <inheritdoc cref="ToGUIContent(string, Texture2D, string)"/>
        public static GUIContent ToGUIContent(this string text, Texture2D icon)
        {
            return new GUIContent(text, icon);
        }

        /// <inheritdoc cref="ToGUIContent(string, Texture2D, string)"/>
        public static GUIContent ToGUIContent(this string text, string tooltip)
        {
            return new GUIContent(text, tooltip);
        }

        /// <inheritdoc cref="ToGUIContent(string, Texture2D, string)"/>
        public static GUIContent ToGUIContent(Texture2D icon, string tooltip)
        {
            return new GUIContent(icon, tooltip);
        }

        /// <inheritdoc cref="ToGUIContent(string, Texture2D, string)"/>
        public static GUIContent ToGUIContent(this string text)
        {
            return new GUIContent(text);
        }

        #endregion


        #region Private API

        /// <summary>
        /// Checks if the given character is a letter (capital or not) or a digit.
        /// </summary>
        /// <param name="character">The character you want to check.</param>
        /// <returns>Returns true if the given character is a letter or a digit.</returns>
        private static bool IsLetterOrDigit(this char character)
        {
            byte asByte = (byte)character;
            return
                (asByte >= Ascii0 && asByte <= Ascii9) ||
                (asByte >= AsciiA && asByte <= AsciiZ) ||
                (asByte >= Asciia && asByte <= Asciiz);
        }

        #endregion

    }

}