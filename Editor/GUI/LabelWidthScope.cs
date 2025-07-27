using UnityEngine;
using UnityEditor;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Custom GUI scope for customizing the default label width in a block.
    /// </summary>
    public class LabelWidthScope : GUI.Scope
    {

        /// <summary>
        /// The editor label width before applying the custom value.
        /// </summary>
        private float _previousLabelWidth = 0f;

        /// <summary>
        /// Defines the editor label width inside this block.
        /// </summary>
        /// <param name="labelWidth">The expected label width.</param>
        public LabelWidthScope(float labelWidth)
        {
            _previousLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = labelWidth;
        }

        /// <summary>
        /// Reset the original editor label width.
        /// </summary>
        protected override void CloseScope()
        {
            EditorGUIUtility.labelWidth = _previousLabelWidth;
        }

    }

}