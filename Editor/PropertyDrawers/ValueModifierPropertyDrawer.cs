using System;

using UnityEditor;
using UnityEngine;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Draws a <see cref="ValueModifier"/> property on a single line, with its operator and its value side by side.<br/>
    /// If the property is an element of an array or a list, a hint displays the running total of the modifiers up to that element,
    /// computed from a base value of 0 (so a list starting with a <see cref="EOperator.Set"/> modifier shows the actual totals).
    /// </summary>
    [CustomPropertyDrawer(typeof(ValueModifier))]
    public class ValueModifierPropertyDrawer : PropertyDrawer
    {

        private const string OperatorProp = "_operator";
        private const string ValueProp = "_value";

        /// <summary>
        /// The custom style for the running total hint.
        /// </summary>
        private static GUIStyle s_hintLabelStyle = null;

        /// <inheritdoc cref="PropertyDrawer.OnGUI(Rect, SerializedProperty, GUIContent)"/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.hasMultipleDifferentValues)
            {
                using (new EditorGUI.MixedValueScope(true))
                {
                    EditorGUI.PropertyField(position, property, label, true);
                }
                return;
            }

            SerializedProperty operatorProp = property.FindPropertyRelative(OperatorProp);
            SerializedProperty valueProp = property.FindPropertyRelative(ValueProp);

            EditorGUI.BeginProperty(position, label, property);
            Rect rect = EditorGUI.PrefixLabel(position, label);

            // The prefix label already applies the indentation, so the fields must not be indented again
            int indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Draw operator field
            Rect operatorRect = new Rect(rect);
            operatorRect.width = Mathf.Min(MoreGUI.WidthS, rect.width / 2);
            EditorGUI.PropertyField(operatorRect, operatorProp, GUIContent.none);

            // Draw value field, leaving space for the hint if applicable
            Rect valueRect = new Rect(rect);
            valueRect.xMin = operatorRect.xMax + MoreGUI.HMargin;

            if (TryGetRunningTotal(property, out float total))
            {
                // Use the hint text as tooltip too, so the total can still be read if it's clipped
                string hintText = $"{total:0.###}";
                GUIContent hint = new GUIContent(hintText, hintText);
                valueRect.width -= MoreGUI.WidthXS + MoreGUI.HMargin;

                Rect hintRect = new Rect(rect);
                hintRect.xMin = valueRect.xMax + MoreGUI.HMargin;
                EditorGUI.LabelField(hintRect, hint, HintLabelStyle);
            }

            EditorGUI.PropertyField(valueRect, valueProp, GUIContent.none);

            EditorGUI.indentLevel = indentLevel;
            EditorGUI.EndProperty();
        }

        /// <inheritdoc cref="PropertyDrawer.GetPropertyHeight(SerializedProperty, GUIContent)"/>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return property.hasMultipleDifferentValues
                ? EditorGUI.GetPropertyHeight(property, label, true)
                : EditorGUIUtility.singleLineHeight;
        }

        /// <summary>
        /// The custom style for the running total hint, greyed out and in italic.
        /// </summary>
        private static GUIStyle HintLabelStyle
        {
            get
            {
                if (s_hintLabelStyle == null)
                {
                    s_hintLabelStyle = new GUIStyle(EditorStyles.label)
                    {
                        fontStyle = FontStyle.Italic,
                        fontSize = 10,
                        alignment = TextAnchor.MiddleRight,
                        clipping = TextClipping.Clip
                    };
                    s_hintLabelStyle.normal.textColor = EditorStyles.centeredGreyMiniLabel.normal.textColor;
                }
                return s_hintLabelStyle;
            }
        }

        /// <summary>
        /// Computes the running total of the modifiers in the array or list that contains the given property, from the first element up to
        /// that property, using 0 as base value.
        /// </summary>
        /// <param name="property">The <see cref="ValueModifier"/> property of which to compute the running total.</param>
        /// <param name="total">Outputs the running total up to the given property.</param>
        /// <returns>
        /// Returns true if the running total has been computed, or false if the property is not directly an element of an array or a list,
        /// or if one of the modifiers has an invalid operator.
        /// </returns>
        private static bool TryGetRunningTotal(SerializedProperty property, out float total)
        {
            total = 0;
            if (!property.TryGetArrayElementInfo(out string arrayPath, out int index))
                return false;

            // Read the serialized data rather than the target object, so the total reflects the values being edited
            SerializedProperty arrayProp = property.serializedObject.FindProperty(arrayPath);
            if (arrayProp == null || !arrayProp.isArray)
                return false;

            try
            {
                for (int i = 0; i <= index && i < arrayProp.arraySize; i++)
                {
                    SerializedProperty elementProp = arrayProp.GetArrayElementAtIndex(i);
                    ValueModifier modifier = new ValueModifier(
                        (EOperator)elementProp.FindPropertyRelative(OperatorProp).intValue,
                        elementProp.FindPropertyRelative(ValueProp).floatValue
                    );
                    total = modifier.ApplyTo(total);
                }
            }
            // Thrown if an operator value is not defined, which may happen if the enum changed after the data was serialized
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }
            return true;
        }

    }

}
