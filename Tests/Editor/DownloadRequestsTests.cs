using System;
using System.IO;

using NUnit.Framework;

using SideXP.Core.EditorOnly;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="FileDownloadRequest"/>, <see cref="MemoryDownloadRequest"/> and <see cref="DelegateDownloadRequest"/>.
    /// </summary>
    /// <remarks>
    /// The requests are tested on their own, without queue or download. Every test works in its own temporary folder.
    /// </remarks>
    public class DownloadRequestsTests
    {

        /// <summary>
        /// URL of the requests. It's never requested, since no download is made.
        /// </summary>
        private const string Url = "https://example.com/file.bin";

        /// <summary>
        /// A file request that processes the downloaded content before it's written.
        /// </summary>
        private class ReversingFileRequest : FileDownloadRequest
        {

            /// <summary>
            /// The outcome of the processing.
            /// </summary>
            public EDownloadHandlingResult ProcessResult { get; set; } = EDownloadHandlingResult.Success;

            /// <inheritdoc cref="FileDownloadRequest(string, string)"/>
            public ReversingFileRequest(string url, string filePath)
                : base(url, filePath) { }

            /// <summary>
            /// Reverses the downloaded content, or doesn't succeed as set by the test.
            /// </summary>
            /// <inheritdoc/>
            protected override EDownloadHandlingResult Process(byte[] data, out byte[] content, out string error)
            {
                content = (byte[])data.Clone();
                Array.Reverse(content);
                error = ProcessResult != EDownloadHandlingResult.Success ? "Processing failed." : null;
                return ProcessResult;
            }

        }

        private string _tempFolder = null;

        /// <summary>
        /// Creates an empty temporary folder.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _tempFolder = Path.Combine(Path.GetTempPath(), $"SideXP-{nameof(DownloadRequestsTests)}-{Guid.NewGuid():N}");
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

        #region File

        /// <summary>
        /// A file request is identified by its path, and needs a URL and an absolute path.
        /// </summary>
        [Test]
        public void FileRequest_UsesPathAsKey_RejectsInvalidArguments()
        {
            string path = Path.Combine(_tempFolder, "file.bin");
            var request = new FileDownloadRequest(Url, path);

            Assert.AreEqual(path, request.Key);
            Assert.AreEqual("file.bin", request.DisplayName);
            Assert.Throws<ArgumentException>(() => new FileDownloadRequest(string.Empty, path));
            Assert.Throws<ArgumentException>(() => new FileDownloadRequest(Url, "relative/file.bin"));
        }

        /// <summary>
        /// A file request is available once its file exists, and handling writes the downloaded file, with its missing folders.
        /// </summary>
        [Test]
        public void FileRequest_Handle_WritesFile()
        {
            string path = Path.Combine(_tempFolder, "folder", "file.bin");
            var request = new FileDownloadRequest(Url, path);
            Assert.IsFalse(request.IsAvailable());

            EDownloadHandlingResult result = request.Handle(new byte[] { 1, 2, 3 }, out string error);

            Assert.AreEqual(EDownloadHandlingResult.Success, result);
            Assert.IsNull(error);
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, File.ReadAllBytes(path));
            Assert.IsTrue(request.IsAvailable());
        }

        /// <summary>
        /// An existing file is kept, as if another editor had just written it.
        /// </summary>
        [Test]
        public void FileRequest_HandleExistingFile_KeepsIt()
        {
            string path = Path.Combine(_tempFolder, "file.bin");
            File.WriteAllBytes(path, new byte[] { 9 });
            var request = new FileDownloadRequest(Url, path);

            EDownloadHandlingResult result = request.Handle(new byte[] { 1, 2, 3 }, out _);

            Assert.AreEqual(EDownloadHandlingResult.Success, result);
            CollectionAssert.AreEqual(new byte[] { 9 }, File.ReadAllBytes(path));
        }

        /// <summary>
        /// The processed content is written instead of the downloaded one.
        /// </summary>
        [Test]
        public void FileRequest_Process_WritesProcessedContent()
        {
            string path = Path.Combine(_tempFolder, "file.bin");
            var request = new ReversingFileRequest(Url, path);

            request.Handle(new byte[] { 1, 2, 3 }, out _);

            CollectionAssert.AreEqual(new byte[] { 3, 2, 1 }, File.ReadAllBytes(path));
        }

        /// <summary>
        /// Nothing is written if the processing doesn't succeed, and its outcome is returned.
        /// </summary>
        [Test]
        public void FileRequest_ProcessGiveUp_WritesNothing()
        {
            string path = Path.Combine(_tempFolder, "file.bin");
            var request = new ReversingFileRequest(Url, path) { ProcessResult = EDownloadHandlingResult.GiveUp };

            EDownloadHandlingResult result = request.Handle(new byte[] { 1, 2, 3 }, out string error);

            Assert.AreEqual(EDownloadHandlingResult.GiveUp, result);
            Assert.AreEqual("Processing failed.", error);
            Assert.IsFalse(File.Exists(path));
        }

        #endregion


        #region Memory

        /// <summary>
        /// A memory request keeps the downloaded content, and is available once it has it.
        /// </summary>
        [Test]
        public void MemoryRequest_Handle_KeepsData()
        {
            var request = new MemoryDownloadRequest(Url);
            Assert.AreEqual(Url, request.Key);
            Assert.IsFalse(request.IsAvailable());
            Assert.IsNull(request.GetText());

            EDownloadHandlingResult result = request.Handle(new byte[] { 0x68, 0x69 }, out _);

            Assert.AreEqual(EDownloadHandlingResult.Success, result);
            Assert.IsTrue(request.IsAvailable());
            Assert.AreEqual("hi", request.GetText());
        }

        /// <summary>
        /// A memory request needs a URL.
        /// </summary>
        [Test]
        public void MemoryRequest_EmptyUrl_Throws()
        {
            Assert.Throws<ArgumentException>(() => new MemoryDownloadRequest(null));
        }

        #endregion


        #region Delegate

        /// <summary>
        /// A delegate request uses its functions to check the availability and handle the downloaded file.
        /// </summary>
        [Test]
        public void DelegateRequest_UsesFunctions()
        {
            byte[] handledData = null;
            bool isAvailable = false;
            var request = new DelegateDownloadRequest(
                "key",
                Url,
                data => { handledData = data; return EDownloadHandlingResult.Retry; },
                () => isAvailable
            );

            Assert.IsFalse(request.IsAvailable());
            isAvailable = true;
            Assert.IsTrue(request.IsAvailable());

            EDownloadHandlingResult result = request.Handle(new byte[] { 1 }, out _);
            Assert.AreEqual(EDownloadHandlingResult.Retry, result);
            CollectionAssert.AreEqual(new byte[] { 1 }, handledData);
        }

        /// <summary>
        /// Without availability function, a delegate request is never available.
        /// </summary>
        [Test]
        public void DelegateRequest_WithoutAvailability_IsNeverAvailable()
        {
            var request = new DelegateDownloadRequest("key", Url, _ => EDownloadHandlingResult.Success);

            Assert.IsFalse(request.IsAvailable());
        }

        /// <summary>
        /// A delegate request needs a key, a URL and a handling function.
        /// </summary>
        [Test]
        public void DelegateRequest_InvalidArguments_Throws()
        {
            Assert.Throws<ArgumentException>(() => new DelegateDownloadRequest(null, Url, _ => EDownloadHandlingResult.Success));
            Assert.Throws<ArgumentException>(() => new DelegateDownloadRequest("key", null, _ => EDownloadHandlingResult.Success));
            Assert.Throws<ArgumentNullException>(() => new DelegateDownloadRequest("key", Url, null));
        }

        #endregion

    }

}
