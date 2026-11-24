#if UNITY_2022_2_OR_NEWER
using System;

using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

using Object = UnityEngine.Object;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// A UI Toolkit field that references an asset through a dropdown (see <see cref="AssetDropdownMenu"/>): it shows the icon and the
    /// name of the referenced asset, and opens the menu of the assets of its type when it's clicked.
    /// </summary>
    /// <remarks>
    /// The field looks like the popup fields of Unity, and can be bound to an object reference property like an object field, with
    /// <see cref="IBindable.bindingPath"/>. It supports the mixed value of several selected objects. Available from Unity 2022.2, the
    /// version from which the default Inspector uses UI Toolkit.
    /// </remarks>
    public class AssetDropdownField : BaseField<Object>
    {

        #region Fields

        /// <summary>
        /// The USS class of the field.
        /// </summary>
        public const string FieldUssClassName = "sxp-asset-dropdown";

        /// <summary>
        /// The USS class of the icon of the referenced asset.
        /// </summary>
        public const string IconUssClassName = FieldUssClassName + "__icon";

        /// <summary>
        /// The size of the icon of the referenced asset, in pixels, the size of the icons of the object fields of the Inspector.
        /// </summary>
        private const float IconSize = 16;

        /// <summary>
        /// The space between the icon and the name of the referenced asset, in pixels.
        /// </summary>
        private const float IconMargin = 2;

        /// <summary>
        /// The type of the assets listed by the dropdown.
        /// </summary>
        private Type _assetType = null;

        /// <summary>
        /// The element that displays the value and opens the menu when it's clicked.
        /// </summary>
        private VisualElement _input = null;

        /// <summary>
        /// The icon of the referenced asset.
        /// </summary>
        private Image _icon = null;

        /// <summary>
        /// The name of the referenced asset.
        /// </summary>
        private TextElement _text = null;

        #endregion


        #region Lifecycle

        /// <inheritdoc cref="AssetDropdownField"/>
        /// <param name="label">The label of the field, or null for a field without label.</param>
        /// <param name="assetType"><inheritdoc cref="_assetType" path="/summary"/></param>
        /// <exception cref="ArgumentNullException">Thrown if the type is null.</exception>
        public AssetDropdownField(string label, Type assetType)
            : this(label, assetType, new VisualElement()) { }

        /// <inheritdoc cref="AssetDropdownField(string, Type)"/>
        /// <param name="input"><inheritdoc cref="_input" path="/summary"/></param>
        private AssetDropdownField(string label, Type assetType, VisualElement input)
            : base(label, input)
        {
            _assetType = assetType ?? throw new ArgumentNullException(nameof(assetType));
            _input = input;

            // The classes of the popup fields of Unity give the field their look
            AddToClassList(BasePopupField<string, string>.ussClassName);
            AddToClassList(FieldUssClassName);
            labelElement.AddToClassList(BasePopupField<string, string>.labelUssClassName);
            _input.AddToClassList(BasePopupField<string, string>.inputUssClassName);

            _icon = new Image { scaleMode = ScaleMode.ScaleToFit, pickingMode = PickingMode.Ignore };
            _icon.AddToClassList(IconUssClassName);
            _icon.style.width = IconSize;
            _icon.style.height = IconSize;
            _icon.style.flexShrink = 0;
            _icon.style.marginRight = IconMargin;
            _input.Add(_icon);

            _text = new TextElement { pickingMode = PickingMode.Ignore };
            _text.AddToClassList(BasePopupField<string, string>.textUssClassName);
            _input.Add(_text);

            VisualElement arrow = new VisualElement { pickingMode = PickingMode.Ignore };
            arrow.AddToClassList(BasePopupField<string, string>.arrowUssClassName);
            _input.Add(arrow);

            // On the field itself rather than the input, so the other callbacks registered on the field still get the event after it
            // stops its propagation
            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
            UpdateDisplay();
        }

        #endregion


        #region Public API

        /// <inheritdoc cref="_assetType"/>
        public Type AssetType => _assetType;

        /// <inheritdoc/>
        public override void SetValueWithoutNotify(Object newValue)
        {
            base.SetValueWithoutNotify(newValue);
            UpdateDisplay();
        }

        /// <summary>
        /// Opens the menu of the assets of the type of the field, under the field. Picking an option changes the value of the field.
        /// </summary>
        public void ShowMenu()
        {
            // With several selected objects, no option is the current value
            Object currentValue = showMixedValue ? null : value;
            AssetDropdownMenu.Create(_assetType, currentValue, Pick).DropDown(_input.worldBound);
        }

        #endregion


        #region Private API

        /// <inheritdoc/>
        protected override void UpdateMixedValueContent()
        {
            UpdateDisplay();
        }

        /// <summary>
        /// Called when the field is clicked. Opens the menu if the input is clicked, like the popup fields of Unity, which open on the
        /// press of the button.
        /// </summary>
        /// <param name="evt">The emitted event.</param>
        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != (int)MouseButton.LeftMouse)
                return;

            // The label doesn't open the menu
            VisualElement target = evt.target as VisualElement;
            if (target == null || (target != _input && !_input.Contains(target)))
                return;

            ShowMenu();
            evt.StopPropagation();
        }

        /// <summary>
        /// Called when the field is submitted with the keyboard while it has the focus. Opens the menu.
        /// </summary>
        /// <param name="evt">The emitted event.</param>
        private void OnNavigationSubmit(NavigationSubmitEvent evt)
        {
            ShowMenu();
            evt.StopPropagation();
        }

        /// <summary>
        /// Sets the value of the field to an asset picked in the menu, and notifies the change.
        /// </summary>
        /// <param name="picked">The picked asset, or null for <i>None</i>.</param>
        private void Pick(Object picked)
        {
            if (!showMixedValue)
            {
                value = picked;
                return;
            }

            // The value of the first selected object may already be the picked asset, which wouldn't notify a change: the change is
            // notified anyway, so the binding writes it to all the selected objects
            Object previousValue = value;
            SetValueWithoutNotify(picked);
            using (ChangeEvent<Object> evt = ChangeEvent<Object>.GetPooled(previousValue, picked))
            {
                evt.target = this;
                SendEvent(evt);
            }
        }

        /// <summary>
        /// Shows the icon and the name of the referenced asset, or the mixed value of several selected objects.
        /// </summary>
        private void UpdateDisplay()
        {
            // Called by the base constructor before the elements exist, when it sets the initial value
            if (_text == null)
                return;

            if (showMixedValue)
            {
                _icon.image = null;
                _icon.style.display = DisplayStyle.None;
                _text.text = mixedValueString;
                return;
            }

            Texture2D icon = AssetDropdownMenu.GetValueIcon(value);
            _icon.image = icon;
            _icon.style.display = icon != null ? DisplayStyle.Flex : DisplayStyle.None;
            _text.text = AssetDropdownMenu.GetValueText(value);
            _input.tooltip = value != null ? AssetDatabase.GetAssetPath(value) : null;
        }

        #endregion

    }

}
#endif
