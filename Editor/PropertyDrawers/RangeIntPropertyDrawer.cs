using UnityEditor;
using UnityEngine;

namespace SideXP.Core.EditorOnly
{

    /// <inheritdoc cref="RangePropertyDrawer"/>
    [CustomPropertyDrawer(typeof(RangeInt))]
    public class RangeIntPropertyDrawer : PropertyDrawer
    {

        /// <inheritdoc cref="PropertyDrawer.OnGUI(Rect, SerializedProperty, GUIContent)"/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            RangePropertyDrawer.DrawRangeGUI(position, property, label);
        }

        /// <inheritdoc cref="PropertyDrawer.GetPropertyHeight(SerializedProperty, GUIContent)"/>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return RangePropertyDrawer.GetHeight();
        }

    }

}
