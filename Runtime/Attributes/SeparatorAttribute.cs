using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Draws a separator line before the next property (using custom property drawer).
    /// </summary>
    public class SeparatorAttribute : PropertyAttribute
    {

        /// <summary>
        /// If enabled, the separator will have the exact current view width.
        /// </summary>
        public bool Wide { get; set; } = false;

    }

}