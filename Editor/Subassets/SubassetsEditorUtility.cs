using System;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace SideXP.Core.EditorOnly
{

    public static class SubassetsEditorUtility
    {

        /// <summary>
        /// Gets the list of the types that can be selected to create a subasset.
        /// </summary>
        /// <param name="subassetsBaseType">The expected base type of the subassets that can be assigned to the list. That this type must
        /// derive from <see cref="ScriptableObject"/>.</param>
        /// <returns>Returns the informations about the allowed subassets, where keys are the types, and values are the display
        /// names.</returns>
        public static Dictionary<Type, GUIContent> GetAllowedSubassetsInfos(Type subassetsBaseType)
        {
            Dictionary<Type, GUIContent> allowedSubassetTypes = new Dictionary<Type, GUIContent>();
            if (subassetsBaseType == null || !subassetsBaseType.Is<ScriptableObject>())
            {
                Debug.LogError($"Only types that derive from {nameof(ScriptableObject)} can be selected for subassets ({(subassetsBaseType != null ? subassetsBaseType.FullName : "null")} given).");
                return allowedSubassetTypes;
            }

            // Filter only relevant subasset types
            using (var list = new ListPoolScope<(Type type, GUIContent label)>())
            {
                foreach (Type t in TypeCache.GetTypesDerivedFrom(subassetsBaseType))
                {
                    if (t.IsAbstract || t.IsInterface || t.IsGenericType || t.GenericTypeArguments.Length > 0)
                        continue;

                    GUIContent content = new GUIContent();
                    SubassetLabelAttribute labelAttribute = t.GetCustomAttribute<SubassetLabelAttribute>();
                    content.text = labelAttribute != null && !string.IsNullOrEmpty(labelAttribute.Name)
                        ? labelAttribute.Name
                        : ObjectNames.NicifyVariableName(t.Name);
                    content.tooltip = labelAttribute != null ? labelAttribute.Description : null;
                    list.Add((t, content));
                }

                list.Sort((a, b) => a.label.text.CompareTo(b.label.text));
                // Register each filtered type
                foreach ((Type type, GUIContent label) in list)
                    allowedSubassetTypes.Add(type, label);
            }

            return allowedSubassetTypes;
        }

    }

}