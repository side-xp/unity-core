using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Displays a button to enable/disable a boolean value instead of the regular 
    /// </summary>
    public class BooltonAttribute : PropertyAttribute
    {

        /// <summary>
        /// The name of the function to call (from this script) when the button is clicked.
        /// </summary>
        /// <remarks>The callback can be a function (no return type and a single boolean parameter), or a boolean setter.</remarks>
        public string CallbackName { get; set; } = null;

        /// <summary>
        /// By default, a button with a label "Enable/Disable" will be displayed for this property. If this option is enabled, a regular
        /// toggle field will be used, so the named callback can still be invoked without changing the common usage of this property.
        /// </summary>
        public bool Toggle { get; set; } = false;

        /// <inheritdoc cref="BooltonAttribute"/>
        public BooltonAttribute()
            : this(null) { }

        /// <inheritdoc cref="BooltonAttribute"/>
        /// <param name="callbackName"><inheritdoc cref="CallbackName" path="/summary"/> <inheritdoc cref="CallbackName" path="/remarks"/></param>
        public BooltonAttribute(string callbackName)
        {
            CallbackName = callbackName;
        }

    }

}
