using System;
using System.Collections.Generic;
using System.IO;

using NUnit.Framework;

using SideXP.Core.EditorOnly;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="IconStorage"/>.
    /// </summary>
    /// <remarks>
    /// Every test works in a temporary folder set as <see cref="IconStorage.RootFolderOverride"/>, so the real shared folder is never
    /// touched.
    /// </remarks>
    public class IconStorageTests
    {

        /// <summary>
        /// Path to the expected icons folder, relative to <see cref="PathEditorUtility.UserCachePath"/>.
        /// </summary>
        private const string DefaultSubfolder = "Sideways Experiments/Icons";

        private string _tempFolder = null;

        /// <summary>
        /// Creates an empty temporary folder, and uses it as the shared folder.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            // Normalized like the paths built by IconStorage (no-op outside of Windows, where temp paths have no backslash)
            _tempFolder = Path.Combine(Path.GetTempPath(), $"SideXP-{nameof(IconStorageTests)}-{Guid.NewGuid():N}").Replace('\\', '/');
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

        #region Root folder

        /// <summary>
        /// The override replaces the default shared folder.
        /// </summary>
        [Test]
        public void RootFolder_WithOverride_UsesOverride()
        {
            Assert.AreEqual(_tempFolder, IconStorage.RootFolder);
            Assert.AreEqual($"{_tempFolder}/index.json", IconStorage.IndexFilePath);
        }

        /// <summary>
        /// Without override, the folder set in the preferences is used (or the default one if none is set).
        /// </summary>
        [Test]
        public void RootFolder_WithoutOverride_UsesConfiguredFolder()
        {
            IconStorage.RootFolderOverride = null;

            Assert.AreEqual(IconStorage.ConfiguredRootFolder, IconStorage.RootFolder);
        }

        /// <summary>
        /// The default shared folder is the default relative folder, in the user's cache folder.
        /// </summary>
        [Test]
        public void DefaultRootFolder_IsDefaultSubfolder_InUserCache()
        {
            Assert.IsTrue(Path.IsPathRooted(IconStorage.DefaultRootFolder));
            Assert.AreEqual($"{PathEditorUtility.UserCachePath}/{DefaultSubfolder}", IconStorage.DefaultRootFolder);
        }

        #endregion


        #region Paths

        /// <summary>
        /// Only lowercase letters, digits and hyphens are allowed in icon names.
        /// </summary>
        [TestCase("home", true)]
        [TestCase("a-b-2", true)]
        [TestCase(null, false)]
        [TestCase("", false)]
        [TestCase("Home", false)]
        [TestCase("home_16", false)]
        [TestCase("arrow left", false)]
        [TestCase("../home", false)]
        [TestCase("home\n", false)]
        public void IsValidIconName_ChecksAllowedCharacters(string name, bool expected)
        {
            Assert.AreEqual(expected, IconStorage.IsValidIconName(name));
        }

        /// <summary>
        /// Category folder names are lowercase with hyphens, and empty categories go to the same folder as an "Other" category.
        /// </summary>
        [TestCase("Buildings", "buildings")]
        [TestCase("Version control", "version-control")]
        [TestCase("E-commerce", "e-commerce")]
        [TestCase("  Text  ", "text")]
        [TestCase("Arrows & Shapes", "arrows-shapes")]
        [TestCase("Other", "other")]
        [TestCase(null, "other")]
        [TestCase("", "other")]
        [TestCase("   ", "other")]
        [TestCase("???", "other")]
        public void GetCategoryFolderName_FormatsCategory(string category, string expected)
        {
            Assert.AreEqual(expected, IconStorage.GetCategoryFolderName(category));
        }

        /// <summary>
        /// Icon file names are made of the icon name, the size and the style.
        /// </summary>
        [TestCase("home", 16, EIconStyle.Outline, "home_16_outline.png")]
        [TestCase("a-b-2", 64, EIconStyle.Filled, "a-b-2_64_filled.png")]
        public void GetIconFileName_FormatsNameSizeAndStyle(string name, int pixelSize, EIconStyle style, string expected)
        {
            Assert.AreEqual(expected, IconStorage.GetIconFileName(name, pixelSize, style));
        }

        /// <summary>
        /// Icon files are stored in a folder per icon, in a folder per category.
        /// </summary>
        [Test]
        public void GetIconFilePath_IsInIconFolder_InCategoryFolder()
        {
            string expected = $"{_tempFolder}/version-control/git-branch/git-branch_24_outline.png";
            Assert.AreEqual(expected, IconStorage.GetIconFilePath("git-branch", "Version control", 24, EIconStyle.Outline));
        }

        /// <summary>
        /// Invalid icon names are rejected, so they can't point outside of the shared folder.
        /// </summary>
        [Test]
        public void GetIconFilePath_InvalidName_Throws()
        {
            Assert.Throws<ArgumentException>(() => IconStorage.GetIconFilePath("../home", "Buildings", 16, EIconStyle.Outline));
        }

        /// <summary>
        /// Icon files must have a strictly positive size.
        /// </summary>
        [Test]
        public void GetIconFileName_InvalidSize_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => IconStorage.GetIconFileName("home", 0, EIconStyle.Outline));
        }

        #endregion


        #region Downloaded icons

        /// <summary>
        /// Writes an icon file in the shared folder.
        /// </summary>
        /// <param name="name">The name of the icon.</param>
        /// <param name="category">The category of the icon, as written in the index.</param>
        /// <param name="pixelSize">The size of the icon file, in pixels.</param>
        /// <param name="style">The style of the icon.</param>
        /// <param name="bytes">The content of the file.</param>
        private static void WriteIconFile(string name, string category, int pixelSize, EIconStyle style, byte[] bytes)
        {
            FileEditorUtility.WriteFileAtomic(IconStorage.GetIconFilePath(name, category, pixelSize, style), bytes, false);
        }

        /// <summary>
        /// Creates an index file and the files of two icons, in two categories, plus temporary files left by interrupted writes.
        /// </summary>
        private void CreateDownloadedIcons()
        {
            File.WriteAllText(IconStorage.IndexFilePath, "{}");
            File.WriteAllText($"{IconStorage.IndexFilePath}.0123{FileEditorUtility.TempFileExtension}", "partial index");
            WriteIconFile("home", "Buildings", 16, EIconStyle.Outline, new byte[10]);
            WriteIconFile("home", "Buildings", 16, EIconStyle.Filled, new byte[20]);
            WriteIconFile("user", "System", 32, EIconStyle.Outline, new byte[30]);
            File.WriteAllText($"{IconStorage.GetIconFolderPath("user", "System")}/user_64_outline.png.0123.tmp", "partial icon");
        }

        /// <summary>
        /// A style of an icon is stored only if its file exists for every stored size.
        /// </summary>
        [Test]
        public void IsStored_RequiresEveryStoredSize()
        {
            Assert.IsFalse(IconStorage.IsStored("home", "Buildings", EIconStyle.Outline));

            foreach (int size in IconStorage.StoredSizes)
            {
                Assert.IsFalse(IconStorage.IsStored("home", "Buildings", EIconStyle.Outline));
                WriteIconFile("home", "Buildings", size, EIconStyle.Outline, new byte[1]);
            }

            Assert.IsTrue(IconStorage.IsStored("home", "Buildings", EIconStyle.Outline));
            Assert.IsFalse(IconStorage.IsStored("home", "Buildings", EIconStyle.Filled));
        }

        /// <summary>
        /// Only the icon files are measured: not the index, nor the temporary files.
        /// </summary>
        [Test]
        public void MeasureDownloadedIcons_CountsIconFilesOnly()
        {
            CreateDownloadedIcons();

            IconStorage.MeasureDownloadedIcons(out int fileCount, out long byteCount);

            Assert.AreEqual(3, fileCount);
            Assert.AreEqual(60, byteCount);
        }

        /// <summary>
        /// Without shared folder, there's nothing to measure.
        /// </summary>
        [Test]
        public void MeasureDownloadedIcons_MissingFolder_IsEmpty()
        {
            IconStorage.RootFolderOverride = $"{_tempFolder}/missing";

            IconStorage.MeasureDownloadedIcons(out int fileCount, out long byteCount);

            Assert.AreEqual(0, fileCount);
            Assert.AreEqual(0, byteCount);
        }

        /// <summary>
        /// Deleting the downloaded icons deletes every category folder and temporary file, but keeps the index.
        /// </summary>
        [Test]
        public void DeleteDownloadedIcons_DeletesIconsAndKeepsIndex()
        {
            CreateDownloadedIcons();

            List<string> errors = IconStorage.DeleteDownloadedIcons();

            Assert.IsEmpty(errors, string.Join("\n", errors));
            Assert.IsEmpty(Directory.GetDirectories(_tempFolder));
            CollectionAssert.AreEqual(new[] { IconStorage.IndexFilePath }, NormalizePaths(Directory.GetFiles(_tempFolder)));
        }

        /// <summary>
        /// Normalizes paths returned by the file system, so they can be compared with the paths built by <see cref="IconStorage"/>.
        /// </summary>
        /// <param name="paths">The paths to normalize.</param>
        /// <returns>Returns the normalized paths.</returns>
        private static string[] NormalizePaths(string[] paths)
        {
            return Array.ConvertAll(paths, IconStorage.NormalizePath);
        }

        #endregion

    }

}
