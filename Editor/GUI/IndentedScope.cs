using UnityEngine;
using UnityEditor;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Custom GUI Scope for drawing indented fields.
    /// </summary>
    public class IndentedScope : GUI.Scope
    {

        /// <summary>
        /// The indentation level of this scope.
        /// </summary>
        private int _levels = 1;

        /// <summary>
        /// Begins a scope with the given indentation level.
        /// </summary>
        /// <param name="levels">The number of indentation levels to apply. Can't be less than 1.</param>
        public IndentedScope(int levels = 1)
        {
            _levels = levels;
            EditorGUI.indentLevel += levels;
        }

        /// <summary>
        /// Restores the indentation level.
        /// </summary>
        protected override void CloseScope()
        {
            EditorGUI.indentLevel -= _levels;
        }

    }

}