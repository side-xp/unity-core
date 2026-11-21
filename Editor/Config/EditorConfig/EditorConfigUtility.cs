using System;
using System.IO;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using SideXP.Core.Reflection;

using Object = UnityEngine.Object;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Utility class for loading and saving editor configs.
    /// </summary>
    /// <remarks>
    /// Editor configs should inherit from <see cref="EditorConfig{T}"/>, which uses this utility to load and save their data.
    /// </remarks>
    public static class EditorConfigUtility
    {

        #region Subclasses

        /// <summary>
        /// Groups a loaded editor config with the data last read from or written to its storage.
        /// </summary>
        private class LoadedConfig
        {

            /// <summary>
            /// The loaded config instance.
            /// </summary>
            public object Config = null;

            /// <summary>
            /// The serialized data of the config as it was last read from or written to its storage, or null if nothing has been
            /// stored yet.
            /// </summary>
            public string StoredJson = null;

        }

        #endregion


        #region Fields

        private const string EditorPrefsKeyPrefix = "EditorConfigs_";
        private const string FileExtension = ".json";

        private const string ObsoleteMessage = "Inherit from EditorConfig<T> instead, and use its Instance property and Save() function.";

        /// <summary>
        /// The list of the loaded editor configs.
        /// </summary>
        private static List<LoadedConfig> s_loadedConfigs = new List<LoadedConfig>();

        #endregion


        #region Public API

        /// <summary>
        /// Gets the loaded config for a given type, or loads it if it's not already loaded.
        /// </summary>
        /// <typeparam name="TConfig">The type of the editor config to get or load.</typeparam>
        /// <returns>Returns the loaded editor config.</returns>
        [Obsolete(ObsoleteMessage)]
        public static TConfig Get<TConfig>()
            where TConfig : IEditorConfig
        {
            return Get(out TConfig instance) ? instance : default;
        }

        /// <param name="instance">Outputs the loaded editor config.</param>
        /// <returns>Returns true if the expected object has been loaded successfully.</returns>
        /// <inheritdoc cref="Get{T}()"/>
        [Obsolete(ObsoleteMessage)]
        public static bool Get<TConfig>(out TConfig instance)
            where TConfig : IEditorConfig
        {
            return GetOrLoad(out instance, config => config.PostLoad());
        }

        /// <summary>
        /// Saves the given editor config to its storage.
        /// </summary>
        /// <param name="config">The config to save.</param>
        /// <returns>Returns true if the object has been saved successfully.</returns>
        [Obsolete(ObsoleteMessage)]
        public static bool Save(IEditorConfig config)
        {
            return Write(config, true);
        }

        /// <summary>
        /// Saves all the loaded editor configs that have changed since they were last read or written.
        /// </summary>
        /// <returns>Returns true if all the editor configs have been saved successfully.</returns>
        public static bool SaveAll()
        {
            CleanDisposedConfigs();
            bool success = true;
            foreach (LoadedConfig loadedConfig in s_loadedConfigs.ToArray())
            {
                if (!Write(loadedConfig.Config, false))
                    success = false;
            }
            return success;
        }

        #endregion


        #region Internal API

        /// <summary>
        /// Gets the loaded config for a given type, or loads it if it's not already loaded.
        /// </summary>
        /// <typeparam name="TConfig">The type of the editor config to get or load.</typeparam>
        /// <param name="instance">Outputs the loaded editor config.</param>
        /// <param name="onLoaded">Called once when the config has just been loaded, after it has been registered.</param>
        /// <returns>Returns true if the expected object has been found or loaded successfully.</returns>
        internal static bool GetOrLoad<TConfig>(out TConfig instance, Action<TConfig> onLoaded)
        {
            CleanDisposedConfigs();
            foreach (LoadedConfig loadedConfig in s_loadedConfigs)
            {
                if (loadedConfig.Config is TConfig typedConfig)
                {
                    instance = typedConfig;
                    return true;
                }
            }

            if (!TryLoad(out instance, out string json))
                return false;

            // Register the config before notifying it, so the callback can use the config's instance getter without loading it again
            s_loadedConfigs.Add(new LoadedConfig { Config = instance, StoredJson = json });
            onLoaded?.Invoke(instance);
            return true;
        }

        /// <summary>
        /// Writes the given editor config to its storage.
        /// </summary>
        /// <param name="config">The config to save.</param>
        /// <param name="force">
        /// If enabled, the config is written even if its data didn't change since it was last read or written.
        /// </param>
        /// <returns>Returns true if the object has been saved successfully, or if there was nothing to save.</returns>
        internal static bool Write(object config, bool force)
        {
            if (config == null)
                return false;

            Type configType = config.GetType();
            if (!TryGetStorage(configType, "save", out EEditorConfigScope scope, out string path))
                return false;

            string json = EditorJsonUtility.ToJson(config, true);
            // Consider the operation valid if there's no data to save
            if (string.IsNullOrWhiteSpace(json))
                return true;

            LoadedConfig loadedConfig = s_loadedConfigs.Find(i => ReferenceEquals(i.Config, config));
            // Skip if the data didn't change, so the storage isn't rewritten each time Unity saves the project
            if (!force && loadedConfig != null && loadedConfig.StoredJson == json)
                return true;

            if (scope == EEditorConfigScope.Editor)
            {
                EditorPrefs.SetString(path, json);
            }
            else
            {
                try
                {
                    // The target directory may not exist (as the /UserSettings directory in older Unity versions)
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    File.WriteAllText(path, json);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    Debug.LogError($"Failed to save editor config for an object of type {configType.FullName}. See previous logs for more info.", config as Object);
                    return false;
                }
            }

            if (loadedConfig != null)
                loadedConfig.StoredJson = json;

            return true;
        }

        /// <summary>
        /// Removes the loaded configs of a given type from the loaded list, and destroys them, without saving them.
        /// </summary>
        /// <remarks>
        /// The next access to a config of that type will load it again from its storage.
        /// </remarks>
        /// <param name="configType">The type of the configs to unload. Configs of derived types are also unloaded.</param>
        internal static void Unload(Type configType)
        {
            for (int i = s_loadedConfigs.Count - 1; i >= 0; i--)
            {
                object config = s_loadedConfigs[i].Config;
                if (!configType.IsInstanceOfType(config))
                    continue;

                s_loadedConfigs.RemoveAt(i);
                Destroy(config);
            }
        }

        /// <summary>
        /// Removes all the loaded configs from the loaded list, and destroys them, without saving them.
        /// </summary>
        /// <remarks>
        /// Configs are created with <see cref="HideFlags.DontSave"/> so Unity doesn't unload them on scene changes. But that also makes
        /// them survive script reloads, so they must be destroyed manually before that happens, or they would leak.
        /// </remarks>
        internal static void UnloadAll()
        {
            foreach (LoadedConfig loadedConfig in s_loadedConfigs)
                Destroy(loadedConfig.Config);

            s_loadedConfigs.Clear();
        }

        #endregion


        #region Private API

        /// <summary>
        /// Gets the storage location of an editor config type, as defined by its <see cref="EditorConfigAttribute"/>.
        /// </summary>
        /// <param name="configType">The type of the editor config of which to get the storage location.</param>
        /// <param name="operation">The name of the operation being done, used in error messages.</param>
        /// <param name="scope">Outputs the scope in which the config is stored.</param>
        /// <param name="path">
        /// Outputs the absolute path to the file that stores the config, or the <see cref="EditorPrefs"/> key if the scope is
        /// <see cref="EEditorConfigScope.Editor"/>.
        /// </param>
        /// <returns>Returns true if the storage location has been computed successfully.</returns>
        private static bool TryGetStorage(Type configType, string operation, out EEditorConfigScope scope, out string path)
        {
            scope = default;
            path = null;

            if (!configType.TryGetAttribute(out EditorConfigAttribute editorConfigAttribute))
            {
                Debug.LogError($"Failed to {operation} editor config for an object of type {configType.FullName}: That type doesn't use {nameof(EditorConfigAttribute)}, so the system can't {operation} it using the appropriate scope.");
                return false;
            }

            scope = editorConfigAttribute.Scope;
            string key = !string.IsNullOrWhiteSpace(editorConfigAttribute.Key)
                ? editorConfigAttribute.Key
                : configType.FullName;

            switch (scope)
            {
                case EEditorConfigScope.Project:
                    path = $"{PathEditorUtility.ProjectSettingsPath}/{key}{FileExtension}";
                    return true;

                case EEditorConfigScope.User:
                    path = $"{PathEditorUtility.UserSettingsPath}/{key}{FileExtension}";
                    return true;

                case EEditorConfigScope.Editor:
                    path = $"{EditorPrefsKeyPrefix}{key}";
                    return true;

                default:
                    Debug.LogError($"Failed to {operation} editor config for an object of type {configType.FullName}: Unsupported scope \"{scope}\".");
                    return false;
            }
        }

        /// <summary>
        /// Tries to load the stored config for a given type.
        /// </summary>
        /// <typeparam name="TConfig">The type of the editor config to load.</typeparam>
        /// <param name="instance">Outputs a created object of the given type with the loaded data.</param>
        /// <param name="json">Outputs the data read from the config's storage, or null if nothing has been stored yet.</param>
        /// <returns>Returns true if the object has been loaded successfully.</returns>
        private static bool TryLoad<TConfig>(out TConfig instance, out string json)
        {
            instance = default;
            json = null;

            if (!TryGetStorage(typeof(TConfig), "load", out EEditorConfigScope scope, out string path))
                return false;

            if (scope == EEditorConfigScope.Editor)
            {
                json = EditorPrefs.GetString(path, null);
            }
            else
            {
                try
                {
                    // A missing file is not considered as an error
                    if (File.Exists(path))
                        json = File.ReadAllText(path);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    Debug.LogError($"Failed to load editor config for an object of type {typeof(TConfig).FullName}. See previous logs for more info.");
                    return false;
                }
            }

            if (string.IsNullOrWhiteSpace(json))
                json = null;

            instance = CreateInstance<TConfig>();
            // Overwrite the default values of the created instance with the stored data
            if (json != null)
                EditorJsonUtility.FromJsonOverwrite(json, instance);

            return true;
        }

        /// <summary>
        /// Creates a new instance of an editor config type, with its default values.
        /// </summary>
        /// <typeparam name="TConfig">The type of the editor config to create.</typeparam>
        /// <returns>Returns the created instance.</returns>
        private static TConfig CreateInstance<TConfig>()
        {
            if (!typeof(TConfig).Is<ScriptableObject>())
                return Activator.CreateInstance<TConfig>();

            ScriptableObject scriptableObject = ScriptableObject.CreateInstance(typeof(TConfig));
            scriptableObject.name = typeof(TConfig).Name;
            // Prevent Unity from unloading the instance (which happens on scene changes if nothing else references it)
            scriptableObject.hideFlags = HideFlags.DontSave;
            return (TConfig)(object)scriptableObject;
        }

        /// <summary>
        /// Destroys a config instance if it's a Unity object that hasn't been destroyed yet.
        /// </summary>
        /// <param name="config">The config to destroy.</param>
        private static void Destroy(object config)
        {
            if (config is Object obj && obj != null)
                Object.DestroyImmediate(obj);
        }

        /// <summary>
        /// Removes the loaded configs that have been destroyed from the loaded list.
        /// </summary>
        /// <remarks>
        /// Loaded configs can't be unloaded by Unity since they use <see cref="HideFlags.DontSave"/>, but they can still be destroyed
        /// explicitly by any script.
        /// </remarks>
        private static void CleanDisposedConfigs()
        {
            for (int i = s_loadedConfigs.Count - 1; i >= 0; i--)
            {
                object config = s_loadedConfigs[i].Config;
                if (config == null || (config is Object obj && obj == null))
                    s_loadedConfigs.RemoveAt(i);
            }
        }

        #endregion

    }

}
