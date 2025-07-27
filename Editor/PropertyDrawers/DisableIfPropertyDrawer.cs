using SideXP.Core.Reflection;

using UnityEditor;
using UnityEngine;

namespace SideXP.Core.EditorOnly
{

    /// <inheritdoc cref="DisableIfAttribute"/>
    [CustomPropertyDrawer(typeof(DisableIfAttribute))]
    public class DisableIfPropertyDrawer : PropertyDrawer
    {

        /// <inheritdoc cref="PropertyDrawer.OnGUI(Rect, SerializedProperty, GUIContent)"/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            DisableIfAttribute attr = attribute as DisableIfAttribute;
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
            DisableIfAttribute attr = attribute as DisableIfAttribute;
            return !Evaluate(property) && attr.HideIfDisabled
                ? 0f
                : base.GetPropertyHeight(property, label);
        }

        /// <summary>
        /// Checks if the condition field or property is not valid or not checked.
        /// </summary>
        /// <param name="property">The property decorated with <see cref="DisableIfAttribute"/>.</param>
        /// <returns>Returns true if the condition field or property is not valid or not checked.</returns>
        private bool Evaluate(SerializedProperty property)
        {
            DisableIfAttribute attr = attribute as DisableIfAttribute;

            // Try to check a serialized field value
            SerializedProperty conditionProperty = property.serializedObject.FindProperty(attr.PropertyName);
            if (conditionProperty != null)
                return conditionProperty.propertyType == SerializedPropertyType.Boolean && !conditionProperty.boolValue;

            // Try to check a non-serialized field or property value
            FieldOrPropertyInfo nonSerializedConditionProperty =
                ReflectionUtility.GetFieldOrProperty(property.serializedObject.targetObject, attr.PropertyName, true);

            return nonSerializedConditionProperty == null || !nonSerializedConditionProperty.GetValue<bool>(property.serializedObject.targetObject);
        }

    }

}