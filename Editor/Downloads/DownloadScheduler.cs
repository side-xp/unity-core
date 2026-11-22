using System;
using System.Collections.Generic;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Drives several download queues, and limits the total number of downloads in progress at the same time.
    /// </summary>
    /// <remarks>
    /// Free download slots are given to the queues in turn, one at a time, and the first queue served changes on each update. So a
    /// queue with many requests doesn't hold back the others. Each queue still respects its own
    /// <see cref="DownloadQueue.MaxConcurrentDownloads"/>.<br/>
    /// The scheduler doesn't run by itself: its owner must call <see cref="Update(double)"/> regularly (as from editor updates).
    /// </remarks>
    internal class DownloadScheduler
    {

        #region Fields

        /// <summary>
        /// Default maximum number of downloads in progress at the same time, all queues included.
        /// </summary>
        internal const int DefaultMaxConcurrentDownloads = 20;

        /// <summary>
        /// The queues driven by this scheduler, in the order they've been added.
        /// </summary>
        private readonly List<DownloadQueue> _queues = new List<DownloadQueue>();

        /// <summary>
        /// The maximum number of downloads in progress at the same time, all queues included.
        /// </summary>
        private int _maxConcurrentDownloads = DefaultMaxConcurrentDownloads;

        /// <summary>
        /// The index of the queue served first on the next update.
        /// </summary>
        private int _firstQueueIndex = 0;

        #endregion


        #region Lifecycle

        /// <summary>
        /// Creates a download scheduler.
        /// </summary>
        /// <param name="maxConcurrentDownloads">
        /// The maximum number of downloads in progress at the same time, all queues included. Must be at least 1.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the maximum number of downloads is less than 1.</exception>
        internal DownloadScheduler(int maxConcurrentDownloads = DefaultMaxConcurrentDownloads)
        {
            MaxConcurrentDownloads = maxConcurrentDownloads;
        }

        #endregion


        #region Internal API

        /// <summary>
        /// The maximum number of downloads in progress at the same time, all queues included. Lowering it doesn't abort the downloads
        /// in progress: new ones are started once there are less of them.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value is less than 1.</exception>
        internal int MaxConcurrentDownloads
        {
            get => _maxConcurrentDownloads;
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "A scheduler must allow at least one download at a time.");

                _maxConcurrentDownloads = value;
            }
        }

        /// <summary>
        /// The number of requests waiting or downloading, all queues included.
        /// </summary>
        internal int Count
        {
            get
            {
                int count = 0;
                foreach (DownloadQueue queue in _queues)
                    count += queue.Count;

                return count;
            }
        }

        /// <summary>
        /// The number of downloads in progress, all queues included.
        /// </summary>
        internal int DownloadingCount
        {
            get
            {
                int count = 0;
                foreach (DownloadQueue queue in _queues)
                    count += queue.DownloadingCount;

                return count;
            }
        }

        /// <summary>
        /// Adds a queue to drive, unless it's already driven by this scheduler.
        /// </summary>
        /// <param name="queue">The queue to add.</param>
        /// <exception cref="ArgumentNullException">Thrown if the queue is null.</exception>
        internal void Add(DownloadQueue queue)
        {
            if (queue == null)
                throw new ArgumentNullException(nameof(queue));

            if (!_queues.Contains(queue))
                _queues.Add(queue);
        }

        /// <summary>
        /// Stops driving a queue. Its downloads in progress are not aborted.
        /// </summary>
        /// <param name="queue">The queue to remove.</param>
        internal void Remove(DownloadQueue queue)
        {
            _queues.Remove(queue);
        }

        /// <summary>
        /// Handles the completed downloads of every queue, then starts new downloads in the free slots.
        /// </summary>
        /// <param name="time">The current time, in seconds (as <see cref="UnityEditor.EditorApplication.timeSinceStartup"/>).</param>
        internal void Update(double time)
        {
            // Copied, since callbacks may add or remove queues
            DownloadQueue[] queues = _queues.ToArray();
            foreach (DownloadQueue queue in queues)
                queue.HandleCompletedDownloads(time);

            if (queues.Length == 0)
                return;

            int firstIndex = _firstQueueIndex % queues.Length;
            _firstQueueIndex = (firstIndex + 1) % queues.Length;

            int downloadingCount = DownloadingCount;
            bool hasStarted = true;
            while (hasStarted && downloadingCount < MaxConcurrentDownloads)
            {
                // One slot per queue on each pass, so the slots are shared between the queues that need them
                hasStarted = false;
                for (int i = 0; i < queues.Length && downloadingCount < MaxConcurrentDownloads; i++)
                {
                    if (queues[(firstIndex + i) % queues.Length].TryStartNext(time))
                    {
                        downloadingCount++;
                        hasStarted = true;
                    }
                }
            }
        }

        #endregion

    }

}
