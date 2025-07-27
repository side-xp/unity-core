using UnityEditor;

using UnityEngine;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Displays a value on GUI, but remap this value in the target field.
    /// </summary>
    [CustomPropertyDrawer(typeof(RemapAttribute))]
    public class RemapPropertyDrawer : PropertyDrawer
    {

        private static readonly SerializedPropertyType[] SupportedTypes =
        {
            SerializedPropertyType.Integer,
            SerializedPropertyType.Float,
            SerializedPropertyType.Vector2,
            SerializedPropertyType.Vector2Int,
            SerializedPropertyType.Vector3,
            SerializedPropertyType.Vector3Int,
            SerializedPropertyType.Vector4,
        };

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Cancel if the property doesn't have appropriate value type
            if (property.hasMultipleDifferentValues || !IsSupported(property))
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            RemapAttribute attr = attribute as RemapAttribute;

            Rect rect = position;
            if (!string.IsNullOrEmpty(attr.Units))
            {
                rect.width -= MoreGUI.WidthXS + MoreGUI.HMargin;
            }

            Remap(property, attr.ToMin, attr.ToMax, attr.FromMin, attr.FromMax, attr.Clamped);
            EditorGUI.PropertyField(rect, property, label);

            if (!string.IsNullOrEmpty(attr.Units))
            {
                rect.x += rect.width + MoreGUI.HMargin;
                rect.width = MoreGUI.WidthXS;
                EditorGUI.LabelField(rect, attr.Units);
            }

            Remap(property, attr.FromMin, attr.FromMax, attr.ToMin, attr.ToMax, attr.Clamped);
        }

        /// <summary>
        /// Checks if the given property type is supported by this property drawer.
        /// </summary>
        /// <param name="property">The property to check.</param>
        /// <returns>Returns true if the given property type is supported by this property drawer.</returns>
        private bool IsSupported(SerializedProperty property)
        {
            foreach (SerializedPropertyType supportedType in SupportedTypes)
            {
                if (property.propertyType == supportedType)
                    return true;
            }
            return false;
        }

        private void Remap(SerializedProperty property, float fromMin, float fromMax, float toMin, float toMax, bool clamped)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                {
                    property.intValue = (int)Math.Remap(property.intValue, fromMin, fromMax, toMin, toMax);
                    if (clamped)
                        property.intValue = (int)Mathf.Clamp(property.intValue, toMin, toMax);
                }
                break;

                case SerializedPropertyType.Vector2:
                {
                    float x = Math.Remap(property.vector2Value.x, fromMin, fromMax, toMin, toMax);
                    float y = Math.Remap(property.vector2Value.y, fromMin, fromMax, toMin, toMax);

                    if (clamped)
                    {
                        x = Mathf.Clamp(x, toMin, toMax);
                        y = Mathf.Clamp(y, toMin, toMax);
                    }

                    property.vector2Value = new Vector2(x, y);
                }
                break;

                case SerializedPropertyType.Vector2Int:
                {
                    int x = (int)Math.Remap(property.vector2IntValue.x, fromMin, fromMax, toMin, toMax);
                    int y = (int)Math.Remap(property.vector2IntValue.y, fromMin, fromMax, toMin, toMax);

                    if (clamped)
                    {
                        x = (int)Mathf.Clamp(x, toMin, toMax);
                        y = (int)Mathf.Clamp(y, toMin, toMax);
                    }

                    property.vector2IntValue = new Vector2Int(x, y);
                }
                break;

                case SerializedPropertyType.Vector3:
                {
                    float x = Math.Remap(property.vector3Value.x, fromMin, fromMax, toMin, toMax);
                    float y = Math.Remap(property.vector3Value.y, fromMin, fromMax, toMin, toMax);
                    float z = Math.Remap(property.vector3Value.z, fromMin, fromMax, toMin, toMax);

                    if (clamped)
                    {
                        x = Mathf.Clamp(x, toMin, toMax);
                        y = Mathf.Clamp(y, toMin, toMax);
                        z = Mathf.Clamp(z, toMin, toMax);
                    }

                    property.vector3Value = new Vector3(x, y, z);
                }
                break;

                case SerializedPropertyType.Vector3Int:
                {
                    int x = (int)Math.Remap(property.vector3IntValue.x, fromMin, fromMax, toMin, toMax);
                    int y = (int)Math.Remap(property.vector3IntValue.y, fromMin, fromMax, toMin, toMax);
                    int z = (int)Math.Remap(property.vector3IntValue.z, fromMin, fromMax, toMin, toMax);

                    if (clamped)
                    {
                        x = (int)Mathf.Clamp(x, toMin, toMax);
                        y = (int)Mathf.Clamp(y, toMin, toMax);
                        z = (int)Mathf.Clamp(z, toMin, toMax);
                    }

                    property.vector3IntValue = new Vector3Int(x, y, z);
                }
                break;

                case SerializedPropertyType.Vector4:
                {
                    float x = Math.Remap(property.vector4Value.x, fromMin, fromMax, toMin, toMax);
                    float y = Math.Remap(property.vector4Value.y, fromMin, fromMax, toMin, toMax);
                    float z = Math.Remap(property.vector4Value.z, fromMin, fromMax, toMin, toMax);
                    float w = Math.Remap(property.vector4Value.w, fromMin, fromMax, toMin, toMax);

                    if (clamped)
                    {
                        x = Mathf.Clamp(x, toMin, toMax);
                        y = Mathf.Clamp(y, toMin, toMax);
                        z = Mathf.Clamp(z, toMin, toMax);
                        w = Mathf.Clamp(w, toMin, toMax);
                    }

                    property.vector4Value = new Vector4(x, y, z, w);
                }
                break;

                default:
                {
                    property.floatValue = Math.Remap(property.floatValue, fromMin, fromMax, toMin, toMax);
                    if (clamped)
                        property.floatValue = Mathf.Clamp(property.floatValue, toMin, toMax);
                }
                break;
            }
        }

    }

}