using System;
using System.Globalization;

using UnityEditor;
using UnityEngine;

using SideXP.Core.Reflection;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Extension functions for <see cref="SerializedProperty"/> instances.
    /// </summary>
    public static class SerializedPropertyExtensions
    {

        #region Public API

        /// <summary>
        /// Gets the field label of a serialized property.
        /// </summary>
        /// <param name="property">The property you want to get the label.</param>
        /// <returns>Returns the label of the given property.</returns>
        public static GUIContent GetLabel(this SerializedProperty property)
        {
            return new GUIContent(property.displayName, property.tooltip);
        }

        /// <summary>
        /// Gets the target object of this <see cref="SerializedProperty"/>.<br/>
        /// This function is inspired by the SpacePuppy Unity Framework:
        /// https://github.com/lordofduct/spacepuppy-unity-framework-4.0/blob/master/Framework/com.spacepuppy.core/Editor/src/EditorHelper.cs
        /// </summary>
        /// <param name="property">The property of which you want to get the target object.</param>
        /// <returns>Returns the found target object.</returns>
        public static object GetTarget(this SerializedProperty property)
        {
            // The object is for now the serialized container of the represented property
            object obj = property.serializedObject.targetObject;
            return ReflectionUtility.GetNestedObject(obj, property.propertyPath);
        }

        /// <inheritdoc cref="GetTarget(SerializedProperty)"/>
        /// <typeparam name="T">The expected target type.</typeparam>
        public static T GetTarget<T>(this SerializedProperty property)
        {
            return (T)GetTarget(property);
        }

        /// <summary>
        /// Gets the type of a property, using its path.
        /// </summary>
        /// <remarks>Note that this function doesn't need the property to be assigned.</remarks>
        /// <param name="property">The property you want to get the type.</param>
        /// <returns>Returns the target type of the property.</returns>
        public static Type GetTargetType(this SerializedProperty property)
        {
            return TryGetTargetType(property, out Type targetType) ? targetType : null;
        }

        /// <param name="targetType">Outputs the target type of the property.</param>
        /// <returns>Returns true if the target type has been retrieved successfully.</returns>
        /// <inheritdoc cref="GetTargetType(SerializedProperty)"/>
        public static bool TryGetTargetType(this SerializedProperty property, out Type targetType)
        {
            FieldOrPropertyInfo info = ReflectionUtility.GetFieldOrPropertyFromPath(property.serializedObject.targetObject.GetType(), property.propertyPath, true);
            targetType = info != null ? info.Type : null;
            return targetType != null;
        }

        /// <summary>
        /// Checks if the given property is an array element.
        /// </summary>
        /// <remarks>This can be useful to remove unnecessary labels in the displayed items.</remarks>
        /// <param name="property">The property to check.</param>
        /// <returns>Returns true if the given property is an array element.</returns>
        public static bool IsArrayElement(this SerializedProperty property)
        {
            // Unity always represents an array/list element with a ".Array.data[i]" path segment. Matching that exact marker
            // (rather than a bare "Array") avoids false positives for fields whose name merely contains "Array".
            return property.propertyPath.Contains(ReflectionUtility.ArrayDataMarker);
        }

        /// <summary>
        /// Gets the path of the array or list that contains the given property, and the index of the property in that collection.
        /// </summary>
        /// <remarks>
        /// Unlike <see cref="IsArrayElement(SerializedProperty)"/>, this function only succeeds if the property is directly an element of
        /// the collection (like "myContainer.Array.data[3]"), not if it's nested in an element (like "myContainer.Array.data[3].myField").
        /// </remarks>
        /// <param name="property">The property to check.</param>
        /// <param name="arrayPath">
        /// Outputs the path of the array or list that contains the property (usable with
        /// <see cref="SerializedObject.FindProperty(string)"/>), or null if the property is not directly an element of a collection.
        /// </param>
        /// <param name="index">
        /// Outputs the index of the property in its collection, or -1 if it's not directly an element of a collection.
        /// </param>
        /// <returns>Returns true if the property is directly an element of an array or a list.</returns>
        public static bool TryGetArrayElementInfo(this SerializedProperty property, out string arrayPath, out int index)
        {
            arrayPath = null;
            index = -1;

            // Only direct elements end with the index segment
            string path = property.propertyPath;
            if (!path.EndsWith("]", StringComparison.Ordinal))
                return false;

            int markerIndex = path.LastIndexOf(ReflectionUtility.ArrayDataMarker, StringComparison.Ordinal);
            if (markerIndex < 0)
                return false;

            int indexStart = markerIndex + ReflectionUtility.ArrayDataMarker.Length;
            string indexString = path.Substring(indexStart, path.Length - indexStart - 1);
            if (!int.TryParse(indexString, NumberStyles.None, CultureInfo.InvariantCulture, out int parsedIndex))
                return false;

            arrayPath = path.Substring(0, markerIndex);
            index = parsedIndex;
            return true;
        }

        #endregion

    }

}