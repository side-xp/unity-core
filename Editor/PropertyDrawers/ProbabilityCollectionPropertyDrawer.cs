using System;
using System.Reflection;

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

using SideXP.Core;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Base custom property drawer for <see cref="ProbabilityCollection"/> fields.
    /// </summary>
    [CustomPropertyDrawer(typeof(ProbabilityCollection), true)]
    public class ProbabilityCollectionPropertyDrawer : PropertyDrawer
    {

        private const string ItemsProp = "_items";
        private const string ItemDataProp = "_data";
        private const string ItemProbabilityProp = "_probability";

        public const float MinProbability = 0f;
        public const float MaxProbability = 100f;

        private const float EffectiveValueFieldWidth = MoreGUI.WidthXS + MoreGUI.WidthXS / 2;

        private ProbabilityCollection _collection = null;
        private ReorderableList _itemsList = null;
    
        /// <inheritdoc cref="PropertyDrawer.OnGUI(Rect, SerializedProperty, GUIContent)"/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Use normal editor if multiple values selected
            if (property.hasMultipleDifferentValues)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            // Initialize items list if needed
            if (_itemsList == null)
                InitItemsList(property);

            // Display default field if the items list can't be initialized
            if (_itemsList == null)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            _itemsList.DoList(position);
        }

        /// <inheritdoc cref="PropertyDrawer.GetPropertyHeight(SerializedProperty, GUIContent)"/>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return _itemsList != null
                ? _itemsList.GetHeight()
                : base.GetPropertyHeight(property, label);
        }

        /// <summary>
        /// Initializes the reorderable list of items to display, based on a given property.
        /// </summary>
        /// <param name="property">The collection property.</param>
        private void InitItemsList(SerializedProperty property)
        {
            SerializedProperty itemsProp = property.FindPropertyRelative(ItemsProp);
            if (itemsProp == null)
            {
                Debug.LogWarning($"The custom editor for {nameof(ProbabilityCollection)} expects a property named \"{ItemsProp}\" that contains the actual items of the collection. But none has been found on the property {property.name}, of type {property.type}.", property.serializedObject.targetObject);
                return;
            }

            Type collectionType = property.GetTargetType();
            ProbabilityCollectionOptionsAttribute collectionOptions = null;
            try
            {
                if (collectionType != null)
                    collectionOptions = collectionType.GetCustomAttribute<ProbabilityCollectionOptionsAttribute>();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            try
            {
                _collection = property.GetTarget() as ProbabilityCollection;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            _itemsList = new ReorderableList
            (
                property.serializedObject,
                itemsProp,
                collectionOptions == null || collectionOptions.Reorderable,
                true,
                collectionOptions == null || collectionOptions.AllowAddOrRemove,
                collectionOptions == null || collectionOptions.AllowAddOrRemove
            );

            _itemsList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, property.GetLabel(), EditorStyles.boldLabel);

            _itemsList.drawElementCallback = (position, index, isActive, isFocused) =>
            {
                SerializedProperty itemProperty = _itemsList.serializedProperty.GetArrayElementAtIndex(index);
                IProbabilityItem item = null;
                try
                {
                    item = itemProperty.GetTarget() as IProbabilityItem;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }

                Rect rect = new Rect(position);
                rect.height = EditorGUIUtility.singleLineHeight;
                rect.y += (position.height - rect.height) / 2;

                // Item field or label
                SerializedProperty itemDataProperty = itemProperty.FindPropertyRelative(ItemDataProp);
                rect.width = EditorGUIUtility.labelWidth;
                if (itemDataProperty == null)
                    EditorGUI.LabelField(rect, item != null && !string.IsNullOrEmpty(item.Label) ? new GUIContent(item.Label) : itemProperty.GetLabel());
                else
                    EditorGUI.PropertyField(rect, itemDataProperty, GUIContent.none, true);

                rect.x += rect.width + MoreGUI.HMargin;
                rect.width = position.width - rect.width - EffectiveValueFieldWidth - MoreGUI.HMargin * 2;
                SerializedProperty probabilityValueProp = itemProperty.FindPropertyRelative(ItemProbabilityProp);
                if (probabilityValueProp == null)
                {
                    Debug.LogWarning($"The custom editor for {nameof(ProbabilityCollection)} expects a property named \"{ItemProbabilityProp}\" on items that defines their probability value. But none has been found on the property {property.name}, of type {property.type}.", property.serializedObject.targetObject);
                    using (new EnabledScope(false))
                    {
                        EditorGUI.Slider(rect, 0, MinProbability, MaxProbability);
                    }
                }
                else
                {
                    probabilityValueProp.floatValue = EditorGUI.Slider(rect, probabilityValueProp.floatValue, MinProbability, MaxProbability);
                }

                rect.x += rect.width + MoreGUI.HMargin;
                rect.width = EffectiveValueFieldWidth;
                if (_collection != null && item != null)
                {
                    EditorGUI.LabelField(rect, $"{_collection.GetProbabilityPercents(item.Data):##0.00}%", EditorStyles.label.AlignRight());
                }
                else
                {
                    using (new EnabledScope(false))
                    {
                        EditorGUI.LabelField(rect, "?? %", EditorStyles.label.AlignRight());
                    }
                }
            };
        }

    }

}