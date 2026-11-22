using System;
using System.Collections.Generic;
using System.IO;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Compares two versions of the index, and cleans up the stored icon files that don't match the new one.
    /// </summary>
    /// <remarks>
    /// The Tabler version is not part of the folder layout, so after a refresh, the files of an icon must follow its new category and
    /// styles. Icons added to the catalog need no change, since they're downloaded on demand.
    /// </remarks>
    internal static class IconIndexDiff
    {

        #region Public API

        /// <summary>
        /// Lists the changes to apply to the stored icon files when replacing an index with another.
        /// </summary>
        /// <remarks>
        /// For an icon that changed category and lost a style, the folder is moved first, so the style change uses the new category.
        /// Changes in the category name that don't change its folder (as a different case) don't move anything.
        /// </remarks>
        /// <param name="oldIndex">The index to replace, or null if there's none yet.</param>
        /// <param name="newIndex">The new index.</param>
        /// <returns>Returns the changes to apply, in order.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the new index is null.</exception>
        internal static List<IconIndexChange> Compare(IconIndex oldIndex, IconIndex newIndex)
        {
            if (newIndex == null)
                throw new ArgumentNullException(nameof(newIndex));

            var changes = new List<IconIndexChange>();
            if (oldIndex == null || oldIndex.icons == null)
                return changes;

            Dictionary<string, IconIndexEntry> newEntries = MakeLookup(newIndex);
            foreach (IconIndexEntry oldEntry in oldIndex.icons)
            {
                // Entries that can't have stored files
                if (oldEntry == null || !IconStorage.IsValidIconName(oldEntry.name))
                    continue;

                if (!newEntries.TryGetValue(oldEntry.name, out IconIndexEntry newEntry))
                {
                    changes.Add(IconIndexChange.DeleteFolder(oldEntry.name, oldEntry.category));
                    continue;
                }

                string category = oldEntry.category;
                if (IconStorage.GetCategoryFolderName(oldEntry.category) != IconStorage.GetCategoryFolderName(newEntry.category))
                {
                    changes.Add(IconIndexChange.MoveFolder(oldEntry.name, oldEntry.category, newEntry.category));
                    category = newEntry.category;
                }

                if (oldEntry.hasFilled && !newEntry.hasFilled)
                    changes.Add(IconIndexChange.DeleteStyleFiles(oldEntry.name, category, EIconStyle.Filled));
            }

            return changes;
        }

        /// <summary>
        /// Applies changes to the stored icon files, and deletes the category folders left empty.
        /// </summary>
        /// <remarks>
        /// A change that fails doesn't stop the others. Its files stay in place, which is harmless: an icon is only considered available
        /// if its file exists where the new index expects it, and the files left behind are deleted with the downloaded icons.
        /// </remarks>
        /// <param name="changes">The changes to apply, in order.</param>
        /// <returns>Returns the errors of the changes that failed, or an empty list if all of them succeeded.</returns>
        internal static List<string> Apply(IEnumerable<IconIndexChange> changes)
        {
            var errors = new List<string>();
            foreach (IconIndexChange change in changes)
            {
                try
                {
                    ApplyChange(change);
                }
                catch (Exception exception)
                when (exception is IOException || exception is UnauthorizedAccessException || exception is ArgumentException)
                {
                    errors.Add($"{change}: {exception.Message}");
                }
            }
            return errors;
        }

        #endregion


        #region Private API

        /// <summary>
        /// Makes a lookup of the entries of an index by icon name.
        /// </summary>
        /// <param name="index">The index of which to get the entries.</param>
        /// <returns>Returns the entries of the index, by icon name.</returns>
        private static Dictionary<string, IconIndexEntry> MakeLookup(IconIndex index)
        {
            var lookup = new Dictionary<string, IconIndexEntry>();
            if (index.icons == null)
                return lookup;

            foreach (IconIndexEntry entry in index.icons)
            {
                if (entry != null && !string.IsNullOrEmpty(entry.name))
                    lookup[entry.name] = entry;
            }
            return lookup;
        }

        /// <summary>
        /// Applies a single change to the stored icon files.
        /// </summary>
        /// <param name="change">The change to apply.</param>
        /// <exception cref="IOException">Thrown if a file or folder can't be moved or deleted.</exception>
        /// <exception cref="ArgumentException">Thrown if the icon name of the change is not valid.</exception>
        private static void ApplyChange(IconIndexChange change)
        {
            string folder = IconStorage.GetIconFolderPath(change.Name, change.Category);
            if (!Directory.Exists(folder))
                return;

            switch (change.Type)
            {
                case EIconIndexChange.MoveFolder:
                    string targetFolder = IconStorage.GetIconFolderPath(change.Name, change.TargetCategory);
                    // The icon has already been downloaded in its new category (as by another editor), so the old files are not needed
                    if (Directory.Exists(targetFolder))
                    {
                        Directory.Delete(folder, true);
                    }
                    else
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(targetFolder));
                        Directory.Move(folder, targetFolder);
                    }
                    break;

                case EIconIndexChange.DeleteFolder:
                    Directory.Delete(folder, true);
                    break;

                case EIconIndexChange.DeleteStyleFiles:
                    // Icon names never contain underscores, so this only matches the files of this icon in this style
                    string pattern = $"{change.Name}_*_{IconStorage.GetStyleName(change.Style)}{IconStorage.IconFileExtension}";
                    foreach (string file in Directory.GetFiles(folder, pattern))
                        File.Delete(file);
                    break;
            }

            FileEditorUtility.TryDeleteEmptyFolder(Path.GetDirectoryName(folder));
        }

        #endregion

    }

}
