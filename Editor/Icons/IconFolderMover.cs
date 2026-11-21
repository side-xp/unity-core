using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Moves the icons from one shared folder to another, and deletes the previous folder once every file has been moved.
    /// </summary>
    /// <remarks>
    /// A target folder is only accepted if it's missing, empty, or already an icons folder (it contains an index file). So the icons
    /// are never mixed with other files, and the previous folder can be deleted safely.
    /// </remarks>
    internal static class IconFolderMover
    {

        #region Fields

        /// <summary>
        /// Number of copied files between two progress reports. Reporting progress for every file would slow down the copy, since
        /// there can be tens of thousands of icon files.
        /// </summary>
        private const int ProgressReportInterval = 50;

        #endregion


        #region Public API

        /// <summary>
        /// Checks if the icons can be moved from a folder to another.
        /// </summary>
        /// <param name="sourceFolder">The absolute path to the current icons folder.</param>
        /// <param name="targetFolder">The absolute path to the folder where to move the icons.</param>
        /// <param name="error">Outputs the reason why the icons can't be moved, or null if they can.</param>
        /// <returns>Returns true if the icons can be moved.</returns>
        internal static bool CanMove(string sourceFolder, string targetFolder, out string error)
        {
            if (string.IsNullOrWhiteSpace(targetFolder))
            {
                error = "No target folder has been selected.";
                return false;
            }

            sourceFolder = IconStorage.NormalizePath(sourceFolder);
            targetFolder = IconStorage.NormalizePath(targetFolder);

            if (PathsEqual(sourceFolder, targetFolder))
            {
                error = "The selected folder is already the icons folder.";
                return false;
            }

            if (PathsEqual(targetFolder, IconStorage.NormalizePath(Path.GetPathRoot(targetFolder))))
            {
                error = "The root of a drive can't be used as the icons folder.";
                return false;
            }

            if (IsInFolder(targetFolder, sourceFolder) || IsInFolder(sourceFolder, targetFolder))
            {
                error = "The icons folder can't be moved into itself, or into one of its parent folders.";
                return false;
            }

            if (IconStorage.HasEntries(targetFolder) && !File.Exists($"{targetFolder}/{IconStorage.IndexFileName}"))
            {
                error = "The selected folder is not empty, and doesn't contain icons. "
                    + "Select an empty folder, or an existing icons folder.";
                return false;
            }

            error = null;
            return true;
        }

        /// <summary>
        /// Moves the icons from a folder to another.
        /// </summary>
        /// <remarks>
        /// If the target folder already contains icons, the files are merged, and the existing ones are kept. If the move fails or is
        /// cancelled, the copied files are deleted and the source folder is kept as is. If it succeeds, the source folder is deleted.
        /// Files that can't be deleted (for example if another editor is using them) are reported with a warning, but don't fail the
        /// move.
        /// </remarks>
        /// <param name="sourceFolder">The absolute path to the current icons folder.</param>
        /// <param name="targetFolder">The absolute path to the folder where to move the icons.</param>
        /// <param name="emptyParentsToDelete">
        /// The number of parent folders of the source folder to delete if they're empty after the move. Meant for the default folder,
        /// so the parent folders created for it don't stay behind.
        /// </param>
        /// <param name="onProgress">
        /// Called while files are copied, with the number of copied files and the total number of files. Returns true to cancel the
        /// move. Can be null.
        /// </param>
        /// <param name="error">Outputs the reason why the move failed, or null if it succeeded.</param>
        /// <returns>Returns true if every file has been moved to the target folder.</returns>
        internal static bool Move(string sourceFolder, string targetFolder, int emptyParentsToDelete, Func<int, int, bool> onProgress,
            out string error)
        {
            if (!CanMove(sourceFolder, targetFolder, out error))
                return false;

            sourceFolder = IconStorage.NormalizePath(sourceFolder);
            targetFolder = IconStorage.NormalizePath(targetFolder);

            // Nothing to move, so the target folder can be used right away
            if (!Directory.Exists(sourceFolder))
                return true;

            if (!TryMoveFolder(sourceFolder, targetFolder))
            {
                if (!TryCopyFiles(sourceFolder, targetFolder, onProgress, out List<string> sourceFiles, out error))
                    return false;

                DeleteFiles(sourceFiles);
                DeleteEmptyFolders(sourceFolder, true);

                if (Directory.Exists(sourceFolder))
                    Debug.LogWarning($"Icons moved to {targetFolder}, but some files couldn't be deleted from {sourceFolder}.");
            }

            DeleteEmptyParents(sourceFolder, emptyParentsToDelete);
            return true;
        }

        #endregion


        #region Private API

        /// <summary>
        /// Tries to move the whole source folder at once, which is instant on the same volume.
        /// </summary>
        /// <param name="sourceFolder">The absolute path to the current icons folder.</param>
        /// <param name="targetFolder">The absolute path to the folder where to move the icons.</param>
        /// <returns>
        /// Returns true if the folder has been moved, or false if it can't be moved at once (the target folder exists, it's on another
        /// volume, or a file is in use), in which case nothing has changed.
        /// </returns>
        private static bool TryMoveFolder(string sourceFolder, string targetFolder)
        {
            if (Directory.Exists(targetFolder))
                return false;

            string targetParent = Path.GetDirectoryName(targetFolder);
            if (!string.IsNullOrEmpty(targetParent))
                Directory.CreateDirectory(targetParent);

            try
            {
                Directory.Move(sourceFolder, targetFolder);
                return true;
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }

            return false;
        }

        /// <summary>
        /// Copies every icons file from the source folder to the target folder, keeping the existing files in the target folder.
        /// </summary>
        /// <remarks>
        /// Temporary files left by an interrupted write are not copied. If the copy fails or is cancelled, the copied files are deleted.
        /// </remarks>
        /// <param name="sourceFolder">The absolute path to the current icons folder.</param>
        /// <param name="targetFolder">The absolute path to the folder where to move the icons.</param>
        /// <param name="onProgress">
        /// Called while files are copied, with the number of copied files and the total number of files. Returns true to cancel the
        /// copy. Can be null.
        /// </param>
        /// <param name="sourceFiles">Outputs the paths to all the files of the source folder, including the ones not copied.</param>
        /// <param name="error">Outputs the reason why the copy failed, or null if it succeeded.</param>
        /// <returns>Returns true if every file has been copied.</returns>
        private static bool TryCopyFiles(string sourceFolder, string targetFolder, Func<int, int, bool> onProgress,
            out List<string> sourceFiles, out string error)
        {
            sourceFiles = new List<string>(Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories));
            var copiedFiles = new List<string>();
            bool hasTargetFolder = Directory.Exists(targetFolder);

            try
            {
                for (int i = 0; i < sourceFiles.Count; i++)
                {
                    if (onProgress != null && i % ProgressReportInterval == 0 && onProgress(i, sourceFiles.Count))
                    {
                        error = "The move has been cancelled.";
                        Rollback(copiedFiles, targetFolder, hasTargetFolder);
                        return false;
                    }

                    if (sourceFiles[i].EndsWith(IconStorage.TempFileExtension, StringComparison.OrdinalIgnoreCase))
                        continue;

                    // Normalized, since the returned paths may not keep the separators of the source folder path
                    string relativePath = IconStorage.NormalizePath(sourceFiles[i]).Substring(sourceFolder.Length + 1);
                    string targetFile = $"{targetFolder}/{relativePath}";
                    if (IconStorage.WriteFileAtomic(targetFile, File.ReadAllBytes(sourceFiles[i]), false))
                        copiedFiles.Add(targetFile);
                }
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                error = $"The icons couldn't be copied to the selected folder: {exception.Message}";
                Rollback(copiedFiles, targetFolder, hasTargetFolder);
                return false;
            }

            error = null;
            return true;
        }

        /// <summary>
        /// Deletes the files copied by a failed or cancelled copy, and the folders created for them.
        /// </summary>
        /// <param name="copiedFiles">The absolute paths to the copied files.</param>
        /// <param name="targetFolder">The absolute path to the folder where the icons were copied.</param>
        /// <param name="hasTargetFolder">Whether the target folder existed before the copy, in which case it's kept.</param>
        private static void Rollback(List<string> copiedFiles, string targetFolder, bool hasTargetFolder)
        {
            DeleteFiles(copiedFiles);
            DeleteEmptyFolders(targetFolder, !hasTargetFolder);
        }

        /// <summary>
        /// Deletes files, ignoring the ones that can't be deleted.
        /// </summary>
        /// <param name="files">The absolute paths to the files to delete.</param>
        private static void DeleteFiles(List<string> files)
        {
            foreach (string file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
        }

        /// <summary>
        /// Deletes all the empty subfolders of a folder, from the deepest ones, ignoring the ones that can't be deleted.
        /// </summary>
        /// <param name="folder">The absolute path to the folder to clean up.</param>
        /// <param name="deleteFolder">If enabled, also deletes the folder itself if it's empty after the clean-up.</param>
        private static void DeleteEmptyFolders(string folder, bool deleteFolder)
        {
            if (!Directory.Exists(folder))
                return;

            foreach (string subfolder in Directory.GetDirectories(folder))
                DeleteEmptyFolders(subfolder, true);

            if (deleteFolder)
                IconStorage.TryDeleteEmptyFolder(folder);
        }

        /// <summary>
        /// Deletes the parent folders of a folder, as long as they're empty.
        /// </summary>
        /// <param name="folder">The absolute path to the folder of which to delete the parents.</param>
        /// <param name="count">The maximum number of parent folders to delete.</param>
        private static void DeleteEmptyParents(string folder, int count)
        {
            string parent = Path.GetDirectoryName(folder);
            for (int i = 0; i < count && !string.IsNullOrEmpty(parent); i++)
            {
                if (!IconStorage.TryDeleteEmptyFolder(parent))
                    return;

                parent = Path.GetDirectoryName(parent);
            }
        }

        /// <summary>
        /// Checks if two normalized paths point to the same folder.
        /// </summary>
        /// <remarks>
        /// Case-insensitive, as on Windows and macOS. On case-sensitive file systems, this can only refuse more moves than needed,
        /// never accept a move into the source folder.
        /// </remarks>
        /// <param name="a">The first normalized path.</param>
        /// <param name="b">The second normalized path.</param>
        /// <returns>Returns true if both paths point to the same folder.</returns>
        private static bool PathsEqual(string a, string b)
        {
            return string.Equals(a.TrimEnd('/'), b.TrimEnd('/'), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks if a normalized path is inside another folder.
        /// </summary>
        /// <param name="path">The normalized path to check.</param>
        /// <param name="folder">The normalized path to the folder.</param>
        /// <returns>Returns true if the path is inside the folder, at any depth.</returns>
        private static bool IsInFolder(string path, string folder)
        {
            return path.StartsWith($"{folder.TrimEnd('/')}/", StringComparison.OrdinalIgnoreCase);
        }

        #endregion

    }

}
