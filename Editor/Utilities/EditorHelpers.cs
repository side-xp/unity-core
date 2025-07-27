using System;
using System.Reflection;

using UnityEngine;
using UnityEditor;

using SideXP.Core.Reflection;

using Object = UnityEngine.Object;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Miscellaneous functions for working in the editor context.
    /// </summary>
    public static class EditorHelpers
    {

        #region Fields

        /// <summary>
        /// Caches the object to focus (using <see cref="FocusObject(Object, bool, bool)"/>).
        /// </summary>
        private static Object s_objectToFocus = null;

        /// <summary>
        /// Caches the object to select (using <see cref="FocusObject(Object, bool, bool)"/>).
        /// </summary>
        private static Object s_objectToSelect = null;

        #endregion


        #region Public API

        /// <summary>
        /// Highlights and select an object. If the object is an asset, focus the project window and highlight it from project view. If it's
        /// an object in a scene, highlight it from the hierarchy.
        /// </summary>
        /// <param name="obj">The object you want to focus.</param>
        /// <param name="pingObject">Should the object be highlighted in the Project view?</param>
        /// <param name="selectObject">Should the object be selected?</param>
        public static void FocusObject(Object obj, bool pingObject = true, bool selectObject = true)
        {
            // This function uses <see cref="EditorApplication.delayCall"/> to wait for all the active editor classes to update.
            // This prevents some exceptions to spawn in the console when the selection or highlight changes between two updates of inspectors.

            if (pingObject)
            {
                s_objectToFocus = obj;
                EditorApplication.delayCall += PingObject;
            }

            if (selectObject)
            {
                s_objectToSelect = obj;
                EditorApplication.delayCall += SelectObject;
            }
        }

        /// <summary>
        /// Finds an open (or loaded) <see cref="EditorWindow"/> by title.
        /// </summary>
        /// <param name="windowTitle">The title of the window to find.</param>
        /// <inheritdoc cref="FindEditorWindow(Type)"/>
        public static EditorWindow FindEditorWindow(string windowTitle)
        {
            foreach (EditorWindow w in Resources.FindObjectsOfTypeAll<EditorWindow>())
            {
                if (w.titleContent.text == windowTitle)
                    return w;
            }
            return null;
        }

        /// <returns>Returns true if a window instance has been found.</returns>
        /// <inheritdoc cref="FindEditorWindow(string)"/>
        /// <inheritdoc cref="FindEditorWindow(Type, out EditorWindow)"/>
        public static bool FindEditorWindow(string windowTitle, out EditorWindow window)
        {
            window = FindEditorWindow(windowTitle);
            return window != null;
        }

        /// <summary>
        /// Finds an open (or loaded) <see cref="EditorWindow"/> by type.
        /// </summary>
        /// <param name="windowType">The expected type of the window to find.</param>
        /// <returns>Returns the found window instance.</returns>
        public static EditorWindow FindEditorWindow(Type windowType)
        {
            foreach (EditorWindow w in Resources.FindObjectsOfTypeAll<EditorWindow>())
            {
                if (w.GetType().IsAssignableFrom(windowType))
                    return w;
            }
            return null;
        }

        /// <param name="window">Outputs the found window instance.</param>
        /// <returns>Returns true if a window instance has been found.</returns>
        /// <inheritdoc cref="FindEditorWindow(Type)"/>
        public static bool FindEditorWindow(Type windowType, out EditorWindow window)
        {
            window = FindEditorWindow(windowType);
            return window != null;
        }

        /// <typeparam name="T">The expected type of the window to find.</typeparam>
        /// <inheritdoc cref="FindEditorWindow(Type)"/>
        public static T FindEditorWindow<T>()
            where T : EditorWindow
        {
            EditorWindow w = FindEditorWindow(typeof(T));
            return w as T;
        }

        /// <inheritdoc cref="FindEditorWindow(Type, out EditorWindow)"/>
        /// <inheritdoc cref="FindEditorWindow{T}()"/>
        public static bool FindEditorWindow<T>(out T window)
            where T : EditorWindow
        {
            window = FindEditorWindow<T>();
            return window != null;
        }

        /// <inheritdoc cref="GetLabel(Type, string, string, BindingFlags)"/>
        public static GUIContent GetLabel(Type type, string fieldOrPropertyName, BindingFlags bindingFlags = ReflectionUtility.InstanceFlags)
        {
            return GetLabel(type, fieldOrPropertyName, null, bindingFlags);
        }

        /// <summary>
        /// Creates a <see cref="GUIContent"/> value from the named field or property label and tooltip.
        /// </summary>
        /// <param name="type">The type of the object from which you want to get the field (or property) data.</param>
        /// <param name="fieldOrPropertyName">The name of the field (or property) of which you want to get the data.</param>
        /// <param name="customLabel">If defined, this text is used as is for the <see cref="GUIContent"/>'s label.</param>
        /// <param name="bindingFlags">Filters to use for finding the field or property. By default, targets only instance data, public and non-public.</param>
        /// <returns>Returns the created <see cref="GUIContent"/> value.</returns>
        public static GUIContent GetLabel(Type type, string fieldOrPropertyName, string customLabel, BindingFlags bindingFlags = ReflectionUtility.InstanceFlags)
        {
            string tooltip = GetTooltip(type, fieldOrPropertyName, bindingFlags);
            return new GUIContent
            {
                text = string.IsNullOrEmpty(customLabel) ? ObjectNames.NicifyVariableName(fieldOrPropertyName) : customLabel,
                tooltip = tooltip
            };
        }

        /// <inheritdoc cref="GetLabel(object, string, string, BindingFlags)"/>
        public static GUIContent GetLabel(object obj, string fieldOrPropertyName, BindingFlags bindingFlags = ReflectionUtility.InstanceFlags)
        {
            return GetLabel(obj.GetType(), fieldOrPropertyName, null, bindingFlags);
        }

        /// <param name="obj">The type of the object from which you want to get the field (or property) data.</param>
        /// <inheritdoc cref="GetLabel(Type, string, string, BindingFlags)"/>
        public static GUIContent GetLabel(object obj, string fieldOrPropertyName, string customLabel, BindingFlags bindingFlags = ReflectionUtility.InstanceFlags)
        {
            return GetLabel(obj.GetType(), fieldOrPropertyName, customLabel, bindingFlags);
        }

        /// <inheritdoc cref="GetLabel{T}(string, string, BindingFlags)"/>
        public static GUIContent GetLabel<T>(string fieldOrPropertyName, BindingFlags bindingFlags = ReflectionUtility.InstanceFlags)
        {
            return GetLabel(typeof(T), fieldOrPropertyName, null, bindingFlags);
        }

        /// <typeparam name="T">The type of the object from which you want to get the field (or property) data.</typeparam>
        /// <inheritdoc cref="GetLabel(Type, string, string, BindingFlags)"/>
        public static GUIContent GetLabel<T>(string fieldOrPropertyName, string customLabel, BindingFlags bindingFlags = ReflectionUtility.InstanceFlags)
        {
            return GetLabel(typeof(T), fieldOrPropertyName, customLabel, bindingFlags);
        }

        /// <summary>
        /// Gets the tooltip of a named field or property.
        /// </summary>
        /// <inheritdoc cref="GetLabel(Type, string, string, BindingFlags)"/>
        /// <returns>Returs the found tooltip value, otherwise <see cref="string.Empty"/>.</returns>
        public static string GetTooltip(Type type, string fieldOrPropertyName, BindingFlags bindingFlags = ReflectionUtility.InstanceFlags)
        {
            // Find a field with the given name, marked with the Tooltip attribute
            FieldInfo field = Array.Find(type.GetFields(bindingFlags), f => f.Name == fieldOrPropertyName && f.IsDefined(typeof(TooltipAttribute)));
            if (field != null)
                return field.GetCustomAttribute<TooltipAttribute>().tooltip;

            // Find a property with the given name, marked with the Tooltip attribute
            PropertyInfo property = Array.Find(type.GetProperties(bindingFlags), p => p.Name == fieldOrPropertyName && p.IsDefined(typeof(TooltipAttribute)));
            if (property != null)
                return property.GetCustomAttribute<TooltipAttribute>().tooltip;

            return string.Empty;
        }

        /// <inheritdoc cref="GetTooltip(Type, string, BindingFlags)"/>
        /// <inheritdoc cref="GetLabel(object, string, string, BindingFlags)"/>
        public static string GetTooltip(object obj, string fieldOrPropertyName, BindingFlags bindingFlags = ReflectionUtility.InstanceFlags)
        {
            return GetTooltip(obj.GetType(), fieldOrPropertyName, bindingFlags);
        }

        /// <inheritdoc cref="GetTooltip(Type, string, BindingFlags)"/>
        /// <inheritdoc cref="GetLabel{T}(string, string, BindingFlags)"/>
        public static string GetTooltip<T>(string fieldOrPropertyName, BindingFlags bindingFlags = ReflectionUtility.InstanceFlags)
        {
            return GetTooltip(typeof(T), fieldOrPropertyName, bindingFlags);
        }

        /// <summary>
        /// Tries to get the active folder path from the Project view if it's open.
        /// </summary>
        /// <param name="path">Outputs the relative path to the fuond active folder.</param>
        /// <returns>Returns true if a path has been found.</returns>
        public static bool TryGetActiveFolderPath(out string path)
        {
            MethodInfo tryGetActiveFolderPathFunc = typeof(ProjectWindowUtil).GetMethod("TryGetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);
            object[] args = new object[] { null };
            bool found = (bool)tryGetActiveFolderPathFunc.Invoke(null, args);
            path = (string)args[0];
            return found;
        }

        #endregion


        #region Private API

        /// <summary>
        /// Highlights the cached asset. If the object is an asset, focus the project window and highlight it from project view. If it's an object in the scene, highlight it from the hierarchy.
        /// </summary>
        private static void PingObject()
        {
            if (s_objectToFocus == null)
                return;

            if (s_objectToFocus.IsAsset())
                EditorUtility.FocusProjectWindow();

            EditorGUIUtility.PingObject(s_objectToFocus);
            s_objectToFocus = null;
        }

        /// <summary>
        /// Selects the cached asset.
        /// </summary>
        private static void SelectObject()
        {
            Selection.activeObject = s_objectToSelect;
            s_objectToSelect = null;
        }

        #endregion

    }

}