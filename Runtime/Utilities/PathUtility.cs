using System.IO;
using System.Collections.Generic;

using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Miscellaneous functions for working with path strings.
    /// </summary>
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    public static class PathUtility
    {

        #region Fields

        /// <summary>
        /// The character used by default as path directory separator.
        /// </summary>
        public const char DefaultDirectorySeparator = '/';

        /// <summary>
        /// Name of the /Assets directory.
        /// </summary>
        public const string AssetsDirectory = "Assets";

        /// <summary>
        /// Name of the /Resources directory.
        /// </summary>
        public const string ResourcesDirectory = "Resources";

        /// <summary>
        /// The base relative path to the /Resources directory at the root of the /Assets directory.
        /// </summary>
        public const string BaseResourcesPath = AssetsDirectory + "/" + ResourcesDirectory;

        /// <summary>
        /// The absolute path to this project.<br/>
        /// In the editor, this is the path to the project's folder (the root folder, not /Assets). In build, it's the path to the persistent data path directory (same as <see cref="PersistentDataPath"/>).<br/>
        /// This value is meant to be used by custom editors or in-game features that require to store persistent data.
        /// </summary>
        public static readonly string ProjectPath = null;

        /// <summary>
        /// The absolute path to the game content directory.<br/>
        /// In the editor, this is the path to the project's /Assets folder. In build, it's the path to the folder that contains the game executable. Note that in build, that directory is readonly, so you must use <see cref="PersistentDataPath"/> to store persistent data.
        /// </summary>
        /// <seealso href="https://docs.unity3d.com/ScriptReference/Application-dataPath.html"/>
        public static readonly string DataPath = null;

        /// <summary>
        /// The absolute path to the persistent data directory.<br/>
        /// This path varies from plaforms, but is guaranteed to be writable, so you can use it to store persistent data.
        /// </summary>
        /// <seealso href="https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html"/>
        public static readonly string PersistentDataPath = null;

        /// <summary>
        /// The absolute path to the temporary data directory.
        /// </summary>
        /// <seealso href="https://docs.unity3d.com/ScriptReference/Application-temporaryCachePath.html"/>
        public static readonly string TemporaryDataPath = null;

        #endregion


        #region Lifecycle

        /// <summary>
        /// Static constructor.
        /// </summary>
        static PathUtility()
        {
            DataPath = ToPath(Application.dataPath);
            PersistentDataPath = ToPath(Application.persistentDataPath);
            TemporaryDataPath = ToPath(Application.temporaryCachePath);

#if UNITY_EDITOR
            ProjectPath = ToPath(DataPath.Substring(0, DataPath.Length - $"/{AssetsDirectory}".Length));
#else
            ProjectPath = PersistentDataPath;
#endif
        }

        #endregion


        #region Public API

        /// <summary>
        /// Converts the given string into a path, by removing forbidden characters and use the same character as directory separator.
        /// </summary>
        /// <param name="str">The string to convert.</param>
        /// <param name="normalize">If enabled, the package names will be replaced by their actual folder name.</param>
        /// <param name="separator">The character to use as directory separator.</param>
        /// <returns>Returns the converted string.</returns>
        public static string ToPath(string str, bool normalize = false, char separator = DefaultDirectorySeparator)
        {
            ToPath(ref str, normalize, separator);
            return str;
        }

        /// <inheritdoc cref="ToPath(string, bool, char)"/>
        public static void ToPath(ref string str, bool normalize = false, char separator = DefaultDirectorySeparator)
        {
            // Cancel if the input string is not valid
            if (string.IsNullOrEmpty(str))
                return;

            str = str.Trim();

            // Remove invalid characters in path
            foreach (char invalidChar in Path.GetInvalidPathChars())
                str = str.Replace(invalidChar.ToString(), "");

            // Remove invalid characters in file name
            string fileName = Path.GetFileName(str);
            if (!string.IsNullOrEmpty(fileName))
            {
                foreach (char invalidChar in Path.GetInvalidFileNameChars())
                    fileName = fileName.Replace(invalidChar.ToString(), "");

                string dirName = Path.GetDirectoryName(str);
                str = dirName;
                if (!string.IsNullOrEmpty(fileName))
                {
                    if (!string.IsNullOrEmpty(str))
                        str += separator;

                    str += fileName;
                }
            }

            if (normalize)
                str = Path.GetFullPath(str);

            // Use only 1 character as separator
            str = str.Replace(Path.DirectorySeparatorChar, separator);
            str = str.Replace(Path.AltDirectorySeparatorChar, separator);
        }

        /// <summary>
        /// Converts the given strings into paths, by removing forbidden characters and use the same character as directory separator.
        /// </summary>
        /// <inheritdoc cref="ToPath(string, char, bool)"/>
        /// <param name="strs">The collection of strings to convert.</param>
        public static string[] ToPaths(IEnumerable<string> strs, bool normalize = false, char separator = DefaultDirectorySeparator)
        {
            List<string> paths = new List<string>();
            foreach (string str in strs)
                paths.Add(ToPath(str, normalize, separator));

            return paths.ToArray();
        }

        /// <summary>
        /// If the given string represents a relative paths, it's combined with <see cref="ProjectPath"/> to make it abolute.
        /// </summary>
        /// <param name="path">The path string to convert. If that path is already absolute, this function returns it as is. If it's null or empty, this function returns the value of <see cref="ProjectPath"/>.</param>
        /// <returns>Returns the absolute path string.</returns>
        public static string ToAbsolutePath(string path)
        {
            ToAbsolutePath(ref path);
            return path;
        }

        /// <inheritdoc cref="ToAbsolutePath(string)"/>
        public static void ToAbsolutePath(ref string path)
        {
            // Cancel if the given path is not valid
            if (string.IsNullOrEmpty(path))
            {
                path = ProjectPath;
                return;
            }

            // Use ToPath() to remove forbidden characters
            ToPath(ref path, false);
            // Resolve path (and so resolve the com.* package paths to real file path)
            path = Path.GetFullPath(path);
            // Then call ToPath() again to ensure separator character consistency
            ToPath(ref path, false);
        }

        /// <summary>
        /// If the given string represents an absolute path leading to this project, this function makes it relative to <see cref="ProjectPath"/>.
        /// </summary>
        /// <param name="path">The path string to convert. If that path is already relative, this function returns it as is. If it's null or empty, this function returns an empty string.</param>
        /// <returns>Returns the relative path string.</returns>
        /// <inheritdoc cref="ToPath(ref string, bool, char)"/>
        public static string ToRelativePath(string path, bool normalize = false)
        {
            ToRelativePath(ref path, normalize);
            return path;
        }

        /// <inheritdoc cref="ToRelativePath(string, bool)"/>
        public static void ToRelativePath(ref string path, bool normalize = false)
        {
            // Cancel if the given path is not valid
            if (string.IsNullOrEmpty(path))
            {
                path = string.Empty;
                return;
            }

            ToPath(ref path, normalize);
            // Cancel if the path is absolute but not related to the project
            if (!path.StartsWith(ProjectPath))
                return;

            // Resolve path, then re-use ToPath() to ensure separator character consistency
            path = Path.GetFullPath(path);
            ToPath(ref path);

            int index = ProjectPath.Length;
            // If the input path is longer than the project path, discard the first separator character
            if (path.Length > index)
                index++;

            // Remove the absolute part from the path string
            path = path.Substring(index);
        }

        /// <summary>
        /// Checks if the given path leads to this project.
        /// </summary>
        /// <param name="path">The path string to check. If that path is relative this function returns true.</param>
        /// <returns>Returns true if the given path leads to this project.</returns>
        public static bool IsProjectPath(string path)
        {
            return IsProjectPath(ref path);
        }

        /// <inheritdoc cref="IsProjectPath(string)"/>
        public static bool IsProjectPath(ref string path)
        {
            // Cancel if the path is not valid or relative
            if (string.IsNullOrEmpty(path))
                return true;

            ToPath(ref path, true);
            if (!Path.IsPathRooted(path))
                return true;

            return path.StartsWith(ProjectPath);
        }

        #endregion

    }

}
