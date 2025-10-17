using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using Object = UnityEngine.Object;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Displays a field to create or edit a subasset for a property marked with the [Subasset] attribute.
    /// </summary>
    [CustomPropertyDrawer(typeof(SubassetAttribute))]
    public class SubassetPropertyDrawer : PropertyDrawer
    {

        private const float ButtonWidth = MoreGUI.WidthS;
        private const string ScriptProp = "m_Script";
        private const float Indent = 16f;

        /// <summary>
        /// Caches the allowed subasset types.
        /// </summary>
        private Dictionary<Type, GUIContent> _allowedSubassetTypes = null;

        /// <inheritdoc cref="PropertyDrawer.OnGUI(Rect, SerializedProperty, GUIContent)"/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Cancel if the decorated property is not declared inside a ScriptableObject class
#if UNITY_2023_1_OR_NEWER
            if (property.serializedObject.targetObject is not ScriptableObject)
#else
            if (!(property.serializedObject.targetObject is ScriptableObject))
#endif
            {
                Debug.LogError($"The [Subasset] attribute is designed to be used only in {nameof(ScriptableObject)} classes.", property.serializedObject.targetObject);
#if UNITY_2023_1_OR_NEWER
                using (new EditorGUI.MixedValueScope(true))
#endif
                    EditorGUI.PropertyField(position, property, label);
                return;
            }

            // Display default GUI if multiple objects selected
            if (property.hasMultipleDifferentValues)
            {
#if UNITY_2023_1_OR_NEWER
                using (new EditorGUI.MixedValueScope(true))
#endif
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            Type subassetsBaseType = null;
            try
            {
                subassetsBaseType = property.GetTargetType();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Debug.LogError($"An error has occured while trying to get the subasset base type expected for the property {property.serializedObject.targetObject.GetType().Name}.{property.name}. See previous log for more info.", property.serializedObject.targetObject);
            }

            // Cancel if the expected subassets type is not valid
            if (subassetsBaseType == null || !subassetsBaseType.Is<ScriptableObject>())
            {
                Debug.LogError($"The generic type of SubassetsList<T> properties must derive from {nameof(ScriptableObject)}.", property.serializedObject.targetObject);
#if UNITY_2023_1_OR_NEWER
                using (new EditorGUI.MixedValueScope(true))
#endif
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            SubassetAttribute subassetAttr = attribute as SubassetAttribute;

            Rect rect = position;
            rect.height = EditorGUIUtility.singleLineHeight;
            // Center the field vertically if the property is displayed as an array element
            if (property.IsArrayElement())
                rect.y = (position.height - rect.height) / 2;

            DrawHeader(rect);
            if (!property.isExpanded || property.objectReferenceValue == null)
                return;

            rect.x += Indent;
            rect.width -= Indent;
            SerializedObject itemObj = new SerializedObject(property.objectReferenceValue);
            SerializedProperty tmpProp = itemObj.GetIterator();
            tmpProp.NextVisible(true);

            // For each visible property
            while (tmpProp.NextVisible(false))
            {
                rect.y += MoreGUI.VMargin + rect.height;
                rect.height = EditorGUI.GetPropertyHeight(tmpProp);
                EditorGUI.PropertyField(rect, tmpProp, true);
            }

            itemObj.ApplyModifiedProperties();

            void DrawHeader(Rect headerPosition)
            {
                Rect headerRect = headerPosition;
                headerRect.height = EditorGUIUtility.singleLineHeight;
                headerRect.width = EditorGUIUtility.labelWidth;
                // Foldout label
                property.isExpanded = EditorGUI.Foldout(headerRect, property.isExpanded, label, true);

                headerRect.x += headerRect.width;
                headerRect.width = headerPosition.width - headerRect.width - MoreGUI.HMargin - ButtonWidth;
                // Enable the object reference field only if there's no defined reference but external assets are allowed, OR if the assigned asset is not a subasset
                using (new EnabledScope((property.objectReferenceValue == null && subassetAttr.AllowExternal) || (property.objectReferenceValue != null && !property.objectReferenceValue.IsSubassetOf(property.serializedObject.targetObject))))
                {
                    using (var scope = new EditorGUI.ChangeCheckScope())
                    {
                        Object newRef = EditorGUI.ObjectField(headerRect, property.objectReferenceValue, subassetsBaseType, false);
                        if (scope.changed)
                        {
                            // If the assigned asset is now null
                            if (newRef == null)
                            {
                                // If the previously assigned asset was a subasset, destroy it
                                if (property.objectReferenceValue.IsSubassetOf(property.serializedObject.targetObject))
                                {
                                    Object.DestroyImmediate(property.objectReferenceValue, true);
                                    property.objectReferenceValue = null;
                                    property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                                    property.serializedObject.targetObject.SaveAndReimport();
                                }
                            }
                            property.serializedObject.ApplyModifiedProperties();
                        }
                    }
                }

                headerRect.x += headerRect.width + MoreGUI.HMargin;
                headerRect.width = ButtonWidth;
                // If the subasset reference is missing, display a button to create one
                if (property.objectReferenceValue == null)
                {
                    if (GUI.Button(headerRect, new GUIContent("Create", $"Display a menu so you can select the type of the subasset to create among the available implementations of {subassetsBaseType.Name} in your project.")))
                    {
                        SelectAndCreateSubasset(property, subassetsBaseType);
                    }
                }
                else
                {
                    // Else, if the referenced object is a subasset of the inspected asset, display a button to destroy it
                    if (property.objectReferenceValue.IsSubassetOf(property.serializedObject.targetObject))
                    {
                        if (GUI.Button(headerRect, new GUIContent("Delete", "Destroy the subasset and reset this field to a null value.")))
                        {
                            if (EditorUtility.DisplayDialog("Destroy Subasset", "The subasset will be removed from the project. This operation can't be undone.\nProceed?", "Yes, delete the subasset", "No"))
                            {
                                Object.DestroyImmediate(property.objectReferenceValue, true);
                                property.objectReferenceValue = null;
                                property.serializedObject.ApplyModifiedProperties();
                                property.serializedObject.targetObject.SaveAndReimport();
                            }
                        }
                    }
                    // Else, if the referenced object is an external asset, display a button to locate it
                    else
                    {
                        if (GUI.Button(headerRect, new GUIContent("Locate", "Locate the assigned asset in your project.")))
                        {
                            EditorHelpers.FocusObject(property.objectReferenceValue, true, false);
                        }
                    }
                }
            }
        }

        /// <inheritdoc cref="PropertyDrawer.GetPropertyHeight(SerializedProperty, GUIContent)"/>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = base.GetPropertyHeight(property, label);
            if (property.IsArrayElement())
                height += MoreGUI.VMargin * 2;

            if (!property.isExpanded || property.objectReferenceValue == null)
                return height;

            SerializedObject itemObj = new SerializedObject(property.objectReferenceValue);
            SerializedProperty prop = itemObj.GetIterator();
            prop.NextVisible(true);

            // Increase height for each visible property
            while (prop.NextVisible(false))
            {
                // Ignore the default script property
                if (prop.name == ScriptProp)
                    continue;
                else
                    height += MoreGUI.VMargin + EditorGUI.GetPropertyHeight(prop, true);
            }

            return height;
        }

        /// <summary>
        /// Gets the list of the allowed subassets for the given property.
        /// </summary>
        /// <returns>Returns the informations about the allowed subassets for the given property, where keys are the types, and values are the
        /// display names.</returns>
        /// <inheritdoc cref="SubassetsEditorUtility.GetAllowedSubassetsInfos(Type)"/>
        private Dictionary<Type, GUIContent> GetAllowedSubassets(Type subassetsBaseType)
        {
            if (_allowedSubassetTypes != null)
                return _allowedSubassetTypes;

            _allowedSubassetTypes = SubassetsEditorUtility.GetAllowedSubassetsInfos(subassetsBaseType);
            return _allowedSubassetTypes;
        }

        /// <summary>
        /// Displays a menu to select the subasset to create and reference.
        /// </summary>
        /// <param name="property">The property that stores the subasset reference.</param>
        /// <param name="subassetsBaseType">The base type of the allowed subassets.</param>
        private void SelectAndCreateSubasset(SerializedProperty property, Type subassetsBaseType)
        {
            Dictionary<Type, GUIContent> allowedSubassets = GetAllowedSubassets(subassetsBaseType);
            // Cancel if there's no available subasset type to select
            if (allowedSubassets.Count <= 0)
            {
                Debug.LogWarning($"No implementation of the expected subasset type ({subassetsBaseType}) found for the property {property.serializedObject.targetObject.GetType()}.{property.propertyPath}.", property.serializedObject.targetObject);
                return;
            }

            GenericMenu menu = new GenericMenu();

            // For each allowed subasset type
#if UNITY_2023_1_OR_NEWER
            foreach ((Type t, GUIContent label) in allowedSubassets)
                menu.AddItem(label, false, () => CreateSubasset(property, t, label.text));
#else
            foreach (KeyValuePair<Type, GUIContent> item in allowedSubassets)
                menu.AddItem(item.Value, false, () => CreateSubasset(property, item.Key, item.Value.text));
#endif

            menu.ShowAsContext();
        }

        /// <summary>
        /// Creates an asset of the given type, attach it as a subasset and set it as the reference value of the given property.
        /// </summary>
        /// <param name="property">The property that stores the subasset reference.</param>
        /// <param name="assetType">The type of the subasset to create.</param>
        /// <param name="name">The name of the subasset to create. If not defiend, uses the "nicified" asset type name.</param>
        private void CreateSubasset(SerializedProperty property, Type assetType, string name = null)
        {
            if (property.objectReferenceValue != null)
            {
                Object.DestroyImmediate(property.objectReferenceValue);
                property.objectReferenceValue = null;
                property.serializedObject.targetObject.SaveAndReimport();
            }

            ScriptableObject subasset = ScriptableObject.CreateInstance(assetType);
            subasset.name = !string.IsNullOrEmpty(name) ? name : ObjectNames.NicifyVariableName(assetType.Name);

            // Attach subasset & save database
            AssetDatabase.AddObjectToAsset(subasset, property.serializedObject.targetObject);
            AssetDatabase.SaveAssets();

            property.objectReferenceValue = subasset;
            property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

    }

}