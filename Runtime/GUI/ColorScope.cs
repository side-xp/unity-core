using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Custom GUI Scope for temporarily changing the GUI color.
    /// </summary>
    public class ColorScope : GUI.Scope
    {

        /// <summary>
        /// Caches the GUI color before this scope is created.
        /// </summary>
        private Color _previousColor = Color.white;

        /// <summary>
        /// Begins a scope with custom GUI color.
        /// </summary>
        /// <param name="color">The new GUI color.</param>
        public ColorScope(Color color)
        {
            _previousColor = GUI.color;
            GUI.color = color;
        }

        /// <summary>
        /// Begins a scope with custom GUI color.
        /// </summary>
        /// <param name="color">The new GUI color.</param>
        /// <param name="ignoreAlpha">If enabled, the alpha component of the given color is ignored.</param>
        public ColorScope(FColor color, bool ignoreAlpha = false)
        {
            _previousColor = GUI.color;
            GUI.color = color.ToColor(ignoreAlpha);
            GUI.contentColor = Color.white;
            GUI.backgroundColor = Color.white;
        }

        /// <summary>
        /// Restores the previous GUI color.
        /// </summary>
        protected override void CloseScope()
        {
            GUI.color = _previousColor;
        }

    }

}