using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using NUnit.Framework;

using UnityEngine;

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
        /// Path to the expected icons folder, relative to the user's cache folder.
        /// </summary>
        private const string DefaultSubfolder = "SideXP/Icons";

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

        /// <summary>
        /// Makes a function that reads environment variables from a dictionary instead of the real environment.
        /// </summary>
        /// <param name="variables">The environment variables, as name and value pairs.</param>
        /// <returns>Returns the function to pass to <see cref="IconStorage.ResolveRootFolder"/>.</returns>
        private static Func<string, string> MakeEnvironment(params (string name, string value)[] variables)
        {
            var dictionary = new Dictionary<string, string>();
            foreach ((string name, string value) in variables)
                dictionary[name] = value;

            return name => dictionary.TryGetValue(name, out string value) ? value : null;
        }

        /// <summary>
        /// Checks that no temporary file is left in the temporary folder.
        /// </summary>
        private void AssertNoTempFile()
        {
            string[] tempFiles = Directory.GetFiles(_tempFolder, $"*{IconStorage.TempFileExtension}", SearchOption.AllDirectories);
            Assert.IsEmpty(tempFiles);
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
        /// The default shared folder is an absolute path ending with the default relative folder.
        /// </summary>
        [Test]
        public void DefaultRootFolder_IsAbsolute_EndsWithDefaultSubfolder()
        {
            Assert.IsTrue(Path.IsPathRooted(IconStorage.DefaultRootFolder));
            StringAssert.EndsWith(DefaultSubfolder, IconStorage.DefaultRootFolder);
        }

        /// <summary>
        /// On Windows, the shared folder is in the local application data folder, with backslashes replaced by slashes.
        /// </summary>
        [Test]
        public void ResolveRootFolder_Windows_UsesLocalAppData()
        {
            // Rooted on the current OS, so the test also works when run from a macOS or Linux editor
            string localAppData = Path.Combine(Path.GetPathRoot(Path.GetTempPath()), "Users", "someone", "AppData", "Local");
            Func<string, string> environment = MakeEnvironment(("LOCALAPPDATA", localAppData));

            string rootFolder = IconStorage.ResolveRootFolder(RuntimePlatform.WindowsEditor, environment);

            Assert.AreEqual($"{localAppData.Replace('\\', '/')}/{DefaultSubfolder}", rootFolder);
            StringAssert.DoesNotContain("\\", rootFolder);
        }

        /// <summary>
        /// On Windows, a trailing separator in the local application data folder doesn't make a double separator.
        /// </summary>
        [Test]
        public void ResolveRootFolder_WindowsWithTrailingSeparator_HasNoDoubleSeparator()
        {
            string localAppData = Path.Combine(Path.GetPathRoot(Path.GetTempPath()), "Users", "someone", "AppData", "Local");
            Func<string, string> environment = MakeEnvironment(("LOCALAPPDATA", $"{localAppData}\\"));

            string rootFolder = IconStorage.ResolveRootFolder(RuntimePlatform.WindowsEditor, environment);

            Assert.AreEqual($"{localAppData.Replace('\\', '/')}/{DefaultSubfolder}", rootFolder);
        }

        /// <summary>
        /// On macOS, the shared folder is in the user's caches folder.
        /// </summary>
        [Test]
        public void ResolveRootFolder_MacOS_UsesLibraryCaches()
        {
            string home = "/Users/someone";
            Func<string, string> environment = MakeEnvironment(("HOME", home));

            string rootFolder = IconStorage.ResolveRootFolder(RuntimePlatform.OSXEditor, environment);

            Assert.AreEqual($"{home}/Library/Caches/{DefaultSubfolder}", rootFolder);
        }

        /// <summary>
        /// On Linux, the shared folder is in the folder defined by XDG_CACHE_HOME, when defined.
        /// </summary>
        [Test]
        public void ResolveRootFolder_LinuxWithXdgCacheHome_UsesXdgCacheHome()
        {
            string xdgCacheHome = "/home/someone/custom-cache";
            Func<string, string> environment = MakeEnvironment(("XDG_CACHE_HOME", xdgCacheHome), ("HOME", "/home/someone"));

            string rootFolder = IconStorage.ResolveRootFolder(RuntimePlatform.LinuxEditor, environment);

            Assert.AreEqual($"{xdgCacheHome}/{DefaultSubfolder}", rootFolder);
        }

        /// <summary>
        /// On Linux, the shared folder is in ~/.cache when XDG_CACHE_HOME is not defined.
        /// </summary>
        [Test]
        public void ResolveRootFolder_LinuxWithoutXdgCacheHome_UsesDotCache()
        {
            string home = "/home/someone";
            Func<string, string> environment = MakeEnvironment(("HOME", home));

            string rootFolder = IconStorage.ResolveRootFolder(RuntimePlatform.LinuxEditor, environment);

            Assert.AreEqual($"{home}/.cache/{DefaultSubfolder}", rootFolder);
        }

        /// <summary>
        /// On Linux, a relative XDG_CACHE_HOME is ignored, as required by the XDG Base Directory specification.
        /// </summary>
        [Test]
        public void ResolveRootFolder_LinuxWithRelativeXdgCacheHome_UsesDotCache()
        {
            string home = "/home/someone";
            Func<string, string> environment = MakeEnvironment(("XDG_CACHE_HOME", "relative/cache"), ("HOME", home));

            string rootFolder = IconStorage.ResolveRootFolder(RuntimePlatform.LinuxEditor, environment);

            Assert.AreEqual($"{home}/.cache/{DefaultSubfolder}", rootFolder);
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


        #region Files

        /// <summary>
        /// A new file is written with its content, and its missing folders are created.
        /// </summary>
        [Test]
        public void WriteFileAtomic_NewFile_WritesFileAndFolders()
        {
            string path = Path.Combine(_tempFolder, "buildings", "home", "home_16_outline.png");
            byte[] bytes = { 1, 2, 3 };

            bool isWritten = IconStorage.WriteFileAtomic(path, bytes, false);

            Assert.IsTrue(isWritten);
            CollectionAssert.AreEqual(bytes, File.ReadAllBytes(path));
            AssertNoTempFile();
        }

        /// <summary>
        /// Without overwrite, an existing file is kept as is, as if another editor had just written it.
        /// </summary>
        [Test]
        public void WriteFileAtomic_TargetExists_KeepsExistingFile()
        {
            string path = Path.Combine(_tempFolder, "file.png");
            byte[] existingBytes = { 1, 2, 3 };
            File.WriteAllBytes(path, existingBytes);

            bool isWritten = IconStorage.WriteFileAtomic(path, new byte[] { 4, 5, 6 }, false);

            Assert.IsFalse(isWritten);
            CollectionAssert.AreEqual(existingBytes, File.ReadAllBytes(path));
            AssertNoTempFile();
        }

        /// <summary>
        /// With overwrite, an existing file is replaced.
        /// </summary>
        [Test]
        public void WriteFileAtomic_TargetExistsWithOverwrite_ReplacesFile()
        {
            string path = Path.Combine(_tempFolder, "file.png");
            File.WriteAllBytes(path, new byte[] { 1, 2, 3 });
            byte[] newBytes = { 4, 5, 6 };

            bool isWritten = IconStorage.WriteFileAtomic(path, newBytes, true);

            Assert.IsTrue(isWritten);
            CollectionAssert.AreEqual(newBytes, File.ReadAllBytes(path));
            AssertNoTempFile();
        }

        /// <summary>
        /// Text files are written in UTF-8 without BOM.
        /// </summary>
        [Test]
        public void WriteFileAtomic_Text_WritesUtf8WithoutBom()
        {
            string path = IconStorage.IndexFilePath;
            string text = "{\"version\":\"3.52.0\",\"name\":\"é\"}";

            IconStorage.WriteFileAtomic(path, text, true);

            CollectionAssert.AreEqual(new UTF8Encoding(false).GetBytes(text), File.ReadAllBytes(path));
            AssertNoTempFile();
        }

        #endregion


        #region Downloaded icons

        /// <summary>
        /// Creates an index file and the files of two icons, in two categories, plus temporary files left by interrupted writes.
        /// </summary>
        private void CreateDownloadedIcons()
        {
            File.WriteAllText(IconStorage.IndexFilePath, "{}");
            File.WriteAllText($"{IconStorage.IndexFilePath}.0123{IconStorage.TempFileExtension}", "partial index");
            IconStorage.WriteFileAtomic(IconStorage.GetIconFilePath("home", "Buildings", 16, EIconStyle.Outline), new byte[10], false);
            IconStorage.WriteFileAtomic(IconStorage.GetIconFilePath("home", "Buildings", 16, EIconStyle.Filled), new byte[20], false);
            IconStorage.WriteFileAtomic(IconStorage.GetIconFilePath("user", "System", 32, EIconStyle.Outline), new byte[30], false);
            File.WriteAllText($"{IconStorage.GetIconFolderPath("user", "System")}/user_64_outline.png.0123.tmp", "partial icon");
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
