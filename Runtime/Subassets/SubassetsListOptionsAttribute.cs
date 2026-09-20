using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Defines additional options for <see cref="SubassetsList{T}"/> properties.
    /// </summary>
    public class SubassetsListOptionsAttribute : PropertyAttribute
    {

        /// <summary>
        /// By default, subassets can be renamed from the inspector. If enabled, subassets can't be renamed at all.
        /// </summary>
        public bool DisallowRename { get; set; } = false;

        /// <summary>
        /// By default, the properties of the subassets can be edited from the list itself.<br/>
        /// If enabled, each item is displayed as a disabled object field instead. The subassets can still be selected and edited from the
        /// inspector by double-clicking on that field.
        /// </summary>
        /// <remarks>This is especially useful if the subassets have <see cref="SubassetsList{T}"/> properties themselves, since nesting
        /// these lists makes the inspector compute wrong GUI heights.</remarks>
        public bool NotEditable { get; set; } = false;

        /// <summary>
        /// By default, the list is allowed to contain several instances of the same subasset type.<br/>
        /// If enabled, types already existing in the list will be disabled in the inspector.
        /// </summary>
        public bool Unique { get; set; } = false;

        /// <inheritdoc cref="SubassetsListOptionsAttribute"/>
        public SubassetsListOptionsAttribute() { }

    }

}
