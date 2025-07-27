using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Extension functions for <see cref="Color"/> and <see cref="Color32"/> values.
    /// </summary>
    public static class ColorExtensions
    {

        /// <summary>
        /// Sets the components of the given color.
        /// </summary>
        /// <param name="color">The color of which you want to set the values.</param>
        /// <param name="r">The red value of the color, between 0 and 1.</param>
        /// <param name="g">The green value of the color, between 0 and 1.</param>
        /// <param name="b">The blue value of the color, between 0 and 1.</param>
        /// <param name="a">The alpha value of the color, between 0 and 1.</param>
        public static void Set(this ref Color color, float r, float g, float b, float a)
        {
            color.r = r;
            color.g = g;
            color.b = b;
            color.a = a;
        }

        /// <inheritdoc cref="Set(ref Color, float, float, float, float)"/>
        /// <param name="value">The value for all color channels of the color, between 0 and 1.</param>
        /// <param name="alpha">The alpha value of the color, between 0 and 1.</param>
        public static void Set(this ref Color color, float value, float alpha)
        {
            color.r = value;
            color.g = value;
            color.b = value;
            color.a = alpha;
        }

        /// <inheritdoc cref="Set(ref Color, float, float)"/>
        public static void Set(this ref Color color, float value)
        {
            color.r = value;
            color.g = value;
            color.b = value;
            color.a = value;
        }

        /// <inheritdoc cref="Set(ref Color, float, float, float, float)"/>
        /// <param name="r">The red value of the color, between 0 and 255.</param>
        /// <param name="g">The green value of the color, between 0 and 255.</param>
        /// <param name="b">The blue value of the color, between 0 and 255.</param>
        /// <param name="a">The alpha value of the color, between 0 and 255.</param>
        public static void Set(this ref Color32 color, byte r, byte g, byte b, byte a)
        {
            color.r = r;
            color.g = g;
            color.b = b;
            color.a = a;
        }

        /// <inheritdoc cref="Set(ref Color, float, float)"/>
        /// <param name="value">The blue value of the color, between 0 and 255.</param>
        /// <param name="alpha">The alpha value of the color, between 0 and 255.</param>
        public static void Set(this ref Color32 color, byte value, byte alpha)
        {
            color.r = value;
            color.g = value;
            color.b = value;
            color.a = alpha;
        }

        /// <inheritdoc cref="Set(ref Color, float)"/>
        public static void Set(this ref Color32 color, byte value)
        {
            color.r = value;
            color.g = value;
            color.b = value;
            color.a = value;
        }

        /// <inheritdoc cref="ColorUtility.ToHexRGB(Color)"/>
        public static string ToHexRGB(this Color color)
        {
            return ColorUtility.ToHexRGB(color);
        }

        /// <inheritdoc cref="ColorUtility.ToHexRGB(Color32)"/>
        public static string ToHexRGB(this Color32 color)
        {
            return ColorUtility.ToHexRGB(color);
        }

        /// <inheritdoc cref="ColorUtility.ToHexRGBA(Color)"/>
        public static string ToHexRGBA(this Color color)
        {
            return ColorUtility.ToHexRGBA(color);
        }

        /// <inheritdoc cref="ColorUtility.ToHexRGBA(Color)"/>
        public static string ToHexRGBA(this Color32 color)
        {
            return ColorUtility.ToHexRGBA(color);
        }

        /// <inheritdoc cref="ColorUtility.FromHex(ref Color, string)"/>
        public static bool FromHex(this Color color, string hexString)
        {
            return ColorUtility.FromHex(ref color, hexString);
        }

        /// <inheritdoc cref="ColorUtility.FromHex32(ref Color32, string)"/>
        public static bool FromHex32(this Color32 color, string hexString)
        {
            return ColorUtility.FromHex32(ref color, hexString);
        }

        /// <inheritdoc cref="ColorUtility.GetOverlayTint(Color)"/>
        public static Color GetOverlayTint(this Color color)
        {
            return ColorUtility.GetOverlayTint(color);
        }

        /// <inheritdoc cref="ColorUtility.GetOverlayTint(Color32)"/>
        public static Color32 GetOverlayTint(this Color32 color)
        {
            return ColorUtility.GetOverlayTint(color);
        }

    }

}