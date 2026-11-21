using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using UnityEngine;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Resolves the paths in the shared folder where editor icons and their index are stored, and writes files into it safely.
    /// </summary>
    /// <remarks>
    /// The shared folder is per user and doesn't depend on Unity, so every project, session and Unity version on this machine reuses
    /// the same files. Several editors may write into it at the same time, hence <see cref="WriteFileAtomic(string, byte[], bool)"/>.
    /// </remarks>
    internal static class IconStorage
    {

        #region Fields

        /// <summary>
        /// Path to the default shared folder, relative to the user's cache folder.
        /// </summary>
        /// <remarks>
        /// Uses <see cref="PathUtility.DefaultDirectorySeparator"/> like every path in this class, which is supported by every OS.
        /// </remarks>
        internal const string DefaultRelativeFolder = "Sideways Experiments/Icons";

        /// <summary>
        /// Name of the index file, at the root of the shared folder.
        /// </summary>
        internal const string IndexFileName = "index.json";

        /// <summary>
        /// Name of the category folder for icons without a category. Shared with the icons of an actual "Other" category, if any.
        /// </summary>
        internal const string OtherFolderName = "other";

        /// <summary>
        /// Name of the outline style, used in icon file names and in the Tabler catalog.
        /// </summary>
        internal const string OutlineStyleName = "outline";

        /// <summary>
        /// Name of the filled style, used in icon file names and in the Tabler catalog.
        /// </summary>
        internal const string FilledStyleName = "filled";

        /// <summary>
        /// Extension of the stored icon files.
        /// </summary>
        internal const string IconFileExtension = ".png";

        /// <summary>
        /// Extension of the temporary files written before being moved into place.
        /// </summary>
        internal const string TempFileExtension = ".tmp";

        /// <summary>
        /// The sizes in which icon files are stored, in pixels, from the smallest.
        /// </summary>
        /// <remarks>
        /// Each icon size on screen at 100% and 200% display scale. The intermediate sizes also cover 150%, and avoid drawing a
        /// larger file smaller, which blurs thin strokes.
        /// </remarks>
        internal static readonly int[] StoredSizes = { 16, 24, 32, 48, 64 };

        /// <summary>
        /// Matches a valid icon name. Tabler icon names only use these characters, which also prevents a name from pointing outside
        /// of its category folder.
        /// </summary>
        private static readonly Regex ValidIconNameRegex = new Regex(@"^[a-z0-9-]+\z");

        /// <summary>
        /// Matches the characters to replace with a hyphen in a category folder name.
        /// </summary>
        private static readonly Regex CategoryFolderSeparatorsRegex = new Regex(@"[^a-z0-9]+");

        /// <summary>
        /// Encoding used for text files, without BOM.
        /// </summary>
        private static readonly Encoding TextEncoding = new UTF8Encoding(false);

        /// <summary>
        /// Cached path to the default shared folder, resolved on first use.
        /// </summary>
        private static string s_defaultRootFolder = null;

        #endregion


        #region Root folder

        /// <summary>
        /// Path to a folder to use instead of the configured shared folder, or null to use the configured one. Meant for tests.
        /// </summary>
        internal static string RootFolderOverride { get; set; } = null;

        /// <summary>
        /// Absolute path to the shared folder where icons and their index are stored.
        /// </summary>
        internal static string RootFolder => !string.IsNullOrEmpty(RootFolderOverride) ? RootFolderOverride : ConfiguredRootFolder;

        /// <summary>
        /// Absolute path to the shared folder set in the preferences, or <see cref="DefaultRootFolder"/> if none is set. Ignores
        /// <see cref="RootFolderOverride"/>.
        /// </summary>
        internal static string ConfiguredRootFolder
        {
            get
            {
                CorePreferences preferences = CorePreferences.I;
                return preferences != null && !string.IsNullOrEmpty(preferences.IconsFolder) ? preferences.IconsFolder : DefaultRootFolder;
            }
        }

        /// <summary>
        /// Absolute path to the default shared folder for the current platform, ignoring the preferences and
        /// <see cref="RootFolderOverride"/>.
        /// </summary>
        internal static string DefaultRootFolder
        {
            get
            {
                if (s_defaultRootFolder == null)
                    s_defaultRootFolder = ResolveRootFolder(Application.platform, Environment.GetEnvironmentVariable);

                return s_defaultRootFolder;
            }
        }

        /// <summary>
        /// Absolute path to the index file.
        /// </summary>
        internal static string IndexFilePath => $"{RootFolder}/{IndexFileName}";

        /// <summary>
        /// Resolves the path to the shared folder for a given platform.
        /// </summary>
        /// <remarks>
        /// The folder is resolved explicitly per platform, since <see cref="Environment.SpecialFolder.LocalApplicationData"/> hasn't
        /// always mapped to the same folder on macOS and Linux across the Mono and .NET versions used by Unity.
        /// </remarks>
        /// <param name="platform">The platform of the running editor.</param>
        /// <param name="getEnvironmentVariable">Gets the value of an environment variable, or null if it's not defined.</param>
        /// <returns>Returns the absolute path to the shared folder, using <see cref="PathUtility.DefaultDirectorySeparator"/>.</returns>
        internal static string ResolveRootFolder(RuntimePlatform platform, Func<string, string> getEnvironmentVariable)
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

            return $"{cacheFolder.TrimEnd(PathUtility.DefaultDirectorySeparator)}/{DefaultRelativeFolder}";
        }

        /// <summary>
        /// Converts a file or folder path to an absolute path, using <see cref="PathUtility.DefaultDirectorySeparator"/> and without
        /// trailing separator, so it can be compared with the paths built by this class.
        /// </summary>
        /// <param name="path">The path to normalize. Relative paths are resolved from the current directory.</param>
        /// <returns>Returns the normalized path.</returns>
        internal static string NormalizePath(string path)
        {
            string fullPath = Path.GetFullPath(path);

            // Only on Windows, since a backslash is a valid character in file names on other platforms
            if (Path.DirectorySeparatorChar == '\\')
                fullPath = fullPath.Replace('\\', PathUtility.DefaultDirectorySeparator);

            // Keep the separator of a root folder ("C:/" or "/")
            string trimmedPath = fullPath.TrimEnd(PathUtility.DefaultDirectorySeparator);
            return trimmedPath.Length > 0 && !trimmedPath.EndsWith(":") ? trimmedPath : fullPath;
        }

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


        #region Paths

        /// <summary>
        /// Checks if a name is a valid icon name.
        /// </summary>
        /// <param name="name">The name to check.</param>
        /// <returns>Returns true if the name only uses lowercase letters, digits and hyphens.</returns>
        internal static bool IsValidIconName(string name)
        {
            return !string.IsNullOrEmpty(name) && ValidIconNameRegex.IsMatch(name);
        }

        /// <summary>
        /// Gets the name of the folder for a category.
        /// </summary>
        /// <param name="category">The category name, as written in the index.</param>
        /// <returns>
        /// Returns the category name in lowercase, with any sequence of other characters than letters and digits replaced by a hyphen
        /// (<c>Version control</c> becomes <c>version-control</c>), or <see cref="OtherFolderName"/> if the category is empty.
        /// </returns>
        internal static string GetCategoryFolderName(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                return OtherFolderName;

            string folderName = CategoryFolderSeparatorsRegex.Replace(category.ToLowerInvariant(), "-").Trim('-');
            return folderName.Length > 0 ? folderName : OtherFolderName;
        }

        /// <summary>
        /// Gets the name of a style, as used in icon file names and in the Tabler catalog.
        /// </summary>
        /// <param name="style">The style of the icon.</param>
        /// <returns>Returns the name of the style.</returns>
        internal static string GetStyleName(EIconStyle style)
        {
            return style == EIconStyle.Filled ? FilledStyleName : OutlineStyleName;
        }

        /// <summary>
        /// Gets the absolute path to the folder that contains the files of an icon.
        /// </summary>
        /// <param name="name">The name of the icon.</param>
        /// <param name="category">The category of the icon, as written in the index.</param>
        /// <returns>Returns the absolute path to the icon folder.</returns>
        /// <exception cref="ArgumentException">Thrown if the icon name is not valid.</exception>
        internal static string GetIconFolderPath(string name, string category)
        {
            ThrowIfInvalidIconName(name);
            return $"{RootFolder}/{GetCategoryFolderName(category)}/{name}";
        }

        /// <summary>
        /// Gets the name of an icon file.
        /// </summary>
        /// <param name="name">The name of the icon.</param>
        /// <param name="pixelSize">The size of the icon file, in pixels.</param>
        /// <param name="style">The style of the icon.</param>
        /// <returns>Returns the file name, formatted as <c>{name}_{size}_{style}.png</c>.</returns>
        /// <exception cref="ArgumentException">Thrown if the icon name is not valid.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the pixel size is not strictly positive.</exception>
        internal static string GetIconFileName(string name, int pixelSize, EIconStyle style)
        {
            ThrowIfInvalidIconName(name);
            if (pixelSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(pixelSize), pixelSize, "The size of an icon file must be strictly positive.");

            return $"{name}_{pixelSize}_{GetStyleName(style)}{IconFileExtension}";
        }

        /// <summary>
        /// Gets the absolute path to an icon file.
        /// </summary>
        /// <param name="name">The name of the icon.</param>
        /// <param name="category">The category of the icon, as written in the index.</param>
        /// <param name="pixelSize">The size of the icon file, in pixels.</param>
        /// <param name="style">The style of the icon.</param>
        /// <returns>Returns the absolute path to the icon file.</returns>
        /// <exception cref="ArgumentException">Thrown if the icon name is not valid.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the pixel size is not strictly positive.</exception>
        internal static string GetIconFilePath(string name, string category, int pixelSize, EIconStyle style)
        {
            return $"{GetIconFolderPath(name, category)}/{GetIconFileName(name, pixelSize, style)}";
        }

        /// <summary>
        /// Throws an exception if a name is not a valid icon name.
        /// </summary>
        /// <param name="name">The name to check.</param>
        /// <exception cref="ArgumentException">Thrown if the icon name is not valid.</exception>
        private static void ThrowIfInvalidIconName(string name)
        {
            if (!IsValidIconName(name))
            {
                string message = $"Invalid icon name \"{name}\": only lowercase letters, digits and hyphens are allowed.";
                throw new ArgumentException(message, nameof(name));
            }
        }

        #endregion


        #region Files

        /// <summary>
        /// Writes a file safely, even if another editor writes the same file at the same time.
        /// </summary>
        /// <remarks>
        /// The content is written to a temporary file next to the target, then moved into place, which is atomic on the same volume.
        /// So the target file is either missing or complete, never partially written. Missing folders are created.
        /// </remarks>
        /// <param name="path">The absolute path to the file to write.</param>
        /// <param name="bytes">The content of the file.</param>
        /// <param name="overwrite">
        /// If enabled, replaces the target file if it already exists. Otherwise, keeps the existing file, assuming that another editor
        /// wrote the same content.
        /// </param>
        /// <returns>Returns true if the file has been written, or false if it already existed and has been kept.</returns>
        /// <exception cref="IOException">Thrown if the file can't be written.</exception>
        internal static bool WriteFileAtomic(string path, byte[] bytes, bool overwrite)
        {
            string folder = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(folder))
                Directory.CreateDirectory(folder);

            if (!overwrite && File.Exists(path))
                return false;

            string tempPath = $"{path}.{Guid.NewGuid():N}{TempFileExtension}";
            try
            {
                File.WriteAllBytes(tempPath, bytes);

                if (overwrite && File.Exists(path))
                {
                    File.Replace(tempPath, path, null);
                    return true;
                }

                try
                {
                    File.Move(tempPath, path);
                    return true;
                }
                // Another editor wrote the target file after the check above
                catch (IOException) when (File.Exists(path))
                {
                    if (!overwrite)
                        return false;

                    File.Replace(tempPath, path, null);
                    return true;
                }
            }
            finally
            {
                TryDeleteFile(tempPath);
            }
        }

        /// <summary>
        /// Writes a text file safely, using UTF-8 without BOM.
        /// </summary>
        /// <remarks>
        /// See <see cref="WriteFileAtomic(string, byte[], bool)"/> for details.
        /// </remarks>
        /// <param name="path">The absolute path to the file to write.</param>
        /// <param name="text">The content of the file.</param>
        /// <param name="overwrite">
        /// If enabled, replaces the target file if it already exists. Otherwise, keeps the existing file, assuming that another editor
        /// wrote the same content.
        /// </param>
        /// <returns>Returns true if the file has been written, or false if it already existed and has been kept.</returns>
        /// <exception cref="IOException">Thrown if the file can't be written.</exception>
        internal static bool WriteFileAtomic(string path, string text, bool overwrite)
        {
            return WriteFileAtomic(path, TextEncoding.GetBytes(text), overwrite);
        }

        /// <summary>
        /// Deletes a file if it exists, ignoring errors.
        /// </summary>
        /// <param name="path">The absolute path to the file to delete.</param>
        private static void TryDeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        /// <summary>
        /// Deletes a folder if it's empty, ignoring errors.
        /// </summary>
        /// <param name="folder">The absolute path to the folder to delete.</param>
        /// <returns>Returns true if the folder has been deleted, or didn't exist.</returns>
        internal static bool TryDeleteEmptyFolder(string folder)
        {
            if (!Directory.Exists(folder))
                return true;

            try
            {
                if (HasEntries(folder))
                    return false;

                Directory.Delete(folder);
                return true;
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }

            return false;
        }

        /// <summary>
        /// Checks if a folder contains at least one file or subfolder.
        /// </summary>
        /// <param name="folder">The absolute path to the folder to check.</param>
        /// <returns>Returns true if the folder exists and is not empty.</returns>
        internal static bool HasEntries(string folder)
        {
            if (!Directory.Exists(folder))
                return false;

            // The enumeration stops at the first entry, and foreach disposes the enumerator
            foreach (string _ in Directory.EnumerateFileSystemEntries(folder))
                return true;

            return false;
        }

        #endregion


        #region Downloaded icons

        /// <summary>
        /// Measures the icon files stored in the shared folder.
        /// </summary>
        /// <param name="fileCount">Outputs the number of icon files.</param>
        /// <param name="byteCount">Outputs the total size of the icon files, in bytes.</param>
        /// <exception cref="IOException">Thrown if the shared folder can't be read.</exception>
        internal static void MeasureDownloadedIcons(out int fileCount, out long byteCount)
        {
            fileCount = 0;
            byteCount = 0;

            var rootFolder = new DirectoryInfo(RootFolder);
            if (!rootFolder.Exists)
                return;

            // Icon files are only in category folders, the root folder only contains the index
            foreach (DirectoryInfo categoryFolder in rootFolder.EnumerateDirectories())
            {
                foreach (FileInfo file in categoryFolder.EnumerateFiles($"*{IconFileExtension}", SearchOption.AllDirectories))
                {
                    fileCount++;
                    byteCount += file.Length;
                }
            }
        }

        /// <summary>
        /// Deletes all the icon files stored in the shared folder, so they're downloaded again when requested. The index is kept.
        /// </summary>
        /// <remarks>
        /// Every folder in the shared folder is deleted, since the shared folder only contains the index and the category folders.
        /// A folder that can't be deleted (for example if another editor is using one of its files) doesn't stop the others.
        /// </remarks>
        /// <returns>Returns the errors for the folders that couldn't be deleted, or an empty list if all of them have been.</returns>
        internal static List<string> DeleteDownloadedIcons()
        {
            var errors = new List<string>();
            if (!Directory.Exists(RootFolder))
                return errors;

            foreach (string folder in Directory.GetDirectories(RootFolder))
            {
                try
                {
                    Directory.Delete(folder, true);
                }
                catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
                {
                    errors.Add($"{folder}: {exception.Message}");
                }
            }

            // Temporary files left by an interrupted write of the index
            foreach (string file in Directory.GetFiles(RootFolder, $"*{TempFileExtension}"))
                TryDeleteFile(file);

            return errors;
        }

        #endregion

    }

}
