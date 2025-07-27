using UnityEngine;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Editor config for the Core package features.
    /// </summary>
    [EditorConfig(EEditorConfigScope.Project)]
    public class CoreEditorConfig : ScriptableObject, IEditorConfig
    {

        #region Fields

        [Header("Demos")]

        [SerializeField]
        [Boolton(nameof(EnableDemos), Toggle = true)]
        [Tooltip("If enabled, adds a define to your project, so you can see \"Demos\" menus from the main toolbar and in \"Add Component\" popup.")]
        private bool _enableDemos = false;

        [Header("Scripts and Types")]

        [SerializeField, FolderPath]
        [Tooltip("When querying project types using " + nameof(ScriptUtility) + ", exclude the types from scripts in the given directories. Relative paths are resolved from the project's root directory.")]
        private string[] _excludedDirectories = { "Packages" };

        [SerializeField]
        [Tooltip("When querying project types using " + nameof(ScriptUtility) + ", exclude the types from the named assemblies.")]
        private string[] _excludedAssembliesNames = { };

        [SerializeField]
        [Boolton(nameof(TrackTypesMigrations), Toggle = true)]
        [Tooltip("If enabled, track the renamed types and store these informations on disk." +
            "\nThis option is required to make [TypeRef] feature work as expected as your project grows.")]
        private bool _trackTypesMigrations = false;

        #endregion


        #region Lifecycle

        /// <inheritdoc cref="IEditorConfig.PostLoad"/>
        public void PostLoad()
        {
            DemosEnabledForCurrentPlatform = _enableDemos;
        }

        #endregion


        #region Public API

        /// <summary>
        /// Gets the loaded settings or load them from disk if not already.
        /// </summary>
        public static CoreEditorConfig Instance => EditorConfigUtility.Get<CoreEditorConfig>();

        /// <inheritdoc cref="Instance"/>
        public static CoreEditorConfig I => Instance;

        /// <inheritdoc cref="_enableDemos"/>
        public bool EnableDemos
        {
            get => _enableDemos;
            set
            {
                DemosEnabledForCurrentPlatform = value;
                _enableDemos = value;
            }
        }

        /// <inheritdoc cref="_trackTypesMigrations"/>
        public bool TrackTypesMigrations
        {
            get => _trackTypesMigrations;
            set
            {
                _trackTypesMigrations = value;
                if (_trackTypesMigrations)
                    TypesMigration.Reload();
            }
        }

        /// <inheritdoc cref="_excludedDirectories"/>
        public string[] ExcludedDirectories => _excludedDirectories;

        /// <inheritdoc cref="_excludedAssembliesNames"/>
        public string[] ExcludedAssembliesNames => _excludedAssembliesNames;

        #endregion


        #region Private API

        /// <summary>
        /// Checks if the demos define exists in Player Settings.
        /// </summary>
        private bool DemosEnabledForCurrentPlatform
        {
            get => DefinesUtility.IsDefined(Constants.DemosDefine);
            set
            {
                if (value)
                    DefinesUtility.AddDefine(Constants.DemosDefine);
                else
                    DefinesUtility.RemoveDefine(Constants.DemosDefine);
            }
        }

        #endregion

    }

}
