using System;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

using SideXP.Core.Reflection;

using Object = UnityEngine.Object;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Displays a reorderable list to add or remove subassets for a <see cref="SubassetsList{T}"/> property.
    /// </summary>
    [CustomPropertyDrawer(typeof(SubassetsListBase), true)]
    public class SubassetsListPropertyDrawer : PropertyDrawer
    {

        private const float ItemRowPadding = 2f;
        private const float FoldoutIndent = 12f;
        private const float MiniFoldoutWidth = 4f;
        private const string SubassetsListProp = "_subassetsList";
        private const string ScriptProp = "m_Script";

        /// <summary>
        /// Caches the allowed subasset types.
        /// </summary>
        private Dictionary<Type, GUIContent> _allowedSubassetTypes = null;

        /// <summary>
        /// The created subassets reorderable list created for the inspected property.
        /// </summary>
        private ReorderableList _subassetsReorderableList = null;

        /// <inheritdoc cref="PropertyDrawer.OnGUI(Rect, SerializedProperty, GUIContent)"/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty subassetsListProp = property.FindPropertyRelative(SubassetsListProp);

            // Cancel if the decorated property is not declared inside a ScriptableObject class
#if UNITY_2023_OR_NEWER
            if (property.serializedObject.targetObject is not ScriptableObject)
#else
            if (!(property.serializedObject.targetObject is ScriptableObject))
#endif
            {
                Debug.LogError($"SubassetsList<T> properties are designed to be used only in {nameof(ScriptableObject)} classes.", property.serializedObject.targetObject);
#if UNITY_2023_OR_NEWER
                using (new EditorGUI.MixedValueScope(true))
#endif
                    EditorGUI.PropertyField(position, subassetsListProp, label);
                return;
            }

            // Display default GUI if multiple objects selected
            if (property.hasMultipleDifferentValues)
            {
#if UNITY_2023_OR_NEWER
                using (new EditorGUI.MixedValueScope(true))
#endif
                EditorGUI.PropertyField(position, subassetsListProp, label);
                return;
            }

            Type subassetsBaseType = GetExpectedSubassetsType(property);
            // Cancel if the expected subassets type is not valid
            if (subassetsBaseType == null || !subassetsBaseType.Is<ScriptableObject>())
            {
                Debug.LogError($"The generic type of SubassetsList<T> properties must derive from {nameof(ScriptableObject)}.", property.serializedObject.targetObject);
#if UNITY_2023_OR_NEWER
                using (new EditorGUI.MixedValueScope(true))
#endif
                EditorGUI.PropertyField(position, subassetsListProp, label);
                return;
            }

            // Draw the subassets reorderable list's GUI
            GetSubassetsReorderableList(subassetsListProp, label, subassetsBaseType).DoList(position);
        }

        /// <inheritdoc cref="PropertyDrawer.GetPropertyHeight(SerializedProperty, GUIContent)"/>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Type subassetsBaseType = GetExpectedSubassetsType(property);
            if (subassetsBaseType != null && subassetsBaseType.Is<ScriptableObject>())
            {
                SerializedProperty subassetsListProp = property.FindPropertyRelative(SubassetsListProp);
                return GetSubassetsReorderableList(subassetsListProp, label, subassetsBaseType).GetHeight();
            }
            else
            {
                return base.GetPropertyHeight(property, label);
            }
        }

        /// <summary>
        /// Gets the subassets as a reorderable list, or create it if it does not exist yet.
        /// </summary>
        /// <param name="innerListProp">The inner serialized list property from the inspected <see cref="SubassetsList{T}"/>.</param>
        /// <param name="label">The label to display as header.</param>
        /// <param name="subassetsBaseType">The expected base type of the subassets that can be assigned to the list. Assumes that this type
        /// derives from <see cref="ScriptableObject"/>.</param>
        /// <returns>Returns the subassets as a reorderable list.</returns>
        private ReorderableList GetSubassetsReorderableList(SerializedProperty innerListProp, GUIContent label, Type subassetsBaseType)
        {
            if (_subassetsReorderableList != null)
                return _subassetsReorderableList;

            _subassetsReorderableList = new ReorderableList(innerListProp.serializedObject, innerListProp);

            // Draw subassets list header (using the property's label)
            _subassetsReorderableList.drawHeaderCallback = (rect) =>
            {
                EditorGUI.LabelField(rect, label, EditorStyles.boldLabel);
            };

            // Add item behavior
            _subassetsReorderableList.onAddDropdownCallback = (rect, list) =>
            {
                Dictionary<Type, GUIContent> allowedSubassets = GetAllowedSubassets(subassetsBaseType);
                // Cancel if there's no available subasset type to select
                if (allowedSubassets.Count <= 0)
                {
                    Debug.LogWarning($"No implementation of the expected subasset type ({subassetsBaseType}) found for the property {list.serializedProperty.serializedObject.targetObject.GetType()}.{list.serializedProperty.propertyPath}.", list.serializedProperty.serializedObject.targetObject);
                    return;
                }

                SubassetsListOptionsAttribute optionsAttribute = attribute as SubassetsListOptionsAttribute;
                GenericMenu menu = new GenericMenu();

                // For each allowed subasset type
#if UNITY_2023_OR_NEWER
                foreach ((Type t, GUIContent label) in allowedSubassets)
                {
#else
                foreach (KeyValuePair<Type, GUIContent> item in allowedSubassets)
                {
                    Type t = item.Key;
                    GUIContent label = item.Value;
#endif
                    // Check if another subasset with the same type already exists in the list
                    bool containsItem = false;
                    for (int i = 0; i < list.serializedProperty.arraySize; i++)
                    {
                        SerializedProperty itemProp = list.serializedProperty.GetArrayElementAtIndex(i);
                        if (itemProp.objectReferenceValue != null && itemProp.objectReferenceValue.GetType() == t)
                        {
                            containsItem = true;
                            break;
                        }
                    }

                    // If another subasset of the current type exists in the list but unique mode is enabled
                    if (containsItem && optionsAttribute != null && optionsAttribute.Unique)
                    {
                        menu.AddDisabledItem(label, true);
                        return;
                    }
                    // Else, add menu item
                    else
                    {
                        menu.AddItem(label, containsItem, () => CreateAndAddSubasset(innerListProp, t, label.text));
                    }
                }

                menu.ShowAsContext();
            };

            // Remove item behavior
            _subassetsReorderableList.onRemoveCallback = (list) =>
            {
                if (!EditorUtility.DisplayDialog("Delete Subasset", "This operation can't be undone.\nProceed?", "Delete", "Cancel"))
                    return;

                SerializedProperty itemProp = list.serializedProperty.GetArrayElementAtIndex(list.index);

                // Destroy subasset if applicable
                if (itemProp.objectReferenceValue != null)
                {
                    Object.DestroyImmediate(itemProp.objectReferenceValue, true);
                    itemProp.serializedObject.targetObject.SaveAndReimport();
                }

                // Remove item from the list
                itemProp.objectReferenceValue = null;
                list.serializedProperty.DeleteArrayElementAtIndex(list.index);
                list.serializedProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            };

            // Calculate item height
            _subassetsReorderableList.elementHeightCallback = (index) =>
            {
                SerializedProperty itemProp = _subassetsReorderableList.serializedProperty.GetArrayElementAtIndex(index);
                if (itemProp.objectReferenceValue == null || !itemProp.isExpanded)
                    return EditorGUIUtility.singleLineHeight + ItemRowPadding * 2;

                // Header row + vertical padding
                float height = EditorGUIUtility.singleLineHeight + ItemRowPadding * 2;

                SerializedObject itemObj = new SerializedObject(itemProp.objectReferenceValue);
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
            };

            // Item GUI
            _subassetsReorderableList.drawElementCallback = (position, index, isActive, isFocused) =>
            {
                SerializedProperty itemProp = _subassetsReorderableList.serializedProperty.GetArrayElementAtIndex(index);

                Rect rect = position;
                rect.y += ItemRowPadding;
                rect.height = EditorGUIUtility.singleLineHeight;

                // Cancel if the reference to the asset is not defined
                if (itemProp.objectReferenceValue == null)
                {
                    EditorGUI.HelpBox(rect, $"Null reference", MessageType.Error);
                    return;
                }

                SubassetLabelAttribute subassetLabelAttribute = itemProp.objectReferenceValue.GetType().GetCustomAttribute<SubassetLabelAttribute>();

                // Get subasset label
                GUIContent label = new GUIContent(itemProp.objectReferenceValue.name);
                if (subassetLabelAttribute != null && !string.IsNullOrWhiteSpace(subassetLabelAttribute.Description))
                    label.tooltip = subassetLabelAttribute.Description;

                // Indent the whole rect so the foldout icon doesn't merge with the drag icon of the Reorderable List GUI
                rect.x += FoldoutIndent;
                rect.width -= FoldoutIndent;

                Rect tmpRect = rect;

                SubassetsListOptionsAttribute optionsAttribute = attribute as SubassetsListOptionsAttribute;
                // If renaming subassets is allowed, draw only the foldout icon and a text field
                if (optionsAttribute == null || !optionsAttribute.DisllowRename)
                {
                    Rect headerRect = tmpRect;
                    headerRect.width = MiniFoldoutWidth;
                    itemProp.isExpanded = EditorGUI.Foldout(headerRect, itemProp.isExpanded, new GUIContent(string.Empty, label.tooltip), true);

                    headerRect.x += headerRect.width;
                    headerRect.width = tmpRect.width - headerRect.width;
                    using (var scope = new EditorGUI.ChangeCheckScope())
                    {
                        string newName = EditorGUI.DelayedTextField(headerRect, itemProp.objectReferenceValue.name);
                        if (scope.changed)
                        {
                            itemProp.objectReferenceValue.name = newName;
                            AssetDatabase.SaveAssets();
                        }
                    }

                    // Fix offset to make sure all the fields are aligned with the text field
                    rect.x += MiniFoldoutWidth;
                    rect.width -= MiniFoldoutWidth;
                    tmpRect = rect;
                }
                // Else, just draw a foldout with label
                else
                {
                    itemProp.isExpanded = EditorGUI.Foldout(tmpRect, itemProp.isExpanded, label, true, EditorStyles.foldout.Bold());
                }

                itemProp.serializedObject.ApplyModifiedProperties();

                // Stop is the property is not expanded
                if (!itemProp.isExpanded)
                    return;

                SerializedObject itemObj = new SerializedObject(itemProp.objectReferenceValue);
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
            };

            // Reorder items behavior
            _subassetsReorderableList.onReorderCallback = (list) =>
            {
                list.serializedProperty.serializedObject.ApplyModifiedProperties();
            };

            return _subassetsReorderableList;
        }

        /// <summary>
        /// Gets the list of the allowed subassets for the given list property.
        /// </summary>
        /// <returns>Returns the informations about the allowed subassets for the given list property, where keys are the types, and values are
        /// the display names.</returns>
        /// <inheritdoc cref="SubassetsEditorUtility.GetAllowedSubassetsInfos(Type)"/>
        private Dictionary<Type, GUIContent> GetAllowedSubassets(Type subassetsBaseType)
        {
            if (_allowedSubassetTypes != null)
                return _allowedSubassetTypes;

            _allowedSubassetTypes = SubassetsEditorUtility.GetAllowedSubassetsInfos(subassetsBaseType);
            return _allowedSubassetTypes;
        }

        /// <summary>
        /// Creates a subasset of a given type, and attach it to the given list property.
        /// </summary>
        /// <param name="assetType">The type of the subasset to create.</param>
        /// <inheritdoc cref="GetSubassetsReorderableList(SerializedProperty, Type)"/>
        private void CreateAndAddSubasset(SerializedProperty innerListProp, Type assetType, string name = null)
        {
            ScriptableObject subasset = ScriptableObject.CreateInstance(assetType);
            subasset.name = !string.IsNullOrEmpty(name) ? name : ObjectNames.NicifyVariableName(assetType.Name);

            // Attach subasset & save database
            AssetDatabase.AddObjectToAsset(subasset, innerListProp.serializedObject.targetObject);
            AssetDatabase.SaveAssets();

            // Add the created subasset to the list
            int index = innerListProp.arraySize;
            innerListProp.InsertArrayElementAtIndex(index);
            innerListProp.GetArrayElementAtIndex(index).objectReferenceValue = subasset;
            innerListProp.serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>
        /// Gets the expected type of subassets, based on the property's related field's declaration.
        /// </summary>
        /// <param name="property">The property being inspected (not the inner subassets list property).</param>
        /// <returns>Returns the expected type of subassets.</returns>
        private Type GetExpectedSubassetsType(SerializedProperty property)
        {
            Type subassetsBaseType = null;
            try
            {
                FieldInfo propertyInfo = property.serializedObject.targetObject.GetType().GetField(property.name, ReflectionUtility.InstanceFlags);
                subassetsBaseType = propertyInfo.FieldType.GenericTypeArguments[0];
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Debug.LogError($"An error has occured while trying to get the subassets base type expected for the property {property.serializedObject.targetObject.GetType().Name}.{property.name}. See previous log for more info.", property.serializedObject.targetObject);
            }

            return subassetsBaseType;
        }

    }

}