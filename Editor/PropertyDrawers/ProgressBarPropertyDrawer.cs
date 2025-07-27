using UnityEngine;
using UnityEditor;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Draws a progress bar next to a numeric field in the editor.
    /// </summary>
    [CustomPropertyDrawer(typeof(ProgressBarAttribute))]
    public class ProgressBarPropertyDrawer : PropertyDrawer
    {

        private const string GuiControlNamePrefix = nameof(ProgressBarPropertyDrawer) + "_";
        private const float BackgroundColorMultiplier = .4f;

        /// <inheritdoc cref="PropertyDrawer.OnGUI(Rect, SerializedProperty, GUIContent)"/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Cancel if the property doesn't have appropriate value type
            if (property.hasMultipleDifferentValues || (property.propertyType != SerializedPropertyType.Float && property.propertyType != SerializedPropertyType.Integer))
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            ProgressBarAttribute attr = attribute as ProgressBarAttribute;
            string controlName = GuiControlNamePrefix + label.text;
            bool useFullWidth = attr.Wide && GUI.GetNameOfFocusedControl() != controlName;

            using (new EnabledScope(!attr.Readonly))
            {
                // The bar will take the full field width if the ReplaceField option is enabled and the focused control is not that field
                float barWidth = useFullWidth
                    ? position.width - EditorGUIUtility.labelWidth - MoreGUI.HMargin
                    : (position.width - EditorGUIUtility.labelWidth - MoreGUI.HMargin) / 2;

                float fieldWidth = position.width - barWidth - MoreGUI.HMargin;

                Rect rect = new Rect(position);
                rect.width = useFullWidth ? position.width : fieldWidth;

                GUI.SetNextControlName(controlName);
                EditorGUI.PropertyField(rect, property, label);

                float value = property.propertyType == SerializedPropertyType.Float ? property.floatValue : property.intValue;
                if (attr.Clamp)
                {
                    value = Mathf.Clamp(value, attr.Min, attr.Max);
                    if (property.propertyType == SerializedPropertyType.Float)
                        property.floatValue = value;
                    else if (property.propertyType == SerializedPropertyType.Integer)
                        property.intValue = Mathf.FloorToInt(value);
                    property.serializedObject.ApplyModifiedProperties();
                }

                rect.x += fieldWidth + MoreGUI.HMargin;
                rect.width = barWidth;

                float ratio = Mathf.Clamp01(value.Ratio(attr.Min, attr.Max));
                Color color = attr.Color;
                color.a = 1f;
                Color backgroundColor = color * BackgroundColorMultiplier;
                backgroundColor.a = 1f;
                Color fontColor = ratio >= .5f ? color.GetOverlayTint() : backgroundColor.GetOverlayTint();

                EditorGUI.DrawRect(rect, backgroundColor);
                EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width * ratio, rect.height), color);
                EditorGUI.LabelField(rect, attr.GetLabel(value), EditorStyles.label.TextAlignment(TextAnchor.MiddleCenter).FontColor(fontColor).Bold());
            }
        }

    }

}