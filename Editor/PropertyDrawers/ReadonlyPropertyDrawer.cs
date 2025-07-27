using UnityEngine;
using UnityEditor;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Renders the property field as a disabled in the inspector.
    /// </summary>
    [CustomPropertyDrawer(typeof(ReadonlyAttribute))]
    public class ReadonlyPropertyDrawer : PropertyDrawer
    {

        /// <inheritdoc cref="PropertyDrawer.OnGUI(Rect, SerializedProperty, GUIContent)"/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EnabledScope(false))
                EditorGUI.PropertyField(position, property, label);
        }

    }

}