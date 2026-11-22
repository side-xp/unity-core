using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Downloads resources on demand from editor updates: checks if they're already available, downloads them, retries the failed
    /// downloads, and handles the downloaded files as defined by each <see cref="DownloadRequest"/>.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Requests with the same key share the same download. Failed downloads are retried as decided by
    /// <see cref="DownloadRetryPolicy"/>.</item>
    /// <item>Each queue limits its own number of downloads in progress (see
    /// <see cref="EditorDownloadQueueSettings.MaxConcurrentDownloads"/>), and all the queues share
    /// <see cref="MaxTotalConcurrentDownloads"/>. Free slots are given to the queues in turn.</item>
    /// <item>From Unity 2020.1, the downloads of a queue can be displayed as a task in the Background Tasks window (see
    /// <see cref="EditorDownloadQueueSettings.ShouldReportProgress"/>).</item>
    /// <item>Downloads in progress are aborted before a domain reload, and the queues are lost with the other static data. Code
    /// that still needs its resources requests them again after the reload.</item>
    /// <item>Given up downloads can be remembered until the editor is closed (see
    /// <see cref="EditorDownloadQueueSettings.ShouldRememberGivenUp"/>).</item>
    /// </list>
    /// A queue is meant to live as long as the domain, as in a static field. Call <see cref="Dispose"/> to stop it before.
    /// </remarks>
    /// <example>
    /// <code>
    /// private static readonly EditorDownloadQueue s_downloads = new EditorDownloadQueue("Download thumbnails");
    ///
    /// public static void RequestThumbnail(string id)
    /// {
    ///     var request = new FileDownloadRequest($"https://example.com/{id}.png", $"{PathEditorUtility.UserCachePath}/MyTool/{id}.png");
    ///     s_downloads.Request(request, r =>
    ///     {
    ///         if (r.Status == EDownloadRequestStatus.Succeeded)
    ///             InternalEditorUtility.RepaintAllViews();
    ///     });
    /// }
    /// </code>
    /// </example>
    public class EditorDownloadQueue : IDisposable
    {

        #region Fields

        /// <summary>
        /// Default value of <see cref="MaxTotalConcurrentDownloads"/>.
        /// </summary>
        public const int DefaultMaxTotalConcurrentDownloads = DownloadScheduler.DefaultMaxConcurrentDownloads;

        /// <summary>
        /// Prefix of the keys of the given up downloads in <see cref="SessionState"/>, followed by the name of the queue.
        /// </summary>
        private const string GivenUpSessionKeyPrefix = "SideXP.Core.EditorOnly.EditorDownloadQueue.GivenUp.";

        /// <summary>
        /// Separator of the given up download keys in <see cref="SessionState"/>.
        /// </summary>
        private const char GivenUpSeparator = '\n';

        /// <summary>
        /// Drives the queues of every <see cref="EditorDownloadQueue"/>.
        /// </summary>
        private static readonly DownloadScheduler s_scheduler = new DownloadScheduler();

        /// <summary>
        /// Whether the scheduler is updated from editor updates.
        /// </summary>
        private static bool s_isUpdating = false;

        /// <summary>
        /// The queues not disposed yet, so they can be updated and aborted together.
        /// </summary>
        private static readonly List<EditorDownloadQueue> s_queues = new List<EditorDownloadQueue>();

        /// <summary>
        /// The queue of this instance.
        /// </summary>
        private readonly DownloadQueue _queue = null;

        /// <summary>
        /// The settings of this queue.
        /// </summary>
        private readonly EditorDownloadQueueSettings _settings = null;

        /// <summary>
        /// The task of the downloads in the Background Tasks window, or null if it's not running.
        /// </summary>
        private EditorProgressTask _task = null;

        /// <summary>
        /// The number of requests completed since the task started.
        /// </summary>
        private int _completedCount = 0;

        /// <summary>
        /// The number of requests failed since the task started.
        /// </summary>
        private int _failedCount = 0;

        /// <summary>
        /// Whether this queue has been disposed.
        /// </summary>
        private bool _isDisposed = false;

        #endregion


        #region Lifecycle

        /// <summary>
        /// Creates a download queue, run from editor updates.
        /// </summary>
        /// <param name="name">
        /// The name of the queue, as displayed in the Background Tasks window (as <c>Download thumbnails</c>). It also identifies the
        /// given up downloads remembered in the session, so give a unique name to each queue.
        /// </param>
        /// <param name="settings">Defines how the queue runs its downloads. If null, the default settings are used.</param>
        /// <exception cref="ArgumentException">Thrown if the name is empty, or if a value of the settings is not valid.</exception>
        public EditorDownloadQueue(string name, EditorDownloadQueueSettings settings = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("The name of a download queue can't be empty.", nameof(name));

            Name = name;
            _settings = settings ?? new EditorDownloadQueueSettings();
            _queue = new DownloadQueue(_settings.MaxConcurrentDownloads, _settings.RetrySettings);
            _queue.Completed += OnRequestCompleted;
            RestoreGivenUp();

            s_scheduler.Add(_queue);
            s_queues.Add(this);
        }

        /// <summary>
        /// Stops the queue: aborts its downloads, and forgets its requests without calling their callbacks. The given up downloads
        /// remembered in the session are kept (see <see cref="ClearFailures"/>).
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            _queue.Clear();
            CancelTask();
            s_scheduler.Remove(_queue);
            s_queues.Remove(this);
            _isDisposed = true;
        }

        #endregion


        #region Public API

        /// <summary>
        /// Called when a request that was in the queue succeeds or fails, after its own callbacks.
        /// </summary>
        /// <remarks>
        /// Not called for the requests that complete right away (already available, or already given up), since they're never in
        /// the queue.
        /// </remarks>
        public event Action<DownloadRequest> RequestCompleted;

        /// <summary>
        /// The maximum number of downloads in progress at the same time, all queues included. Each queue also has its own maximum
        /// (see <see cref="EditorDownloadQueueSettings.MaxConcurrentDownloads"/>). Lowering it doesn't abort the downloads in
        /// progress.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value is less than 1.</exception>
        public static int MaxTotalConcurrentDownloads
        {
            get => s_scheduler.MaxConcurrentDownloads;
            set => s_scheduler.MaxConcurrentDownloads = value;
        }

        /// <summary>
        /// The name of the queue.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The number of requests waiting or downloading.
        /// </summary>
        public int Count => _queue.Count;

        /// <summary>
        /// The number of downloads in progress.
        /// </summary>
        public int DownloadingCount => _queue.DownloadingCount;

        /// <summary>
        /// Requests a resource: downloads it unless it's already available, pending, or given up.
        /// </summary>
        /// <param name="request">The requested resource.</param>
        /// <param name="onComplete">
        /// Called with the request in the queue when it succeeds or fails (see <see cref="DownloadRequest.Status"/>), or right away if
        /// the result is already known. Not called while failed downloads are retried. Can be null.
        /// </param>
        /// <returns>
        /// Returns the request that represents the resource: the given one, or the one already in the queue with the same key. Use
        /// it to follow the progress of the download.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if the request is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the key or the URL of the request is null or empty.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the request is already in another queue, or if this queue has been disposed.
        /// </exception>
        public DownloadRequest Request(DownloadRequest request, Action<DownloadRequest> onComplete = null)
        {
            if (_isDisposed)
                throw new InvalidOperationException($"The download queue \"{Name}\" has been disposed.");

            DownloadRequest queuedRequest = _queue.Request(request, onComplete);
            if (_queue.Count > 0)
            {
                if (_task == null && _settings.ShouldReportProgress)
                    StartTask();

                StartUpdating();
            }

            return queuedRequest;
        }

        /// <summary>
        /// Checks if a request is waiting or downloading.
        /// </summary>
        /// <param name="key">The key of the request.</param>
        /// <returns>Returns true if a request with this key is in the queue.</returns>
        public bool IsPending(string key)
        {
            return _queue.IsPending(key);
        }

        /// <summary>
        /// Checks if a download has been given up by this queue (missing file, too many failures, or the handling asked to give up).
        /// </summary>
        /// <param name="key">The key of the request.</param>
        /// <returns>Returns true if requesting this key fails right away.</returns>
        public bool IsGivenUp(string key)
        {
            return !string.IsNullOrEmpty(key) && _queue.Policy.IsGivenUp(key);
        }

        /// <summary>
        /// Forgets the failures of every download, including the given up ones remembered in the session, so they can be attempted
        /// again.
        /// </summary>
        public void ClearFailures()
        {
            _queue.Policy.Clear();
            SessionState.EraseString(GivenUpSessionKey);
        }

        #endregion


        #region Private API

        /// <summary>
        /// The key of the given up downloads of this queue in <see cref="SessionState"/>.
        /// </summary>
        private string GivenUpSessionKey => $"{GivenUpSessionKeyPrefix}{Name}";

        /// <summary>
        /// Restores the downloads given up before the last domain reload, if they're remembered.
        /// </summary>
        private void RestoreGivenUp()
        {
            if (!_settings.ShouldRememberGivenUp)
                return;

            foreach (string key in SessionState.GetString(GivenUpSessionKey, string.Empty).Split(GivenUpSeparator))
            {
                if (!string.IsNullOrEmpty(key))
                    _queue.Policy.GiveUp(key);
            }
        }

        /// <summary>
        /// Called when a request that was in the queue succeeds or fails: counts it for the task, remembers it if it's given up, and
        /// notifies the listeners.
        /// </summary>
        /// <param name="request">The completed request.</param>
        private void OnRequestCompleted(DownloadRequest request)
        {
            _completedCount++;
            if (request.Status == EDownloadRequestStatus.Failed)
            {
                _failedCount++;
                if (_settings.ShouldRememberGivenUp)
                    SessionState.SetString(GivenUpSessionKey, string.Join(GivenUpSeparator.ToString(), _queue.Policy.GivenUpKeys));
            }

            try
            {
                RequestCompleted?.Invoke(request);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        /// <summary>
        /// Updates the task of the queue after the scheduler update: reports its progress, or ends it if the queue is empty.
        /// </summary>
        /// <param name="time">The current time, in seconds.</param>
        private void OnUpdated(double time)
        {
            if (_queue.Count > 0)
                ReportTask(time);
            else
                FinishTask();
        }

        #endregion


        #region Editor updates

        /// <summary>
        /// Starts updating the scheduler from editor updates, unless it's already updated.
        /// </summary>
        private static void StartUpdating()
        {
            if (s_isUpdating)
                return;

            EditorApplication.update += Update;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            s_isUpdating = true;
        }

        /// <summary>
        /// Stops updating the scheduler.
        /// </summary>
        private static void StopUpdating()
        {
            EditorApplication.update -= Update;
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            s_isUpdating = false;
        }

        /// <summary>
        /// Updates the scheduler and the tasks of the queues, and stops once every queue is empty.
        /// </summary>
        private static void Update()
        {
            double time = EditorApplication.timeSinceStartup;
            s_scheduler.Update(time);

            foreach (EditorDownloadQueue queue in s_queues.ToArray())
                queue.OnUpdated(time);

            if (s_scheduler.Count == 0)
                StopUpdating();
        }

        /// <summary>
        /// Aborts the downloads in progress before scripts are reloaded, since they can't survive a domain reload.
        /// </summary>
        private static void OnBeforeAssemblyReload()
        {
            foreach (EditorDownloadQueue queue in s_queues)
            {
                queue._queue.AbortDownloads();
                queue.CancelTask();
            }

            StopUpdating();
        }

        #endregion


        #region Task

        /// <summary>
        /// Starts the task of the downloads.
        /// </summary>
        private void StartTask()
        {
            _completedCount = 0;
            _failedCount = 0;
            _task = new EditorProgressTask(Name, $"0 / {_queue.Count}");
        }

        /// <summary>
        /// Reports the progress of the downloads: the number of requests done out of all the requested ones, and the current one.
        /// </summary>
        /// <param name="time">The current time, in seconds.</param>
        private void ReportTask(double time)
        {
            if (_task == null)
                return;

            int totalCount = _completedCount + _queue.Count;
            string progress = $"{_completedCount} / {totalCount}";
            DownloadRequest currentRequest = _queue.CurrentRequest;

            string description;
            if (_queue.Policy.IsPaused(time))
                description = $"{progress}: waiting for the network";
            else if (currentRequest != null)
                description = $"{progress}: {currentRequest.DisplayName}";
            else
                description = $"{progress}: waiting to retry";

            _task.Report((float)_completedCount / totalCount, description);
        }

        /// <summary>
        /// Ends the task of the downloads once the queue is empty: failed if some downloads have been given up.
        /// </summary>
        private void FinishTask()
        {
            if (_task == null)
                return;

            if (_failedCount > 0)
                _task.Fail($"{_failedCount} of {_completedCount} downloads failed.");
            else
                _task.Succeed($"{_completedCount} files downloaded.");

            _task = null;
        }

        /// <summary>
        /// Ends the task of the downloads as cancelled, as before a domain reload.
        /// </summary>
        private void CancelTask()
        {
            if (_task == null)
                return;

            _task.Cancel();
            _task = null;
        }

        #endregion

    }

}
