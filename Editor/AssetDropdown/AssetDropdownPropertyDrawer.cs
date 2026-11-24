using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
#if UNITY_2022_2_OR_NEWER
using UnityEngine.UIElements;
#endif

using Object = UnityEngine.Object;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Draws the fields marked with <see cref="AssetDropdownAttribute"/> as dropdowns of assets, with a button that highlights the
    /// referenced asset in the Project window.
    /// </summary>
    /// <remarks>
    /// The drawer implements both IMGUI (all versions) and UI Toolkit (from Unity 2022.2, the version from which the default Inspector
    /// uses UI Toolkit). A field of a type the dropdown doesn't support (see <see cref="AssetDropdownMenu.IsSupported(Type)"/>) shows a
    /// message instead, like the built-in drawers of Unity on fields of the wrong type.
    /// <para>
    /// The button belongs to the drawer, not to <see cref="AssetDropdownField"/>, so other tools that reuse the field don't get it.
    /// </para>
    /// </remarks>
    [CustomPropertyDrawer(typeof(AssetDropdownAttribute))]
    public class AssetDropdownPropertyDrawer : PropertyDrawer
    {

        #region Fields

        /// <summary>
        /// The USS class of the element that holds the dropdown and the ping button, in UI Toolkit.
        /// </summary>
        public const string RowUssClassName = "sxp-asset-dropdown-row";

        /// <summary>
        /// The USS class of the button that highlights the referenced asset, in UI Toolkit.
        /// </summary>
        public const string PingButtonUssClassName = "sxp-asset-dropdown-row__ping-button";

        /// <summary>
        /// The name of the icon of the button that highlights the referenced asset, from <see cref="EditorIcons"/>.
        /// </summary>
        private const string PingIcon = "search";

        private const string PingTooltip = "Highlights the referenced asset in the Project window.";

        /// <summary>
        /// The width of the button that highlights the referenced asset, in pixels: square at the height of a line of the Inspector,
        /// with a margin for its border.
        /// </summary>
        private const float PingButtonWidth = 22;

        /// <summary>
        /// The space between the dropdown and the button that highlights the referenced asset, in pixels.
        /// </summary>
        private const float PingButtonMargin = 2;

        /// <summary>
        /// The size of the icon of the referenced asset in the IMGUI dropdown, in pixels, the size of the icons of the object fields.
        /// </summary>
        private static readonly Vector2 IconSize = new Vector2(16, 16);

        #endregion


        #region UI Toolkit

#if UNITY_2022_2_OR_NEWER
        /// <inheritdoc/>
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            if (!TryGetAssetType(property, out Type assetType))
            {
                // Unity's built-in drawers show the same kind of message on fields of the wrong type
                TextField message = new TextField(property.displayName) { value = AssetDropdownMenu.UnsupportedTypeMessage };
                message.SetEnabled(false);
                message.AddToClassList(BaseField<string>.alignedFieldUssClassName);
                return message;
            }

            AssetDropdownField field = new AssetDropdownField(property.displayName, assetType)
            {
                bindingPath = property.propertyPath,
                tooltip = property.tooltip,
            };
            field.AddToClassList(BaseField<Object>.alignedFieldUssClassName);
            field.style.flexGrow = 1;
            field.style.flexShrink = 1;

            VisualElement row = new VisualElement();
            row.AddToClassList(RowUssClassName);
            row.style.flexDirection = FlexDirection.Row;
            row.Add(field);
            row.Add(CreatePingButton(field));
            return row;
        }

        /// <summary>
        /// Creates the button that highlights the asset referenced by a dropdown in the Project window.
        /// </summary>
        /// <param name="field">The dropdown whose value the button highlights.</param>
        /// <returns>Returns the created button.</returns>
        private static Button CreatePingButton(AssetDropdownField field)
        {
            // Reads the value on click, since the binding sets it without notifying a change
            Button button = new Button(() => Ping(field.showMixedValue ? null : field.value)) { tooltip = PingTooltip };
            button.AddToClassList(PingButtonUssClassName);
            button.style.width = PingButtonWidth;
            button.style.flexShrink = 0;
            button.style.marginLeft = PingButtonMargin;
            button.style.marginRight = 0;
            button.style.paddingLeft = 0;
            button.style.paddingRight = 0;
            button.style.justifyContent = Justify.Center;
            button.style.alignItems = Align.Center;

            Icon icon = new Icon(PingIcon) { pickingMode = PickingMode.Ignore };
            button.Add(icon);
            return button;
        }
#endif

        #endregion


        #region IMGUI

        /// <inheritdoc/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!TryGetAssetType(property, out Type assetType))
            {
                EditorGUI.LabelField(position, label.text, AssetDropdownMenu.UnsupportedTypeMessage);
                return;
            }

            label = EditorGUI.BeginProperty(position, label, property);
            Rect fieldPosition = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            Rect pingButtonPosition = new Rect(fieldPosition);
            pingButtonPosition.xMin = fieldPosition.xMax - PingButtonWidth;
            fieldPosition.width = Mathf.Max(0, fieldPosition.width - PingButtonWidth - PingButtonMargin);

            // BeginProperty() sets the mixed value of several selected objects, which the dropdown button draws itself
            Object value = EditorGUI.showMixedValue ? null : property.objectReferenceValue;
            GUIContent content = new GUIContent(AssetDropdownMenu.GetValueText(value), AssetDropdownMenu.GetValueIcon(value));

            Vector2 previousIconSize = EditorGUIUtility.GetIconSize();
            EditorGUIUtility.SetIconSize(IconSize);
            bool isClicked = EditorGUI.DropdownButton(fieldPosition, content, FocusType.Keyboard, EditorStyles.popup);
            EditorGUIUtility.SetIconSize(previousIconSize);

            if (isClicked)
            {
                Object[] targets = property.serializedObject.targetObjects;
                string propertyPath = property.propertyPath;
                AssetDropdownMenu.Create(assetType, value, picked => Write(targets, propertyPath, picked)).DropDown(fieldPosition);
            }

            // Not affected by the mixed value: the button has no value of its own
            bool wasMixed = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = false;
            if (EditorIcons.IconButton(pingButtonPosition, PingIcon, new GUIContent(string.Empty, PingTooltip)))
                Ping(value);

            EditorGUI.showMixedValue = wasMixed;
            EditorGUI.EndProperty();
        }

        #endregion


        #region Private API

        /// <summary>
        /// Gets the type of the assets listed by the dropdown of a property.
        /// </summary>
        /// <param name="property">The drawn property.</param>
        /// <param name="assetType">Outputs the type of the assets, or null if the dropdown doesn't support the field.</param>
        /// <returns>Returns true if the dropdown supports the field: an object reference to a supported type of asset.</returns>
        private bool TryGetAssetType(SerializedProperty property, out Type assetType)
        {
            // Resolved by Unity, so it's right for private fields of base classes and fields of nested objects
            assetType = AssetDropdownMenu.GetAssetType(fieldInfo != null ? fieldInfo.FieldType : null);
            return property.propertyType == SerializedPropertyType.ObjectReference && AssetDropdownMenu.IsSupported(assetType);
        }

        /// <summary>
        /// Highlights an asset in the Project window, without selecting it.
        /// </summary>
        /// <param name="asset">The asset to highlight. Does nothing if it's null (<i>None</i>, a missing asset, or the mixed value of
        /// several selected objects).</param>
        private static void Ping(Object asset)
        {
            if (asset != null)
                EditorGUIUtility.PingObject(asset);
        }

        /// <summary>
        /// Writes an asset picked in the IMGUI dropdown to the property of the selected objects, as one Undo step.
        /// </summary>
        /// <remarks>
        /// The menu calls back after the GUI that opened it, when its serialized object may have been disposed (for example if the
        /// selection changed), so the property is found again on a new serialized object.
        /// </remarks>
        /// <param name="targets">The objects whose property is written.</param>
        /// <param name="propertyPath">The path of the property.</param>
        /// <param name="picked">The picked asset, or null for <i>None</i>.</param>
        private static void Write(Object[] targets, string propertyPath, Object picked)
        {
            // Some of the objects may have been destroyed while the menu was open
            List<Object> existingTargets = new List<Object>();
            foreach (Object target in targets)
            {
                if (target != null)
                    existingTargets.Add(target);
            }

            if (existingTargets.Count == 0)
                return;

            using (SerializedObject serializedObject = new SerializedObject(existingTargets.ToArray()))
            {
                SerializedProperty property = serializedObject.FindProperty(propertyPath);
                if (property == null)
                    return;

                property.objectReferenceValue = picked;
                serializedObject.ApplyModifiedProperties();
            }
        }

        #endregion

    }

}
