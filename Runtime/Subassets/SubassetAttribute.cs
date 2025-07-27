using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Marks a property as referencing an asset that should be attached to the declaring class.<br/>
    /// Both the decorated property type and the declaring class must derive from <see cref="ScriptableObject"/>.
    /// </summary>
    /// <remarks>This attribute is designed to work for a single subasset. If you need a list of subassets, consider using
    /// <see cref="SubassetsList{T}"/> instead.</remarks>
    public class SubassetAttribute : PropertyAttribute
    {

        /// <summary>
        /// By default, this attribute states that the expected referenced object must be a subasset attached to a main one. If enabled, the
        /// object field will be enabled anytime so the user can use any asset of the project (not necessarily the created subasset).
        /// </summary>
        public bool AllowExternal { get; set; } = false;

    }

}