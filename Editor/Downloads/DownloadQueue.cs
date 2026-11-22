using System;
using System.Collections.Generic;

using UnityEngine;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Downloads requested resources, and retries the failed downloads, without depending on editor updates.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Requests with the same key share the same download, and their callbacks are all called.</item>
    /// <item>Failed downloads are retried as decided by <see cref="Policy"/>.</item>
    /// <item>No more than <see cref="MaxConcurrentDownloads"/> downloads run at the same time.</item>
    /// </list>
    /// The queue doesn't run by itself: it's driven by a <see cref="DownloadScheduler"/>, which handles its completed downloads
    /// and starts new ones. Callbacks are called from the scheduler updates, or right away if the result is already known.
    /// </remarks>
    internal class DownloadQueue
    {

        #region Fields

        /// <summary>
        /// Default maximum number of downloads in progress at the same time in a queue. A few parallel downloads hide the latency of
        /// each request, without flooding the server when many resources are requested at once.
        /// </summary>
        internal const int DefaultMaxConcurrentDownloads = 4;

        /// <summary>
        /// Error of a request that has been given up before being requested again.
        /// </summary>
        private const string GivenUpError = "The download has been given up after previous failures.";

        /// <summary>
        /// The requests waiting or downloading, in the order they've been requested.
        /// </summary>
        private readonly List<DownloadRequest> _requests = new List<DownloadRequest>();

        /// <summary>
        /// The requests waiting or downloading, by key.
        /// </summary>
        private readonly Dictionary<string, DownloadRequest> _requestsByKey = new Dictionary<string, DownloadRequest>();

        #endregion


        #region Lifecycle

        /// <summary>
        /// Creates a download queue.
        /// </summary>
        /// <param name="maxConcurrentDownloads">The maximum number of downloads in progress at the same time. Must be at least 1.</param>
        /// <param name="retrySettings">Defines when failed downloads are attempted again. If null, the default settings are used.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the maximum number of downloads is less than 1.</exception>
        /// <exception cref="ArgumentException">Thrown if a value of the retry settings is not valid.</exception>
        internal DownloadQueue(int maxConcurrentDownloads = DefaultMaxConcurrentDownloads, DownloadRetrySettings retrySettings = null)
        {
            if (maxConcurrentDownloads < 1)
            {
                string message = "A queue must allow at least one download at a time.";
                throw new ArgumentOutOfRangeException(nameof(maxConcurrentDownloads), maxConcurrentDownloads, message);
            }

            MaxConcurrentDownloads = maxConcurrentDownloads;
            Policy = new DownloadRetryPolicy(retrySettings);
        }

        #endregion


        #region Internal API

        /// <summary>
        /// Called when a request that was in the queue succeeds or fails, after its callbacks.
        /// </summary>
        /// <remarks>
        /// Not called for the requests completed right away, since they're never in the queue.
        /// </remarks>
        internal event Action<DownloadRequest> Completed;

        /// <summary>
        /// The maximum number of downloads in progress at the same time.
        /// </summary>
        internal int MaxConcurrentDownloads { get; }

        /// <summary>
        /// Decides when failed downloads can be attempted again.
        /// </summary>
        internal DownloadRetryPolicy Policy { get; }

        /// <summary>
        /// The number of requests waiting or downloading.
        /// </summary>
        internal int Count => _requests.Count;

        /// <summary>
        /// The number of downloads in progress.
        /// </summary>
        internal int DownloadingCount { get; private set; } = 0;

        /// <summary>
        /// The first request being downloaded, or null if none.
        /// </summary>
        internal DownloadRequest CurrentRequest => _requests.Find(request => request.Download != null);

        /// <summary>
        /// Checks if a request is waiting or downloading.
        /// </summary>
        /// <param name="key">The key of the request.</param>
        /// <returns>Returns true if a request with this key is in the queue.</returns>
        internal bool IsPending(string key)
        {
            return !string.IsNullOrEmpty(key) && _requestsByKey.ContainsKey(key);
        }

        /// <summary>
        /// Requests a resource: downloads it unless it's already available, pending, or given up.
        /// </summary>
        /// <param name="request">The requested resource.</param>
        /// <param name="onComplete">
        /// Called with the request in the queue when it succeeds or fails. Not called while failed downloads are retried. Can be
        /// null.
        /// </param>
        /// <returns>
        /// Returns the request in the queue: the given one, or the one already in the queue with the same key. If the result is
        /// already known, the given request is returned, already done.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if the request is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the key or the URL of the request is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the request is already in another queue.</exception>
        internal DownloadRequest Request(DownloadRequest request, Action<DownloadRequest> onComplete)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            string key = request.Key;
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("The key of a request can't be null or empty.", nameof(request));

            if (string.IsNullOrEmpty(request.Url))
                throw new ArgumentException("The URL of a request can't be null or empty.", nameof(request));

            if (_requestsByKey.TryGetValue(key, out DownloadRequest pendingRequest))
            {
                if (onComplete != null)
                    pendingRequest.Callbacks.Add(onComplete);

                return pendingRequest;
            }

            if (request.Queue != null)
                throw new InvalidOperationException($"The request \"{key}\" is already in another queue.");

            if (Policy.IsGivenUp(key))
            {
                SetDone(request, EDownloadRequestStatus.Failed, request.Error ?? GivenUpError);
                Notify(onComplete, request);
                return request;
            }

            if (IsAvailable(request))
            {
                SetDone(request, EDownloadRequestStatus.Succeeded, null);
                Notify(onComplete, request);
                return request;
            }

            request.Status = EDownloadRequestStatus.Waiting;
            request.Error = null;
            request.Queue = this;
            if (onComplete != null)
                request.Callbacks.Add(onComplete);

            _requests.Add(request);
            _requestsByKey[key] = request;
            return request;
        }

        /// <summary>
        /// Handles the completed downloads: the downloaded files are handled, and the failures are reported to the retry policy.
        /// </summary>
        /// <param name="time">The current time, in seconds (as <see cref="UnityEditor.EditorApplication.timeSinceStartup"/>).</param>
        internal void HandleCompletedDownloads(double time)
        {
            foreach (DownloadRequest request in _requests.ToArray())
            {
                if (request.Download != null && request.Download.IsDone)
                    HandleCompletedDownload(request, time);
            }
        }

        /// <summary>
        /// Starts downloading the first waiting request that can be attempted, unless the maximum number of downloads is reached.
        /// </summary>
        /// <remarks>
        /// Requests that became available in the meantime (as stored by another editor) are completed instead of being downloaded.
        /// </remarks>
        /// <param name="time">The current time, in seconds (as <see cref="UnityEditor.EditorApplication.timeSinceStartup"/>).</param>
        /// <returns>Returns true if a download has been started.</returns>
        internal bool TryStartNext(double time)
        {
            if (DownloadingCount >= MaxConcurrentDownloads)
                return false;

            foreach (DownloadRequest request in _requests.ToArray())
            {
                // Completed by a callback of another request
                if (request.Queue != this || request.Download != null || !Policy.CanAttempt(request.Key, time))
                    continue;

                if (IsAvailable(request))
                {
                    Complete(request, EDownloadRequestStatus.Succeeded, null);
                    continue;
                }

                try
                {
                    request.Download = WebDownloader.Start(request.Url, request.TimeoutSeconds);
                }
                catch (ArgumentException exception)
                {
                    Policy.GiveUp(request.Key);
                    Complete(request, EDownloadRequestStatus.Failed, exception.Message);
                    continue;
                }

                request.Status = EDownloadRequestStatus.Downloading;
                DownloadingCount++;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Aborts the downloads in progress, as before a domain reload. The requests stay in the queue, and are downloaded again
        /// later.
        /// </summary>
        internal void AbortDownloads()
        {
            foreach (DownloadRequest request in _requests)
            {
                if (request.Download == null)
                    continue;

                request.Download.Abort();
                request.Download = null;
                request.Status = EDownloadRequestStatus.Waiting;
            }

            DownloadingCount = 0;
        }

        /// <summary>
        /// Aborts the downloads in progress, and removes every request from the queue, without calling their callbacks. The failures
        /// of the retry policy are kept.
        /// </summary>
        internal void Clear()
        {
            AbortDownloads();
            foreach (DownloadRequest request in _requests)
            {
                request.Queue = null;
                request.Status = EDownloadRequestStatus.None;
                request.Callbacks.Clear();
            }

            _requests.Clear();
            _requestsByKey.Clear();
        }

        #endregion


        #region Private API

        /// <summary>
        /// Handles the completed download of a request: handles the downloaded file, or reports the failure to the retry policy.
        /// </summary>
        /// <param name="request">The request of which the download is done.</param>
        /// <param name="time">The current time, in seconds.</param>
        private void HandleCompletedDownload(DownloadRequest request, double time)
        {
            WebDownloadResult result = request.Download.Result;
            request.Download = null;
            request.Status = EDownloadRequestStatus.Waiting;
            DownloadingCount--;

            string key = request.Key;
            EWebDownloadStatus status = result.Status;
            string error = result.Error;
            if (result.IsSuccess)
            {
                switch (Handle(request, result.Data, out error))
                {
                    case EDownloadHandlingResult.Success:
                        Policy.ReportSuccess(key);
                        Complete(request, EDownloadRequestStatus.Succeeded, null);
                        return;

                    case EDownloadHandlingResult.GiveUp:
                        Policy.GiveUp(key);
                        Complete(request, EDownloadRequestStatus.Failed, error);
                        return;

                    // The downloaded file couldn't be handled, which may work on the next attempt
                    default:
                        status = EWebDownloadStatus.Failed;
                        break;
                }
            }

            request.Error = error;
            Policy.ReportFailure(key, status, time);
            if (Policy.IsGivenUp(key))
                Complete(request, EDownloadRequestStatus.Failed, error);
        }

        /// <summary>
        /// Removes a request from the queue, and calls its callbacks.
        /// </summary>
        /// <param name="request">The request to complete.</param>
        /// <param name="status">The final state of the request.</param>
        /// <param name="error">The reason why the request failed, or null if it succeeded.</param>
        private void Complete(DownloadRequest request, EDownloadRequestStatus status, string error)
        {
            _requests.Remove(request);
            _requestsByKey.Remove(request.Key);
            request.Queue = null;
            SetDone(request, status, error);

            Action<DownloadRequest>[] callbacks = request.Callbacks.ToArray();
            request.Callbacks.Clear();
            foreach (Action<DownloadRequest> callback in callbacks)
                Notify(callback, request);

            Notify(Completed, request);
        }

        /// <summary>
        /// Sets the final state of a request.
        /// </summary>
        /// <param name="request">The request to update.</param>
        /// <param name="status">The final state of the request.</param>
        /// <param name="error">The reason why the request failed, or null if it succeeded.</param>
        private static void SetDone(DownloadRequest request, EDownloadRequestStatus status, string error)
        {
            request.Status = status;
            request.Error = status == EDownloadRequestStatus.Failed
                ? error ?? "The download failed."
                : null;
        }

        /// <summary>
        /// Checks if the resource of a request is available, and logs the exceptions, so a faulty request doesn't break the queue.
        /// </summary>
        /// <param name="request">The request to check.</param>
        /// <returns>Returns true if the resource is available, or false if it's not or if the check threw an exception.</returns>
        private static bool IsAvailable(DownloadRequest request)
        {
            try
            {
                return request.IsAvailable();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return false;
            }
        }

        /// <summary>
        /// Handles a downloaded file, and logs the exceptions, so a faulty request doesn't break the queue.
        /// </summary>
        /// <param name="request">The request of which the file has been downloaded.</param>
        /// <param name="data">The content of the downloaded file.</param>
        /// <param name="error">Outputs the reason why the file couldn't be handled, or null if it has been.</param>
        /// <returns>Returns the outcome of the handling, or <see cref="EDownloadHandlingResult.Retry"/> if it threw an exception.</returns>
        private static EDownloadHandlingResult Handle(DownloadRequest request, byte[] data, out string error)
        {
            try
            {
                EDownloadHandlingResult result = request.Handle(data, out error);
                if (result != EDownloadHandlingResult.Success && string.IsNullOrEmpty(error))
                    error = "The downloaded file couldn't be handled.";

                return result;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                error = $"The downloaded file couldn't be handled: {exception.Message}";
                return EDownloadHandlingResult.Retry;
            }
        }

        /// <summary>
        /// Calls a callback, and logs its exceptions, so a faulty callback doesn't break the queue.
        /// </summary>
        /// <param name="callback">The callback to call. Can be null.</param>
        /// <param name="request">The completed request.</param>
        private static void Notify(Action<DownloadRequest> callback, DownloadRequest request)
        {
            if (callback == null)
                return;

            try
            {
                callback(request);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        #endregion

    }

}
