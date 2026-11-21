using System;
using System.Collections;
using System.Diagnostics;
using System.IO;

using NUnit.Framework;

using UnityEngine.TestTools;

using SideXP.Core.EditorOnly;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="IconDownloadQueue"/> using the real Tabler files.
    /// </summary>
    /// <remarks>
    /// These tests need a network connection, so they only run when selected by hand. They work in a temporary folder set as
    /// <see cref="IconStorage.RootFolderOverride"/>, so the real shared folder is never touched.
    /// </remarks>
    [Explicit("Uses the network.")]
    [Category("Network")]
    public class IconDownloadQueueNetworkTests
    {

        /// <summary>
        /// A Tabler version known to contain the <c>home</c> icon in both styles. Published npm versions never change.
        /// </summary>
        private const string Version = "3.52.0";

        /// <summary>
        /// Maximum duration of the downloads, in seconds.
        /// </summary>
        private const int TimeoutSeconds = 60;

        private string _tempFolder = null;

        /// <summary>
        /// Creates an empty temporary folder, and uses it as the shared folder.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            string tempFolderName = $"SideXP-{nameof(IconDownloadQueueNetworkTests)}-{Guid.NewGuid():N}";
            _tempFolder = IconStorage.NormalizePath(Path.Combine(Path.GetTempPath(), tempFolderName));
            Directory.CreateDirectory(_tempFolder);
            IconStorage.RootFolderOverride = _tempFolder;
            IconDownloader.DownloadOverride = null;
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
        /// The real <c>home</c> icon is downloaded, processed and stored in both styles.
        /// </summary>
        /// <returns>Returns the test coroutine, which lets the editor update while the downloads run.</returns>
        [UnityTest]
        public IEnumerator Request_RealIcon_StoresBothStyles()
        {
            var queue = new IconDownloadQueue(new IconRetryPolicy());
            var home = new IconIndexEntry { name = "home", category = "Buildings", hasFilled = true };
            bool? isStored = null;
            queue.Request(Version, home, EIconStyle.Outline, result => isStored = result);

            Stopwatch stopwatch = Stopwatch.StartNew();
            while (queue.Count > 0 && stopwatch.Elapsed.TotalSeconds < TimeoutSeconds)
            {
                queue.Update(stopwatch.Elapsed.TotalSeconds);
                yield return null;
            }

            queue.AbortDownloads();
            Assert.AreEqual(true, isStored);
            Assert.IsTrue(IconStorage.IsStored("home", "Buildings", EIconStyle.Outline));
            Assert.IsTrue(IconStorage.IsStored("home", "Buildings", EIconStyle.Filled));
        }

    }

}
