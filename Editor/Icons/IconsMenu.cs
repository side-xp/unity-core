using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using UnityEngine;
using UnityEditor;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Main toolbar menus to manage the editor icons stored in the shared folder.
    /// </summary>
    internal static class IconsMenu
    {

        #region Fields

        /// <summary>
        /// Base path of the editor icons menus.
        /// </summary>
        private const string MenuPath = EditorConstants.ToolbarMenu + "/Icons";

        /// <summary>
        /// Path of the menu that refreshes the icons index.
        /// </summary>
        private const string RefreshIndexMenu = MenuPath + "/Refresh Icons Index";

        /// <summary>
        /// Path of the menu that deletes the downloaded icons.
        /// </summary>
        private const string DeleteIconsMenu = MenuPath + "/Delete Downloaded Icons";

        /// <summary>
        /// Title of the dialogs displayed when deleting the downloaded icons.
        /// </summary>
        private const string DeleteIconsTitle = "Delete downloaded icons";

        /// <summary>
        /// Number of bytes in a kilobyte, as used to display file sizes.
        /// </summary>
        private const long BytesPerKilobyte = 1024;

        /// <summary>
        /// Units used to display file sizes, from bytes.
        /// </summary>
        private static readonly string[] SizeUnits = { "B", "KB", "MB", "GB" };

        #endregion


        #region Menus

        /// <summary>
        /// Downloads the latest version of the icons index, and cleans up the downloaded icons that don't match it anymore.
        /// </summary>
        [MenuItem(RefreshIndexMenu)]
        private static void RefreshIndex()
        {
            IconIndexRefresher.Refresh(true);
        }

        /// <summary>
        /// Checks if the icons index can be refreshed.
        /// </summary>
        /// <returns>Returns true if no refresh is in progress.</returns>
        [MenuItem(RefreshIndexMenu, true)]
        private static bool CanRefreshIndex()
        {
            return !IconIndexRefresher.IsRefreshing;
        }

        /// <summary>
        /// Deletes all the downloaded icons after confirmation, so they're downloaded again when requested.
        /// </summary>
        [MenuItem(DeleteIconsMenu)]
        private static void DeleteDownloadedIcons()
        {
            // @todo Pause icon downloads while deleting, once the download queue exists.
            string folder = IconStorage.RootFolder;
            int fileCount;
            long byteCount;
            try
            {
                IconStorage.MeasureDownloadedIcons(out fileCount, out byteCount);
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                EditorUtility.DisplayDialog(DeleteIconsTitle, $"The icons folder couldn't be read: {exception.Message}", "OK");
                return;
            }

            if (fileCount == 0)
            {
                EditorUtility.DisplayDialog(DeleteIconsTitle, $"There are no downloaded icons in:\n{folder}", "OK");
                return;
            }

            string message = $"Delete {fileCount} downloaded icon files ({FormatSize(byteCount)}) from:\n{folder}\n\n"
                + "They will be downloaded again when needed. The icons index is kept.";
            if (!EditorUtility.DisplayDialog(DeleteIconsTitle, message, "Delete", "Cancel"))
                return;

            List<string> errors = IconStorage.DeleteDownloadedIcons();
            if (errors.Count > 0)
            {
                Debug.LogWarning("Some downloaded icons couldn't be deleted (they may be used by another editor):\n"
                    + string.Join("\n", errors));
                string errorMessage = "Some downloaded icons couldn't be deleted. See the console for more info.";
                EditorUtility.DisplayDialog(DeleteIconsTitle, errorMessage, "OK");
            }
        }

        /// <summary>
        /// Checks if the downloaded icons can be deleted.
        /// </summary>
        /// <returns>Returns true if no refresh of the icons index is in progress, since it may move or delete icon folders.</returns>
        [MenuItem(DeleteIconsMenu, true)]
        private static bool CanDeleteDownloadedIcons()
        {
            return !IconIndexRefresher.IsRefreshing;
        }

        #endregion


        #region Private API

        /// <summary>
        /// Formats a file size for display.
        /// </summary>
        /// <example><c>FormatSize(3_600_000)</c> returns <c>"3.4 MB"</c>.</example>
        /// <param name="byteCount">The size to format, in bytes.</param>
        /// <returns>Returns the formatted size, in the largest unit that keeps the value above 1.</returns>
        internal static string FormatSize(long byteCount)
        {
            double size = byteCount;
            int unitIndex = 0;
            while (size >= BytesPerKilobyte && unitIndex < SizeUnits.Length - 1)
            {
                size /= BytesPerKilobyte;
                unitIndex++;
            }

            return unitIndex == 0
                ? $"{byteCount} {SizeUnits[0]}"
                : $"{size.ToString("0.#", CultureInfo.InvariantCulture)} {SizeUnits[unitIndex]}";
        }

        #endregion

    }

}
