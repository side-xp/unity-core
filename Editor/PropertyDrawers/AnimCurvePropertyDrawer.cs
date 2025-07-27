using UnityEditor;

using UnityEngine;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Draws an animation curve field with custom settings.
    /// </summary>
    [CustomPropertyDrawer(typeof(AnimCurveAttribute))]
    public class AnimCurvePropertyDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Cancel if multiple values selected or invalid property type
            if (property.hasMultipleDifferentValues || property.propertyType != SerializedPropertyType.AnimationCurve)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            AnimCurveAttribute attr = attribute as AnimCurveAttribute;
            EditorGUI.CurveField(position, property, attr.CurveColor, attr.Ranges, label);
            property.serializedObject.ApplyModifiedProperties();
        }

    }

}