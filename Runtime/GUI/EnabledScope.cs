using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Custom GUI Scope for drawing disabled fields.
    /// </summary>
    public class EnabledScope : GUI.Scope
    {

        /// <summary>
        /// Caches the previous "enabled" state of the GUI before using this scope, so that state can be recovered after closing it.
        /// </summary>
        private bool _previousState = false;

        /// <summary>
        /// Begins a scope with disabled fields.
        /// </summary>
        /// <param name="enabled">If true, make the fields in this block are enabled.</param>
        public EnabledScope(bool enabled = true)
        {
            _previousState = GUI.enabled;
            GUI.enabled = enabled;
        }

        /// <summary>
        /// Restores the enabled state of the GUI.
        /// </summary>
        protected override void CloseScope()
        {
            GUI.enabled = _previousState;
        }

    }

}