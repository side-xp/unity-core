using UnityEngine;
using UnityEditor;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Custom property drawer for <see cref="OptionalValue{T}"/> fields.
    /// </summary>
    [CustomPropertyDrawer(typeof(OptionalValue<>))]
    public class OptionalValuePropertyDrawer : PropertyDrawer
    {

        private const string EnabledProp = "_enabled";
        private const string ValueProp = "_value";

        /// <inheritdoc cref="PropertyDrawer.OnGUI(Rect, SerializedProperty, GUIContent)"/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Use normal editor if multiple values selected
            if (property.hasMultipleDifferentValues)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            Rect rect = new Rect(position);
            rect.width = rect.height;

            // Toggle
            SerializedProperty enabledProp = property.FindPropertyRelative(EnabledProp);
            enabledProp.boolValue = EditorGUI.Toggle(rect, enabledProp.boolValue);

            // Label & value field
            rect.x += rect.width;
            using (new LabelWidthScope(EditorGUIUtility.labelWidth - rect.width))
            {
                rect.width = position.width - rect.width;
                SerializedProperty valueProp = property.FindPropertyRelative(ValueProp);
                using (new EnabledScope(enabledProp.boolValue))
                    EditorGUI.PropertyField(rect, valueProp, label);
            }
        }

        /// <inheritdoc cref="PropertyDrawer.GetPropertyHeight(SerializedProperty, GUIContent)"/>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

    }

}