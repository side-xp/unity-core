using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Makes a field enabled only if a named boolean property is not checked.
    /// </summary>
    public class DisableIfAttribute : PropertyAttribute
    {

        /// <summary>
        /// The name of the boolean property that shouldn't be checked.
        /// </summary>
        public string PropertyName { get; private set; } = null;

        /// <summary>
        /// If enabled, the field is hidden if the condition is not fulfilled, instead of being just disabled.
        /// </summary>
        public bool HideIfDisabled { get; set; } = false;

        /// <inheritdoc cref="DisableIfAttribute"/>
        /// <param name="propertyName">The name of the property that shouldn't be checked.</param>
        public DisableIfAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }

    }

}