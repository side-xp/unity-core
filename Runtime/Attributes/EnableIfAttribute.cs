using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Makes a field enabled only if a named boolean field or property is checked.
    /// </summary>
    public class EnableIfAttribute : PropertyAttribute
    {

        /// <summary>
        /// The name of the boolean field or property that should be checked.
        /// </summary>
        public string PropertyName { get; private set; } = null;

        /// <summary>
        /// If enabled, the field is hidden if the condition is not fulfilled, instead of being just disabled.
        /// </summary>
        public bool HideIfDisabled { get; set; } = false;

        /// <inheritdoc cref="EnableIfAttribute"/>
        /// <param name="propertyName">The name of the field or property that should be checked.</param>
        public EnableIfAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }

    }

}