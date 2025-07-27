using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using SideXP.Core.Reflection;

using Object = UnityEngine.Object;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Miscellaneous functions for drawing user interfaces in the editor.
    /// </summary>
    public class MoreEditorGUI
    {

        #region Fields

        // Inspectors
        public const string ScriptProperty = "m_Script";

        // Separators
        public static readonly Color SeparatorColor = new Color(1, 1, 1, .4f);
        public static readonly Color DarkSeparatorColor = new Color(0, 0, 0, .53f);
        public const float DefaultSeparatorSize = 2f;

        // Pagination

        /// <summary>
        /// Defines the width ratio for drawing pagination field's page index and total number of pages.
        /// </summary>
        private const float PaginationMainFieldWidthRatio = .5f;

        /// <summary>
        /// Defines the width ratio of the single-page change button, relative to the remaining space after drawing the main page field.
        /// </summary>
        private const float PaginationButtonWidthRatio = .6f;

        private const float PaginationMainFieldSeparatorWidth = 6f;

        // Styles

        /// <summary>
        /// The custom style used for buttons that just contains an icon.
        /// </summary>
        private static GUIStyle s_iconButtonStyle = null;

        /// <summary>
        /// The custom style for titles.
        /// </summary>
        private static GUIStyle s_titleLabelStyle = null;

        #endregion


        #region Inspector

        /// <inheritdoc cref="DrawDefaultInspector(Object, IEnumerable{string}, IEnumerable{string})"/>
        public static void DrawDefaultInspector(Object obj, params string[] ignoredProperties)
        {
            DrawDefaultInspector(new SerializedObject(obj), ignoredProperties);
        }

        /// <param name="obj">The object of which you want to draw the inspector.</param>
        /// <inheritdoc cref="DrawDefaultInspector(SerializedObject, IEnumerable{string}, IEnumerable{string})"/>
        public static void DrawDefaultInspector(Object obj, IEnumerable<string> ignoredProperties, IEnumerable<string> delveProperties)
        {
            DrawDefaultInspector(new SerializedObject(obj), ignoredProperties, delveProperties);
        }

        /// <summary>
        /// Draws the default inspector of a given object, without the script property.
        /// </summary>
        /// <param name="serializedObj">The serialized representation of the object of which you want to draw the inspector.</param>
        /// <inheritdoc cref="DrawDefaultInspector(SerializedObject, IEnumerable{string}, IEnumerable{string})"/>
        public static void DrawDefaultInspector(SerializedObject serializedObj, params string[] ignoredProperties)
        {
            DrawDefaultInspector(serializedObj, ignoredProperties, null);
        }

        /// <summary>
        /// Draws the default inspector of a given object, without the script property.
        /// </summary>
        /// <param name="serializedObj">The serialized representation of the object of which you want to draw the inspector.</param>
        /// <inheritdoc cref="DrawPropertyField(SerializedProperty, IEnumerable{string}, IEnumerable{string})"/>
        public static void DrawDefaultInspector(SerializedObject serializedObj, IEnumerable<string> ignoredProperties, IEnumerable<string> delveProperties)
        {
            SerializedProperty prop = serializedObj.GetIterator();
            prop.NextVisible(true);

            do
            {
                DrawPropertyField(prop, ignoredProperties, delveProperties);
            }
            while (prop.NextVisible(false));

            serializedObj.ApplyModifiedProperties();
        }

        /// <inheritdoc cref="DrawPropertyField(SerializedProperty, IEnumerable{string}, IEnumerable{string})"/>
        public static bool DrawPropertyField(SerializedProperty property)
        {
            return DrawPropertyField(property, null, null);
        }

        /// <summary>
        /// Draws the property field for the given property if it's not ignored.
        /// </summary>
        /// <param name="property">The property of which you want to draw the field, if applicable.</param>
        /// <param name="ignoredProperties">The name of the properties that should not appear in the inspector.</param>
        /// <param name="delveProperties">The name of the properties that should be "delved". Delved properties are skipped, but they child properties are still displayed. In the inspector, this will result in displaying the child properties without foldout field above to expand the parent.</param>
        /// <returns>Returns true if the property field has been drawn on GUI.</returns>
        public static bool DrawPropertyField(SerializedProperty property, IEnumerable<string> ignoredProperties, IEnumerable<string> delveProperties)
        {
            // Cancel if the property is the script property
            if (property.name == ScriptProperty)
                return false;

            if (ignoredProperties != null)
            {
                // Cancel if the property should be hidden
                foreach (string ignoredPropName in ignoredProperties)
                {
                    if (property.name == ignoredPropName)
                        return false;
                }
            }

            if (delveProperties != null)
            {
                // Check if the property is meant to be "delved"
                foreach (string delvePropName in delveProperties)
                {
                    // If the property should be "delved"
                    if (property.name == delvePropName)
                    {
                        // if the property has children properties
                        if (property.hasVisibleChildren)
                        {
                            property.isExpanded = true;
                            // The property to draw is now the next inner property
                            if (property.NextVisible(true))
                                break;
                            else
                                return false;
                        }
                        // Else, cancel if the property doesn't has child properties (and so it's hidden)
                        else
                        {
                            return false;
                        }
                    }
                }
            }

            EditorGUILayout.PropertyField(property, true);
            return true;
        }

        #endregion


        #region Non-Serialized Inspector

        /// <summary>
        /// Draws a field in the inspector for a value that doesn't have a serialized representation.
        /// </summary>
        /// <remarks>
        /// Supported property types:<br/>
        /// - float<br/>
        /// - int<br/>
        /// - bool<br/>
        /// - string<br/>
        /// - enum<br/>
        /// - Object
        /// </remarks>
        /// <param name="obj">The object that owns the field.</param>
        /// <param name="position">Rectangle on the screen to use for the field.</param>
        /// <param name="field">The reflection info about the field.</param>
        /// <param name="label">The label of the field to draw on GUI.</param>
        /// <returns>Returns true if the value changed.</returns>
        public static bool PropertyField(object obj, Rect position, FieldInfo field, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();

            if (field.FieldType == typeof(float))
                return DrawFieldAndSetValue(() => FloatField(position, (float)field.GetValue(obj), label));
            else if (field.FieldType == typeof(int))
                return DrawFieldAndSetValue(() => IntField(position, (int)field.GetValue(obj), label));
            else if (field.FieldType == typeof(bool))
                return DrawFieldAndSetValue(() => ToggleField(position, (bool)field.GetValue(obj), label));
            else if (field.FieldType == typeof(string))
                return DrawFieldAndSetValue(() => TextField(position, (string)field.GetValue(obj), label));
            else if (field.FieldType == typeof(Enum))
                return DrawFieldAndSetValue(() => EnumPopupField(position, (Enum)field.GetValue(obj), label));
            else if (field.FieldType.Is<Object>())
                return DrawFieldAndSetValue(() => ObjectField(position, (Object)field.GetValue(obj), field.FieldType, label, false));

            // Fallback: draw warning to state that the field type is not supported
            Rect rect = new Rect(position);
            // If the property has a label, draw it
            if (label != null && (!string.IsNullOrWhiteSpace(label.text) || label.image != null))
            {
                rect.width = EditorGUIUtility.labelWidth;
                EditorGUI.LabelField(rect, label);

                rect.x += rect.width;
                rect.width = position.width - rect.width;
            }

            EditorGUI.HelpBox(rect, $"Unsupported property type \"{obj.GetType().Name}\"", MessageType.Warning);
            EditorGUI.EndChangeCheck();
            return false;

            // Calls the given function to draw the field, and set the field value if applicable
            bool DrawFieldAndSetValue<T>(Func<T> drawField)
            {
                T value = drawField();
                if (EditorGUI.EndChangeCheck())
                {
                    field.SetValue(obj, value);
                    return true;
                }
                return false;
            }
        }

        /// <inheritdoc cref="PropertyField(object, Rect, FieldInfo, GUIContent)"/>
        public static bool PropertyField(object obj, Rect position, FieldInfo field, string label)
        {
            return PropertyField(obj, position, field, new GUIContent(label));
        }

        /// <param name="autoLabel">If enabled, this function creates a <see cref="GUIContent"/> to use as label for the field, using the
        /// name of the field as text, and optionally the content of its <see cref="TooltipAttribute"/> as tooltip.</param>
        /// <inheritdoc cref="PropertyField(object, Rect, FieldInfo, GUIContent)"/>
        public static bool PropertyField(object obj, Rect position, FieldInfo field, bool autoLabel = false)
        {
            GUIContent label = null;
            if (autoLabel)
            {
                label = new GUIContent(ObjectNames.NicifyVariableName(field.Name));
                if (field.TryGetAttribute(out TooltipAttribute tooltipAttr))
                    label.tooltip = tooltipAttr.tooltip;
            }

            return PropertyField(obj, position, field, label);
        }

        /// <remarks>Uses layout GUI.</remarks>
        /// <inheritdoc cref="PropertyField(object, Rect, FieldInfo, GUIContent)"/>
        public static bool PropertyField(object obj, FieldInfo field, GUIContent label)
        {
            return PropertyField(obj, EditorGUILayout.GetControlRect(), field, label);
        }

        /// <inheritdoc cref="PropertyField(object, FieldInfo, GUIContent)"/>
        public static bool PropertyField(object obj, FieldInfo field, string label)
        {
            return PropertyField(obj, field, new GUIContent(label));
        }

        /// <inheritdoc cref="PropertyField(object, FieldInfo, GUIContent)"/>
        public static bool PropertyField(object obj, FieldInfo field, bool autoLabel = false)
        {
            return PropertyField(obj, EditorGUILayout.GetControlRect(), field, autoLabel);
        }

        /// <summary>
        /// Makes a text field for entering floats.
        /// </summary>
        /// <param name="value">The value to edit.</param>
        /// <param name="style">Optional <see cref="GUIStyle"/>.</param>
        /// <returns>The value entered by the user.</returns>
        /// <inheritdoc cref="PropertyField(object, Rect, FieldInfo, GUIContent)"/>
        public static float FloatField(Rect position, float value, GUIContent label, GUIStyle style = null)
        {
            bool hasLabel = HasLabel(label);

            if (!hasLabel && style == null)
                return EditorGUI.FloatField(position, value);
            if (!hasLabel && style != null)
                return EditorGUI.FloatField(position, value, style);
            else if (hasLabel && style == null)
                return EditorGUI.FloatField(position, label, value);
            else
                return EditorGUI.FloatField(position, label, value, style);
        }

        /// <inheritdoc cref="FloatField(Rect, float, GUIContent, GUIStyle)"/>
        public static float FloatField(Rect position, float value, string label, GUIStyle style = null)
        {
            return FloatField(position, value, new GUIContent(label), style);
        }

        /// <inheritdoc cref="FloatField(Rect, float, GUIContent, GUIStyle)"/>
        public static float FloatField(Rect position, float value)
        {
            return FloatField(position, value, null as GUIContent, null);
        }

        /// <inheritdoc cref="FloatField(Rect, float, GUIContent, GUIStyle)"/>
        /// <inheritdoc cref="PropertyField(object, FieldInfo, GUIContent)" path="/remarks"/>
        public static float FloatField(float value, GUIContent label, GUIStyle style = null)
        {
            return FloatField(EditorGUILayout.GetControlRect(), value, label, style);
        }

        /// <inheritdoc cref="FloatField(Rect, float, GUIContent, GUIStyle)"/>
        public static float FloatField(float value, string label, GUIStyle style = null)
        {
            return FloatField(value, new GUIContent(label), style);
        }

        /// <inheritdoc cref="FloatField(Rect, float, GUIContent, GUIStyle)"/>
        public static float FloatField(float value)
        {
            return FloatField(value, null as GUIContent, null);
        }

        /// <summary>
        /// Makes a text field for entering integers.
        /// </summary>
        /// <inheritdoc cref="FloatField(Rect, float, GUIContent, GUIStyle)"/>
        /// <inheritdoc cref="PropertyField(object, Rect, FieldInfo, GUIContent)"/>
        public static int IntField(Rect position, int value, GUIContent label, GUIStyle style = null)
        {
            bool hasLabel = HasLabel(label);

            if (!hasLabel && style == null)
                return EditorGUI.IntField(position, value);
            if (!hasLabel && style != null)
                return EditorGUI.IntField(position, value, style);
            else if (hasLabel && style == null)
                return EditorGUI.IntField(position, label, value);
            else
                return EditorGUI.IntField(position, label, value, style);
        }

        /// <inheritdoc cref="IntField(Rect, int, GUIContent, GUIStyle)"/>
        public static int IntField(Rect position, int value, string label, GUIStyle style = null)
        {
            return IntField(position, value, new GUIContent(label), style);
        }

        /// <inheritdoc cref="IntField(Rect, int, GUIContent, GUIStyle)"/>
        public static int IntField(Rect position, int value)
        {
            return IntField(position, value, null as GUIContent, null);
        }

        /// <inheritdoc cref="IntField(Rect, int, GUIContent, GUIStyle)"/>
        /// <inheritdoc cref="PropertyField(object, FieldInfo, GUIContent)" path="/remarks"/>
        public static int IntField(int value, GUIContent label, GUIStyle style = null)
        {
            return IntField(EditorGUILayout.GetControlRect(), value, label, style);
        }

        /// <inheritdoc cref="IntField(int, GUIContent, GUIStyle)"/>
        public static int IntField(int value, string label, GUIStyle style = null)
        {
            return IntField(value, new GUIContent(label), style);
        }

        /// <inheritdoc cref="IntField(int, GUIContent, GUIStyle)"/>
        public static int IntField(int value)
        {
            return IntField(value, null as GUIContent, null);
        }

        /// <summary>
        /// Makes a toggle.
        /// </summary>
        /// <inheritdoc cref="FloatField(Rect, float, GUIContent, GUIStyle)"/>
        /// <inheritdoc cref="PropertyField(object, Rect, FieldInfo, GUIContent)"/>
        public static bool ToggleField(Rect position, bool value, GUIContent label, GUIStyle style = null)
        {
            bool hasLabel = HasLabel(label);

            if (!hasLabel && style == null)
                return EditorGUI.Toggle(position, value);
            if (!hasLabel && style != null)
                return EditorGUI.Toggle(position, value, style);
            else if (hasLabel && style == null)
                return EditorGUI.Toggle(position, label, value);
            else
                return EditorGUI.Toggle(position, label, value, style);
        }

        /// <inheritdoc cref="ToggleField(Rect, bool, GUIContent, GUIStyle)"/>
        public static bool ToggleField(Rect position, bool value, string label, GUIStyle style = null)
        {
            return ToggleField(position, value, new GUIContent(label), style);
        }

        /// <inheritdoc cref="ToggleField(Rect, bool, GUIContent, GUIStyle)"/>
        public static bool ToggleField(Rect position, bool value)
        {
            return ToggleField(position, value, null as GUIContent, null);
        }

        /// <inheritdoc cref="ToggleField(Rect, bool, GUIContent, GUIStyle)"/>
        /// <inheritdoc cref="PropertyField(object, FieldInfo, GUIContent)" path="/remarks"/>
        public static bool ToggleField(bool value, GUIContent label, GUIStyle style = null)
        {
            return ToggleField(EditorGUILayout.GetControlRect(), value, label, style);
        }

        /// <inheritdoc cref="ToggleField(bool, GUIContent, GUIStyle)"/>
        public static bool ToggleField(bool value, string label, GUIStyle style = null)
        {
            return ToggleField(value, new GUIContent(label), style);
        }

        /// <inheritdoc cref="ToggleField(bool, GUIContent, GUIStyle)"/>
        public static bool ToggleField(bool value)
        {
            return ToggleField(value, null as GUIContent, null);
        }

        /// <summary>
        /// Makes a text field.
        /// </summary>
        /// <inheritdoc cref="FloatField(Rect, float, GUIContent, GUIStyle)"/>
        /// <inheritdoc cref="PropertyField(object, Rect, FieldInfo, GUIContent)"/>
        public static string TextField(Rect position, string value, GUIContent label, GUIStyle style = null)
        {
            bool hasLabel = HasLabel(label);

            if (!hasLabel && style == null)
                return EditorGUI.TextField(position, value);
            if (!hasLabel && style != null)
                return EditorGUI.TextField(position, value, style);
            else if (hasLabel && style == null)
                return EditorGUI.TextField(position, label, value);
            else
                return EditorGUI.TextField(position, label, value, style);
        }

        /// <inheritdoc cref="TextField(Rect, string, GUIContent, GUIStyle)"/>
        public static string TextField(Rect position, string value, string label, GUIStyle style = null)
        {
            return TextField(position, value, new GUIContent(label), style);
        }

        /// <inheritdoc cref="TextField(Rect, string, GUIContent, GUIStyle)"/>
        public static string TextField(Rect position, string value)
        {
            return TextField(position, value, null as GUIContent, null);
        }

        /// <inheritdoc cref="TextField(Rect, string, GUIContent, GUIStyle)"/>
        /// <inheritdoc cref="PropertyField(object, FieldInfo, GUIContent)" path="/remarks"/>
        public static string TextField(string value, GUIContent label, GUIStyle style = null)
        {
            return TextField(EditorGUILayout.GetControlRect(), value, label, style);
        }

        /// <inheritdoc cref="TextField(string, GUIContent, GUIStyle)"/>
        public static string TextField(string value, string label, GUIStyle style = null)
        {
            return TextField(value, new GUIContent(label), style);
        }

        /// <inheritdoc cref="TextField(string, GUIContent, GUIStyle)"/>
        public static string TextField(string value)
        {
            return TextField(value, null as GUIContent, null);
        }

        /// <summary>
        /// Makes an enum popup selection field.
        /// </summary>
        /// <inheritdoc cref="FloatField(Rect, float, GUIContent, GUIStyle)"/>
        /// <inheritdoc cref="PropertyField(object, Rect, FieldInfo, GUIContent)"/>
        public static Enum EnumPopupField(Rect position, Enum value, GUIContent label, GUIStyle style = null)
        {
            bool hasLabel = HasLabel(label);

            if (!hasLabel && style == null)
                return EditorGUI.EnumPopup(position, value);
            if (!hasLabel && style != null)
                return EditorGUI.EnumPopup(position, value, style);
            else if (hasLabel && style == null)
                return EditorGUI.EnumPopup(position, label, value);
            else
                return EditorGUI.EnumPopup(position, label, value, style);
        }

        /// <inheritdoc cref="EnumPopupField(Rect, Enum, GUIContent, GUIStyle)"/>
        public static Enum EnumPopupField(Rect position, Enum value, string label, GUIStyle style = null)
        {
            return EnumPopupField(position, value, new GUIContent(label), style);
        }

        /// <inheritdoc cref="EnumPopupField(Rect, Enum, GUIContent, GUIStyle)"/>
        public static Enum EnumPopupField(Rect position, Enum value)
        {
            return EnumPopupField(position, value, null as GUIContent, null);
        }

        /// <inheritdoc cref="EnumPopupField(Rect, Enum, GUIContent, GUIStyle)"/>
        /// <inheritdoc cref="PropertyField(object, FieldInfo, GUIContent)" path="/remarks"/>
        public static Enum EnumPopupField(Enum value, GUIContent label, GUIStyle style = null)
        {
            return EnumPopupField(EditorGUILayout.GetControlRect(), value, label, style);
        }

        /// <inheritdoc cref="EnumPopupField(Enum, GUIContent, GUIStyle)"/>
        public static Enum EnumPopupField(Enum value, string label, GUIStyle style = null)
        {
            return EnumPopupField(value, new GUIContent(label), style);
        }

        /// <inheritdoc cref="EnumPopupField(Enum, GUIContent, GUIStyle)"/>
        public static Enum EnumPopupField(Enum value)
        {
            return EnumPopupField(value, null as GUIContent, null);
        }

        /// <summary>
        /// Makes an object field. You can assign objects either by drag and drop objects or by selecting an object using the Object Picker.
        /// </summary>
        /// <param name="type">The type of the objects that can be assigned.</param>
        /// <inheritdoc cref="FloatField(Rect, float, GUIContent, GUIStyle)"/>
        /// <inheritdoc cref="PropertyField(object, Rect, FieldInfo, GUIContent)"/>
        public static Object ObjectField(Rect position, Object value, Type type, GUIContent label, bool allowSceneObjects = true)
        {
            bool hasLabel = HasLabel(label);

            if (hasLabel)
                return EditorGUI.ObjectField(position, label, value, type, allowSceneObjects);
            else
                return EditorGUI.ObjectField(position, value, type, allowSceneObjects);
        }

        /// <inheritdoc cref="ObjectField(Rect, Object, Type, GUIContent, bool)"/>
        public static Object ObjectField(Rect position, Object value, Type type, string label, bool allowSceneObjects = true)
        {
            return ObjectField(position, value, type, new GUIContent(label), allowSceneObjects);
        }

        /// <inheritdoc cref="ObjectField(Rect, Object, Type, GUIContent, bool)"/>
        public static Object ObjectField(Rect position, Object value, Type type, bool allowSceneObjects = true)
        {
            return ObjectField(position, value, type, null as GUIContent, allowSceneObjects);
        }

        /// <inheritdoc cref="ObjectField(Rect, Object, Type, GUIContent, bool)"/>
        /// <inheritdoc cref="PropertyField(object, FieldInfo, GUIContent)" path="/remarks"/>
        public static Object ObjectField(Object value, Type type, GUIContent label, bool allowSceneObjects = true)
        {
            return ObjectField(EditorGUILayout.GetControlRect(), value, type, label, allowSceneObjects);
        }

        /// <inheritdoc cref="ObjectField(Object, Type, GUIContent, bool)"/>
        public static Object ObjectField(Object value, Type type, string label, bool allowSceneObjects = true)
        {
            return ObjectField(value, type, new GUIContent(label), allowSceneObjects);
        }

        /// <inheritdoc cref="ObjectField(Object, Type, GUIContent, bool)"/>
        public static Object ObjectField(Object value, Type type, bool allowSceneObjects = true)
        {
            return ObjectField(value, type, null as GUIContent, allowSceneObjects);
        }

        /// <summary>
        /// Checks if a given label is valid.
        /// </summary>
        /// <param name="label">The label to check.</param>
        /// <returns>Returns true if the label is valid.</returns>
        private static bool HasLabel(GUIContent label)
        {
            return label != null && (!string.IsNullOrWhiteSpace(label.text) || label.image != null);
        }

        #endregion


        #region Fields

        /// <inheritdoc cref="AssetNameField(GUIContent, Object, GUILayoutOption[])"/>
        public static bool AssetNameField(Object asset, params GUILayoutOption[] options)
        {
            return AssetNameField(new GUIContent(), asset, options);
        }

        /// <inheritdoc cref="AssetNameField(GUIContent, Object, GUILayoutOption[])"/>
        public static bool AssetNameField(string label, Object asset, params GUILayoutOption[] options)
        {
            return AssetNameField(new GUIContent(label), asset, options);
        }

        /// <param name="options">Options for drawing the field.</param>
        /// <inheritdoc cref="AssetNameField(Rect, GUIContent, Object)"/>
        public static bool AssetNameField(GUIContent label, Object asset, params GUILayoutOption[] options)
        {
            return AssetNameField(EditorGUILayout.GetControlRect(options), label, asset);
        }

        /// <inheritdoc cref="AssetNameField(Rect, GUIContent, Object)"/>
        public static bool AssetNameField(Rect position, Object asset)
        {
            return AssetNameField(position, new GUIContent(), asset);
        }

        /// <inheritdoc cref="AssetNameField(Rect, GUIContent, Object)"/>
        public static bool AssetNameField(Rect position, string label, Object asset)
        {
            return AssetNameField(position, new GUIContent(label), asset);
        }

        /// <summary>
        /// Draws a delayed text field 
        /// </summary>
        /// <param name="position">The available area for drawing the field.</param>
        /// <param name="label">The label of the field. If not provided, only the field is displayed.</param>
        /// <param name="asset">The asset to rename.</param>
        /// <returns>Returns true if the name has been changed.</returns>
        public static bool AssetNameField(Rect position, GUIContent label, Object asset)
        {
            EditorGUI.BeginChangeCheck();

            string name = label == null || string.IsNullOrEmpty(label.text)
                ? EditorGUI.DelayedTextField(position, asset.name)
                : EditorGUI.DelayedTextField(position, label, asset.name);

            if (EditorGUI.EndChangeCheck())
            {
                asset.Rename(name);
                return true;
            }
            return false;
        }

        #endregion


        #region Pagination

        /// <inheritdoc cref="PaginationField(Rect, GUIContent, ref Pagination)"/>
        public static void PaginationField(ref Pagination pagination)
        {
            PaginationField(EditorGUILayout.GetControlRect(false), ref pagination);
        }

        /// <inheritdoc cref="PaginationField(Rect, GUIContent, ref Pagination)"/>
        public static void PaginationField(string label, ref Pagination pagination)
        {
            PaginationField(EditorGUILayout.GetControlRect(false), label, ref pagination);
        }

        /// <inheritdoc cref="PaginationField(Rect, GUIContent, ref Pagination)"/>
        public static void PaginationField(GUIContent label, ref Pagination pagination)
        {
            PaginationField(EditorGUILayout.GetControlRect(false), label, ref pagination);
        }

        /// <inheritdoc cref="PaginationField(Rect, GUIContent, ref Pagination)"/>
        public static void PaginationField(Rect position, ref Pagination pagination)
        {
            float mainFieldWidth = position.width * PaginationMainFieldWidthRatio;
            float controlsAvailableWidth = (position.width - mainFieldWidth - MoreGUI.HMargin * 2) / 2;
            float largeButtonWidth = controlsAvailableWidth * PaginationButtonWidthRatio;
            float smallButtonWidth = controlsAvailableWidth - largeButtonWidth - MoreGUI.HMargin;

            Rect rect = new Rect(position);

            // Draw "previous" controls
            using (new EnabledScope(pagination.Page > 0))
            {
                rect.width = smallButtonWidth;
                if (GUI.Button(rect, "<<"))
                    pagination.Page = 0;

                rect.x += rect.width + MoreGUI.HMargin;
                rect.width = largeButtonWidth;
                if (GUI.Button(rect, "<"))
                    pagination.Page--;
            }

            // Draw page index
            int indexPlus1 = pagination.Page + 1;
            rect.x += rect.width + MoreGUI.HMargin;
            rect.width = mainFieldWidth / 2;
            indexPlus1 = EditorGUI.IntField(rect, indexPlus1, EditorStyles.textField.TextAlignment(TextAnchor.MiddleCenter));
            pagination.Page = indexPlus1 - 1;

            // Draw separator
            rect.x += rect.width;
            rect.width = PaginationMainFieldSeparatorWidth;
            EditorGUI.LabelField(rect, "/");

            // Draw pages count
            rect.x += rect.width + MoreGUI.HMargin;
            rect.width = mainFieldWidth / 2 - MoreGUI.HMargin - PaginationMainFieldSeparatorWidth;
            EditorGUI.LabelField(rect, pagination.PagesCount.ToString(), EditorStyles.label.TextAlignment(TextAnchor.MiddleCenter));

            rect.x += rect.width + MoreGUI.HMargin;
            // Draw "next" controls
            using (new EnabledScope(pagination.Page < pagination.PagesCount - 1))
            {
                rect.width = largeButtonWidth;
                if (GUI.Button(rect, ">"))
                    pagination.Page++;

                rect.x += rect.width + MoreGUI.HMargin;
                rect.width = smallButtonWidth;
                if (GUI.Button(rect, ">>"))
                    pagination.Page = pagination.PagesCount;
            }
        }

        /// <inheritdoc cref="PaginationField(Rect, GUIContent, ref Pagination)"/>
        public static void PaginationField(Rect position, string label, ref Pagination pagination)
        {
            PaginationField(position, new GUIContent(label), ref pagination);
        }

        /// <summary>
        /// Draws a field to edit the given pagination value.
        /// </summary>
        /// <param name="label">The label of the field.</param>
        /// <param name="position">The position and size of the field.</param>
        /// <param name="pagination">The pagination value to edit.</param>
        public static void PaginationField(Rect position, GUIContent label, ref Pagination pagination)
        {
            Rect rect = new Rect(position);

            // Draw label
            if (label != null && !string.IsNullOrEmpty(label.text))
            {
                rect.width = EditorGUIUtility.labelWidth;
                EditorGUI.LabelField(rect, label);

                // Draw pagination field
                rect.x += rect.width + MoreGUI.HMargin;
                rect.width -= rect.width + MoreGUI.HMargin;
            }

            // Draw pagination field
            PaginationField(rect, ref pagination);
        }

        #endregion


        #region Styles

        /// <inheritdoc cref="s_iconButtonStyle"/>
        public static GUIStyle IconButtonStyle
        {
            get
            {
                if (s_iconButtonStyle == null)
                {
                    s_iconButtonStyle = GUI.skin.button.Padding(0, 0);
                }
                return s_iconButtonStyle;
            }
        }

        /// <inheritdoc cref="s_titleLabelStyle"/>
        public static GUIStyle TitleStyle
        {
            get
            {
                if (s_titleLabelStyle == null)
                {
                    s_titleLabelStyle = new GUIStyle(EditorStyles.largeLabel);
                    s_titleLabelStyle.fontSize += 2;
                    s_titleLabelStyle.fontStyle = FontStyle.Bold;
                    s_titleLabelStyle.richText = true;
                }
                return s_titleLabelStyle;
            }
        }

        #endregion


        #region Path Fields

        /// <summary>
        /// Draws a field in the inspector to select a directory.
        /// </summary>
        /// <param name="position">The position of the field on the GUI.</param>
        /// <param name="label">The label of the field.</param>
        /// <param name="path">The current path value.</param>
        /// <param name="title">The title of the "select folder" panel.</param>
        /// <param name="defaultName">The default name of the folder to select.</param>
        /// <param name="allowExternal">By default, this function only allow user to select a path to a directory inside the current
        /// project. If enabled, the user can select a path outside the current project. Note that the path won't be converted into a
        /// relative path if it's outside of the project.</param>
        /// <returns>Returns the selected directory path.</returns>
        public static string FolderPathField(Rect position, GUIContent label, string path, string title, string defaultName, bool allowExternal = false)
        {
            Rect rect = new Rect(position);
            
            // Draw text field with label
            rect.width = position.width - MoreGUI.WidthS - MoreGUI.HMargin;
            path = EditorGUI.TextField(rect, label, path);

            // Draw browse button
            rect.x += rect.width + MoreGUI.HMargin;
            rect.width = MoreGUI.WidthS;

            if (GUI.Button(rect, "Browse..."))
            {
                string tmpPath = path;
                if (string.IsNullOrEmpty(tmpPath) || !Directory.Exists(tmpPath.ToAbsolutePath()))
                    tmpPath = PathUtility.AssetsDirectory;

                tmpPath = EditorUtility.OpenFolderPanel(title, tmpPath, defaultName);

                // Cancel if no path has been selected (panel has been canceled)
                if (string.IsNullOrEmpty(tmpPath))
                    return path;

                // Cancel if the selected path doesn't target the current project
                if (allowExternal && !PathUtility.IsProjectPath(tmpPath))
                {
                    Debug.LogWarning("The selected folder must be in this project.");
                    return path;
                }

                path = tmpPath.ToRelativePath();
            }

            return path;
        }

        /// <inheritdoc cref="FolderPathField(Rect, GUIContent, string, string, string, bool)"/>
        public static string FolderPathField(Rect position, string label, string path, string title, string defaultName, bool allowExternal = false)
        {
            return FolderPathField(position, new GUIContent(label), path, title, defaultName, allowExternal);
        }

        /// <inheritdoc cref="FolderPathField(Rect, GUIContent, string, string, string, bool)"/>
        public static string FolderPathField(GUIContent label, string path, string title, string defaultName, bool allowExternal = false)
        {
            Rect rect = EditorGUILayout.GetControlRect(true);
            rect = EditorGUI.IndentedRect(rect);
            return FolderPathField(rect, label, path, title, defaultName, allowExternal);
        }

        /// <inheritdoc cref="FolderPathField(Rect, GUIContent, string, string, string, bool)"/>
        public static string FolderPathField(string label, string path, string title, string defaultName, bool allowExternal = false)
        {
            return FolderPathField(new GUIContent(label), path, title, defaultName, allowExternal);
        }

        #endregion


        #region Separators

        /// <inheritdoc cref="HorizontalSeparator(float, bool, bool)"/>
        public static void HorizontalSeparator(bool wide = false, bool dark = false)
        {
            HorizontalSeparator(DefaultSeparatorSize, wide, dark);
        }

        /// <param name="dark">If enabled, uses the dark separator color.</param>
        /// <inheritdoc cref="HorizontalSeparator(float, Color, bool)"/>
        public static void HorizontalSeparator(float size, bool wide = false, bool dark = false)
        {
            HorizontalSeparator(size, dark ? DarkSeparatorColor : SeparatorColor, wide);
        }

        /// <summary>
        /// Draws an horizontal line.
        /// </summary>
        /// <param name="size">The height of the separator.</param>
        /// <param name="color">The color of the separator.</param>
        /// <param name="wide">If enabled, the separator will use the full view width. This is designed to draw a separator that doesn't
        /// use the margins in the inspector window.</param>
        public static void HorizontalSeparator(float size, Color color, bool wide = false)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, size);

            if (wide && rect.width < EditorGUIUtility.currentViewWidth)
            {
                rect.x = 0;
                rect.width = EditorGUIUtility.currentViewWidth;
            }

            EditorGUI.DrawRect(rect, color);
        }

        /// <summary>
        /// Draws a vertical line.
        /// </summary>
        /// <param name="size">The width of the separator.</param>
        /// <param name="color">The color of the separator.</param>
        public static void VerticalSeparator(float size, Color color)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, GUILayout.Width(size));
            EditorGUI.DrawRect(rect, color);
        }

        /// <inheritdoc cref="VerticalSeparator(float, Color)"/>
        public static void VerticalSeparator(float size)
        {
            VerticalSeparator(size, SeparatorColor);
        }

        /// <inheritdoc cref="VerticalSeparator(float, Color)"/>
        public static void VerticalSeparator()
        {
            VerticalSeparator(DefaultSeparatorSize, SeparatorColor);
        }

        #endregion

    }

}