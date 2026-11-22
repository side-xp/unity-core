using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;

using SideXP.Core.EditorOnly;

using Object = UnityEngine.Object;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="IconDownloadQueue"/>.
    /// </summary>
    /// <remarks>
    /// Downloads are replaced by <see cref="FakeWebDownload"/>, time is given by the tests, and every test works in a temporary
    /// folder set as <see cref="IconStorage.RootFolderOverride"/>. Processing uses the GPU, so these tests work in the editor, but not
    /// in batch mode with <c>-nographics</c>.
    /// </remarks>
    public class IconDownloadQueueTests
    {

        /// <summary>
        /// Tabler version of the requested icons.
        /// </summary>
        private const string Version = "3.52.0";

        /// <summary>
        /// Size of the generated PNG files, in pixels.
        /// </summary>
        private const int PngSize = 32;

        /// <summary>
        /// Small duration used to check times right before a delay ends, in seconds.
        /// </summary>
        private const double Epsilon = 0.01;

        /// <summary>
        /// An icon that exists in both styles.
        /// </summary>
        private static readonly IconIndexEntry Home = new IconIndexEntry { name = "home", category = "Buildings", hasFilled = true };

        /// <summary>
        /// An icon that only exists in the outline style.
        /// </summary>
        private static readonly IconIndexEntry User = new IconIndexEntry { name = "user", category = "System", hasFilled = false };

        /// <summary>
        /// A valid PNG file, as answered by the fake server.
        /// </summary>
        private static byte[] s_png = null;

        private string _tempFolder = null;
        private DownloadRetryPolicy _policy = null;
        private IconDownloadQueue _queue = null;

        /// <summary>
        /// The keys of the items given up by the queue.
        /// </summary>
        private List<string> _givenUpKeys = null;

        /// <summary>
        /// The downloads started by the queue, in order.
        /// </summary>
        private List<FakeWebDownload> _downloads = null;

        /// <summary>
        /// Answers a download request of the queue. Answers with a valid PNG by default.
        /// </summary>
        private Func<string, FakeWebDownload> _answer = null;

        /// <summary>
        /// Generates the PNG answered by the fake server.
        /// </summary>
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var texture = new Texture2D(PngSize, PngSize, TextureFormat.RGBA32, false, true);
            texture.SetPixels32(new Color32[PngSize * PngSize]);
            s_png = texture.EncodeToPNG();
            Object.DestroyImmediate(texture);
        }

        /// <summary>
        /// Creates an empty temporary folder as the shared folder, a new queue, and replaces the downloads by a fake server.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            string tempFolderName = $"SideXP-{nameof(IconDownloadQueueTests)}-{Guid.NewGuid():N}";
            _tempFolder = IconStorage.NormalizePath(Path.Combine(Path.GetTempPath(), tempFolderName));
            Directory.CreateDirectory(_tempFolder);
            IconStorage.RootFolderOverride = _tempFolder;

            _givenUpKeys = new List<string>();
            _policy = new DownloadRetryPolicy();
            _queue = new IconDownloadQueue(_policy, _givenUpKeys.Add);

            _downloads = new List<FakeWebDownload>();
            _answer = Success;
            WebDownloader.DownloadOverride = url =>
            {
                FakeWebDownload download = _answer(url);
                _downloads.Add(download);
                return download;
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
            if (Directory.Exists(_tempFolder))
                Directory.Delete(_tempFolder, true);
        }

        /// <summary>
        /// Answers with a valid PNG.
        /// </summary>
        /// <param name="url">The requested URL.</param>
        /// <returns>Returns the fake download.</returns>
        private static FakeWebDownload Success(string url)
        {
            return new FakeWebDownload(url, WebDownloadResult.Success(s_png));
        }

        /// <summary>
        /// Answers with a failure.
        /// </summary>
        /// <param name="url">The requested URL.</param>
        /// <param name="status">The outcome of the download.</param>
        /// <returns>Returns the fake download.</returns>
        private static FakeWebDownload Failure(string url, EWebDownloadStatus status)
        {
            return new FakeWebDownload(url, WebDownloadResult.Failure(status, status.ToString()));
        }

        /// <summary>
        /// Stores the files of a style of an icon in every size, as if it had been downloaded before.
        /// </summary>
        /// <param name="entry">The index entry of the icon.</param>
        /// <param name="style">The style to store.</param>
        private static void Store(IconIndexEntry entry, EIconStyle style)
        {
            foreach (int size in IconStorage.StoredSizes)
                FileEditorUtility.WriteFileAtomic(IconStorage.GetIconFilePath(entry.name, entry.category, size, style), s_png, false);
        }

        /// <summary>
        /// Updates the queue twice at the same time: once to start the downloads, once to handle them (fake downloads are done
        /// right away).
        /// </summary>
        /// <param name="time">The current time, in seconds.</param>
        private void UpdateTwice(double time)
        {
            _queue.Update(time);
            _queue.Update(time);
        }

        #region Requests

        /// <summary>
        /// Requesting an icon downloads and stores both its styles, from the given version.
        /// </summary>
        [Test]
        public void Request_DownloadsAndStoresBothStyles()
        {
            bool? isStored = null;
            _queue.Request(Version, Home, EIconStyle.Outline, result => isStored = result);
            Assert.IsNull(isStored);

            UpdateTwice(0);

            Assert.AreEqual(true, isStored);
            Assert.AreEqual(0, _queue.Count);
            Assert.IsTrue(IconStorage.IsStored(Home.name, Home.category, EIconStyle.Outline));
            Assert.IsTrue(IconStorage.IsStored(Home.name, Home.category, EIconStyle.Filled));
            string[] expectedUrls =
            {
                TablerSource.GetIconUrl(Version, "home", EIconStyle.Outline),
                TablerSource.GetIconUrl(Version, "home", EIconStyle.Filled),
            };
            CollectionAssert.AreEquivalent(expectedUrls, _downloads.ConvertAll(download => download.Url));
        }

        /// <summary>
        /// Requesting the filled style of an icon without filled style downloads its outline style instead.
        /// </summary>
        [Test]
        public void Request_FilledWithoutFilledStyle_DownloadsOutline()
        {
            bool? isStored = null;
            _queue.Request(Version, User, EIconStyle.Filled, result => isStored = result);

            UpdateTwice(0);

            Assert.AreEqual(true, isStored);
            Assert.AreEqual(1, _downloads.Count);
            Assert.AreEqual(TablerSource.GetIconUrl(Version, "user", EIconStyle.Outline), _downloads[0].Url);
        }

        /// <summary>
        /// Requesting an icon already stored completes right away, without download.
        /// </summary>
        [Test]
        public void Request_AlreadyStored_CompletesRightAway()
        {
            Store(Home, EIconStyle.Outline);
            Store(Home, EIconStyle.Filled);

            bool? isStored = null;
            _queue.Request(Version, Home, EIconStyle.Outline, result => isStored = result);

            Assert.AreEqual(true, isStored);
            Assert.AreEqual(0, _queue.Count);
            _queue.Update(0);
            Assert.IsEmpty(_downloads);
        }

        /// <summary>
        /// Only the styles with missing files are downloaded.
        /// </summary>
        [Test]
        public void Request_OneStyleStored_DownloadsOtherStyleOnly()
        {
            Store(Home, EIconStyle.Outline);

            bool? isStored = null;
            _queue.Request(Version, Home, EIconStyle.Outline, result => isStored = result);
            UpdateTwice(0);

            Assert.AreEqual(true, isStored);
            Assert.AreEqual(1, _downloads.Count);
            Assert.AreEqual(TablerSource.GetIconUrl(Version, "home", EIconStyle.Filled), _downloads[0].Url);
        }

        /// <summary>
        /// Requesting the same icon twice downloads it once, and notifies both requests.
        /// </summary>
        [Test]
        public void Request_SameIconTwice_DownloadsOnce()
        {
            int notifiedCount = 0;
            _queue.Request(Version, Home, EIconStyle.Outline, _ => notifiedCount++);
            _queue.Request(Version, Home, EIconStyle.Outline, _ => notifiedCount++);

            UpdateTwice(0);

            Assert.AreEqual(2, notifiedCount);
            Assert.AreEqual(2, _downloads.Count);
        }

        /// <summary>
        /// No more downloads than the maximum run at the same time.
        /// </summary>
        [Test]
        public void Update_ManyRequests_LimitsConcurrentDownloads()
        {
            _answer = url => new FakeWebDownload(url, WebDownloadResult.Success(s_png), true);
            int requestCount = IconDownloadQueue.MaxConcurrentDownloads + 2;
            for (int i = 0; i < requestCount; i++)
                _queue.Request(Version, new IconIndexEntry { name = $"icon-{i}", category = "Shapes" }, EIconStyle.Outline, null);

            _queue.Update(0);

            Assert.AreEqual(IconDownloadQueue.MaxConcurrentDownloads, _queue.DownloadingCount);
            Assert.AreEqual(requestCount, _queue.Count);
        }

        /// <summary>
        /// An invalid version or index entry is rejected.
        /// </summary>
        [Test]
        public void Request_InvalidVersionOrEntry_Throws()
        {
            Assert.Throws<ArgumentException>(() => _queue.Request("latest", Home, EIconStyle.Outline, null));
            Assert.Throws<ArgumentException>(() => _queue.Request(Version, null, EIconStyle.Outline, null));
            var invalidEntry = new IconIndexEntry { name = "../home" };
            Assert.Throws<ArgumentException>(() => _queue.Request(Version, invalidEntry, EIconStyle.Outline, null));
        }

        #endregion


        #region Failures

        /// <summary>
        /// A failed download is retried after the retry delay, without notifying the request in between.
        /// </summary>
        [Test]
        public void Update_FailedDownload_IsRetriedAfterDelay()
        {
            int attemptCount = 0;
            _answer = url => attemptCount++ == 0 ? Failure(url, EWebDownloadStatus.Failed) : Success(url);
            bool? isStored = null;
            _queue.Request(Version, User, EIconStyle.Outline, result => isStored = result);

            UpdateTwice(0);
            Assert.IsNull(isStored);

            _queue.Update(DownloadRetrySettings.DefaultFirstRetryDelay - Epsilon);
            Assert.AreEqual(1, _downloads.Count);

            UpdateTwice(DownloadRetrySettings.DefaultFirstRetryDelay);
            Assert.AreEqual(true, isStored);
            Assert.AreEqual(2, _downloads.Count);
        }

        /// <summary>
        /// A downloaded file that is not a valid image is retried like a failed download.
        /// </summary>
        [Test]
        public void Update_InvalidImage_IsRetried()
        {
            _answer = url => new FakeWebDownload(url, WebDownloadResult.Success(new byte[] { 1, 2, 3 }));
            bool? isStored = null;
            _queue.Request(Version, User, EIconStyle.Outline, result => isStored = result);

            UpdateTwice(0);

            Assert.IsNull(isStored);
            Assert.AreEqual(1, _policy.GetFailureCount(IconDownloadQueue.GetKey(Version, "user", EIconStyle.Outline)));
            Assert.AreEqual(1, _queue.Count);
        }

        /// <summary>
        /// A missing file is given up and reported, and requesting it again completes right away.
        /// </summary>
        [Test]
        public void Update_NotFound_GivesUpAndRemembers()
        {
            _answer = url => Failure(url, EWebDownloadStatus.NotFound);
            bool? isStored = null;
            _queue.Request(Version, User, EIconStyle.Outline, result => isStored = result);

            UpdateTwice(0);

            Assert.AreEqual(false, isStored);
            CollectionAssert.AreEqual(new[] { IconDownloadQueue.GetKey(Version, "user", EIconStyle.Outline) }, _givenUpKeys);

            isStored = null;
            _queue.Request(Version, User, EIconStyle.Outline, result => isStored = result);
            Assert.AreEqual(false, isStored);
            Assert.AreEqual(1, _downloads.Count);
        }

        /// <summary>
        /// A missing filled file doesn't prevent the outline style from being stored.
        /// </summary>
        [Test]
        public void Update_FilledNotFound_StillStoresOutline()
        {
            _answer = url => url.Contains("/filled/") ? Failure(url, EWebDownloadStatus.NotFound) : Success(url);
            bool? isOutlineStored = null;
            _queue.Request(Version, Home, EIconStyle.Outline, result => isOutlineStored = result);

            UpdateTwice(0);

            Assert.AreEqual(true, isOutlineStored);
            Assert.IsTrue(_policy.IsGivenUp(IconDownloadQueue.GetKey(Version, "home", EIconStyle.Filled)));
            Assert.AreEqual(0, _queue.Count);
        }

        /// <summary>
        /// A network error pauses every download until the end of the network pause.
        /// </summary>
        [Test]
        public void Update_NetworkError_PausesEveryDownload()
        {
            _answer = url => url.Contains("/user.") ? Failure(url, EWebDownloadStatus.NetworkError) : Success(url);
            _queue.Request(Version, User, EIconStyle.Outline, null);
            UpdateTwice(0);

            _queue.Request(Version, new IconIndexEntry { name = "star", category = "Shapes" }, EIconStyle.Outline, null);
            _queue.Update(DownloadRetrySettings.DefaultFirstNetworkPause - Epsilon);
            Assert.AreEqual(1, _downloads.Count);

            _queue.Update(DownloadRetrySettings.DefaultFirstNetworkPause);
            Assert.AreEqual(3, _downloads.Count);
        }

        /// <summary>
        /// Aborted downloads are not failures: their items stay in the queue, and are downloaded again.
        /// </summary>
        [Test]
        public void AbortDownloads_KeepsItemsForLater()
        {
            _answer = url => new FakeWebDownload(url, WebDownloadResult.Success(s_png), true);
            _queue.Request(Version, User, EIconStyle.Outline, null);
            _queue.Update(0);

            _queue.AbortDownloads();

            Assert.IsTrue(_downloads[0].IsAborted);
            Assert.AreEqual(0, _queue.DownloadingCount);
            Assert.AreEqual(1, _queue.Count);

            _queue.Update(0);
            Assert.AreEqual(2, _downloads.Count);
        }

        /// <summary>
        /// A callback that throws is logged, and doesn't prevent the other callbacks from being called.
        /// </summary>
        [Test]
        public void Update_ThrowingCallback_IsLoggedAndOthersAreCalled()
        {
            bool? isStored = null;
            _queue.Request(Version, User, EIconStyle.Outline, _ => throw new InvalidOperationException("Faulty callback"));
            _queue.Request(Version, User, EIconStyle.Outline, result => isStored = result);
            LogAssert.Expect(LogType.Exception, new Regex("Faulty callback"));

            UpdateTwice(0);

            Assert.AreEqual(true, isStored);
        }

        #endregion

    }

}
