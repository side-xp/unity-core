using System.Reflection;

using UnityEngine;
using UnityEditor;

using SideXP.Core.Reflection;

namespace SideXP.Core.EditorOnly
{

    /// <inheritdoc cref="BooltonAttribute"/>
    [CustomPropertyDrawer(typeof(BooltonAttribute))]
    public class BooltonPropertyDrawer : PropertyDrawer
    {

        /// <inheritdoc cref="PropertyDrawer.OnGUI(Rect, SerializedProperty, GUIContent)"/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Boolean)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            BooltonAttribute booltonAttribute = attribute as BooltonAttribute;
            bool execCallback = false;

            // Draw regular field if required
            if (booltonAttribute.Toggle)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(position, property, label);
                if (EditorGUI.EndChangeCheck())
                {
                    property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    execCallback = true;
                }
            }
            // Otherwise, draw "enable/disable" button
            else
            {
                // Draw label
                Rect rect = new Rect(position);
                rect.width = EditorGUIUtility.labelWidth;
                EditorGUI.LabelField(rect, label);

                // Draw button
                rect.x += rect.width;
                rect.width = position.width - rect.width;
                if (GUI.Button(rect, property.boolValue ? "Disable" : "Enable"))
                {
                    property.boolValue = !property.boolValue;
                    property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    execCallback = true;
                }
            }

            // Execute the callback if applicable
            if (execCallback)
            {
                foreach (Object target in property.serializedObject.targetObjects)
                    ExecuteCallback(target, booltonAttribute.CallbackName, property.boolValue);
            }
        }

        /// <summary>
        /// Try to execute the named callback on a given object.
        /// </summary>
        /// <param name="obj">The object from which to invoke the callback.</param>
        /// <param name="callbackName">The name of the boolean property of the function to invoke, which must match the following prototype:
        /// <code>void Func(bool)</code></param>
        /// <param name="value">The value to set.</param>
        private void ExecuteCallback(Object obj, string callbackName, bool value)
        {
            // Cancel if the given callback name is not valid
            if (string.IsNullOrWhiteSpace(callbackName))
                return;

            // If the callback name targets a property with a setter, use it
            PropertyInfo propertyInfo = obj.GetType().GetProperty(callbackName, ReflectionUtility.InstanceFlags | BindingFlags.Static);
            if (propertyInfo != null && propertyInfo.PropertyType == typeof(bool) && propertyInfo.CanWrite)
            {
                propertyInfo.SetValue(obj, value);
                return;
            }

            // Otherwise, try to get a function that takes a single boolean parameter
            MethodInfo methodInfo = obj.GetType().GetMethod(callbackName, ReflectionUtility.InstanceFlags | BindingFlags.Static);
            if (methodInfo != null)
            {
                ParameterInfo[] parametersInfo = methodInfo.GetParameters();
                if (parametersInfo.Length == 1 && parametersInfo[0].ParameterType == typeof(bool))
                {
                    methodInfo.Invoke(obj, new object[] { value });
                    return;
                }
            }

            Debug.LogWarning($"Failed to invoke the expected callback \"{callbackName}\" from this Boolton: No boolean property found with that name, and no method match the function prototype \"void {callbackName}(bool)\".", obj);
        }

    }

}
