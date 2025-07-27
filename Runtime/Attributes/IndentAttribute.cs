using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Indents a property in the inspector (using custom property drawer).
    /// </summary>
    public class IndentAttribute : PropertyAttribute
    {

        /// <summary>
        /// The number of levels to add to the current indent level.
        /// </summary>
        public int Levels { get; private set; } = 1;

        /// <summary>
        /// Indents this property in the inspector.
        /// </summary>
        /// <param name="levels">The number of levels to add to the current indent level.</param>
        public IndentAttribute(int levels = 1)
        {
            Levels = levels;
        }

    }

}