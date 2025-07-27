using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Miscellaneous functions and values for drawing user interfaces.
    /// </summary>
    public static class MoreGUI
    {

        #region Fields

        public const float HeightXL = 48f;
        public const float HeightL = 36f;
        public const float HeightM = 28f;
        public const float HeightS = 20f;
        public const float HeightXS = 16f;

        public const float WidthXL = 200f;
        public const float WidthL = 148f;
        public const float WidthM = 112f;
        public const float WidthS = 80f;
        public const float WidthXS = 40f;

        /// <summary>
        /// Size of an horizontal margin.
        /// </summary>
        public const float HMargin = 2f;

        /// <summary>
        /// Size of a vertical margin.
        /// </summary>
        public const float VMargin = 2f;

        public static readonly GUILayoutOption HeightXLOpt = GUILayout.Height(HeightXL);
        public static readonly GUILayoutOption HeightLOpt = GUILayout.Height(HeightL);
        public static readonly GUILayoutOption HeightMOpt = GUILayout.Height(HeightM);
        public static readonly GUILayoutOption HeightSOpt = GUILayout.Height(HeightS);
        public static readonly GUILayoutOption HeightXSOpt = GUILayout.Height(HeightXS);

        public static readonly GUILayoutOption WidthXLOpt = GUILayout.Width(WidthXL);
        public static readonly GUILayoutOption WidthLOpt = GUILayout.Width(WidthL);
        public static readonly GUILayoutOption WidthMOpt = GUILayout.Width(WidthM);
        public static readonly GUILayoutOption WidthSOpt = GUILayout.Width(WidthS);
        public static readonly GUILayoutOption WidthXSOpt = GUILayout.Width(WidthXS);

        #endregion


        #region GUI

        /// <summary>
        /// Make a single-line text field where the user can edit a integer value.
        /// </summary>
        /// <param name="position">Rectangle on the screen to use for the field.</param>
        /// <param name="value">The current value.</param>
        /// <param name="style">The style to use. If left out, the <see cref="GUISkin.textField"/> style from the current
        /// <see cref="GUISkin"/> is used.</param>
        /// <returns>Returns the edited value.</returns>
        public static int IntField(Rect position, int value, GUIStyle style)
        {
            string valueText = GUI.TextField(position, value.ToString(), style);
            return int.TryParse(valueText, out int parsedValue) ? parsedValue : value;
        }

        /// <inheritdoc cref="IntField(Rect, int, GUIStyle)"/>
        public static int IntField(Rect position, int value)
        {
            return IntField(position, value, GUI.skin.textField);
        }

        /// <inheritdoc cref="IntField(Rect, int, GUIStyle)"/>
        /// <remarks>Uses layout GUI.</remarks>
        public static int IntField(int value, GUIStyle style, params GUILayoutOption[] options)
        {
            string valueText = GUILayout.TextField(value.ToString(), style, options);
            return int.TryParse(valueText, out int parsedValue) ? parsedValue : value;
        }

        /// <inheritdoc cref="IntField(int, GUIStyle)"/>
        public static int IntField(int value, params GUILayoutOption[] options)
        {
            return IntField(value, GUI.skin.textField, options);
        }

        #endregion

    }

}