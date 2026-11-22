using UnityEngine;
using UnityEngine.UIElements;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// UI Toolkit element that displays an editor icon from <see cref="EditorIcons"/>.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Shows the loading placeholder until the icon is downloaded, then updates itself.</item>
    /// <item>Sets its own width and height from <see cref="IconSize"/>. Sizes set from code take precedence over USS, so to use a
    /// custom size, set the <see cref="SizeUssProperty"/> custom USS property in points (as <c>--sxp-icon-size: 20;</c>).</item>
    /// <item>The icon is drawn as background image, so USS can tint it with <c>-unity-background-image-tint-color</c>.</item>
    /// <item>Has the <see cref="UssClassName"/> USS class, and a USS class per size (as <see cref="SmallUssClassName"/>).</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// &lt;ui:UXML xmlns:ui="UnityEngine.UIElements" xmlns:sxp="SideXP.Core.EditorOnly"&gt;
    ///     &lt;sxp:Icon icon="home" icon-style="Filled" icon-color="White" icon-size="Large" /&gt;
    /// &lt;/ui:UXML&gt;
    /// </code>
    /// </example>
#if UNITY_2023_2_OR_NEWER
    [UxmlElement]
#endif
    public partial class Icon : VisualElement
    {

        #region Subclasses

#if !UNITY_2023_2_OR_NEWER
        /// <summary>
        /// Creates <see cref="Icon"/> elements from UXML.
        /// </summary>
        public new class UxmlFactory : UxmlFactory<Icon, UxmlTraits> { }

        /// <summary>
        /// Defines the UXML attributes of <see cref="Icon"/> elements.
        /// </summary>
        public new class UxmlTraits : VisualElement.UxmlTraits
        {

            /// <summary>
            /// The name of the icon.
            /// </summary>
            private readonly UxmlStringAttributeDescription _icon = new UxmlStringAttributeDescription { name = IconAttribute };

            /// <summary>
            /// The style of the icon.
            /// </summary>
            private readonly UxmlEnumAttributeDescription<EIconStyle> _iconStyle = new UxmlEnumAttributeDescription<EIconStyle>
            {
                name = IconStyleAttribute,
                defaultValue = EIconStyle.Outline,
            };

            /// <summary>
            /// The color of the icon.
            /// </summary>
            private readonly UxmlEnumAttributeDescription<EIconColor> _iconColor = new UxmlEnumAttributeDescription<EIconColor>
            {
                name = IconColorAttribute,
                defaultValue = EIconColor.Auto,
            };

            /// <summary>
            /// The size of the icon on screen.
            /// </summary>
            private readonly UxmlEnumAttributeDescription<EIconSize> _iconSize = new UxmlEnumAttributeDescription<EIconSize>
            {
                name = IconSizeAttribute,
                defaultValue = EIconSize.Small,
            };

            /// <inheritdoc/>
            public override void Init(VisualElement element, IUxmlAttributes bag, CreationContext context)
            {
                base.Init(element, bag, context);
                ((Icon)element).SetIcon(
                    _icon.GetValueFromBag(bag, context),
                    _iconStyle.GetValueFromBag(bag, context),
                    _iconColor.GetValueFromBag(bag, context),
                    _iconSize.GetValueFromBag(bag, context)
                );
            }

        }
#endif

        #endregion


        #region Fields

        /// <summary>
        /// USS class of every icon element.
        /// </summary>
        public const string UssClassName = "sxp-icon";

        /// <summary>
        /// USS class of the icon elements of size <see cref="EIconSize.Small"/>.
        /// </summary>
        public const string SmallUssClassName = UssClassName + "--small";

        /// <summary>
        /// USS class of the icon elements of size <see cref="EIconSize.Medium"/>.
        /// </summary>
        public const string MediumUssClassName = UssClassName + "--medium";

        /// <summary>
        /// USS class of the icon elements of size <see cref="EIconSize.Large"/>.
        /// </summary>
        public const string LargeUssClassName = UssClassName + "--large";

        /// <summary>
        /// Custom USS property that replaces the size from <see cref="IconSize"/>, in points (as <c>--sxp-icon-size: 20;</c>).
        /// </summary>
        public const string SizeUssProperty = "--sxp-icon-size";

        /// <summary>
        /// Name of the UXML attribute of <see cref="IconName"/>. The <c>name</c> attribute can't be used, since it's already used by
        /// <see cref="VisualElement"/>.
        /// </summary>
        private const string IconAttribute = "icon";

        /// <summary>
        /// Name of the UXML attribute of <see cref="IconStyle"/>.
        /// </summary>
        private const string IconStyleAttribute = "icon-style";

        /// <summary>
        /// Name of the UXML attribute of <see cref="IconColor"/>.
        /// </summary>
        private const string IconColorAttribute = "icon-color";

        /// <summary>
        /// Name of the UXML attribute of <see cref="IconSize"/>.
        /// </summary>
        private const string IconSizeAttribute = "icon-size";

        /// <summary>
        /// Reads <see cref="SizeUssProperty"/> from the resolved styles.
        /// </summary>
        private static readonly CustomStyleProperty<float> SizeStyleProperty = new CustomStyleProperty<float>(SizeUssProperty);

        /// <summary>
        /// The name of the displayed icon.
        /// </summary>
        private string _iconName = null;

        /// <summary>
        /// The style of the displayed icon.
        /// </summary>
        private EIconStyle _iconStyle = EIconStyle.Outline;

        /// <summary>
        /// The color of the displayed icon.
        /// </summary>
        private EIconColor _iconColor = EIconColor.Auto;

        /// <summary>
        /// The size of the displayed icon on screen.
        /// </summary>
        private EIconSize _iconSize = EIconSize.Small;

        /// <summary>
        /// The size set with <see cref="SizeUssProperty"/>, in points, or 0 if none.
        /// </summary>
        private float _customPointSize = 0f;

        /// <summary>
        /// Increased on each refresh, so a texture loaded for a previous icon is ignored.
        /// </summary>
        private int _requestId = 0;

        #endregion


        #region Lifecycle

        /// <summary>
        /// Creates an icon element without icon.
        /// </summary>
        public Icon()
            : this(null) { }

        /// <summary>
        /// Creates an icon element.
        /// </summary>
        /// <param name="iconName">The name of the icon, as <c>home</c>.</param>
        /// <param name="iconStyle">The style of the icon.</param>
        /// <param name="iconColor">The color of the icon.</param>
        /// <param name="iconSize">The size of the icon on screen.</param>
        public Icon(string iconName, EIconStyle iconStyle = EIconStyle.Outline, EIconColor iconColor = EIconColor.Auto,
            EIconSize iconSize = EIconSize.Small)
        {
            AddToClassList(UssClassName);
            style.flexShrink = 0f;
            style.flexGrow = 0f;
            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
            SetIcon(iconName, iconStyle, iconColor, iconSize);
        }

        #endregion


        #region Public API

        /// <summary>
        /// The name of the displayed icon, as <c>home</c>. Null or empty for no icon.
        /// </summary>
#if UNITY_2023_2_OR_NEWER
        [UxmlAttribute(IconAttribute)]
#endif
        public string IconName
        {
            get => _iconName;
            set => SetIcon(value, _iconStyle, _iconColor, _iconSize);
        }

        /// <summary>
        /// The style of the displayed icon.
        /// </summary>
#if UNITY_2023_2_OR_NEWER
        [UxmlAttribute(IconStyleAttribute)]
#endif
        public EIconStyle IconStyle
        {
            get => _iconStyle;
            set => SetIcon(_iconName, value, _iconColor, _iconSize);
        }

        /// <summary>
        /// The color of the displayed icon.
        /// </summary>
#if UNITY_2023_2_OR_NEWER
        [UxmlAttribute(IconColorAttribute)]
#endif
        public EIconColor IconColor
        {
            get => _iconColor;
            set => SetIcon(_iconName, _iconStyle, value, _iconSize);
        }

        /// <summary>
        /// The size of the displayed icon on screen. Replaced by <see cref="SizeUssProperty"/> if it's set.
        /// </summary>
#if UNITY_2023_2_OR_NEWER
        [UxmlAttribute(IconSizeAttribute)]
#endif
        public EIconSize IconSize
        {
            get => _iconSize;
            set => SetIcon(_iconName, _iconStyle, _iconColor, value);
        }

        /// <summary>
        /// The size of the element on screen, in points: from <see cref="SizeUssProperty"/> if it's set, or from
        /// <see cref="IconSize"/>.
        /// </summary>
        public float PointSize => _customPointSize > 0f ? _customPointSize : EditorIcons.GetPointSize(_iconSize);

        /// <summary>
        /// Sets the displayed icon, and updates the element once.
        /// </summary>
        /// <param name="iconName">The name of the icon, as <c>home</c>. Null or empty for no icon.</param>
        /// <param name="iconStyle">The style of the icon.</param>
        /// <param name="iconColor">The color of the icon.</param>
        /// <param name="iconSize">The size of the icon on screen.</param>
        public void SetIcon(string iconName, EIconStyle iconStyle, EIconColor iconColor, EIconSize iconSize)
        {
            _iconName = iconName;
            _iconStyle = iconStyle;
            _iconColor = iconColor;
            _iconSize = iconSize;
            Refresh();
        }

        #endregion


        #region Private API

        /// <summary>
        /// Updates the size, the USS classes and the image of the element.
        /// </summary>
        private void Refresh()
        {
            EnableInClassList(SmallUssClassName, _iconSize == EIconSize.Small);
            EnableInClassList(MediumUssClassName, _iconSize == EIconSize.Medium);
            EnableInClassList(LargeUssClassName, _iconSize == EIconSize.Large);

            float pointSize = PointSize;
            style.width = pointSize;
            style.height = pointSize;

            int requestId = ++_requestId;
            if (string.IsNullOrEmpty(_iconName))
            {
                style.backgroundImage = StyleKeyword.None;
                return;
            }

            Texture2D texture = EditorIcons.GetIcon(_iconName, _iconStyle, _iconColor, pointSize, out bool isAvailable);
            style.backgroundImage = new StyleBackground(texture);
            if (isAvailable)
                return;

            EditorIcons.LoadIcon(_iconName, icon =>
            {
                // Ignore an icon loaded for a previous name, style, color or size
                if (requestId == _requestId)
                    style.backgroundImage = new StyleBackground(icon);
            }, _iconStyle, _iconColor, pointSize);
        }

        /// <summary>
        /// Reads the custom size of the element from USS, and updates the element if it changed.
        /// </summary>
        /// <param name="customStyleEvent">The event sent when the styles of the element have been resolved.</param>
        private void OnCustomStyleResolved(CustomStyleResolvedEvent customStyleEvent)
        {
            float customPointSize = customStyleEvent.customStyle.TryGetValue(SizeStyleProperty, out float value) && value > 0f
                ? value
                : 0f;

            if (Mathf.Approximately(customPointSize, _customPointSize))
                return;

            _customPointSize = customPointSize;
            Refresh();
        }

        #endregion

    }

}
