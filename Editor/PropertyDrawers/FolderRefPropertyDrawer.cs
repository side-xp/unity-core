using UnityEngine;
using UnityEditor;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Draws an object field to set the reference to a folder in the project.
    /// </summary>
    [CustomPropertyDrawer(typeof(FolderRefAttribute))]
    public class FolderRefPropertyDrawer : PropertyDrawer
    {

        /// <inheritdoc cref="PropertyDrawer.OnGUI(Rect, SerializedProperty, GUIContent)"/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Stop if several objects are selected
            if (property.hasMultipleDifferentValues)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            // Cancel if the property doesn't have the expected type
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                Rect rect = new Rect(position);
                rect.width = EditorGUIUtility.labelWidth;
                EditorGUI.LabelField(rect, label);
                rect.x += rect.width;
                rect.width = position.width - rect.width;
                EditorGUI.HelpBox(rect, $"Expected {nameof(Object)} property type", MessageType.Warning);
                return;
            }

            EditorGUI.BeginChangeCheck();
            Object obj = EditorGUI.ObjectField(position, label, property.objectReferenceValue, typeof(Object), false);
            if (EditorGUI.EndChangeCheck())
            {
                // Stop if the assigned object is null
                if (obj == null)
                {
                    property.objectReferenceValue = null;
                    return;
                }

                // Cancel if the assigned object is not a folder
                if (!AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(obj)))
                {
                    Debug.LogWarning("This field expects a folder from your game project.");
                    return;
                }

                property.objectReferenceValue = obj;
            }
        }

    }

}