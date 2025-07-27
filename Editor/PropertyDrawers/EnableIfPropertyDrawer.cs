using SideXP.Core.Reflection;

using UnityEditor;
using UnityEngine;

namespace SideXP.Core.EditorOnly
{

    /// <inheritdoc cref="EnableIfAttribute"/>
    [CustomPropertyDrawer(typeof(EnableIfAttribute))]
    public class EnableIfPropertyDrawer : PropertyDrawer
    {

        /// <inheritdoc cref="PropertyDrawer.OnGUI(Rect, SerializedProperty, GUIContent)"/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EnableIfAttribute attr = attribute as EnableIfAttribute;
            bool valid = Evaluate(property);

            // Cancel if the condition is not fulfilled and the field should be hidden
            if (!valid && attr.HideIfDisabled)
                return;

            GUI.enabled = valid;
            EditorGUI.PropertyField(position, property, label);
            GUI.enabled = true;
        }

        /// <inheritdoc cref="PropertyDrawer.GetPropertyHeight(SerializedProperty, GUIContent)"/>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            EnableIfAttribute attr = attribute as EnableIfAttribute;
            return !Evaluate(property) && attr.HideIfDisabled
                ? 0f
                : base.GetPropertyHeight(property, label);
        }

        /// <summary>
        /// Checks if the condition field or property is valid and checked.
        /// </summary>
        /// <param name="property">The property decorated with <see cref="EnableIfAttribute"/>.</param>
        /// <returns>Returns true if the condition field or property is valid and checked.</returns>
        private bool Evaluate(SerializedProperty property)
        {
            EnableIfAttribute attr = attribute as EnableIfAttribute;

            // Try to check a serialized field value
            SerializedProperty conditionProperty = property.serializedObject.FindProperty(attr.PropertyName);
            if (conditionProperty != null)
                return conditionProperty.propertyType == SerializedPropertyType.Boolean && conditionProperty.boolValue;

            // Try to check a non-serialized field or property value
            FieldOrPropertyInfo nonSerializedConditionProperty =
                ReflectionUtility.GetFieldOrProperty(property.serializedObject.targetObject, attr.PropertyName, true);

            return nonSerializedConditionProperty != null && nonSerializedConditionProperty.GetValue<bool>(property.serializedObject.targetObject);
        }

    }

}