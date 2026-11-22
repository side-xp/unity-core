using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Resolves the paths in the shared folder where editor icons and their index are stored, and manages the stored files.
    /// </summary>
    /// <remarks>
    /// The shared folder is per user and doesn't depend on Unity, so every project, session and Unity version on this machine reuses
    /// the same files. Several editors may write into it at the same time, so files are written with
    /// <see cref="FileEditorUtility.WriteFileAtomic(string, byte[], bool)"/>.
    /// </remarks>
    internal static class IconStorage
    {

        #region Fields

        /// <summary>
        /// Path to the default shared folder, relative to <see cref="PathEditorUtility.UserCachePath"/>.
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
        private const string OtherFolderName = "other";

        /// <summary>
        /// Name of the outline style, used in icon file names and in the Tabler catalog.
        /// </summary>
        private const string OutlineStyleName = "outline";

        /// <summary>
        /// Name of the filled style, used in icon file names and in the Tabler catalog.
        /// </summary>
        private const string FilledStyleName = "filled";

        /// <summary>
        /// Extension of the stored icon files.
        /// </summary>
        internal const string IconFileExtension = ".png";

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
        internal static string DefaultRootFolder => $"{PathEditorUtility.UserCachePath}/{DefaultRelativeFolder}";

        /// <summary>
        /// Absolute path to the index file.
        /// </summary>
        internal static string IndexFilePath => $"{RootFolder}/{IndexFileName}";

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


        #region Downloaded icons

        /// <summary>
        /// Checks if the files of an icon are stored for a given style, in every stored size.
        /// </summary>
        /// <param name="name">The name of the icon.</param>
        /// <param name="category">The category of the icon, as written in the index.</param>
        /// <param name="style">The style of the icon.</param>
        /// <returns>Returns true if a file exists for each of <see cref="StoredSizes"/>.</returns>
        /// <exception cref="ArgumentException">Thrown if the icon name is not valid.</exception>
        internal static bool IsStored(string name, string category, EIconStyle style)
        {
            foreach (int size in StoredSizes)
            {
                if (!File.Exists(GetIconFilePath(name, category, size, style)))
                    return false;
            }
            return true;
        }

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
            foreach (string file in Directory.GetFiles(RootFolder, $"*{FileEditorUtility.TempFileExtension}"))
                FileEditorUtility.TryDeleteFile(file);

            return errors;
        }

        #endregion

    }

}
