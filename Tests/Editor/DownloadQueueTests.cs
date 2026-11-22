using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;

using SideXP.Core.EditorOnly;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="DownloadQueue"/>, driven by a <see cref="DownloadScheduler"/>.
    /// </summary>
    /// <remarks>
    /// Downloads are replaced by <see cref="FakeWebDownload"/>, requests by <see cref="FakeDownloadRequest"/>, and time is given by
    /// the tests.
    /// </remarks>
    public class DownloadQueueTests
    {

        /// <summary>
        /// Small duration used to check times right before a delay ends, in seconds.
        /// </summary>
        private const double Epsilon = 0.01;

        /// <summary>
        /// The content of the files answered by the fake server.
        /// </summary>
        private static readonly byte[] Data = { 1, 2, 3 };

        private DownloadQueue _queue = null;
        private DownloadScheduler _scheduler = null;

        /// <summary>
        /// The requests completed by the queue, as reported by <see cref="DownloadQueue.Completed"/>.
        /// </summary>
        private List<DownloadRequest> _completedRequests = null;

        /// <summary>
        /// The downloads started by the queue, in order.
        /// </summary>
        private List<FakeWebDownload> _downloads = null;

        /// <summary>
        /// Answers a download request of the queue. Answers with <see cref="Data"/> by default.
        /// </summary>
        private Func<string, FakeWebDownload> _answer = null;

        /// <summary>
        /// Creates a new queue driven by a new scheduler, and replaces the downloads by a fake server.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _queue = new DownloadQueue();
            _completedRequests = new List<DownloadRequest>();
            _queue.Completed += _completedRequests.Add;
            _scheduler = new DownloadScheduler();
            _scheduler.Add(_queue);

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
        /// Restores the real downloads.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            WebDownloader.DownloadOverride = null;
        }

        /// <summary>
        /// Answers with <see cref="Data"/>.
        /// </summary>
        /// <param name="url">The requested URL.</param>
        /// <returns>Returns the fake download.</returns>
        private static FakeWebDownload Success(string url)
        {
            return new FakeWebDownload(url, WebDownloadResult.Success(Data));
        }

        /// <summary>
        /// Answers with <see cref="Data"/>, but stays in progress until completed by the test.
        /// </summary>
        /// <param name="url">The requested URL.</param>
        /// <returns>Returns the fake download.</returns>
        private static FakeWebDownload Pending(string url)
        {
            return new FakeWebDownload(url, WebDownloadResult.Success(Data), true);
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
        /// Updates the scheduler twice at the same time: once to start the downloads, once to handle them (fake downloads are done
        /// right away).
        /// </summary>
        /// <param name="time">The current time, in seconds.</param>
        private void UpdateTwice(double time)
        {
            _scheduler.Update(time);
            _scheduler.Update(time);
        }

        #region Requests

        /// <summary>
        /// A requested resource is downloaded, handled, and reported as succeeded.
        /// </summary>
        [Test]
        public void Request_DownloadsAndHandles()
        {
            var request = new FakeDownloadRequest("home");
            DownloadRequest completedRequest = null;
            _queue.Request(request, r => completedRequest = r);
            Assert.AreEqual(EDownloadRequestStatus.Waiting, request.Status);

            UpdateTwice(0);

            Assert.AreSame(request, completedRequest);
            Assert.AreEqual(EDownloadRequestStatus.Succeeded, request.Status);
            Assert.IsNull(request.Error);
            Assert.AreEqual(1f, request.Progress);
            CollectionAssert.AreEqual(new[] { Data }, request.HandledData);
            CollectionAssert.AreEqual(new[] { request.Url }, _downloads.ConvertAll(download => download.Url));
            CollectionAssert.AreEqual(new[] { request }, _completedRequests);
            Assert.AreEqual(0, _queue.Count);
        }

        /// <summary>
        /// A resource already available completes right away, without download, and is not reported as completed by the queue.
        /// </summary>
        [Test]
        public void Request_Available_CompletesRightAway()
        {
            var request = new FakeDownloadRequest("home") { IsResourceAvailable = true };
            DownloadRequest completedRequest = null;

            _queue.Request(request, r => completedRequest = r);

            Assert.AreSame(request, completedRequest);
            Assert.AreEqual(EDownloadRequestStatus.Succeeded, request.Status);
            Assert.AreEqual(0, _queue.Count);
            _scheduler.Update(0);
            Assert.IsEmpty(_downloads);
            Assert.IsEmpty(_completedRequests);
        }

        /// <summary>
        /// Requesting the same key twice downloads it once, returns the request already in the queue, and notifies both callers.
        /// </summary>
        [Test]
        public void Request_SameKeyTwice_SharesDownload()
        {
            var firstRequest = new FakeDownloadRequest("home");
            var secondRequest = new FakeDownloadRequest("home");
            var notifiedRequests = new List<DownloadRequest>();

            DownloadRequest firstQueued = _queue.Request(firstRequest, notifiedRequests.Add);
            DownloadRequest secondQueued = _queue.Request(secondRequest, notifiedRequests.Add);
            UpdateTwice(0);

            Assert.AreSame(firstRequest, firstQueued);
            Assert.AreSame(firstRequest, secondQueued);
            CollectionAssert.AreEqual(new[] { firstRequest, firstRequest }, notifiedRequests);
            Assert.AreEqual(1, _downloads.Count);
            Assert.AreEqual(EDownloadRequestStatus.None, secondRequest.Status);
        }

        /// <summary>
        /// A completed request can be requested again.
        /// </summary>
        [Test]
        public void Request_AgainAfterSuccess_ChecksAvailabilityAgain()
        {
            var request = new FakeDownloadRequest("home");
            _queue.Request(request, null);
            UpdateTwice(0);

            request.IsResourceAvailable = false;
            _queue.Request(request, null);
            UpdateTwice(0);

            Assert.AreEqual(EDownloadRequestStatus.Succeeded, request.Status);
            Assert.AreEqual(2, _downloads.Count);
        }

        /// <summary>
        /// A request can't be in two queues at the same time.
        /// </summary>
        [Test]
        public void Request_InAnotherQueue_Throws()
        {
            var request = new FakeDownloadRequest("home");
            new DownloadQueue().Request(request, null);

            Assert.Throws<InvalidOperationException>(() => _queue.Request(request, null));
        }

        /// <summary>
        /// A request needs a key and a URL.
        /// </summary>
        [Test]
        public void Request_InvalidRequest_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _queue.Request(null, null));
            Assert.Throws<ArgumentException>(() => _queue.Request(new FakeDownloadRequest(string.Empty), null));
        }

        /// <summary>
        /// A resource that became available after being requested (as stored by another editor) is not downloaded.
        /// </summary>
        [Test]
        public void Update_AvailableInTheMeantime_CompletesWithoutDownload()
        {
            var request = new FakeDownloadRequest("home");
            _queue.Request(request, null);

            request.IsResourceAvailable = true;
            _scheduler.Update(0);

            Assert.AreEqual(EDownloadRequestStatus.Succeeded, request.Status);
            Assert.IsEmpty(_downloads);
            CollectionAssert.AreEqual(new[] { request }, _completedRequests);
        }

        /// <summary>
        /// A request reports the progress of its download while it's downloading.
        /// </summary>
        [Test]
        public void Update_Downloading_ReportsProgress()
        {
            _answer = Pending;
            var request = new FakeDownloadRequest("home");
            _queue.Request(request, null);

            _scheduler.Update(0);

            Assert.AreEqual(EDownloadRequestStatus.Downloading, request.Status);
            Assert.AreEqual(_downloads[0].Progress, request.Progress);
            Assert.AreSame(request, _queue.CurrentRequest);
        }

        /// <summary>
        /// No more downloads than the maximum of the queue run at the same time.
        /// </summary>
        [Test]
        public void Update_ManyRequests_LimitsConcurrentDownloads()
        {
            _answer = Pending;
            int requestCount = DownloadQueue.DefaultMaxConcurrentDownloads + 2;
            for (int i = 0; i < requestCount; i++)
                _queue.Request(new FakeDownloadRequest($"icon-{i}"), null);

            _scheduler.Update(0);

            Assert.AreEqual(DownloadQueue.DefaultMaxConcurrentDownloads, _queue.DownloadingCount);
            Assert.AreEqual(requestCount, _queue.Count);
        }

        /// <summary>
        /// A queue must allow at least one download at a time.
        /// </summary>
        [Test]
        public void Constructor_InvalidMaxConcurrentDownloads_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new DownloadQueue(0));
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
            var request = new FakeDownloadRequest("home");
            bool isNotified = false;
            _queue.Request(request, _ => isNotified = true);

            UpdateTwice(0);
            Assert.IsFalse(isNotified);
            Assert.AreEqual(EDownloadRequestStatus.Waiting, request.Status);
            Assert.AreEqual(nameof(EWebDownloadStatus.Failed), request.Error);

            _scheduler.Update(DownloadRetrySettings.DefaultFirstRetryDelay - Epsilon);
            Assert.AreEqual(1, _downloads.Count);

            UpdateTwice(DownloadRetrySettings.DefaultFirstRetryDelay);
            Assert.IsTrue(isNotified);
            Assert.AreEqual(EDownloadRequestStatus.Succeeded, request.Status);
            Assert.AreEqual(2, _downloads.Count);
        }

        /// <summary>
        /// A downloaded file that couldn't be handled is retried like a failed download.
        /// </summary>
        [Test]
        public void Update_HandlingRetry_IsRetried()
        {
            var request = new FakeDownloadRequest("home") { OnHandle = _ => EDownloadHandlingResult.Retry };
            _queue.Request(request, null);

            UpdateTwice(0);

            Assert.AreEqual(EDownloadRequestStatus.Waiting, request.Status);
            Assert.IsNotNull(request.Error);
            Assert.AreEqual(1, _queue.Policy.GetFailureCount(request.Key));
            Assert.AreEqual(1, _queue.Count);
        }

        /// <summary>
        /// A downloaded file that can't be handled fails right away, and requesting it again fails without download.
        /// </summary>
        [Test]
        public void Update_HandlingGiveUp_FailsRightAway()
        {
            var request = new FakeDownloadRequest("home") { OnHandle = _ => EDownloadHandlingResult.GiveUp };
            _queue.Request(request, null);

            UpdateTwice(0);

            Assert.AreEqual(EDownloadRequestStatus.Failed, request.Status);
            Assert.AreEqual($"Handling result: {EDownloadHandlingResult.GiveUp}", request.Error);
            CollectionAssert.AreEqual(new[] { request }, _completedRequests);

            DownloadRequest completedRequest = null;
            _queue.Request(new FakeDownloadRequest("home"), r => completedRequest = r);
            Assert.AreEqual(EDownloadRequestStatus.Failed, completedRequest.Status);
            Assert.AreEqual(1, _downloads.Count);
        }

        /// <summary>
        /// A handling that throws is logged, and retried like a failed download.
        /// </summary>
        [Test]
        public void Update_ThrowingHandling_IsLoggedAndRetried()
        {
            var request = new FakeDownloadRequest("home") { OnHandle = _ => throw new InvalidOperationException("Faulty handling") };
            _queue.Request(request, null);
            LogAssert.Expect(LogType.Exception, new Regex("Faulty handling"));

            UpdateTwice(0);

            Assert.AreEqual(EDownloadRequestStatus.Waiting, request.Status);
            StringAssert.Contains("Faulty handling", request.Error);
            Assert.AreEqual(1, _queue.Policy.GetFailureCount(request.Key));
        }

        /// <summary>
        /// A missing file is given up and reported.
        /// </summary>
        [Test]
        public void Update_NotFound_GivesUp()
        {
            _answer = url => Failure(url, EWebDownloadStatus.NotFound);
            var request = new FakeDownloadRequest("home");
            DownloadRequest completedRequest = null;
            _queue.Request(request, r => completedRequest = r);

            UpdateTwice(0);

            Assert.AreSame(request, completedRequest);
            Assert.AreEqual(EDownloadRequestStatus.Failed, request.Status);
            Assert.AreEqual(nameof(EWebDownloadStatus.NotFound), request.Error);
            Assert.IsTrue(_queue.Policy.IsGivenUp(request.Key));
            CollectionAssert.AreEqual(new[] { request }, _completedRequests);
        }

        /// <summary>
        /// A network error pauses every download of the queue until the end of the network pause.
        /// </summary>
        [Test]
        public void Update_NetworkError_PausesEveryDownload()
        {
            _answer = url => url.EndsWith("/user") ? Failure(url, EWebDownloadStatus.NetworkError) : Success(url);
            _queue.Request(new FakeDownloadRequest("user"), null);
            UpdateTwice(0);

            _queue.Request(new FakeDownloadRequest("star"), null);
            _scheduler.Update(DownloadRetrySettings.DefaultFirstNetworkPause - Epsilon);
            Assert.AreEqual(1, _downloads.Count);

            _scheduler.Update(DownloadRetrySettings.DefaultFirstNetworkPause);
            Assert.AreEqual(3, _downloads.Count);
        }

        /// <summary>
        /// Aborted downloads are not failures: their requests stay in the queue, and are downloaded again.
        /// </summary>
        [Test]
        public void AbortDownloads_KeepsRequestsForLater()
        {
            _answer = Pending;
            var request = new FakeDownloadRequest("home");
            _queue.Request(request, null);
            _scheduler.Update(0);

            _queue.AbortDownloads();

            Assert.IsTrue(_downloads[0].IsAborted);
            Assert.AreEqual(EDownloadRequestStatus.Waiting, request.Status);
            Assert.AreEqual(0, _queue.DownloadingCount);
            Assert.AreEqual(1, _queue.Count);

            _scheduler.Update(0);
            Assert.AreEqual(2, _downloads.Count);
        }

        /// <summary>
        /// Clearing the queue aborts its downloads, and forgets its requests without calling their callbacks.
        /// </summary>
        [Test]
        public void Clear_ForgetsRequestsWithoutCallbacks()
        {
            _answer = Pending;
            var request = new FakeDownloadRequest("home");
            bool isNotified = false;
            _queue.Request(request, _ => isNotified = true);
            _scheduler.Update(0);

            _queue.Clear();
            _scheduler.Update(0);

            Assert.IsTrue(_downloads[0].IsAborted);
            Assert.IsFalse(isNotified);
            Assert.AreEqual(EDownloadRequestStatus.None, request.Status);
            Assert.AreEqual(0, _queue.Count);
            Assert.AreEqual(1, _downloads.Count);
        }

        /// <summary>
        /// A callback that throws is logged, and doesn't prevent the other callbacks from being called.
        /// </summary>
        [Test]
        public void Update_ThrowingCallback_IsLoggedAndOthersAreCalled()
        {
            bool isNotified = false;
            _queue.Request(new FakeDownloadRequest("home"), _ => throw new InvalidOperationException("Faulty callback"));
            _queue.Request(new FakeDownloadRequest("home"), _ => isNotified = true);
            LogAssert.Expect(LogType.Exception, new Regex("Faulty callback"));

            UpdateTwice(0);

            Assert.IsTrue(isNotified);
        }

        #endregion

    }

}
