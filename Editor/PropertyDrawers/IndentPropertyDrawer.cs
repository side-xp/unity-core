using UnityEngine;
using UnityEditor;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Indents the property field.
    /// </summary>
    [CustomPropertyDrawer(typeof(IndentAttribute))]
    public class IndentPropertyDrawer : PropertyDrawer
    {

        /// <inheritdoc cref="PropertyDrawer.OnGUI(Rect, SerializedProperty, GUIContent)"/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new IndentedScope((attribute as IndentAttribute).Levels))
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }

    }

}