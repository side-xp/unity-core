using UnityEngine;
using UnityEditor;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Custom GUI scope for drawing the images of <see cref="GUIContent"/> at the size on screen of editor icons.
    /// </summary>
    /// <remarks>
    /// IMGUI draws the image of a <see cref="GUIContent"/> at its size in pixels, as if it was in points. So without this scope, an
    /// icon from <see cref="EditorIcons"/> is drawn larger at high display scales (as twice as large at 200%), since its file has more
    /// pixels to stay sharp.
    /// </remarks>
    /// <example>
    /// <code>
    /// using (new IconSizeScope(EIconSize.Small))
    ///     GUILayout.Label(EditorIcons.IconContent("home", "Home"));
    /// </code>
    /// </example>
    public class IconSizeScope : GUI.Scope
    {

        /// <summary>
        /// The icon size set before this scope is created.
        /// </summary>
        private Vector2 _previousSize = Vector2.zero;

        /// <summary>
        /// Begins a scope that draws the images of <see cref="GUIContent"/> at the size on screen of editor icons.
        /// </summary>
        /// <param name="size">The size of the icons on screen.</param>
        public IconSizeScope(EIconSize size)
        {
            _previousSize = EditorGUIUtility.GetIconSize();
            float pointSize = EditorIcons.GetPointSize(size);
            EditorGUIUtility.SetIconSize(new Vector2(pointSize, pointSize));
        }

        /// <summary>
        /// Restores the previous icon size.
        /// </summary>
        protected override void CloseScope()
        {
            EditorGUIUtility.SetIconSize(_previousSize);
        }

    }

}
