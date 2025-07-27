using UnityEngine;
using UnityEditor;

namespace SideXP.Core.EditorOnly
{

    /// <inheritdoc cref="FolderPathAttribute"/>
    [CustomPropertyDrawer(typeof(FolderPathAttribute))]
    public class FolderPathPropertyDrawer : PropertyDrawer
    {

        /// <inheritdoc cref="PropertyDrawer.OnGUI(Rect, SerializedProperty, GUIContent)"/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.y += (position.height - EditorGUIUtility.singleLineHeight) / 2f;
            position.height = EditorGUIUtility.singleLineHeight;

            // Cancel if multiple values selected or invalid property type
            if (property.hasMultipleDifferentValues || property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            FolderPathAttribute attr = attribute as FolderPathAttribute;

            EditorGUI.BeginChangeCheck();
            property.stringValue = MoreEditorGUI.FolderPathField(position, property.IsArrayElement() ? GUIContent.none : label, property.stringValue, attr.Title, attr.DefaultName, attr.AllowExternal);
            if (EditorGUI.EndChangeCheck())
                property.serializedObject.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return property.IsArrayElement()
                ? EditorGUIUtility.singleLineHeight + MoreGUI.VMargin * 2
                : base.GetPropertyHeight(property, label);
        }

    }

}