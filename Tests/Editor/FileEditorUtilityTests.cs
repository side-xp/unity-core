using System;
using System.IO;
using System.Text;

using NUnit.Framework;

using SideXP.Core.EditorOnly;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="FileEditorUtility"/>.
    /// </summary>
    /// <remarks>
    /// Every test works in its own temporary folder, deleted after the test.
    /// </remarks>
    public class FileEditorUtilityTests
    {

        private string _tempFolder = null;

        /// <summary>
        /// Creates an empty temporary folder.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _tempFolder = Path.Combine(Path.GetTempPath(), $"SideXP-{nameof(FileEditorUtilityTests)}-{Guid.NewGuid():N}");
            Directory.CreateDirectory(_tempFolder);
        }

        /// <summary>
        /// Deletes the temporary folder.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempFolder))
                Directory.Delete(_tempFolder, true);
        }

        /// <summary>
        /// Checks that no temporary file is left in the temporary folder.
        /// </summary>
        private void AssertNoTempFile()
        {
            string[] tempFiles = Directory.GetFiles(_tempFolder, $"*{FileEditorUtility.TempFileExtension}", SearchOption.AllDirectories);
            Assert.IsEmpty(tempFiles);
        }

        #region Write

        /// <summary>
        /// A new file is written with its content, and its missing folders are created.
        /// </summary>
        [Test]
        public void WriteFileAtomic_NewFile_WritesFileAndFolders()
        {
            string path = Path.Combine(_tempFolder, "a", "b", "file.bin");
            byte[] bytes = { 1, 2, 3 };

            bool isWritten = FileEditorUtility.WriteFileAtomic(path, bytes, false);

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
            string path = Path.Combine(_tempFolder, "file.bin");
            byte[] existingBytes = { 1, 2, 3 };
            File.WriteAllBytes(path, existingBytes);

            bool isWritten = FileEditorUtility.WriteFileAtomic(path, new byte[] { 4, 5, 6 }, false);

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
            string path = Path.Combine(_tempFolder, "file.bin");
            File.WriteAllBytes(path, new byte[] { 1, 2, 3 });
            byte[] newBytes = { 4, 5, 6 };

            bool isWritten = FileEditorUtility.WriteFileAtomic(path, newBytes, true);

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
            string path = Path.Combine(_tempFolder, "file.json");
            string text = "{\"version\":\"3.52.0\",\"name\":\"é\"}";

            FileEditorUtility.WriteFileAtomic(path, text, true);

            CollectionAssert.AreEqual(new UTF8Encoding(false).GetBytes(text), File.ReadAllBytes(path));
            AssertNoTempFile();
        }

        #endregion


        #region Delete

        /// <summary>
        /// An existing file is deleted.
        /// </summary>
        [Test]
        public void TryDeleteFile_ExistingFile_DeletesFile()
        {
            string path = Path.Combine(_tempFolder, "file.bin");
            File.WriteAllBytes(path, new byte[] { 1 });

            Assert.IsTrue(FileEditorUtility.TryDeleteFile(path));
            Assert.IsFalse(File.Exists(path));
        }

        /// <summary>
        /// A missing file counts as deleted.
        /// </summary>
        [Test]
        public void TryDeleteFile_MissingFile_IsTrue()
        {
            Assert.IsTrue(FileEditorUtility.TryDeleteFile(Path.Combine(_tempFolder, "missing.bin")));
        }

        /// <summary>
        /// An empty folder is deleted.
        /// </summary>
        [Test]
        public void TryDeleteEmptyFolder_EmptyFolder_DeletesFolder()
        {
            string folder = Path.Combine(_tempFolder, "empty");
            Directory.CreateDirectory(folder);

            Assert.IsTrue(FileEditorUtility.TryDeleteEmptyFolder(folder));
            Assert.IsFalse(Directory.Exists(folder));
        }

        /// <summary>
        /// A folder that contains a file or a subfolder is kept.
        /// </summary>
        [Test]
        public void TryDeleteEmptyFolder_NotEmpty_KeepsFolder()
        {
            string folderWithFile = Path.Combine(_tempFolder, "with-file");
            Directory.CreateDirectory(folderWithFile);
            File.WriteAllBytes(Path.Combine(folderWithFile, "file.bin"), new byte[] { 1 });

            string folderWithSubfolder = Path.Combine(_tempFolder, "with-subfolder");
            Directory.CreateDirectory(Path.Combine(folderWithSubfolder, "subfolder"));

            Assert.IsFalse(FileEditorUtility.TryDeleteEmptyFolder(folderWithFile));
            Assert.IsFalse(FileEditorUtility.TryDeleteEmptyFolder(folderWithSubfolder));
            Assert.IsTrue(Directory.Exists(folderWithFile));
            Assert.IsTrue(Directory.Exists(folderWithSubfolder));
        }

        /// <summary>
        /// A missing folder counts as deleted.
        /// </summary>
        [Test]
        public void TryDeleteEmptyFolder_MissingFolder_IsTrue()
        {
            Assert.IsTrue(FileEditorUtility.TryDeleteEmptyFolder(Path.Combine(_tempFolder, "missing")));
        }

        #endregion


        #region Entries

        /// <summary>
        /// A folder has entries only if it exists and contains a file or a subfolder.
        /// </summary>
        [Test]
        public void HasEntries_ChecksFilesAndSubfolders()
        {
            string folder = Path.Combine(_tempFolder, "folder");
            Assert.IsFalse(FileEditorUtility.HasEntries(folder));

            Directory.CreateDirectory(folder);
            Assert.IsFalse(FileEditorUtility.HasEntries(folder));

            Directory.CreateDirectory(Path.Combine(folder, "subfolder"));
            Assert.IsTrue(FileEditorUtility.HasEntries(folder));
        }

        #endregion

    }

}
