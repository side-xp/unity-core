using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;

using SideXP.Core.EditorOnly;

using Object = UnityEngine.Object;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="IconDownloadRequest"/> and <see cref="IconDownloads"/>.
    /// </summary>
    /// <remarks>
    /// Downloads are replaced by <see cref="FakeWebDownload"/>, and every test works in a temporary folder set as
    /// <see cref="IconStorage.RootFolderOverride"/>. The shared queue is reset before and after each test. The generic behavior of
    /// the queue (retries, network pauses, concurrent downloads) is tested by <see cref="DownloadQueueTests"/>. Processing uses the
    /// GPU, so these tests work in the editor, but not in batch mode with <c>-nographics</c>.
    /// </remarks>
    public class IconDownloadsTests
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
        /// Maximum number of editor frames to wait for downloads, so a broken download fails the test instead of waiting forever.
        /// </summary>
        private const int MaxFrames = 200;

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
        /// Creates an empty temporary folder as the shared folder, resets the shared queue, and replaces the downloads by a fake
        /// server.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            string tempFolderName = $"SideXP-{nameof(IconDownloadsTests)}-{Guid.NewGuid():N}";
            _tempFolder = IconStorage.NormalizePath(Path.Combine(Path.GetTempPath(), tempFolderName));
            Directory.CreateDirectory(_tempFolder);
            IconStorage.RootFolderOverride = _tempFolder;

            IconDownloads.Reset();
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
        /// Resets the shared queue, restores the real downloads and the default shared folder, and deletes the temporary folder.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            IconDownloads.Reset();
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
        /// Answers with a missing file.
        /// </summary>
        /// <param name="url">The requested URL.</param>
        /// <returns>Returns the fake download.</returns>
        private static FakeWebDownload NotFound(string url)
        {
            return new FakeWebDownload(url, WebDownloadResult.Failure(EWebDownloadStatus.NotFound, "Not Found", 404));
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
        /// Waits for editor frames until the shared queue is empty.
        /// </summary>
        /// <returns>Returns the coroutine that waits.</returns>
        private static IEnumerator WaitForDownloads()
        {
            for (int i = 0; i < MaxFrames && IconDownloads.Count > 0; i++)
                yield return null;

            Assert.AreEqual(0, IconDownloads.Count, "The downloads didn't complete.");
        }

        #region Request

        /// <summary>
        /// A request is identified by the version, the icon and the style, and downloads the PNG of that style.
        /// </summary>
        [Test]
        public void Request_UsesVersionIconAndStyle()
        {
            var request = new IconDownloadRequest(Version, Home, EIconStyle.Filled);

            Assert.AreEqual("3.52.0/home/filled", request.Key);
            Assert.AreEqual(TablerSource.GetIconUrl(Version, "home", EIconStyle.Filled), request.Url);
            Assert.AreEqual("home (filled)", request.DisplayName);
        }

        /// <summary>
        /// An invalid version or index entry is rejected.
        /// </summary>
        [Test]
        public void Request_InvalidVersionOrEntry_Throws()
        {
            Assert.Throws<ArgumentException>(() => new IconDownloadRequest("latest", Home, EIconStyle.Outline));
            Assert.Throws<ArgumentException>(() => new IconDownloadRequest(Version, null, EIconStyle.Outline));
            var invalidEntry = new IconIndexEntry { name = "../home" };
            Assert.Throws<ArgumentException>(() => new IconDownloadRequest(Version, invalidEntry, EIconStyle.Outline));
        }

        /// <summary>
        /// Handling a downloaded PNG stores its files in every size, which makes the request available.
        /// </summary>
        [Test]
        public void Request_Handle_StoresEverySize()
        {
            var request = new IconDownloadRequest(Version, Home, EIconStyle.Outline);
            Assert.IsFalse(request.IsAvailable());

            EDownloadHandlingResult result = request.Handle(s_png, out string error);

            Assert.AreEqual(EDownloadHandlingResult.Success, result, error);
            Assert.IsTrue(request.IsAvailable());
            Assert.IsTrue(IconStorage.IsStored(Home.name, Home.category, EIconStyle.Outline));
            Assert.IsFalse(IconStorage.IsStored(Home.name, Home.category, EIconStyle.Filled));
        }

        /// <summary>
        /// A downloaded file that is not a valid image is given up, since downloading it again won't help.
        /// </summary>
        [Test]
        public void Request_HandleInvalidImage_GivesUp()
        {
            var request = new IconDownloadRequest(Version, User, EIconStyle.Outline);

            EDownloadHandlingResult result = request.Handle(new byte[] { 1, 2, 3 }, out string error);

            Assert.AreEqual(EDownloadHandlingResult.GiveUp, result);
            Assert.IsNotNull(error);
            Assert.IsFalse(request.IsAvailable());
        }

        #endregion


        #region Downloads

        /// <summary>
        /// Requesting an icon downloads and stores both its styles, from the given version.
        /// </summary>
        [UnityTest]
        public IEnumerator Request_DownloadsAndStoresBothStyles()
        {
            bool? isStored = null;
            IconDownloads.Request(Version, Home, EIconStyle.Outline, result => isStored = result);
            Assert.IsNull(isStored);
            Assert.IsTrue(IconDownloads.IsPending(Version, "home", EIconStyle.Outline));

            yield return WaitForDownloads();

            Assert.AreEqual(true, isStored);
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
        [UnityTest]
        public IEnumerator Request_FilledWithoutFilledStyle_DownloadsOutline()
        {
            bool? isStored = null;
            IconDownloads.Request(Version, User, EIconStyle.Filled, result => isStored = result);

            yield return WaitForDownloads();

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
            IconDownloads.Request(Version, Home, EIconStyle.Outline, result => isStored = result);

            Assert.AreEqual(true, isStored);
            Assert.AreEqual(0, IconDownloads.Count);
        }

        /// <summary>
        /// Only the styles with missing files are downloaded.
        /// </summary>
        [UnityTest]
        public IEnumerator Request_OneStyleStored_DownloadsOtherStyleOnly()
        {
            Store(Home, EIconStyle.Outline);

            bool? isStored = null;
            IconDownloads.Request(Version, Home, EIconStyle.Outline, result => isStored = result);
            yield return WaitForDownloads();

            Assert.AreEqual(true, isStored);
            Assert.AreEqual(1, _downloads.Count);
            Assert.AreEqual(TablerSource.GetIconUrl(Version, "home", EIconStyle.Filled), _downloads[0].Url);
        }

        /// <summary>
        /// A missing file is given up, and requesting it again completes right away.
        /// </summary>
        [UnityTest]
        public IEnumerator Request_NotFound_GivesUp()
        {
            _answer = NotFound;
            bool? isStored = null;
            IconDownloads.Request(Version, User, EIconStyle.Outline, result => isStored = result);

            yield return WaitForDownloads();

            Assert.AreEqual(false, isStored);
            Assert.IsTrue(IconDownloads.IsGivenUp(Version, "user", EIconStyle.Outline));

            isStored = null;
            IconDownloads.Request(Version, User, EIconStyle.Outline, result => isStored = result);
            Assert.AreEqual(false, isStored);
            Assert.AreEqual(1, _downloads.Count);
        }

        /// <summary>
        /// A missing filled file doesn't prevent the outline style from being stored.
        /// </summary>
        [UnityTest]
        public IEnumerator Request_FilledNotFound_StillStoresOutline()
        {
            _answer = url => url.Contains("/filled/") ? NotFound(url) : Success(url);
            bool? isOutlineStored = null;
            IconDownloads.Request(Version, Home, EIconStyle.Outline, result => isOutlineStored = result);

            yield return WaitForDownloads();

            Assert.AreEqual(true, isOutlineStored);
            Assert.IsTrue(IconDownloads.IsGivenUp(Version, "home", EIconStyle.Filled));
            Assert.IsFalse(IconDownloads.IsGivenUp(Version, "home", EIconStyle.Outline));
        }

        #endregion

    }

}
