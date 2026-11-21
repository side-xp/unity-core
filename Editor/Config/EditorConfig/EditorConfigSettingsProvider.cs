using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine.UIElements;

using Object = UnityEngine.Object;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Displays an editor config in the Project Settings or Preferences window, using its inspector.
    /// </summary>
    /// <remarks>
    /// Changes are saved as soon as they're made from the window, and when the page is closed.
    /// </remarks>
    /// <example>
    /// <code>
    /// public static class MyToolSettingsProvider
    /// {
    ///     [SettingsProvider]
    ///     private static SettingsProvider Register()
    ///     {
    ///         return EditorConfigSettingsProvider.Create&lt;MyToolSettings&gt;("Project/My Tool", SettingsScope.Project);
    ///     }
    /// }
    /// </code>
    /// </example>
    public class EditorConfigSettingsProvider : SettingsProvider
    {

        #region Fields

        /// <summary>
        /// Gets the config to display, resolved each time the page needs it so it's never a stale reference.
        /// </summary>
        private Func<Object> _getConfig = null;

        /// <summary>
        /// The editor used to draw the config, created when the page is opened and destroyed when it's closed.
        /// </summary>
        private Editor _editor = null;

        #endregion


        #region Lifecycle

        /// <inheritdoc cref="EditorConfigSettingsProvider"/>
        /// <param name="path">The path of the page in the settings window, as <c>"Project/My Tool"</c>.</param>
        /// <param name="scope">Defines if the page is displayed in the Project Settings or the Preferences window.</param>
        /// <param name="getConfig">Gets the config to display.</param>
        /// <param name="keywords">The keywords used to find the page from the window's search field.</param>
        public EditorConfigSettingsProvider(string path, SettingsScope scope, Func<Object> getConfig, IEnumerable<string> keywords = null)
            : base(path, scope, keywords)
        {
            _getConfig = getConfig ?? throw new ArgumentNullException(nameof(getConfig));
        }

        /// <summary>
        /// Creates a page to display the given editor config.
        /// </summary>
        /// <typeparam name="T">The type of the editor config to display.</typeparam>
        /// <param name="path">The path of the page in the settings window, as <c>"Project/My Tool"</c>.</param>
        /// <param name="scope">Defines if the page is displayed in the Project Settings or the Preferences window.</param>
        /// <param name="keywords">
        /// The keywords used to find the page from the window's search field. If null, the keywords are generated from the names of
        /// the config's fields.
        /// </param>
        /// <returns>Returns the created settings provider.</returns>
        public static EditorConfigSettingsProvider Create<T>(string path, SettingsScope scope, IEnumerable<string> keywords = null)
            where T : EditorConfig<T>
        {
            EditorConfigSettingsProvider provider = new EditorConfigSettingsProvider(path, scope, () => EditorConfig<T>.Instance, keywords);

            T config = EditorConfig<T>.Instance;
            if (keywords == null && config != null)
                provider.keywords = GetSearchKeywordsFromSerializedObject(new SerializedObject(config));

            return provider;
        }

        /// <inheritdoc/>
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);
            EnsureEditor();
        }

        /// <inheritdoc/>
        public override void OnDeactivate()
        {
            SaveConfig();
            DestroyEditor();
            base.OnDeactivate();
        }

        #endregion


        #region UI

        /// <inheritdoc/>
        public override void OnGUI(string searchContext)
        {
            if (!EnsureEditor())
            {
                EditorGUILayout.HelpBox("Failed to load the settings of this page. See the console for more info.", MessageType.Warning);
                return;
            }

            EditorGUI.BeginChangeCheck();
            _editor.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
                SaveConfig();
        }

        #endregion


        #region Private API

        /// <summary>
        /// Makes sure the editor exists and targets the current config instance, or creates it.
        /// </summary>
        /// <remarks>
        /// The config instance can change while the page is open (as when it's unloaded and loaded again), so the editor is recreated
        /// if its target has been destroyed.
        /// </remarks>
        /// <returns>Returns true if the editor is ready to be drawn.</returns>
        private bool EnsureEditor()
        {
            if (_editor != null && _editor.target != null)
                return true;

            DestroyEditor();
            Object config = _getConfig();
            if (config == null)
                return false;

            _editor = Editor.CreateEditor(config);
            return _editor != null;
        }

        /// <summary>
        /// Destroys the editor used to draw the config, if any.
        /// </summary>
        private void DestroyEditor()
        {
            if (_editor != null)
                Object.DestroyImmediate(_editor);

            _editor = null;
        }

        /// <summary>
        /// Writes the displayed config to its storage, if it changed since it was last read or written.
        /// </summary>
        private void SaveConfig()
        {
            if (_editor != null && _editor.target != null)
                EditorConfigUtility.Write(_editor.target, false);
        }

        #endregion

    }

}
