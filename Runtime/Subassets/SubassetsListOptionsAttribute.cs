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
        public bool DisllowRename { get; set; } = false;

        /// <summary>
        /// By default, the list is allowed to contain several instances of the same subasset type.<br/>
        /// If enabled, types already existing in the list will be disabled in the inspector.
        /// </summary>
        public bool Unique { get; set; } = false;

        /// <inheritdoc cref="SubassetsListOptionsAttribute"/>
        public SubassetsListOptionsAttribute() { }

    }

}