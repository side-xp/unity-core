using System;

using UnityEngine;
using UnityEditor;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Synchronizes the serialized <see cref="Type.AssemblyQualifiedName"/> in a property marked with <see cref="TypeRefAttribute"/> with
    /// the current name of the type, ensuring that the value is still valid even after renaming that type.
    /// </summary>
    [CustomPropertyDrawer(typeof(TypeRefAttribute))]
    public class TypeRefPropertyDrawer : PropertyDrawer
    {

        /// <inheritdoc cref="PropertyDrawer.OnGUI(Rect, SerializedProperty, GUIContent)"/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Use normal editor if multiple values selected
            if (property.hasMultipleDifferentValues)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            if (property.propertyType != SerializedPropertyType.String)
            {
                Debug.LogWarning($"You can only use {nameof(TypeRefAttribute)} on string properties.");
            }
            else
            {
                if (TypesMigration.Resolve(property.stringValue, out Type type))
                {
                    if (property.stringValue != type.AssemblyQualifiedName)
                    {
                        property.stringValue = type.AssemblyQualifiedName;
                        property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    }
                }
            }

            using (new EnabledScope(!(attribute as TypeRefAttribute).Readonly))
                EditorGUI.PropertyField(position, property, label);
        }

    }

}