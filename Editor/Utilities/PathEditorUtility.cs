using System;
using System.IO;

using UnityEngine;
using UnityEditor;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Miscellaneous functions for working with path strings.
    /// </summary>
    [InitializeOnLoad]
    public static class PathEditorUtility
    {

        #region Fields

        /// <summary>
        /// Name of the /ProjectSettings directory.
        /// </summary>
        public const string ProjectSettingsDirectory = "ProjectSettings";

        /// <summary>
        /// Name of the /UserSettings directory.
        /// </summary>
        public const string UserSettingsDirectory = "UserSettings";

        /// <summary>
        /// The absolute path to the project's /ProjectSettings directory.
        /// </summary>
        public static readonly string ProjectSettingsPath = null;

        /// <summary>
        /// The absolute path to the project's /UserSettings directory.
        /// </summary>
        public static readonly string UserSettingsPath = null;

        /// <summary>
        /// The absolute path to the user's cache directory, using <see cref="PathUtility.DefaultDirectorySeparator"/> and without
        /// trailing separator.
        /// </summary>
        /// <remarks>
        /// This directory is per user and doesn't depend on Unity, so every project, session and Unity version on this machine can
        /// share the files stored in it. Store files in a subfolder named after your company or tool, as
        /// <c>{UserCachePath}/MyCompany/MyTool</c>. Its location depends on the platform:
        /// <list type="bullet">
        /// <item>Windows: <c>%LOCALAPPDATA%</c></item>
        /// <item>macOS: <c>~/Library/Caches</c></item>
        /// <item>Linux: <c>$XDG_CACHE_HOME</c>, or <c>~/.cache</c> if it's not defined</item>
        /// </list>
        /// If the platform folder can't be resolved, the system temporary folder is used instead.
        /// </remarks>
        public static readonly string UserCachePath = null;

        #endregion


        #region Lifecycle

        /// <summary>
        /// Static constructor.
        /// </summary>
        static PathEditorUtility()
        {
            ProjectSettingsPath = $"{PathUtility.ProjectPath}/{ProjectSettingsDirectory}";
            UserSettingsPath = $"{PathUtility.ProjectPath}/{UserSettingsDirectory}";
            UserCachePath = ResolveUserCachePath(Application.platform, Environment.GetEnvironmentVariable);
        }

        #endregion


        #region Internal API

        /// <summary>
        /// Resolves the path to the user's cache directory for a given platform.
        /// </summary>
        /// <remarks>
        /// The directory is resolved explicitly per platform, since <see cref="Environment.SpecialFolder.LocalApplicationData"/>
        /// hasn't always mapped to the same folder on macOS and Linux across the Mono and .NET versions used by Unity.
        /// </remarks>
        /// <param name="platform">The platform of the running editor.</param>
        /// <param name="getEnvironmentVariable">Gets the value of an environment variable, or null if it's not defined.</param>
        /// <returns>
        /// Returns the absolute path to the cache directory, using <see cref="PathUtility.DefaultDirectorySeparator"/> and without
        /// trailing separator.
        /// </returns>
        internal static string ResolveUserCachePath(RuntimePlatform platform, Func<string, string> getEnvironmentVariable)
        {
            bool isWindows = platform == RuntimePlatform.WindowsEditor || platform == RuntimePlatform.WindowsPlayer;

            string cacheFolder;
            switch (platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    cacheFolder = GetRootedVariable(getEnvironmentVariable, "LOCALAPPDATA")
                        ?? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    break;

                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    cacheFolder = GetHomeSubfolder(getEnvironmentVariable, "Library/Caches");
                    break;

                // Linux, and any other Unix-like platform, following the XDG Base Directory specification
                default:
                    cacheFolder = GetRootedVariable(getEnvironmentVariable, "XDG_CACHE_HOME")
                        ?? GetHomeSubfolder(getEnvironmentVariable, ".cache");
                    break;
            }

            // Never resolve to a relative path, which would end up in the project folder
            if (string.IsNullOrEmpty(cacheFolder) || !Path.IsPathRooted(cacheFolder))
                cacheFolder = Path.GetTempPath();

            // Only on Windows, since a backslash is a valid character in file names on other platforms
            if (isWindows)
                cacheFolder = cacheFolder.Replace('\\', PathUtility.DefaultDirectorySeparator);

            return cacheFolder.TrimEnd(PathUtility.DefaultDirectorySeparator);
        }

        #endregion


        #region Private API

        /// <summary>
        /// Gets the value of an environment variable, only if it's an absolute path.
        /// </summary>
        /// <param name="getEnvironmentVariable">Gets the value of an environment variable, or null if it's not defined.</param>
        /// <param name="name">The name of the environment variable.</param>
        /// <returns>Returns the value of the variable, or null if it's not defined or not an absolute path.</returns>
        private static string GetRootedVariable(Func<string, string> getEnvironmentVariable, string name)
        {
            string value = getEnvironmentVariable(name);
            return !string.IsNullOrEmpty(value) && Path.IsPathRooted(value) ? value : null;
        }

        /// <summary>
        /// Gets the path to a subfolder of the user's home folder on Unix-like platforms.
        /// </summary>
        /// <param name="getEnvironmentVariable">Gets the value of an environment variable, or null if it's not defined.</param>
        /// <param name="subfolder">The path to the subfolder, relative to the home folder.</param>
        /// <returns>Returns the absolute path to the subfolder, or null if the home folder can't be resolved.</returns>
        private static string GetHomeSubfolder(Func<string, string> getEnvironmentVariable, string subfolder)
        {
            string home = GetRootedVariable(getEnvironmentVariable, "HOME")
                ?? Environment.GetFolderPath(Environment.SpecialFolder.Personal);

            // Without home folder, don't resolve to a subfolder of the system root
            return !string.IsNullOrEmpty(home)
                ? $"{home.TrimEnd(PathUtility.DefaultDirectorySeparator)}/{subfolder}"
                : null;
        }

        #endregion

    }

}
