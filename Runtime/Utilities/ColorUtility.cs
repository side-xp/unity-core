using UnityEngine;

using UnityColorUtility = UnityEngine.ColorUtility;

namespace SideXP.Core
{

    /// <summary>
    /// Miscellanous functions for working with <see cref="Color"/> and <see cref="Color32"/> values.
    /// </summary>
    public static class ColorUtility
    {

        #region Fields

        /// <summary>
        /// The maximum color value.
        /// </summary>
        public const int MaxColorValue = 255;

        /// <summary>
        /// The maximum color value as float.
        /// </summary>
        public const float MaxColorValueFloat = 255f;

        /// <summary>
        /// The number of subdivs a single color channel can have when generating random color.
        /// </summary>
        private const int GeneratedColorSteps = 4;

        #endregion


        #region Public API

        /// <summary>
        /// Converts the given color into an hexadecimal string value.
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>Returns the "color string" with format "RRGGBB".</returns>
        public static string ToHexRGB(Color color)
        {
            return UnityColorUtility.ToHtmlStringRGB(color);
        }

        /// <inheritdoc cref="ToHexRGBA(Color)"/>
        public static string ToHexRGB(Color32 color)
        {
            return ToHexRGB(color);
        }

        /// <inheritdoc cref="ToHexRGB(Color)"/>
        /// <returns>Returns the "color string" with format "RRGGBBAA".</returns>
        public static string ToHexRGBA(Color color)
        {
            return UnityColorUtility.ToHtmlStringRGBA(color);
        }

        /// <inheritdoc cref="ToHexRGBA(Color)"/>
        public static string ToHexRGBA(Color32 color)
        {
            return ToHexRGBA(color);
        }

        /// <summary>
        /// Sets the given color values from the given hexadecimal string.
        /// </summary>
        /// <param name="color">The color to override.</param>
        /// <param name="hexString">
        /// The hexadecimal string:<br/>
        /// - #RGB (becomes RRGGBB)<br/>
        /// - #RRGGBB<br/>
        /// - #RGBA  (becomes RRGGBBAA)<br/>
        /// - #RRGGBBAA
        /// </param>
        /// <returns>Returns true if the string has been parsed successfully.</returns>
        public static bool FromHex(ref Color color, string hexString)
        {
            //string[] split = ParseHexColorString(hexString);
            //if (split == null)
            //    return false;

            //color.r = int.Parse(split[0], System.Globalization.NumberStyles.HexNumber) / MaxColorValueFloat;
            //color.g = int.Parse(split[1], System.Globalization.NumberStyles.HexNumber) / MaxColorValueFloat;
            //color.b = int.Parse(split[2], System.Globalization.NumberStyles.HexNumber) / MaxColorValueFloat;
            //color.a = int.Parse(split[3], System.Globalization.NumberStyles.HexNumber) / MaxColorValueFloat;
            //return true;

            return UnityColorUtility.TryParseHtmlString(hexString, out color);
        }

        /// <summary>
        /// Creates a color value from the given hexadecimal string.
        /// </summary>
        /// <inheritdoc cref="FromHex(ref Color, string)"/>
        /// <returns>Returns the created color value.</returns>
        public static Color FromHex(string hexString)
        {
            Color output = Color.black;
            FromHex(ref output, hexString);
            return output;
        }

        /// <inheritdoc cref="FromHex(ref Color string)"/>
        public static bool FromHex32(ref Color32 color, string hexString)
        {
            string[] split = ParseHexColorString(hexString);
            if (split == null)
                return false;

            color.r = byte.Parse(split[0], System.Globalization.NumberStyles.HexNumber);
            color.g = byte.Parse(split[1], System.Globalization.NumberStyles.HexNumber);
            color.b = byte.Parse(split[2], System.Globalization.NumberStyles.HexNumber);
            color.a = byte.Parse(split[3], System.Globalization.NumberStyles.HexNumber);
            return true;
        }

        /// <inheritdoc cref="FromHex32(ref Color32, string)"/>
        public static Color32 FromHex32(string hexString)
        {
            Color32 output = new Color32(0, 0, 0, MaxColorValue);
            FromHex32(ref output, hexString);
            return output;
        }

        /// <summary>
        /// Generates a random color.
        /// </summary>
        /// <param name="subdivs">The number of subdivs a single color channel can have when generating random color. As an example, if you
        /// set 2, a channel can get value 0, 0.5 or 1.</param>
        /// <returns>Returns the generated color.</returns>
        public static Color GenerateRandomColor(int subdivs = GeneratedColorSteps)
        {
            Color color = Color.white;
            float step = 1f / subdivs;
            color.r = Random.Range(0, subdivs + 1) * step;
            color.g = Random.Range(0, subdivs + 1) * step;
            color.b = Random.Range(0, subdivs + 1) * step;
            return color;
        }

        /// <summary>
        /// Gets the appropriate tint to make the content over the given color as visible as possible.
        /// </summary>
        /// <param name="color">The color to process.</param>
        /// <returns>Returns <see cref="Color.black"/> if the grayscale of the given color is over 0.5, otherwise
        /// <see cref="Color.white"/>.</returns>
        public static Color GetOverlayTint(Color color)
        {
            return color.grayscale >= .5f ? Color.black : Color.white;
        }

        /// <inheritdoc cref="GetOverlayTint(Color)"/>
        public static Color32 GetOverlayTint(Color32 color)
        {
            return GetOverlayTint(color);
        }

        #endregion


        #region Private API

        /// <summary>
        /// Splits an hexadecimal color string in 4 hex values.
        /// </summary>
        /// <param name="hexString">The hexadecimal string (like F9F9F9, F9F9F9F9, #F9F9F9, #F9F9F9F9).</param>
        /// <returns>Returns an array with 4 values, or null if the given hexadecimal string can't be parsed.</returns>
        private static string[] ParseHexColorString(string hexString)
        {
            hexString = hexString.Replace("#", "");
            if (hexString.Length < 8)
            {
                hexString += "ff";
            }

            string[] split = new string[4];
            try
            {
                split[0] = hexString.Substring(0, 2);
                split[1] = hexString.Substring(2, 2);
                split[2] = hexString.Substring(4, 2);
                split[3] = hexString.Substring(6, 2);
            }
            catch (System.Exception)
            {
                return null;
            }

            return split;
        }

        #endregion

    }

}