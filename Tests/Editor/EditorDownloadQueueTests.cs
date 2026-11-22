using System;
using System.Collections;
using System.Collections.Generic;

using NUnit.Framework;

using UnityEngine.TestTools;

using SideXP.Core.EditorOnly;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="EditorDownloadQueue"/>, run from editor updates.
    /// </summary>
    /// <remarks>
    /// Downloads are replaced by <see cref="FakeWebDownload"/>, and requests by <see cref="FakeDownloadRequest"/>. Each test uses
    /// queues with unique names, disposed after the test along with the given up downloads they remembered in the session.
    /// </remarks>
    public class EditorDownloadQueueTests
    {

        /// <summary>
        /// Maximum number of editor frames to wait for downloads, so a broken download fails the test instead of waiting forever.
        /// </summary>
        private const int MaxFrames = 200;

        /// <summary>
        /// The queues created by the test.
        /// </summary>
        private List<EditorDownloadQueue> _queues = null;

        /// <summary>
        /// Whether the fake server answers with a 404 instead of a file.
        /// </summary>
        private bool _isServerMissingFiles = false;

        /// <summary>
        /// Whether the downloads of the fake server stay in progress.
        /// </summary>
        private bool _isServerPending = false;

        /// <summary>
        /// The downloads started by the queues, in order.
        /// </summary>
        private List<FakeWebDownload> _downloads = null;

        /// <summary>
        /// Replaces the downloads by a fake server answering with a file.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _queues = new List<EditorDownloadQueue>();
            _downloads = new List<FakeWebDownload>();
            _isServerMissingFiles = false;
            _isServerPending = false;
            WebDownloader.DownloadOverride = url =>
            {
                WebDownloadResult result = _isServerMissingFiles
                    ? WebDownloadResult.Failure(EWebDownloadStatus.NotFound, "Not Found", 404)
                    : WebDownloadResult.Success(new byte[] { 1 });

                var download = new FakeWebDownload(url, result, _isServerPending);
                _downloads.Add(download);
                return download;
            };
        }

        /// <summary>
        /// Disposes the queues created by the test, forgets what they remembered, and restores the real downloads.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            foreach (EditorDownloadQueue queue in _queues)
            {
                queue.ClearFailures();
                queue.Dispose();
            }

            EditorDownloadQueue.MaxTotalConcurrentDownloads = EditorDownloadQueue.DefaultMaxTotalConcurrentDownloads;
            WebDownloader.DownloadOverride = null;
        }

        /// <summary>
        /// Creates a queue that doesn't display its progress, disposed after the test.
        /// </summary>
        /// <param name="name">The name of the queue. If null, a unique name is used.</param>
        /// <param name="settings">The settings of the queue. If null, the progress is not displayed.</param>
        /// <returns>Returns the created queue.</returns>
        private EditorDownloadQueue CreateQueue(string name = null, EditorDownloadQueueSettings settings = null)
        {
            var queue = new EditorDownloadQueue(
                name ?? $"{nameof(EditorDownloadQueueTests)}-{Guid.NewGuid():N}",
                settings ?? new EditorDownloadQueueSettings { ShouldReportProgress = false }
            );
            _queues.Add(queue);
            return queue;
        }

        /// <summary>
        /// Waits for editor frames until a queue is empty.
        /// </summary>
        /// <param name="queue">The queue to wait for.</param>
        /// <returns>Returns the coroutine that waits.</returns>
        private static IEnumerator WaitForDownloads(EditorDownloadQueue queue)
        {
            for (int i = 0; i < MaxFrames && queue.Count > 0; i++)
                yield return null;

            Assert.AreEqual(0, queue.Count, "The downloads didn't complete.");
        }

        #region Downloads

        /// <summary>
        /// A requested resource is downloaded and handled from editor updates, and its completion is reported.
        /// </summary>
        [UnityTest]
        public IEnumerator Request_DownloadsFromEditorUpdates()
        {
            EditorDownloadQueue queue = CreateQueue();
            var completedRequests = new List<DownloadRequest>();
            queue.RequestCompleted += completedRequests.Add;
            var request = new FakeDownloadRequest("home");
            DownloadRequest notifiedRequest = null;

            DownloadRequest queuedRequest = queue.Request(request, r => notifiedRequest = r);
            Assert.AreSame(request, queuedRequest);
            Assert.IsTrue(queue.IsPending("home"));

            yield return WaitForDownloads(queue);

            Assert.AreSame(request, notifiedRequest);
            Assert.AreEqual(EDownloadRequestStatus.Succeeded, request.Status);
            Assert.IsFalse(queue.IsPending("home"));
            CollectionAssert.AreEqual(new[] { request }, completedRequests);
        }

        /// <summary>
        /// A resource already available completes right away, without starting the editor updates of the queue.
        /// </summary>
        [Test]
        public void Request_Available_CompletesRightAway()
        {
            EditorDownloadQueue queue = CreateQueue();
            var request = new FakeDownloadRequest("home") { IsResourceAvailable = true };

            queue.Request(request);

            Assert.AreEqual(EDownloadRequestStatus.Succeeded, request.Status);
            Assert.AreEqual(0, queue.Count);
        }

        /// <summary>
        /// The downloads of a queue are displayed as a task when enabled, which doesn't change the outcome.
        /// </summary>
        [UnityTest]
        public IEnumerator Request_WithProgress_Completes()
        {
            EditorDownloadQueue queue = CreateQueue(settings: new EditorDownloadQueueSettings { ShouldReportProgress = true });
            var request = new FakeDownloadRequest("home");

            queue.Request(request);
            yield return WaitForDownloads(queue);

            Assert.AreEqual(EDownloadRequestStatus.Succeeded, request.Status);
        }

        #endregion


        #region Given up downloads

        /// <summary>
        /// A given up download is remembered in the session by the queues with the same name, until the failures are cleared.
        /// </summary>
        [UnityTest]
        public IEnumerator GivenUp_IsRememberedInSession()
        {
            _isServerMissingFiles = true;
            string name = $"{nameof(EditorDownloadQueueTests)}-{Guid.NewGuid():N}";
            EditorDownloadQueue queue = CreateQueue(name);
            var request = new FakeDownloadRequest("home");

            queue.Request(request);
            yield return WaitForDownloads(queue);

            Assert.AreEqual(EDownloadRequestStatus.Failed, request.Status);
            Assert.IsTrue(queue.IsGivenUp("home"));
            queue.Dispose();

            // As after a domain reload
            EditorDownloadQueue restoredQueue = CreateQueue(name);
            Assert.IsTrue(restoredQueue.IsGivenUp("home"));
            restoredQueue.ClearFailures();
            Assert.IsFalse(restoredQueue.IsGivenUp("home"));
            Assert.IsFalse(CreateQueue(name).IsGivenUp("home"));
        }

        /// <summary>
        /// A given up download is not remembered in the session when disabled.
        /// </summary>
        [UnityTest]
        public IEnumerator GivenUp_WithoutRemembering_IsForgottenByNewQueues()
        {
            _isServerMissingFiles = true;
            string name = $"{nameof(EditorDownloadQueueTests)}-{Guid.NewGuid():N}";
            var settings = new EditorDownloadQueueSettings { ShouldReportProgress = false, ShouldRememberGivenUp = false };
            EditorDownloadQueue queue = CreateQueue(name, settings);

            queue.Request(new FakeDownloadRequest("home"));
            yield return WaitForDownloads(queue);

            Assert.IsTrue(queue.IsGivenUp("home"));
            Assert.IsFalse(CreateQueue(name, settings).IsGivenUp("home"));
        }

        #endregion


        #region Lifecycle

        /// <summary>
        /// Disposing a queue aborts its downloads, forgets its requests without calling their callbacks, and prevents new requests.
        /// </summary>
        [UnityTest]
        public IEnumerator Dispose_ForgetsRequestsAndRejectsNewOnes()
        {
            _isServerPending = true;
            EditorDownloadQueue queue = CreateQueue();
            bool isNotified = false;
            queue.Request(new FakeDownloadRequest("home"), _ => isNotified = true);
            for (int i = 0; i < MaxFrames && _downloads.Count == 0; i++)
                yield return null;

            queue.Dispose();

            Assert.AreEqual(1, _downloads.Count);
            Assert.IsTrue(_downloads[0].IsAborted);
            Assert.AreEqual(0, queue.Count);
            Assert.IsFalse(isNotified);
            Assert.Throws<InvalidOperationException>(() => queue.Request(new FakeDownloadRequest("user")));
        }

        /// <summary>
        /// A queue needs a name, and valid settings.
        /// </summary>
        [Test]
        public void Constructor_InvalidArguments_Throws()
        {
            Assert.Throws<ArgumentException>(() => new EditorDownloadQueue(" "));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new EditorDownloadQueue("Invalid", new EditorDownloadQueueSettings { MaxConcurrentDownloads = 0 })
            );
        }

        /// <summary>
        /// The queues must allow at least one download at a time in total.
        /// </summary>
        [Test]
        public void MaxTotalConcurrentDownloads_LessThanOne_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => EditorDownloadQueue.MaxTotalConcurrentDownloads = 0);
        }

        #endregion

    }

}
