using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Extension functions for <see cref="FColor"/> values.
    /// </summary>
    public static class FColorExtensions
    {

        public const byte MaxColorValue = 255;
        public const byte HalfColorValue = MaxColorValue / 2;

        /// <summary>
        /// Gets the color value from this color enum.
        /// </summary>
        /// <param name="color">The enum value of which you want to get the color.</param>
        /// <param name="ignoreAlpha">If enabled, returns a color with 100% alpha.</param>
        /// <returns>Returns the processed color.</returns>
        public static Color ToColor(this FColor color, bool ignoreAlpha = false)
        {
            Color output = Color.clear;
            // Cancel if no flag is enabled on the color value
            if (color == 0)
            {
                return output;
            }

            if (color.HasFlag(FColor.Red))
            { output.r = 1f; }
            else if (color.HasFlag(FColor.Maroon))
            { output.r = .5f; }

            if (color.HasFlag(FColor.Green))
            { output.g = 1f; }
            else if (color.HasFlag(FColor.Lime))
            { output.g = .5f; }

            if (color.HasFlag(FColor.Blue))
            { output.b = 1f; }
            else if (color.HasFlag(FColor.Navy))
            { output.b = .5f; }

            if (ignoreAlpha)
            {
                output.a = 1f;
            }
            else
            {
                if (color.HasFlag(FColor.Alpha0))
                { output.a = 0f; }
                else if (color.HasFlag(FColor.Alpha12))
                { output.a = 1f / 8; }
                else if (color.HasFlag(FColor.Alpha25))
                { output.a = 1f / 4; }
                else if (color.HasFlag(FColor.Alpha50))
                { output.a = 1f / 2; }
                else if (color.HasFlag(FColor.Alpha75))
                { output.a = .75f; }
                else if (color.HasFlag(FColor.Alpha87))
                { output.a = .87f; }
                else if (color.HasFlag(FColor.Alpha100))
                { output.a = 1f; }
            }

            return output;
        }

        /// <inheritdoc cref="ToColor(FColor, bool)"/>
        /// <param name="alpha">The fixed alpha value (between 0 and 1) of the color (ignoring the alpha value from the enum).</param>
        public static Color ToColor(this FColor color, float alpha)
        {
            Color output = color.ToColor(true);
            output.a = alpha;
            return output;
        }

        /// <inheritdoc cref="ToColor(FColor, bool)"/>
        public static Color32 ToColor32(this FColor color, bool ignoreAlpha = false)
        {
            Color32 output = new Color32(0, 0, 0, 0);
            // Cancel if no flag is enabled on the color value
            if (color == 0)
            {
                return output;
            }

            if (color.HasFlag(FColor.Red))
            { output.r = MaxColorValue; }
            else if (color.HasFlag(FColor.Maroon))
            { output.r = HalfColorValue; }

            if (color.HasFlag(FColor.Green))
            { output.g = MaxColorValue; }
            else if (color.HasFlag(FColor.Lime))
            { output.g = HalfColorValue; }

            if (color.HasFlag(FColor.Blue))
            { output.b = MaxColorValue; }
            else if (color.HasFlag(FColor.Navy))
            { output.b = HalfColorValue; }

            if (ignoreAlpha)
            {
                output.a = MaxColorValue;
            }
            else
            {
                if (color.HasFlag(FColor.Alpha0))
                { output.a = 0; }
                else if (color.HasFlag(FColor.Alpha12))
                { output.a = MaxColorValue / 8; }
                else if (color.HasFlag(FColor.Alpha25))
                { output.a = MaxColorValue / 4; }
                else if (color.HasFlag(FColor.Alpha50))
                { output.a = MaxColorValue / 2; }
                else if (color.HasFlag(FColor.Alpha75))
                { output.a = (byte)(MaxColorValue * .75f); }
                else if (color.HasFlag(FColor.Alpha87))
                { output.a = (byte)(MaxColorValue * .87f); }
                else if (color.HasFlag(FColor.Alpha100))
                { output.a = MaxColorValue; }
            }

            return output;
        }

        /// <inheritdoc cref="ToColor(FColor, float)"/>
        /// <param name="alpha">The fixed alpha value (between 0 and 255) of the color (ignoring the alpha value from the enum).</param>
        public static Color32 ToColor32(this FColor color, byte alpha)
        {
            Color32 output = color.ToColor32(true);
            output.a = alpha;
            return output;
        }

        /// <inheritdoc cref="ToColor(FColor, float)"/>
        public static Color32 ToColor32(this FColor color, float alpha)
        {
            return color.ToColor32((byte)(alpha != 0 ? (MaxColorValue / alpha) : 0));
        }

    }

}