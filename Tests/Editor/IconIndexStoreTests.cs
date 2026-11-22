using System;
using System.IO;

using NUnit.Framework;

using UnityEngine;

using SideXP.Core.EditorOnly;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="IconIndexStore"/>.
    /// </summary>
    /// <remarks>
    /// Every test works in a temporary folder set as <see cref="IconStorage.RootFolderOverride"/>, so the real shared folder is never
    /// touched.
    /// </remarks>
    public class IconIndexStoreTests
    {

        private string _tempFolder = null;

        /// <summary>
        /// Creates an empty temporary folder, and uses it as the shared folder.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            string tempFolderName = $"SideXP-{nameof(IconIndexStoreTests)}-{Guid.NewGuid():N}";
            _tempFolder = IconStorage.NormalizePath(Path.Combine(Path.GetTempPath(), tempFolderName));
            Directory.CreateDirectory(_tempFolder);
            IconStorage.RootFolderOverride = _tempFolder;
            IconIndexStore.ClearMemory();
        }

        /// <summary>
        /// Restores the default shared folder, and deletes the temporary folder.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            IconStorage.RootFolderOverride = null;
            IconIndexStore.ClearMemory();
            if (Directory.Exists(_tempFolder))
                Directory.Delete(_tempFolder, true);
        }

        /// <summary>
        /// Makes an index with a single icon.
        /// </summary>
        /// <param name="version">The Tabler version of the index.</param>
        /// <returns>Returns the created index.</returns>
        private static IconIndex MakeIndex(string version)
        {
            return new IconIndex
            {
                version = version,
                icons = new[] { new IconIndexEntry { name = "home", category = "Buildings", tags = new[] { "house" } } },
            };
        }

        /// <summary>
        /// Without index file, there's no index.
        /// </summary>
        [Test]
        public void TryGetIndex_WithoutFile_ReturnsFalse()
        {
            Assert.IsFalse(IconIndexStore.TryGetIndex(out IconIndex index));
            Assert.IsNull(index);
        }

        /// <summary>
        /// A written index is read back, from memory and from the file.
        /// </summary>
        [Test]
        public void Write_ThenTryGetIndex_ReturnsIndex()
        {
            IconIndexStore.Write(MakeIndex("3.52.0"));

            Assert.IsTrue(IconIndexStore.TryGetIndex(out IconIndex index));
            Assert.AreEqual("3.52.0", index.version);

            IconIndexStore.ClearMemory();
            Assert.IsTrue(IconIndexStore.TryGetIndex(out index));
            Assert.AreEqual("3.52.0", index.version);
            Assert.AreEqual("home", index.icons[0].name);
        }

        /// <summary>
        /// An index file that can't be read or is not valid is ignored.
        /// </summary>
        [TestCase("not json")]
        [TestCase("{}")]
        [TestCase(@"{""version"":""latest"",""icons"":[]}")]
        public void TryGetIndex_InvalidFile_ReturnsFalse(string content)
        {
            File.WriteAllText(IconStorage.IndexFilePath, content);

            Assert.IsFalse(IconIndexStore.TryGetIndex(out IconIndex index));
            Assert.IsNull(index);
        }

        /// <summary>
        /// An index file changed by another editor is read again.
        /// </summary>
        [Test]
        public void TryGetIndex_FileChanged_ReadsItAgain()
        {
            IconIndexStore.Write(MakeIndex("3.51.0"));
            Assert.IsTrue(IconIndexStore.TryGetIndex(out _));

            File.WriteAllText(IconStorage.IndexFilePath, JsonUtility.ToJson(MakeIndex("3.52.0")));
            // Set explicitly, since file times may not change between two writes made so close
            File.SetLastWriteTimeUtc(IconStorage.IndexFilePath, DateTime.UtcNow.AddMinutes(1));

            Assert.IsTrue(IconIndexStore.TryGetIndex(out IconIndex index));
            Assert.AreEqual("3.52.0", index.version);
        }

        /// <summary>
        /// An index that is not valid can't be written.
        /// </summary>
        [Test]
        public void Write_InvalidIndex_Throws()
        {
            Assert.Throws<ArgumentException>(() => IconIndexStore.Write(MakeIndex("latest")));
            Assert.Throws<ArgumentException>(() => IconIndexStore.Write(null));
            Assert.IsFalse(File.Exists(IconStorage.IndexFilePath));
        }

    }

}
