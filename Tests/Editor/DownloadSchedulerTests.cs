using System;
using System.Collections.Generic;

using NUnit.Framework;

using SideXP.Core.EditorOnly;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="DownloadScheduler"/>.
    /// </summary>
    /// <remarks>
    /// Downloads are replaced by <see cref="FakeWebDownload"/> that stay in progress until completed by the tests, and requests by
    /// <see cref="FakeDownloadRequest"/>.
    /// </remarks>
    public class DownloadSchedulerTests
    {

        /// <summary>
        /// The downloads started by the queues, in order.
        /// </summary>
        private List<FakeWebDownload> _downloads = null;

        /// <summary>
        /// Replaces the downloads by a fake server, of which the downloads stay in progress.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _downloads = new List<FakeWebDownload>();
            WebDownloader.DownloadOverride = url =>
            {
                var download = new FakeWebDownload(url, WebDownloadResult.Success(new byte[] { 1 }), true);
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
        /// Creates a queue driven by a scheduler, with requests waiting in it.
        /// </summary>
        /// <param name="scheduler">The scheduler that drives the queue.</param>
        /// <param name="name">The name of the queue, used as prefix of its request keys.</param>
        /// <param name="requestCount">The number of requests to add to the queue.</param>
        /// <param name="maxConcurrentDownloads">The maximum number of downloads of the queue in progress at the same time.</param>
        /// <returns>Returns the created queue.</returns>
        private static DownloadQueue AddQueue(DownloadScheduler scheduler, string name, int requestCount, int maxConcurrentDownloads)
        {
            var queue = new DownloadQueue(maxConcurrentDownloads);
            for (int i = 0; i < requestCount; i++)
                queue.Request(new FakeDownloadRequest($"{name}-{i}"), null);

            scheduler.Add(queue);
            return queue;
        }

        /// <summary>
        /// No more downloads than the global maximum run at the same time, even if each queue allows more.
        /// </summary>
        [Test]
        public void Update_ManyQueues_LimitsTotalDownloads()
        {
            var scheduler = new DownloadScheduler(5);
            AddQueue(scheduler, "a", 10, 4);
            AddQueue(scheduler, "b", 10, 4);
            AddQueue(scheduler, "c", 10, 4);

            scheduler.Update(0);

            Assert.AreEqual(5, scheduler.DownloadingCount);
            Assert.AreEqual(5, _downloads.Count);
        }

        /// <summary>
        /// Free slots are shared between the queues, instead of all going to the first one.
        /// </summary>
        [Test]
        public void Update_TwoBusyQueues_SharesSlots()
        {
            var scheduler = new DownloadScheduler(4);
            DownloadQueue first = AddQueue(scheduler, "a", 10, 4);
            DownloadQueue second = AddQueue(scheduler, "b", 10, 4);

            scheduler.Update(0);

            Assert.AreEqual(2, first.DownloadingCount);
            Assert.AreEqual(2, second.DownloadingCount);
        }

        /// <summary>
        /// The slots not used by a queue (at its own maximum, or empty) go to the others.
        /// </summary>
        [Test]
        public void Update_QueueAtItsMaximum_GivesSlotsToOthers()
        {
            var scheduler = new DownloadScheduler(6);
            DownloadQueue limited = AddQueue(scheduler, "a", 10, 1);
            AddQueue(scheduler, "empty", 0, 4);
            DownloadQueue other = AddQueue(scheduler, "b", 10, 10);

            scheduler.Update(0);

            Assert.AreEqual(1, limited.DownloadingCount);
            Assert.AreEqual(5, other.DownloadingCount);
        }

        /// <summary>
        /// The first queue served changes on each update, so a single free slot doesn't always go to the same queue.
        /// </summary>
        [Test]
        public void Update_SingleSlot_AlternatesBetweenQueues()
        {
            var scheduler = new DownloadScheduler(1);
            DownloadQueue first = AddQueue(scheduler, "a", 10, 4);
            DownloadQueue second = AddQueue(scheduler, "b", 10, 4);

            scheduler.Update(0);
            Assert.AreEqual(1, first.DownloadingCount);

            _downloads[0].Complete();
            scheduler.Update(0);
            Assert.AreEqual(0, first.DownloadingCount);
            Assert.AreEqual(1, second.DownloadingCount);
        }

        /// <summary>
        /// Lowering the maximum doesn't abort the downloads in progress, but no new one starts until there are less of them.
        /// </summary>
        [Test]
        public void MaxConcurrentDownloads_Lowered_KeepsDownloadsInProgress()
        {
            var scheduler = new DownloadScheduler(4);
            DownloadQueue queue = AddQueue(scheduler, "a", 10, 4);
            scheduler.Update(0);

            scheduler.MaxConcurrentDownloads = 2;
            _downloads[0].Complete();
            scheduler.Update(0);

            Assert.AreEqual(3, queue.DownloadingCount);
            Assert.IsFalse(_downloads.Exists(download => download.IsAborted));
        }

        /// <summary>
        /// A scheduler must allow at least one download at a time.
        /// </summary>
        [Test]
        public void MaxConcurrentDownloads_LessThanOne_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new DownloadScheduler(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new DownloadScheduler().MaxConcurrentDownloads = 0);
        }

        /// <summary>
        /// A removed queue is not driven anymore.
        /// </summary>
        [Test]
        public void Remove_StopsDrivingQueue()
        {
            var scheduler = new DownloadScheduler();
            DownloadQueue queue = AddQueue(scheduler, "a", 1, 4);

            scheduler.Remove(queue);
            scheduler.Update(0);

            Assert.AreEqual(0, queue.DownloadingCount);
            Assert.AreEqual(0, scheduler.Count);
        }

    }

}
