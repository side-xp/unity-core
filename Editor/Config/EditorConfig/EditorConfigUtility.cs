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
    /// Utility class for working with editor configs.
    /// </summary>
    public static class EditorConfigUtility
    {

        #region Fields

        private const string EditorPrefsKeyPrefix = "EditorConfigs_";

        /// <summary>
        /// The list of the loaded editor configs.
        /// </summary>
        private static List<IEditorConfig> s_loadedConfigs = new List<IEditorConfig>();

        #endregion


        #region Public API

        /// <summary>
        /// Get the loaded configs for a given type, or load it if it's not already loaded.
        /// </summary>
        /// <typeparam name="TConfig">The type of the editor config to get or load.</typeparam>
        /// <returns>Returns the loaded editor config.</returns>
        public static TConfig Get<TConfig>()
            where TConfig : IEditorConfig
        {
            return Get(out TConfig instance) ? instance : default;
        }

        /// <param name="instance">Outputs the loaded editor config.</param>
        /// <returns>Returns true if the expected object has been loaded successfully.</returns>
        /// <inheritdoc cref="Get{T}()"/>
        public static bool Get<TConfig>(out TConfig instance)
            where TConfig : IEditorConfig
        {
            CleanDisposedConfigs();
            foreach (IEditorConfig config in s_loadedConfigs.ToArray())
            {
                if (config is TConfig typedConfig)
                {
                    instance = typedConfig;
                    return true;
                }
            }

            if (!TryLoad(out instance))
                return false;

            s_loadedConfigs.Add(instance);
            return true;
        }

        /// <summary>
        /// Saves the given editor config on disk.
        /// </summary>
        /// <param name="config">The config to save.</param>
        /// <returns>Returns true if the object has been saved successfully.</returns>
        public static bool Save(IEditorConfig config)
        {
            // Cancel if the given object type doesn't use the [EditorConfig] attribute
            if (!config.GetType().TryGetAttribute(out EditorConfigAttribute editorConfigAttribute))
            {
                Debug.LogError($"Failed to save editor settings for an object of type {config.GetType().FullName}: That type doesn't use {nameof(EditorConfigAttribute)}, so the system can't save using the appropriate scope.");
                return false;
            }

            string json = EditorJsonUtility.ToJson(config, true);
            // Consider the operation valid if there's no data to save
            if (string.IsNullOrWhiteSpace(json))
                return true;

            // Compute the path to the stored data file (or the prefs key)
            string path = GetConfigFilePath(config.GetType(), editorConfigAttribute.Scope);

            // If the config is meant to be stored in EditorPrefs
            if (editorConfigAttribute.Scope == EEditorConfigScope.Editor)
            {
                EditorPrefs.SetString(path, json);
            }
            // Else, write the config into a file
            else
            {
                // Cancel if the path is not valid
                if (string.IsNullOrWhiteSpace(path))
                {
                    Debug.LogError($"Failed to save editor config for an object of type {config.GetType().FullName}: Unsupported scope \"{editorConfigAttribute.Scope}\".");
                    return false;
                }

                try
                {
                    File.WriteAllText(path, json);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    Debug.LogError($"Failed to save editor config for an object of type {config.GetType().FullName}. See previous logs for more info.", config is Object obj ? obj : null);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Saves all the loaded editor configs.
        /// </summary>
        /// <returns>Returns true if all the editor configs object have been saved successfully.</returns>
        public static bool SaveAll()
        {
            bool success = true;
            foreach (IEditorConfig config in s_loadedConfigs.ToArray())
            {
                if (!Save(config))
                    success = false;
            }
            return success;
        }

        #endregion


        #region Private API

        /// <summary>
        /// Gets the path to the stored editor config file on disk, depending on the scope.
        /// </summary>
        /// <remarks>If the target scope is <see cref="EEditorConfigScope.Editor"/>, this function returns the <see cref="EditorPrefs"/>
        /// key instead of a file path.</remarks>
        /// <param name="configType">The type of the editor config object of which to get the path.</param>
        /// <param name="scope">The scope for which the editor config is stored.</param>
        /// <returns>Returns the computed path, or an empty string if the scope is not valid.</returns>
        private static string GetConfigFilePath(Type configType, EEditorConfigScope scope)
        {
            string path = scope switch
            {
                EEditorConfigScope.Project => $"{PathEditorUtility.ProjectSettingsPath}",
                EEditorConfigScope.User => $"{PathEditorUtility.UserSettingsPath}",
                EEditorConfigScope.Editor => $"{EditorPrefsKeyPrefix}{configType}",
                _ => string.Empty
            };
            return path + $"/{configType.FullName}.json";
        }

        /// <typeparam name="TConfig"><inheritdoc cref="GetConfigFilePath(Type, EEditorConfigScope)" path="/param[@name='configType']"/></typeparam>
        /// <inheritdoc cref="GetConfigFilePath(Type, EEditorConfigScope)"/>
        private static string GetConfigFilePath<TConfig>(EEditorConfigScope scope)
            where TConfig : IEditorConfig
        {
            return GetConfigFilePath(typeof(TConfig), scope);
        }

        /// <summary>
        /// Tries to load the serialized config for a given type.
        /// </summary>
        /// <typeparam name="TConfig">The type of the editor config to load.</typeparam>
        /// <param name="instance">Outputs a created object of the given type with the loaded data.</param>
        /// <returns>Returns true if the object has been loaded successfully.</returns>
        private static bool TryLoad<TConfig>(out TConfig instance)
            where TConfig : IEditorConfig
        {
            // Cancel if the given object type doesn't use the [EditorSettings] attribute
            if (!typeof(TConfig).TryGetAttribute(out EditorConfigAttribute editorConfigAttribute))
            {
                Debug.LogError($"Failed to load editor settings for an object of type {nameof(TConfig)}: That type doesn't use {nameof(EditorConfigAttribute)}, so the system can't load from the appropriate scope.");
                instance = default;
                return false;
            }

            // Compute the path to the stored data file
            string path = GetConfigFilePath<TConfig>(editorConfigAttribute.Scope);

            // Cancel if the path is not valid
            if (string.IsNullOrWhiteSpace(path))
            {
                Debug.LogError($"Failed to load editor config for an object of type {nameof(TConfig)}: Unsupported scope \"{editorConfigAttribute.Scope}\".");
                instance = default;
                return false;
            }

            // Create an instance of the config object
            instance = typeof(TConfig).Is<ScriptableObject>()
                ? (TConfig)(object)ScriptableObject.CreateInstance(typeof(TConfig))
                : Activator.CreateInstance<TConfig>();

            string json = null;
            if (editorConfigAttribute.Scope == EEditorConfigScope.Editor)
            {
                json = EditorPrefs.GetString(path, null);
            }
            else
            {
                // Read data file if it exists
                try
                {
                    json = File.ReadAllText(path);
                }
                // A missing file is not considered as an error
                catch (FileNotFoundException) { }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    Debug.LogError($"Failed to load loading editor config for an object of type {nameof(TConfig)}. See previous logs for more info.");
                    return false;
                }
            }

            // Overwrite the created object instance
            if (!string.IsNullOrWhiteSpace(json))
                EditorJsonUtility.FromJsonOverwrite(json, instance);

            return true;
        }

        /// <summary>
        /// Removes the loaded config assets that have been disabled or disposed.
        /// </summary>
        /// <remarks>Loaded configs may be disabled or disposed automatically by Unity when it restores the "last opened scene" when it
        /// launches, or when the current scene change in the editor.</remarks>
        private static void CleanDisposedConfigs()
        {
            for (int i = s_loadedConfigs.Count - 1; i >= 0; i--)
            {
                if (s_loadedConfigs[i] == null || (s_loadedConfigs[i] is Object obj && obj == null))
                    s_loadedConfigs.RemoveAt(i);
            }
        }

        #endregion

    }

}
