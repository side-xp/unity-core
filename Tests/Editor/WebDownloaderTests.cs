using System;

using NUnit.Framework;

using SideXP.Core.EditorOnly;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="WebDownloader"/> and <see cref="WebDownloadResult"/>, without using the network.
    /// </summary>
    public class WebDownloaderTests
    {

        /// <summary>
        /// URL passed to the downloads of the tests. It's never requested, since downloads are faked.
        /// </summary>
        private const string Url = "https://example.com/file.bin";

        /// <summary>
        /// Restores the real downloads.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            WebDownloader.DownloadOverride = null;
        }

        #region Start

        /// <summary>
        /// A download needs a URL.
        /// </summary>
        [TestCase(null)]
        [TestCase("")]
        public void Start_NullOrEmptyUrl_Throws(string url)
        {
            Assert.Throws<ArgumentException>(() => WebDownloader.Start(url));
        }

        /// <summary>
        /// A download can't have a negative timeout.
        /// </summary>
        [Test]
        public void Start_NegativeTimeout_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => WebDownloader.Start(Url, -1));
        }

        /// <summary>
        /// The override starts the downloads instead of the network.
        /// </summary>
        [Test]
        public void Start_WithOverride_UsesOverride()
        {
            var fakeDownload = new FakeWebDownload(Url, WebDownloadResult.Success(new byte[] { 1 }));
            string requestedUrl = null;
            WebDownloader.DownloadOverride = url =>
            {
                requestedUrl = url;
                return fakeDownload;
            };

            IWebDownload download = WebDownloader.Start(Url);

            Assert.AreSame(fakeDownload, download);
            Assert.AreEqual(Url, requestedUrl);
        }

        #endregion


        #region Result

        /// <summary>
        /// A successful result always has data, even if none is given.
        /// </summary>
        [Test]
        public void Success_NullData_IsEmpty()
        {
            WebDownloadResult result = WebDownloadResult.Success(null);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.Data);
            Assert.IsEmpty(result.Data);
            Assert.IsNull(result.Error);
        }

        /// <summary>
        /// A failed result without error message uses the name of its status.
        /// </summary>
        [Test]
        public void Failure_EmptyError_UsesStatusName()
        {
            WebDownloadResult result = WebDownloadResult.Failure(EWebDownloadStatus.NotFound, null);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsNull(result.Data);
            Assert.AreEqual(nameof(EWebDownloadStatus.NotFound), result.Error);
        }

        /// <summary>
        /// A success can't be created as a failure.
        /// </summary>
        [Test]
        public void Failure_Success_Throws()
        {
            Assert.Throws<ArgumentException>(() => WebDownloadResult.Failure(EWebDownloadStatus.Success, null));
        }

        /// <summary>
        /// The downloaded data can be read as UTF-8 text.
        /// </summary>
        [Test]
        public void GetText_ReadsUtf8()
        {
            WebDownloadResult result = WebDownloadResult.Success(new byte[] { 0x68, 0xC3, 0xA9 });

            Assert.AreEqual("hé", result.GetText());
        }

        #endregion

    }

}
