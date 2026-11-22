using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using NUnit.Framework;

using SideXP.Core.EditorOnly;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="IconIndexRefresh"/>.
    /// </summary>
    /// <remarks>
    /// Downloads are replaced by <see cref="FakeWebDownload"/>, and every test works in a temporary folder set as
    /// <see cref="IconStorage.RootFolderOverride"/>, so neither the network nor the real shared folder are used.
    /// </remarks>
    public class IconIndexRefreshTests
    {

        /// <summary>
        /// The latest Tabler version, as answered by the fake server.
        /// </summary>
        private const string LatestVersion = "3.52.0";

        /// <summary>
        /// A Tabler version older than <see cref="LatestVersion"/>.
        /// </summary>
        private const string OldVersion = "3.51.0";

        /// <summary>
        /// Maximum number of updates to complete a job, so a broken job fails the test instead of looping forever.
        /// </summary>
        private const int MaxUpdates = 10;

        /// <summary>
        /// The catalog of <see cref="LatestVersion"/>, as answered by the fake server.
        /// </summary>
        private const string Catalog = @"{
  ""home"": { ""name"": ""home"", ""category"": ""Buildings"", ""tags"": [""house""],
    ""styles"": {
      ""outline"": { ""version"": ""1.0"", ""unicode"": ""eac1"" },
      ""filled"": { ""version"": ""2.0"", ""unicode"": ""fe2b"" }
    } },
  ""user"": { ""name"": ""user"", ""category"": ""System"", ""tags"": [""person""],
    ""styles"": { ""outline"": { ""version"": ""1.0"", ""unicode"": ""eb4d"" } } }
}";

        private string _tempFolder = null;

        /// <summary>
        /// The answers of the fake server, by URL. URLs without answer are answered with a 404.
        /// </summary>
        private Dictionary<string, Func<string, FakeWebDownload>> _answers = null;

        /// <summary>
        /// The downloads started by the job, in order.
        /// </summary>
        private List<FakeWebDownload> _downloads = null;

        /// <summary>
        /// Creates an empty temporary folder as the shared folder, and replaces the downloads by a fake server that knows the latest
        /// version and its catalog.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            string tempFolderName = $"SideXP-{nameof(IconIndexRefreshTests)}-{Guid.NewGuid():N}";
            _tempFolder = IconStorage.NormalizePath(Path.Combine(Path.GetTempPath(), tempFolderName));
            Directory.CreateDirectory(_tempFolder);
            IconStorage.RootFolderOverride = _tempFolder;
            IconIndexStore.ClearMemory();

            _downloads = new List<FakeWebDownload>();
            _answers = new Dictionary<string, Func<string, FakeWebDownload>>
            {
                { TablerSource.LatestVersionUrl, url => Answer(url, VersionResponse(LatestVersion)) },
                { TablerSource.GetCatalogUrl(LatestVersion), url => Answer(url, Catalog) },
            };

            WebDownloader.DownloadOverride = url =>
            {
                FakeWebDownload download = _answers.TryGetValue(url, out Func<string, FakeWebDownload> answer)
                    ? answer(url)
                    : new FakeWebDownload(url, WebDownloadResult.Failure(EWebDownloadStatus.NotFound, "Not Found", 404));
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
            IconIndexStore.ClearMemory();
            if (Directory.Exists(_tempFolder))
                Directory.Delete(_tempFolder, true);
        }

        /// <summary>
        /// Makes a successful fake download of a text.
        /// </summary>
        /// <param name="url">The URL of the downloaded file.</param>
        /// <param name="text">The downloaded text.</param>
        /// <param name="isPending">If enabled, the download stays in progress until completed by the test.</param>
        /// <returns>Returns the fake download.</returns>
        private static FakeWebDownload Answer(string url, string text, bool isPending = false)
        {
            return new FakeWebDownload(url, WebDownloadResult.Success(Encoding.UTF8.GetBytes(text)), isPending);
        }

        /// <summary>
        /// Makes a response of the jsDelivr data API, shaped like the real one.
        /// </summary>
        /// <param name="version">The resolved version.</param>
        /// <returns>Returns the response.</returns>
        private static string VersionResponse(string version)
        {
            return $"{{\n\t\"type\": \"npm\",\n\t\"name\": \"@tabler/icons-png\",\n\t\"version\": \"{version}\"\n}}";
        }

        /// <summary>
        /// Stores an index of <see cref="OldVersion"/> with a <c>star</c> icon, and creates the files of that icon.
        /// </summary>
        /// <returns>Returns the path to the folder of the <c>star</c> icon.</returns>
        private static string StoreOldIndex()
        {
            var oldIndex = new IconIndex
            {
                version = OldVersion,
                icons = new[] { new IconIndexEntry { name = "star", category = "Shapes" } },
            };
            IconIndexStore.Write(oldIndex);

            string iconPath = IconStorage.GetIconFilePath("star", "Shapes", 16, EIconStyle.Outline);
            FileEditorUtility.WriteFileAtomic(iconPath, new byte[] { 1 }, false);
            return IconStorage.GetIconFolderPath("star", "Shapes");
        }

        /// <summary>
        /// Starts a job, and updates it until it's done.
        /// </summary>
        /// <returns>Returns the completed job.</returns>
        private static IconIndexRefresh RunToCompletion()
        {
            var job = new IconIndexRefresh();
            job.Start();
            for (int i = 0; i < MaxUpdates && !job.IsDone; i++)
                job.Update();

            Assert.IsTrue(job.IsDone, "The job didn't complete.");
            return job;
        }

        /// <summary>
        /// Checks that the stored index is the old one from <see cref="StoreOldIndex"/>.
        /// </summary>
        private static void AssertOldIndexKept()
        {
            IconIndexStore.ClearMemory();
            Assert.IsTrue(IconIndexStore.TryGetIndex(out IconIndex index));
            Assert.AreEqual(OldVersion, index.version);
        }

        #region Success

        /// <summary>
        /// Without stored index, the latest version and its catalog are downloaded, and the converted index is written.
        /// </summary>
        [Test]
        public void Refresh_WithoutIndex_DownloadsAndWritesIndex()
        {
            IconIndexRefresh job = RunToCompletion();

            Assert.IsTrue(job.IsSuccess, job.Error);
            Assert.IsFalse(job.IsUpToDate);
            Assert.AreEqual(LatestVersion, job.Version);
            CollectionAssert.AreEqual(
                new[] { TablerSource.LatestVersionUrl, TablerSource.GetCatalogUrl(LatestVersion) },
                _downloads.ConvertAll(download => download.Url)
            );

            IconIndexStore.ClearMemory();
            Assert.IsTrue(IconIndexStore.TryGetIndex(out IconIndex index));
            Assert.AreEqual(LatestVersion, index.version);
            Assert.AreEqual(2, index.icons.Length);
            Assert.IsTrue(index.icons[0].hasFilled);
        }

        /// <summary>
        /// If the stored index already has the latest version, the catalog is not downloaded.
        /// </summary>
        [Test]
        public void Refresh_SameVersion_IsUpToDate()
        {
            IconIndexStore.Write(new IconIndex { version = LatestVersion, icons = new IconIndexEntry[0] });

            IconIndexRefresh job = RunToCompletion();

            Assert.IsTrue(job.IsSuccess, job.Error);
            Assert.IsTrue(job.IsUpToDate);
            Assert.AreEqual(1, _downloads.Count);
        }

        /// <summary>
        /// Updating to a new version cleans up the files of the icons removed from the catalog.
        /// </summary>
        [Test]
        public void Refresh_NewVersion_CleansUpOldFiles()
        {
            string starFolder = StoreOldIndex();

            IconIndexRefresh job = RunToCompletion();

            Assert.IsTrue(job.IsSuccess, job.Error);
            Assert.IsEmpty(job.CleanUpErrors);
            Assert.IsFalse(Directory.Exists(starFolder));
            Assert.IsTrue(IconIndexStore.TryGetIndex(out IconIndex index));
            Assert.AreEqual(LatestVersion, index.version);
        }

        /// <summary>
        /// The job doesn't move forward while a download is in progress.
        /// </summary>
        [Test]
        public void Update_PendingDownload_WaitsForIt()
        {
            FakeWebDownload versionDownload = null;
            _answers[TablerSource.LatestVersionUrl] = url => versionDownload = Answer(url, VersionResponse(LatestVersion), true);

            var job = new IconIndexRefresh();
            job.Start();
            job.Update();
            job.Update();

            Assert.AreEqual(IconIndexRefresh.EStep.ResolvingVersion, job.Step);

            versionDownload.Complete();
            job.Update();

            Assert.AreEqual(IconIndexRefresh.EStep.DownloadingCatalog, job.Step);
        }

        #endregion


        #region Failures

        /// <summary>
        /// If the version can't be resolved, the job fails, and the stored index is kept.
        /// </summary>
        [Test]
        public void Refresh_VersionNetworkError_FailsAndKeepsIndex()
        {
            StoreOldIndex();
            _answers[TablerSource.LatestVersionUrl] = url =>
                new FakeWebDownload(url, WebDownloadResult.Failure(EWebDownloadStatus.NetworkError, "Cannot resolve host"));

            IconIndexRefresh job = RunToCompletion();

            Assert.IsFalse(job.IsSuccess);
            StringAssert.Contains("Cannot resolve host", job.Error);
            AssertOldIndexKept();
        }

        /// <summary>
        /// An invalid version is rejected before being used in a URL.
        /// </summary>
        [Test]
        public void Refresh_InvalidVersion_FailsWithoutCatalogDownload()
        {
            _answers[TablerSource.LatestVersionUrl] = url => Answer(url, VersionResponse("../../evil"));

            IconIndexRefresh job = RunToCompletion();

            Assert.IsFalse(job.IsSuccess);
            Assert.AreEqual(1, _downloads.Count);
            Assert.IsFalse(File.Exists(IconStorage.IndexFilePath));
        }

        /// <summary>
        /// If the catalog can't be downloaded, the job fails, and the stored index and files are kept.
        /// </summary>
        [Test]
        public void Refresh_CatalogNotFound_FailsAndKeepsIndex()
        {
            string starFolder = StoreOldIndex();
            _answers.Remove(TablerSource.GetCatalogUrl(LatestVersion));

            IconIndexRefresh job = RunToCompletion();

            Assert.IsFalse(job.IsSuccess);
            AssertOldIndexKept();
            Assert.IsTrue(Directory.Exists(starFolder));
        }

        /// <summary>
        /// If the catalog can't be converted, the job fails, and the stored index and files are kept.
        /// </summary>
        [Test]
        public void Refresh_InvalidCatalog_FailsAndKeepsIndex()
        {
            string starFolder = StoreOldIndex();
            _answers[TablerSource.GetCatalogUrl(LatestVersion)] = url => Answer(url, "{}");

            IconIndexRefresh job = RunToCompletion();

            Assert.IsFalse(job.IsSuccess);
            AssertOldIndexKept();
            Assert.IsTrue(Directory.Exists(starFolder));
        }

        /// <summary>
        /// Cancelling the job aborts the current download, and keeps the stored index and files.
        /// </summary>
        [Test]
        public void Cancel_DuringCatalogDownload_AbortsAndKeepsIndex()
        {
            string starFolder = StoreOldIndex();
            FakeWebDownload catalogDownload = null;
            _answers[TablerSource.GetCatalogUrl(LatestVersion)] = url => catalogDownload = Answer(url, Catalog, true);

            var job = new IconIndexRefresh();
            job.Start();
            job.Update();
            job.Cancel();

            Assert.IsTrue(job.IsDone);
            Assert.IsFalse(job.IsSuccess);
            Assert.IsTrue(job.IsCancelled);
            Assert.IsTrue(catalogDownload.IsAborted);
            AssertOldIndexKept();
            Assert.IsTrue(Directory.Exists(starFolder));
        }

        /// <summary>
        /// A job can only be started once.
        /// </summary>
        [Test]
        public void Start_Twice_Throws()
        {
            var job = new IconIndexRefresh();
            job.Start();

            Assert.Throws<InvalidOperationException>(job.Start);
        }

        #endregion

    }

}
