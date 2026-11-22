using System;

using UnityEngine.Networking;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Starts web downloads, polled until they're done.
    /// </summary>
    /// <remarks>
    /// Downloads use <see cref="UnityWebRequest"/>, and keep the downloaded data in memory. They don't survive a domain reload, so
    /// abort them before scripts are reloaded (see <see cref="UnityEditor.AssemblyReloadEvents.beforeAssemblyReload"/>).
    /// </remarks>
    /// <example>
    /// <code>
    /// IWebDownload download = WebDownloader.Start("https://example.com/data.json");
    /// // Then, from editor updates:
    /// if (download.IsDone &amp;&amp; download.Result.IsSuccess)
    ///     Debug.Log(download.Result.GetText());
    /// </code>
    /// </example>
    public static class WebDownloader
    {

        #region Subclasses

        /// <summary>
        /// A download made with <see cref="UnityWebRequest"/>.
        /// </summary>
        private class UnityWebDownload : IWebDownload
        {

            /// <summary>
            /// The request in progress, or null once it's done (it's disposed as soon as its outcome is known).
            /// </summary>
            private UnityWebRequest _request = null;

            /// <summary>
            /// The outcome of the download, or null if it's not done yet.
            /// </summary>
            private WebDownloadResult? _result = null;

            /// <summary>
            /// Starts downloading a file.
            /// </summary>
            /// <param name="url">The URL of the file to download.</param>
            /// <param name="timeoutSeconds">The time after which the download fails, in seconds, or 0 for no timeout.</param>
            public UnityWebDownload(string url, int timeoutSeconds)
            {
                _request = UnityWebRequest.Get(url);
                _request.timeout = timeoutSeconds;
                _request.SendWebRequest();
            }

            /// <inheritdoc/>
            public bool IsDone
            {
                get
                {
                    if (_result.HasValue)
                        return true;

                    if (!_request.isDone)
                        return false;

                    Finish(MakeResult(_request));
                    return true;
                }
            }

            /// <inheritdoc/>
            public float Progress => _result.HasValue ? 1f : _request.downloadProgress;

            /// <inheritdoc/>
            public WebDownloadResult Result
            {
                get
                {
                    if (!IsDone)
                        throw new InvalidOperationException("The result of a download is only available once it's done.");

                    return _result.Value;
                }
            }

            /// <inheritdoc/>
            public void Abort()
            {
                if (_result.HasValue)
                    return;

                _request.Abort();
                Finish(WebDownloadResult.Failure(EWebDownloadStatus.Aborted, "The download has been aborted."));
            }

            /// <summary>
            /// Stores the outcome of the download, and disposes the request.
            /// </summary>
            /// <param name="result">The outcome of the download.</param>
            private void Finish(WebDownloadResult result)
            {
                _result = result;
                _request.Dispose();
                _request = null;
            }

        }

        #endregion


        #region Fields

        /// <summary>
        /// Time after which a download fails by default, in seconds. Enough for small files on a slow connection: pass a larger
        /// timeout for large files.
        /// </summary>
        public const int DefaultTimeoutSeconds = 30;

        /// <summary>
        /// HTTP status code of a missing file.
        /// </summary>
        private const long NotFoundStatusCode = 404;

        #endregion


        #region Public API

        /// <summary>
        /// Starts the downloads instead of <see cref="UnityWebRequest"/>, or null to use it. Meant for tests, so they don't depend on
        /// the network.
        /// </summary>
        internal static Func<string, IWebDownload> DownloadOverride { get; set; } = null;

        /// <summary>
        /// Starts downloading a file.
        /// </summary>
        /// <param name="url">The URL of the file to download.</param>
        /// <param name="timeoutSeconds">The time after which the download fails, in seconds, or 0 for no timeout.</param>
        /// <returns>Returns the download in progress.</returns>
        /// <exception cref="ArgumentException">Thrown if the URL is null or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the timeout is negative.</exception>
        public static IWebDownload Start(string url, int timeoutSeconds = DefaultTimeoutSeconds)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentException("The URL of a download can't be null or empty.", nameof(url));

            if (timeoutSeconds < 0)
            {
                string message = "The timeout of a download can't be negative.";
                throw new ArgumentOutOfRangeException(nameof(timeoutSeconds), timeoutSeconds, message);
            }

            return DownloadOverride != null ? DownloadOverride(url) : new UnityWebDownload(url, timeoutSeconds);
        }

        #endregion


        #region Private API

        /// <summary>
        /// Gets the outcome of a completed request.
        /// </summary>
        /// <param name="request">The completed request.</param>
        /// <returns>Returns the outcome of the request.</returns>
        private static WebDownloadResult MakeResult(UnityWebRequest request)
        {
#if UNITY_2020_2_OR_NEWER
            bool isNetworkError = request.result == UnityWebRequest.Result.ConnectionError;
            bool isFailed = request.result != UnityWebRequest.Result.Success;
#else
            bool isNetworkError = request.isNetworkError;
            bool isFailed = request.isHttpError || !string.IsNullOrEmpty(request.error);
#endif

            if (isNetworkError)
                return WebDownloadResult.Failure(EWebDownloadStatus.NetworkError, request.error);

            if (request.responseCode == NotFoundStatusCode)
                return WebDownloadResult.Failure(EWebDownloadStatus.NotFound, request.error, request.responseCode);

            if (isFailed)
                return WebDownloadResult.Failure(EWebDownloadStatus.Failed, request.error, request.responseCode);

            return WebDownloadResult.Success(request.downloadHandler.data, request.responseCode);
        }

        #endregion

    }

}
