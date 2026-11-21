using System;
using System.Collections.Generic;
using System.IO;

using NUnit.Framework;

using SideXP.Core.EditorOnly;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="IconIndexDiff"/>.
    /// </summary>
    /// <remarks>
    /// The tests applying changes work in a temporary folder set as <see cref="IconStorage.RootFolderOverride"/>, so the real shared
    /// folder is never touched.
    /// </remarks>
    public class IconIndexDiffTests
    {

        private string _tempFolder = null;

        /// <summary>
        /// Creates an empty temporary folder, and uses it as the shared folder.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            string tempFolderName = $"SideXP-{nameof(IconIndexDiffTests)}-{Guid.NewGuid():N}";
            _tempFolder = IconStorage.NormalizePath(Path.Combine(Path.GetTempPath(), tempFolderName));
            Directory.CreateDirectory(_tempFolder);
            IconStorage.RootFolderOverride = _tempFolder;
        }

        /// <summary>
        /// Restores the default shared folder, and deletes the temporary folder.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            IconStorage.RootFolderOverride = null;
            if (Directory.Exists(_tempFolder))
                Directory.Delete(_tempFolder, true);
        }

        /// <summary>
        /// Makes an index entry.
        /// </summary>
        /// <param name="name">The name of the icon.</param>
        /// <param name="category">The category of the icon.</param>
        /// <param name="hasFilled">Whether the icon exists in the filled style.</param>
        /// <returns>Returns the created entry.</returns>
        private static IconIndexEntry Entry(string name, string category, bool hasFilled = false)
        {
            return new IconIndexEntry { name = name, category = category, hasFilled = hasFilled };
        }

        /// <summary>
        /// Makes an index with the given entries.
        /// </summary>
        /// <param name="entries">The entries of the index.</param>
        /// <returns>Returns the created index.</returns>
        private static IconIndex Index(params IconIndexEntry[] entries)
        {
            return new IconIndex { version = "3.52.0", icons = entries };
        }

        /// <summary>
        /// Creates the stored files of an icon, for the given sizes and both styles.
        /// </summary>
        /// <param name="name">The name of the icon.</param>
        /// <param name="category">The category of the icon.</param>
        /// <param name="hasFilled">Whether to create the filled files too.</param>
        private static void CreateIconFiles(string name, string category, bool hasFilled)
        {
            foreach (int size in new[] { 16, 32 })
            {
                string outlinePath = IconStorage.GetIconFilePath(name, category, size, EIconStyle.Outline);
                IconStorage.WriteFileAtomic(outlinePath, new byte[] { 1 }, false);

                if (hasFilled)
                {
                    string filledPath = IconStorage.GetIconFilePath(name, category, size, EIconStyle.Filled);
                    IconStorage.WriteFileAtomic(filledPath, new byte[] { 2 }, false);
                }
            }
        }

        /// <summary>
        /// Applies changes, and checks that all of them succeeded.
        /// </summary>
        /// <param name="changes">The changes to apply.</param>
        private static void ApplyAll(IEnumerable<IconIndexChange> changes)
        {
            List<string> errors = IconIndexDiff.Apply(changes);
            Assert.IsEmpty(errors, string.Join("\n", errors));
        }

        #region Compare

        /// <summary>
        /// Without previous index, there's nothing to clean up.
        /// </summary>
        [Test]
        public void Compare_WithoutOldIndex_HasNoChange()
        {
            Assert.IsEmpty(IconIndexDiff.Compare(null, Index(Entry("home", "Buildings"))));
        }

        /// <summary>
        /// The new index is required, since comparing with nothing would delete every icon.
        /// </summary>
        [Test]
        public void Compare_WithoutNewIndex_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => IconIndexDiff.Compare(Index(Entry("home", "Buildings")), null));
        }

        /// <summary>
        /// Identical indexes, or icons only added to the new one, need no change.
        /// </summary>
        [Test]
        public void Compare_SameOrAddedIcons_HasNoChange()
        {
            IconIndex oldIndex = Index(Entry("home", "Buildings", true));
            IconIndex newIndex = Index(Entry("home", "Buildings", true), Entry("user", "System"));

            Assert.IsEmpty(IconIndexDiff.Compare(oldIndex, newIndex));
        }

        /// <summary>
        /// An icon removed from the catalog has its folder deleted.
        /// </summary>
        [Test]
        public void Compare_RemovedIcon_DeletesFolder()
        {
            List<IconIndexChange> changes = IconIndexDiff.Compare(Index(Entry("home", "Buildings")), Index());

            CollectionAssert.AreEqual(new[] { IconIndexChange.DeleteFolder("home", "Buildings") }, changes);
        }

        /// <summary>
        /// An icon that changed category has its folder moved.
        /// </summary>
        [Test]
        public void Compare_ChangedCategory_MovesFolder()
        {
            List<IconIndexChange> changes = IconIndexDiff.Compare(Index(Entry("home", "Buildings")), Index(Entry("home", "Map")));

            CollectionAssert.AreEqual(new[] { IconIndexChange.MoveFolder("home", "Buildings", "Map") }, changes);
        }

        /// <summary>
        /// A category renamed without changing its folder name (as a different case) doesn't move anything.
        /// </summary>
        [TestCase("Version control", "Version Control")]
        [TestCase("", "Other")]
        public void Compare_CategoryWithSameFolder_HasNoChange(string oldCategory, string newCategory)
        {
            List<IconIndexChange> changes = IconIndexDiff.Compare(Index(Entry("git", oldCategory)), Index(Entry("git", newCategory)));

            Assert.IsEmpty(changes);
        }

        /// <summary>
        /// An icon that lost its filled style has its filled files deleted, but gaining it needs no change.
        /// </summary>
        [Test]
        public void Compare_RemovedFilledStyle_DeletesFilledFiles()
        {
            IconIndex oldIndex = Index(Entry("home", "Buildings", true), Entry("user", "System", false));
            IconIndex newIndex = Index(Entry("home", "Buildings", false), Entry("user", "System", true));

            List<IconIndexChange> changes = IconIndexDiff.Compare(oldIndex, newIndex);

            CollectionAssert.AreEqual(new[] { IconIndexChange.DeleteStyleFiles("home", "Buildings", EIconStyle.Filled) }, changes);
        }

        /// <summary>
        /// An icon that changed category and lost a style is moved first, then its style files are deleted in the new category.
        /// </summary>
        [Test]
        public void Compare_ChangedCategoryAndRemovedStyle_MovesThenDeletesInNewCategory()
        {
            IconIndex oldIndex = Index(Entry("home", "Buildings", true));
            IconIndex newIndex = Index(Entry("home", "Map", false));

            List<IconIndexChange> changes = IconIndexDiff.Compare(oldIndex, newIndex);

            IconIndexChange[] expected =
            {
                IconIndexChange.MoveFolder("home", "Buildings", "Map"),
                IconIndexChange.DeleteStyleFiles("home", "Map", EIconStyle.Filled),
            };
            CollectionAssert.AreEqual(expected, changes);
        }

        /// <summary>
        /// Entries of the old index with an invalid name are ignored, since they can't have stored files.
        /// </summary>
        [Test]
        public void Compare_InvalidOldName_IsIgnored()
        {
            Assert.IsEmpty(IconIndexDiff.Compare(Index(Entry("../home", "Buildings"), null), Index()));
        }

        #endregion


        #region Apply

        /// <summary>
        /// Deleting an icon deletes its folder, and its category folder once empty.
        /// </summary>
        [Test]
        public void Apply_DeleteFolder_DeletesIconAndEmptyCategory()
        {
            CreateIconFiles("home", "Buildings", false);
            CreateIconFiles("user", "System", false);
            CreateIconFiles("users", "System", false);

            ApplyAll(new[] { IconIndexChange.DeleteFolder("home", "Buildings"), IconIndexChange.DeleteFolder("user", "System") });

            Assert.IsFalse(Directory.Exists($"{_tempFolder}/buildings"));
            Assert.IsFalse(Directory.Exists($"{_tempFolder}/system/user"));
            Assert.IsTrue(Directory.Exists($"{_tempFolder}/system/users"));
        }

        /// <summary>
        /// Moving an icon moves its files to the new category folder, and deletes the old category folder once empty.
        /// </summary>
        [Test]
        public void Apply_MoveFolder_MovesFilesToNewCategory()
        {
            CreateIconFiles("home", "Buildings", true);

            ApplyAll(new[] { IconIndexChange.MoveFolder("home", "Buildings", "Map") });

            Assert.IsTrue(File.Exists(IconStorage.GetIconFilePath("home", "Map", 16, EIconStyle.Outline)));
            Assert.IsTrue(File.Exists(IconStorage.GetIconFilePath("home", "Map", 32, EIconStyle.Filled)));
            Assert.IsFalse(Directory.Exists($"{_tempFolder}/buildings"));
        }

        /// <summary>
        /// Moving an icon already stored in its new category keeps the new files, and deletes the old ones.
        /// </summary>
        [Test]
        public void Apply_MoveFolderToExistingFolder_KeepsNewFiles()
        {
            CreateIconFiles("home", "Buildings", false);
            string newFile = IconStorage.GetIconFilePath("home", "Map", 16, EIconStyle.Outline);
            IconStorage.WriteFileAtomic(newFile, new byte[] { 9 }, false);

            ApplyAll(new[] { IconIndexChange.MoveFolder("home", "Buildings", "Map") });

            CollectionAssert.AreEqual(new byte[] { 9 }, File.ReadAllBytes(newFile));
            Assert.IsFalse(Directory.Exists($"{_tempFolder}/buildings"));
        }

        /// <summary>
        /// Deleting a style deletes only the files of that style.
        /// </summary>
        [Test]
        public void Apply_DeleteStyleFiles_DeletesOnlyThatStyle()
        {
            CreateIconFiles("home", "Buildings", true);

            ApplyAll(new[] { IconIndexChange.DeleteStyleFiles("home", "Buildings", EIconStyle.Filled) });

            Assert.IsTrue(File.Exists(IconStorage.GetIconFilePath("home", "Buildings", 16, EIconStyle.Outline)));
            Assert.IsTrue(File.Exists(IconStorage.GetIconFilePath("home", "Buildings", 32, EIconStyle.Outline)));
            Assert.IsFalse(File.Exists(IconStorage.GetIconFilePath("home", "Buildings", 16, EIconStyle.Filled)));
            Assert.IsFalse(File.Exists(IconStorage.GetIconFilePath("home", "Buildings", 32, EIconStyle.Filled)));
        }

        /// <summary>
        /// Changes for icons that have never been downloaded do nothing, and don't fail.
        /// </summary>
        [Test]
        public void Apply_MissingIconFiles_DoesNothing()
        {
            ApplyAll(new[]
            {
                IconIndexChange.MoveFolder("home", "Buildings", "Map"),
                IconIndexChange.DeleteFolder("user", "System"),
                IconIndexChange.DeleteStyleFiles("star", "Shapes", EIconStyle.Filled),
            });

            Assert.IsEmpty(Directory.GetFileSystemEntries(_tempFolder));
        }

        /// <summary>
        /// A change that fails is reported, and doesn't stop the next ones.
        /// </summary>
        [Test]
        public void Apply_InvalidChange_ReportsErrorAndContinues()
        {
            CreateIconFiles("home", "Buildings", false);

            List<string> errors = IconIndexDiff.Apply(new[]
            {
                IconIndexChange.DeleteFolder("../home", "Buildings"),
                IconIndexChange.DeleteFolder("home", "Buildings"),
            });

            Assert.AreEqual(1, errors.Count);
            Assert.IsFalse(Directory.Exists($"{_tempFolder}/buildings"));
        }

        #endregion

    }

}
