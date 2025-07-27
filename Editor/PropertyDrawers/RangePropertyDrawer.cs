using UnityEditor;
using UnityEngine;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Draws a range property on a single line.
    /// </summary>
    [CustomPropertyDrawer(typeof(Range))]
    public class RangePropertyDrawer : PropertyDrawer
    {

        private const string MinProp = "_min";
        private const string MaxProp = "_max";
        private const float LabelWidth = 26f;

        /// <summary>
        /// Gets the height for drawing this range property.
        /// </summary>
        public static float GetHeight()
        {
            return EditorGUIUtility.singleLineHeight;
        }

        /// <summary>
        /// Draws the min/max fields on GUI.
        /// </summary>
        /// <inheritdoc cref="PropertyDrawer.OnGUI(Rect, SerializedProperty, GUIContent)"/>
        public static void DrawRangeGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.hasMultipleDifferentValues)
            {
                using (new EditorGUI.MixedValueScope(true))
                    EditorGUI.PropertyField(position, property, label);

                return;
            }

            SerializedProperty minProp = property.FindPropertyRelative(MinProp);
            SerializedProperty maxProp = property.FindPropertyRelative(MaxProp);

            // Draw label
            Rect rect = new Rect(position);
            rect.width = EditorGUIUtility.labelWidth;
            EditorGUI.LabelField(rect, label);

            rect.x += rect.width;
            // Align label+field width with Vector fields
            rect.width = (position.width - rect.width - MoreGUI.HMargin * 2) / 3;

            using (new LabelWidthScope(LabelWidth))
            {
                // Draw min property
                EditorGUI.PropertyField(rect, minProp, minProp.GetLabel());

                // Draw max property
                rect.x += rect.width + MoreGUI.HMargin;
                EditorGUI.PropertyField(rect, maxProp, maxProp.GetLabel());
            }

            property.serializedObject.ApplyModifiedProperties();
        }

        /// <inheritdoc cref="PropertyDrawer.OnGUI(Rect, SerializedProperty, GUIContent)"/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            DrawRangeGUI(position, property, label);
        }

        /// <inheritdoc cref="PropertyDrawer.GetPropertyHeight(SerializedProperty, GUIContent)"/>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return GetHeight();
        }

    }

}
