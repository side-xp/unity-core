using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;

using SideXP.Core.EditorOnly;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="IconIndexRefresher"/> and the file size display of <see cref="IconsMenu"/>.
    /// </summary>
    /// <remarks>
    /// Refreshes are not interactive, so no dialog blocks the tests. Downloads are replaced by <see cref="FakeWebDownload"/>, and
    /// every test works in a temporary folder set as <see cref="IconStorage.RootFolderOverride"/>.
    /// </remarks>
    public class IconIndexRefresherTests
    {

        /// <summary>
        /// The latest Tabler version, as answered by the fake server.
        /// </summary>
        private const string LatestVersion = "3.52.0";

        /// <summary>
        /// Maximum number of editor frames to wait for a refresh, so a broken refresh fails the test instead of waiting forever.
        /// </summary>
        private const int MaxFrames = 100;

        /// <summary>
        /// The catalog of <see cref="LatestVersion"/>, as answered by the fake server.
        /// </summary>
        private const string Catalog = @"{""home"":{""name"":""home"",""category"":""Buildings"",""tags"":[""house""]}}";

        private string _tempFolder = null;

        /// <summary>
        /// Whether the fake server fails the catalog download.
        /// </summary>
        private bool _isCatalogMissing = false;

        /// <summary>
        /// The URLs of the downloads started, in order.
        /// </summary>
        private List<string> _downloadedUrls = null;

        /// <summary>
        /// Creates an empty temporary folder as the shared folder, and replaces the downloads by a fake server.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            string tempFolderName = $"SideXP-{nameof(IconIndexRefresherTests)}-{Guid.NewGuid():N}";
            _tempFolder = IconStorage.NormalizePath(Path.Combine(Path.GetTempPath(), tempFolderName));
            Directory.CreateDirectory(_tempFolder);
            IconStorage.RootFolderOverride = _tempFolder;
            IconIndexStore.ClearMemory();

            _isCatalogMissing = false;
            _downloadedUrls = new List<string>();
            WebDownloader.DownloadOverride = url =>
            {
                _downloadedUrls.Add(url);
                if (url == TablerSource.LatestVersionUrl)
                    return Answer(url, $"{{\"version\":\"{LatestVersion}\"}}");

                if (url == TablerSource.GetCatalogUrl(LatestVersion) && !_isCatalogMissing)
                    return Answer(url, Catalog);

                return new FakeWebDownload(url, WebDownloadResult.Failure(EWebDownloadStatus.NotFound, "Not Found", 404));
            };
        }

        /// <summary>
        /// Restores the real downloads and the default shared folder, and deletes the temporary folder.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            WebDownloader.DownloadOverride = null;
            IconStorage.RootFolderOverride = null;
            IconIndexStore.ClearMemory();
            if (Directory.Exists(_tempFolder))
                Directory.Delete(_tempFolder, true);
        }

        /// <summary>
        /// Makes a successful fake download of a text.
        /// </summary>
        /// <param name="url">The URL of the downloaded file.</param>
        /// <param name="text">The downloaded text.</param>
        /// <returns>Returns the fake download.</returns>
        private static FakeWebDownload Answer(string url, string text)
        {
            return new FakeWebDownload(url, WebDownloadResult.Success(Encoding.UTF8.GetBytes(text)));
        }

        /// <summary>
        /// Waits for editor frames until no refresh is in progress.
        /// </summary>
        /// <returns>Returns the coroutine that waits.</returns>
        private static IEnumerator WaitForRefresh()
        {
            for (int i = 0; i < MaxFrames && IconIndexRefresher.IsRefreshing; i++)
                yield return null;

            Assert.IsFalse(IconIndexRefresher.IsRefreshing, "The refresh didn't complete.");
        }

        #region Refresher

        /// <summary>
        /// A refresh runs from editor updates, writes the index, and notifies its callback.
        /// </summary>
        /// <returns>Returns the test coroutine.</returns>
        [UnityTest]
        public IEnumerator Refresh_RunsFromEditorUpdates_AndNotifies()
        {
            IconIndexRefresh completedJob = null;
            IconIndexRefresher.Refresh(false, job => completedJob = job);
            Assert.IsTrue(IconIndexRefresher.IsRefreshing);

            yield return WaitForRefresh();

            Assert.IsNotNull(completedJob);
            Assert.IsTrue(completedJob.IsSuccess, completedJob.Error);
            Assert.IsTrue(IconIndexStore.TryGetIndex(out IconIndex index));
            Assert.AreEqual(LatestVersion, index.version);
        }

        /// <summary>
        /// Asking for a refresh while one is in progress waits for it, instead of starting another.
        /// </summary>
        /// <returns>Returns the test coroutine.</returns>
        [UnityTest]
        public IEnumerator Refresh_WhileRefreshing_JoinsCurrentRefresh()
        {
            IconIndexRefresh firstJob = null;
            IconIndexRefresh secondJob = null;
            IconIndexRefresher.Refresh(false, job => firstJob = job);
            IconIndexRefresher.Refresh(false, job => secondJob = job);

            yield return WaitForRefresh();

            Assert.IsNotNull(firstJob);
            Assert.AreSame(firstJob, secondJob);
            Assert.AreEqual(2, _downloadedUrls.Count);
        }

        /// <summary>
        /// A failed refresh that is not interactive is logged as a warning.
        /// </summary>
        /// <returns>Returns the test coroutine.</returns>
        [UnityTest]
        public IEnumerator Refresh_FailedAndNotInteractive_LogsWarning()
        {
            _isCatalogMissing = true;
            LogAssert.Expect(LogType.Warning, new Regex("couldn't be refreshed"));

            IconIndexRefresh completedJob = null;
            IconIndexRefresher.Refresh(false, job => completedJob = job);

            yield return WaitForRefresh();

            Assert.IsFalse(completedJob.IsSuccess);
            Assert.IsFalse(File.Exists(IconStorage.IndexFilePath));
        }

        #endregion


        #region Menus

        /// <summary>
        /// File sizes are displayed in the largest unit that keeps the value above 1.
        /// </summary>
        [TestCase(0, "0 B")]
        [TestCase(1023, "1023 B")]
        [TestCase(1024, "1 KB")]
        [TestCase(1536, "1.5 KB")]
        [TestCase(3600000, "3.4 MB")]
        [TestCase(5368709120, "5 GB")]
        public void FormatSize_UsesLargestUnit(long byteCount, string expected)
        {
            Assert.AreEqual(expected, IconsMenu.FormatSize(byteCount));
        }

        #endregion

    }

}
