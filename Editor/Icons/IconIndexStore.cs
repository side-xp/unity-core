using System;
using System.IO;

using UnityEngine;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Reads and writes the index file in the shared folder.
    /// </summary>
    /// <remarks>
    /// The last read index is kept in memory, and read again only when the file changes (as when another editor refreshes it), so the
    /// file is not parsed each time the index is needed.
    /// </remarks>
    internal static class IconIndexStore
    {

        #region Fields

        /// <summary>
        /// The last read or written index, or null if none.
        /// </summary>
        private static IconIndex s_index = null;

        /// <summary>
        /// The path of the file <see cref="s_index"/> comes from. Compared with the current path, in case the shared folder changed.
        /// </summary>
        private static string s_indexPath = null;

        /// <summary>
        /// The last write time of the file <see cref="s_index"/> comes from, in UTC.
        /// </summary>
        private static DateTime s_indexWriteTime = default;

        #endregion


        #region Public API

        /// <summary>
        /// Gets the stored index.
        /// </summary>
        /// <param name="index">Outputs the stored index, or null if there's none or it's not valid.</param>
        /// <returns>Returns true if a valid index is stored.</returns>
        internal static bool TryGetIndex(out IconIndex index)
        {
            index = null;
            string path = IconStorage.IndexFilePath;

            DateTime writeTime;
            try
            {
                if (!File.Exists(path))
                {
                    ClearMemory();
                    return false;
                }

                writeTime = File.GetLastWriteTimeUtc(path);
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                return false;
            }

            if (s_index != null && s_indexPath == path && s_indexWriteTime == writeTime)
            {
                index = s_index;
                return true;
            }

            ClearMemory();
            if (!TryRead(path, out IconIndex readIndex))
                return false;

            Remember(readIndex, path, writeTime);
            index = readIndex;
            return true;
        }

        /// <summary>
        /// Writes an index as the stored index, replacing the existing one.
        /// </summary>
        /// <param name="index">The index to write.</param>
        /// <exception cref="ArgumentException">Thrown if the index is not valid.</exception>
        /// <exception cref="IOException">Thrown if the index file can't be written.</exception>
        internal static void Write(IconIndex index)
        {
            if (!IsValid(index))
                throw new ArgumentException("The index to write is not valid.", nameof(index));

            string path = IconStorage.IndexFilePath;
            IconStorage.WriteFileAtomic(path, JsonUtility.ToJson(index), true);
            Remember(index, path, File.GetLastWriteTimeUtc(path));
        }

        /// <summary>
        /// Forgets the index kept in memory, so the next access reads the file again.
        /// </summary>
        internal static void ClearMemory()
        {
            s_index = null;
            s_indexPath = null;
            s_indexWriteTime = default;
        }

        #endregion


        #region Private API

        /// <summary>
        /// Reads an index file.
        /// </summary>
        /// <param name="path">The absolute path to the index file.</param>
        /// <param name="index">Outputs the read index, or null if the file can't be read or is not valid.</param>
        /// <returns>Returns true if a valid index has been read.</returns>
        private static bool TryRead(string path, out IconIndex index)
        {
            index = null;
            try
            {
                index = JsonUtility.FromJson<IconIndex>(File.ReadAllText(path));
            }
            catch (Exception exception)
            when (exception is IOException || exception is UnauthorizedAccessException || exception is ArgumentException)
            {
                return false;
            }

            if (IsValid(index))
                return true;

            index = null;
            return false;
        }

        /// <summary>
        /// Checks if an index can be used: it has a valid Tabler version, and a list of icons.
        /// </summary>
        /// <param name="index">The index to check.</param>
        /// <returns>Returns true if the index is valid.</returns>
        private static bool IsValid(IconIndex index)
        {
            return index != null && TablerSource.IsValidVersion(index.version) && index.icons != null;
        }

        /// <summary>
        /// Keeps an index in memory.
        /// </summary>
        /// <param name="index">The index to keep.</param>
        /// <param name="path">The path of the file the index comes from.</param>
        /// <param name="writeTime">The last write time of that file, in UTC.</param>
        private static void Remember(IconIndex index, string path, DateTime writeTime)
        {
            s_index = index;
            s_indexPath = path;
            s_indexWriteTime = writeTime;
        }

        #endregion

    }

}
