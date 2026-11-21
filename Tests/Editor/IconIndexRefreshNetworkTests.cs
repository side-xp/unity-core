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
    /// Tests for <see cref="IconIndexRefresh"/> using the real Tabler files.
    /// </summary>
    /// <remarks>
    /// These tests need a network connection, so they only run when selected by hand. They work in a temporary folder set as
    /// <see cref="IconStorage.RootFolderOverride"/>, so the real shared folder is never touched.
    /// </remarks>
    [Explicit("Uses the network.")]
    [Category("Network")]
    public class IconIndexRefreshNetworkTests
    {

        /// <summary>
        /// Maximum duration of the refresh, in seconds.
        /// </summary>
        private const int TimeoutSeconds = 120;

        /// <summary>
        /// Minimum number of icons expected in the real catalog (5,491 in Tabler 3.52.0).
        /// </summary>
        private const int MinIconCount = 5000;

        private string _tempFolder = null;

        /// <summary>
        /// Creates an empty temporary folder, and uses it as the shared folder.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            string tempFolderName = $"SideXP-{nameof(IconIndexRefreshNetworkTests)}-{Guid.NewGuid():N}";
            _tempFolder = IconStorage.NormalizePath(Path.Combine(Path.GetTempPath(), tempFolderName));
            Directory.CreateDirectory(_tempFolder);
            IconStorage.RootFolderOverride = _tempFolder;
            IconDownloader.DownloadOverride = null;
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
        /// The real catalog is downloaded and converted into a valid index.
        /// </summary>
        /// <returns>Returns the test coroutine, which lets the editor update while the downloads run.</returns>
        [UnityTest]
        public IEnumerator Refresh_RealCatalog_WritesIndex()
        {
            var job = new IconIndexRefresh();
            job.Start();

            Stopwatch stopwatch = Stopwatch.StartNew();
            while (!job.IsDone && stopwatch.Elapsed.TotalSeconds < TimeoutSeconds)
            {
                job.Update();
                yield return null;
            }

            if (!job.IsDone)
                job.Cancel();

            Assert.IsTrue(job.IsSuccess, job.Error);
            IconIndexStore.ClearMemory();
            Assert.IsTrue(IconIndexStore.TryGetIndex(out IconIndex index));
            Assert.AreEqual(job.Version, index.version);
            Assert.GreaterOrEqual(index.icons.Length, MinIconCount);
            Assert.IsTrue(Array.TrueForAll(index.icons, icon => IconStorage.IsValidIconName(icon.name)));
        }

    }

}
