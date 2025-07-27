using UnityEditor;
using UnityEngine;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Draws a separator line before the property field.
    /// </summary>
    [CustomPropertyDrawer(typeof(SeparatorAttribute))]
    public class SeparatorPropertyDrawer : PropertyDrawer
    {

        public const float Space = 12f;
        public const float SeparatorTotalHeight = Space * 2 + MoreEditorGUI.DefaultSeparatorSize;

        /// <inheritdoc cref="PropertyDrawer.OnGUI(Rect, SerializedProperty, GUIContent)"/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SeparatorAttribute attr = attribute as SeparatorAttribute;

            Rect rect = new Rect(position);
            rect.height = MoreEditorGUI.DefaultSeparatorSize;
            rect.y += Space;

            if (attr.Wide)
            {
                rect.width = EditorGUIUtility.currentViewWidth;
                rect.x = 0;
            }

            EditorGUI.DrawRect(rect, MoreEditorGUI.SeparatorColor);

            rect.width = position.width;
            rect.y += Space;
            rect.height = position.height - SeparatorTotalHeight;

            EditorGUI.PropertyField(rect, property, label);
        }

        /// <inheritdoc cref="PropertyDrawer.GetPropertyHeight(SerializedProperty, GUIContent)"/>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) + SeparatorTotalHeight;
        }

    }

}