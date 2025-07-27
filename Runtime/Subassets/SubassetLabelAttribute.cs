using System;

namespace SideXP.Core
{

    /// <summary>
    /// Defines metadata for <see cref="UnityEngine.ScriptableObject"/> classes that can be used as subassets from a property marked with
    /// <see cref="SubassetsListOptionsAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class SubassetLabelAttribute : Attribute
    {

        /// <summary>
        /// The label displayed in the list's dropdown, and the default name of this subasset on its creation.<br/>
        /// If not defined, the "nicified" type name of this class is used.
        /// </summary>
        public string Name { get; set; } = null;

        /// <summary>
        /// The description of the subasset, as displayed in the inspector.
        /// </summary>
        public string Description { get; set; } = null;

        /// <inheritdoc cref="SubassetLabelAttribute"/>
        public SubassetLabelAttribute()
            : this(null, null) { }

        /// <inheritdoc cref="SubassetLabelAttribute(string, string)"/>
        public SubassetLabelAttribute(string name)
            : this(name, null) { }

        /// <inheritdoc cref="SubassetLabelAttribute"/>
        /// <param name="name"><inheritdoc cref="Name" path="/summary"/></param>
        /// <param name="description"><inheritdoc cref="Description" path="/summary"/></param>
        public SubassetLabelAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }

    }

}