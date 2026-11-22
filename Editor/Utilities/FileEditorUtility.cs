using System;
using System.IO;
using System.Text;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Miscellaneous functions for writing and deleting files and folders safely.
    /// </summary>
    /// <remarks>
    /// Meant for files that several editors may use at the same time, as in a folder shared by every project on the machine.
    /// </remarks>
    public static class FileEditorUtility
    {

        #region Fields

        /// <summary>
        /// Extension of the temporary files written by <see cref="WriteFileAtomic(string, byte[], bool)"/> before being moved into place.
        /// </summary>
        /// <remarks>
        /// A temporary file is only left behind if the editor stops while writing it. Its name is the target file name, followed by a
        /// unique identifier and this extension, so it can be found and deleted.
        /// </remarks>
        public const string TempFileExtension = ".tmp";

        /// <summary>
        /// Encoding used for text files, without BOM.
        /// </summary>
        private static readonly Encoding TextEncoding = new UTF8Encoding(false);

        #endregion


        #region Public API

        /// <summary>
        /// Writes a file safely, even if another editor writes the same file at the same time.
        /// </summary>
        /// <remarks>
        /// The content is written to a temporary file next to the target, then moved into place, which is atomic on the same volume.
        /// So the target file is either missing or complete, never partially written. Missing folders are created.
        /// </remarks>
        /// <example>
        /// <code>
        /// FileEditorUtility.WriteFileAtomic("C:/Cache/data.bin", bytes, true);
        /// </code>
        /// </example>
        /// <param name="path">The absolute path to the file to write.</param>
        /// <param name="bytes">The content of the file.</param>
        /// <param name="overwrite">
        /// If enabled, replaces the target file if it already exists. Otherwise, keeps the existing file, assuming that another editor
        /// wrote the same content.
        /// </param>
        /// <returns>Returns true if the file has been written, or false if it already existed and has been kept.</returns>
        /// <exception cref="IOException">Thrown if the file can't be written.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if the file or its folder can't be accessed.</exception>
        public static bool WriteFileAtomic(string path, byte[] bytes, bool overwrite)
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
        /// <exception cref="UnauthorizedAccessException">Thrown if the file or its folder can't be accessed.</exception>
        public static bool WriteFileAtomic(string path, string text, bool overwrite)
        {
            return WriteFileAtomic(path, TextEncoding.GetBytes(text), overwrite);
        }

        /// <summary>
        /// Deletes a file if it exists, ignoring errors.
        /// </summary>
        /// <param name="path">The absolute path to the file to delete.</param>
        /// <returns>Returns true if the file has been deleted, or didn't exist.</returns>
        public static bool TryDeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);

                return true;
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }

            return false;
        }

        /// <summary>
        /// Deletes a folder if it's empty, ignoring errors.
        /// </summary>
        /// <param name="folder">The absolute path to the folder to delete.</param>
        /// <returns>Returns true if the folder has been deleted, or didn't exist.</returns>
        public static bool TryDeleteEmptyFolder(string folder)
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
        public static bool HasEntries(string folder)
        {
            if (!Directory.Exists(folder))
                return false;

            // The enumeration stops at the first entry, and foreach disposes the enumerator
            foreach (string _ in Directory.EnumerateFileSystemEntries(folder))
                return true;

            return false;
        }

        #endregion

    }

}
