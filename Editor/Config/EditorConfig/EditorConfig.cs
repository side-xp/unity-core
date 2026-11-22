using UnityEngine;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Base class for editor configs: a single instance of settings or data, stored outside of the project's assets.
    /// </summary>
    /// <remarks>
    /// This class is meant to work like <c>UnityEditor.ScriptableSingleton&lt;T&gt;</c>, which is not public in Unity 2019.4. The derived
    /// class must use <see cref="EditorConfigAttribute"/> to define where its data is stored. The instance is loaded on first access, and
    /// saved automatically when Unity saves the project, before scripts are reloaded and when the editor quits.
    /// </remarks>
    /// <typeparam name="T">The type of the derived class.</typeparam>
    /// <example>
    /// <code>
    /// [EditorConfig(EEditorConfigScope.Project)]
    /// public class MyToolSettings : EditorConfig&lt;MyToolSettings&gt;
    /// {
    ///     [SerializeField]
    ///     private bool _isEnabled = true;
    ///
    ///     public bool IsEnabled => _isEnabled;
    /// }
    ///
    /// // Then, from anywhere in editor code:
    /// bool isEnabled = MyToolSettings.I.IsEnabled;
    /// </code>
    /// </example>
    public abstract class EditorConfig<T> : ScriptableObject
        where T : EditorConfig<T>
    {

        #region Public API

        /// <summary>
        /// Gets the loaded instance of this config, or loads it from its storage if not already.
        /// </summary>
        /// <remarks>
        /// Returns null if the config can't be loaded, as when the derived class doesn't use <see cref="EditorConfigAttribute"/>.
        /// </remarks>
        public static T Instance => EditorConfigUtility.GetOrLoad(out T instance, i => i.OnLoaded()) ? instance : null;

        /// <inheritdoc cref="Instance"/>
        public static T I => Instance;

        /// <summary>
        /// Writes this config to its storage.
        /// </summary>
        /// <returns>Returns true if the config has been saved successfully.</returns>
        public bool Save()
        {
            return EditorConfigUtility.Write(this, true);
        }

        #endregion


        #region Protected API

        /// <summary>
        /// Called once after this config has been loaded from its storage.
        /// </summary>
        /// <remarks>
        /// This is also called when no data has been stored yet and the instance holds its default values. At this point, the instance
        /// is already registered, so it's safe to use <see cref="Instance"/> from this function.
        /// </remarks>
        protected virtual void OnLoaded() { }

        #endregion

    }

}
