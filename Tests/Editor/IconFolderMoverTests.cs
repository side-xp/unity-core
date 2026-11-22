using System;
using System.IO;

using NUnit.Framework;

using SideXP.Core.EditorOnly;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="IconFolderMover"/>.
    /// </summary>
    /// <remarks>
    /// Every test works in its own temporary folder, so the real icons folder is never touched.
    /// </remarks>
    public class IconFolderMoverTests
    {

        /// <summary>
        /// Number of files above which the copy reports its progress more than once.
        /// </summary>
        private const int ManyFilesCount = 60;

        private string _tempFolder = null;
        private string _sourceFolder = null;
        private string _targetFolder = null;

        /// <summary>
        /// Creates an empty temporary folder, with paths for a source and a target icons folder in it.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            string tempFolderName = $"SideXP-{nameof(IconFolderMoverTests)}-{Guid.NewGuid():N}";
            _tempFolder = IconStorage.NormalizePath(Path.Combine(Path.GetTempPath(), tempFolderName));
            Directory.CreateDirectory(_tempFolder);
            _sourceFolder = $"{_tempFolder}/cache/SideXP/Icons";
            _targetFolder = $"{_tempFolder}/custom/Icons";
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
        /// Creates a file with the given content, and its missing folders.
        /// </summary>
        /// <param name="path">The absolute path to the file to create.</param>
        /// <param name="content">The content of the file.</param>
        private static void CreateFile(string path, string content)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, content);
        }

        /// <summary>
        /// Creates a source folder with an index and two icon files.
        /// </summary>
        private void CreateSourceIcons()
        {
            CreateFile($"{_sourceFolder}/index.json", "source index");
            CreateFile($"{_sourceFolder}/buildings/home/home_16_outline.png", "source home");
            CreateFile($"{_sourceFolder}/system/user/user_16_outline.png", "source user");
        }

        /// <summary>
        /// Checks that the target folder contains the files from <see cref="CreateSourceIcons"/>.
        /// </summary>
        private void AssertTargetHasSourceIcons()
        {
            Assert.AreEqual("source index", File.ReadAllText($"{_targetFolder}/index.json"));
            Assert.AreEqual("source home", File.ReadAllText($"{_targetFolder}/buildings/home/home_16_outline.png"));
            Assert.AreEqual("source user", File.ReadAllText($"{_targetFolder}/system/user/user_16_outline.png"));
        }

        #region Checks

        /// <summary>
        /// A missing or empty target folder is accepted.
        /// </summary>
        [Test]
        public void CanMove_MissingOrEmptyTarget_IsAccepted()
        {
            Assert.IsTrue(IconFolderMover.CanMove(_sourceFolder, _targetFolder, out _));

            Directory.CreateDirectory(_targetFolder);
            Assert.IsTrue(IconFolderMover.CanMove(_sourceFolder, _targetFolder, out _));
        }

        /// <summary>
        /// A non-empty target folder is accepted only if it's an icons folder.
        /// </summary>
        [Test]
        public void CanMove_NonEmptyTarget_IsAcceptedOnlyWithIndex()
        {
            CreateFile($"{_targetFolder}/notes.txt", "user file");
            Assert.IsFalse(IconFolderMover.CanMove(_sourceFolder, _targetFolder, out string error));
            Assert.IsNotNull(error);

            CreateFile($"{_targetFolder}/index.json", "target index");
            Assert.IsTrue(IconFolderMover.CanMove(_sourceFolder, _targetFolder, out _));
        }

        /// <summary>
        /// The icons can't be moved to the same folder, even written differently.
        /// </summary>
        [Test]
        public void CanMove_SameFolder_IsRefused()
        {
            Assert.IsFalse(IconFolderMover.CanMove(_sourceFolder, $"{_sourceFolder}/", out _));
            Assert.IsFalse(IconFolderMover.CanMove(_sourceFolder, $"{_sourceFolder}/../Icons", out _));
        }

        /// <summary>
        /// The icons can't be moved into a subfolder of the icons folder, or into one of its parents.
        /// </summary>
        [Test]
        public void CanMove_NestedFolders_IsRefused()
        {
            Assert.IsFalse(IconFolderMover.CanMove(_sourceFolder, $"{_sourceFolder}/sub", out _));

            // The parent is not empty since it contains the source folder, but it's refused even with an index
            CreateFile($"{_tempFolder}/cache/SideXP/index.json", "parent index");
            Assert.IsFalse(IconFolderMover.CanMove(_sourceFolder, $"{_tempFolder}/cache/SideXP", out _));
        }

        /// <summary>
        /// The root of a drive can't be used as the icons folder.
        /// </summary>
        [Test]
        public void CanMove_DriveRoot_IsRefused()
        {
            Assert.IsFalse(IconFolderMover.CanMove(_sourceFolder, Path.GetPathRoot(_tempFolder), out _));
        }

        #endregion


        #region Move

        /// <summary>
        /// Moving to a missing folder moves every file, and deletes the source folder.
        /// </summary>
        [Test]
        public void Move_ToMissingFolder_MovesFilesAndDeletesSource()
        {
            CreateSourceIcons();

            Assert.IsTrue(IconFolderMover.Move(_sourceFolder, _targetFolder, 0, null, out string error), error);

            AssertTargetHasSourceIcons();
            Assert.IsFalse(Directory.Exists(_sourceFolder));
        }

        /// <summary>
        /// Moving to an existing empty folder copies every file, and deletes the source folder.
        /// </summary>
        [Test]
        public void Move_ToEmptyFolder_CopiesFilesAndDeletesSource()
        {
            CreateSourceIcons();
            Directory.CreateDirectory(_targetFolder);

            Assert.IsTrue(IconFolderMover.Move(_sourceFolder, _targetFolder, 0, null, out string error), error);

            AssertTargetHasSourceIcons();
            Assert.IsFalse(Directory.Exists(_sourceFolder));
        }

        /// <summary>
        /// Moving to an existing icons folder merges the files, and keeps the existing ones.
        /// </summary>
        [Test]
        public void Move_ToIconsFolder_MergesAndKeepsExistingFiles()
        {
            CreateSourceIcons();
            CreateFile($"{_targetFolder}/index.json", "target index");
            CreateFile($"{_targetFolder}/buildings/home/home_16_outline.png", "target home");

            Assert.IsTrue(IconFolderMover.Move(_sourceFolder, _targetFolder, 0, null, out string error), error);

            Assert.AreEqual("target index", File.ReadAllText($"{_targetFolder}/index.json"));
            Assert.AreEqual("target home", File.ReadAllText($"{_targetFolder}/buildings/home/home_16_outline.png"));
            Assert.AreEqual("source user", File.ReadAllText($"{_targetFolder}/system/user/user_16_outline.png"));
            Assert.IsFalse(Directory.Exists(_sourceFolder));
        }

        /// <summary>
        /// Temporary files left by an interrupted write are not copied, but are deleted with the source folder.
        /// </summary>
        [Test]
        public void Move_WithTempFiles_SkipsAndDeletesThem()
        {
            CreateSourceIcons();
            CreateFile($"{_sourceFolder}/buildings/home/home_24_outline.png.0123{FileEditorUtility.TempFileExtension}", "partial");
            Directory.CreateDirectory(_targetFolder);

            Assert.IsTrue(IconFolderMover.Move(_sourceFolder, _targetFolder, 0, null, out string error), error);

            string[] tempFiles = Directory.GetFiles(_targetFolder, $"*{FileEditorUtility.TempFileExtension}", SearchOption.AllDirectories);
            Assert.IsEmpty(tempFiles);
            Assert.IsFalse(Directory.Exists(_sourceFolder));
        }

        /// <summary>
        /// Without source folder, there's nothing to move, and the move succeeds.
        /// </summary>
        [Test]
        public void Move_MissingSource_Succeeds()
        {
            Assert.IsTrue(IconFolderMover.Move(_sourceFolder, _targetFolder, 0, null, out string error), error);
        }

        /// <summary>
        /// A cancelled move deletes the copied files, and keeps the source folder as is.
        /// </summary>
        [Test]
        public void Move_Cancelled_DeletesCopiedFilesAndKeepsSource()
        {
            for (int i = 0; i < ManyFilesCount; i++)
                CreateFile($"{_sourceFolder}/shapes/icon-{i}/icon-{i}_16_outline.png", $"icon {i}");

            Directory.CreateDirectory(_targetFolder);

            // Cancel on the second progress report, so some files have already been copied
            bool isMoved = IconFolderMover.Move(_sourceFolder, _targetFolder, 0, (copied, _) => copied > 0, out string error);

            Assert.IsFalse(isMoved);
            Assert.IsNotNull(error);
            Assert.IsTrue(Directory.Exists(_targetFolder));
            Assert.IsEmpty(Directory.GetFileSystemEntries(_targetFolder));
            Assert.AreEqual(ManyFilesCount, Directory.GetFiles(_sourceFolder, "*", SearchOption.AllDirectories).Length);
        }

        /// <summary>
        /// The empty parents of the source folder are deleted, up to the given count.
        /// </summary>
        [Test]
        public void Move_WithEmptyParentsToDelete_DeletesEmptyParents()
        {
            CreateSourceIcons();

            Assert.IsTrue(IconFolderMover.Move(_sourceFolder, _targetFolder, 1, null, out string error), error);

            Assert.IsFalse(Directory.Exists($"{_tempFolder}/cache/SideXP"));
            Assert.IsTrue(Directory.Exists($"{_tempFolder}/cache"));
        }

        /// <summary>
        /// A parent of the source folder that is not empty is kept.
        /// </summary>
        [Test]
        public void Move_WithNonEmptyParent_KeepsParent()
        {
            CreateSourceIcons();
            CreateFile($"{_tempFolder}/cache/SideXP/other-tool.json", "other tool");

            Assert.IsTrue(IconFolderMover.Move(_sourceFolder, _targetFolder, 1, null, out string error), error);

            Assert.IsTrue(File.Exists($"{_tempFolder}/cache/SideXP/other-tool.json"));
        }

        #endregion

    }

}
