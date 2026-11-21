using System;

using UnityEngine.Networking;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Starts the downloads made for the editor icons.
    /// </summary>
    /// <remarks>
    /// Downloads use <see cref="UnityWebRequest"/>, unless <see cref="DownloadOverride"/> is set, so tests don't depend on the network.
    /// </remarks>
    internal static class IconDownloader
    {

        #region Subclasses

        /// <summary>
        /// A download made with <see cref="UnityWebRequest"/>.
        /// </summary>
        private class WebDownload : IIconDownload
        {

            /// <summary>
            /// The request in progress, or null once it's done (it's disposed as soon as its outcome is known).
            /// </summary>
            private UnityWebRequest _request = null;

            /// <summary>
            /// The outcome of the download, or null if it's not done yet.
            /// </summary>
            private IconDownloadResult? _result = null;

            /// <summary>
            /// Starts downloading a file.
            /// </summary>
            /// <param name="url">The URL of the file to download.</param>
            public WebDownload(string url)
            {
                _request = UnityWebRequest.Get(url);
                _request.timeout = TimeoutSeconds;
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
            public IconDownloadResult Result
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
                Finish(IconDownloadResult.Failure(EIconDownloadStatus.Aborted, "The download has been aborted."));
            }

            /// <summary>
            /// Stores the outcome of the download, and disposes the request.
            /// </summary>
            /// <param name="result">The outcome of the download.</param>
            private void Finish(IconDownloadResult result)
            {
                _result = result;
                _request.Dispose();
                _request = null;
            }

        }

        #endregion


        #region Fields

        /// <summary>
        /// Time after which a download is considered failed, in seconds. Large enough for the catalog (a few megabytes) on a slow
        /// connection.
        /// </summary>
        internal const int TimeoutSeconds = 60;

        /// <summary>
        /// HTTP status code of a missing file.
        /// </summary>
        private const long NotFoundStatusCode = 404;

        #endregion


        #region Public API

        /// <summary>
        /// Starts the downloads instead of <see cref="UnityWebRequest"/>, or null to use it. Meant for tests.
        /// </summary>
        internal static Func<string, IIconDownload> DownloadOverride { get; set; } = null;

        /// <summary>
        /// Starts downloading a file.
        /// </summary>
        /// <param name="url">The URL of the file to download.</param>
        /// <returns>Returns the download in progress.</returns>
        internal static IIconDownload Start(string url)
        {
            return DownloadOverride != null ? DownloadOverride(url) : new WebDownload(url);
        }

        #endregion


        #region Private API

        /// <summary>
        /// Gets the outcome of a completed request.
        /// </summary>
        /// <param name="request">The completed request.</param>
        /// <returns>Returns the outcome of the request.</returns>
        private static IconDownloadResult MakeResult(UnityWebRequest request)
        {
#if UNITY_2020_2_OR_NEWER
            bool isNetworkError = request.result == UnityWebRequest.Result.ConnectionError;
            bool isFailed = request.result != UnityWebRequest.Result.Success;
#else
            bool isNetworkError = request.isNetworkError;
            bool isFailed = request.isHttpError || !string.IsNullOrEmpty(request.error);
#endif

            if (isNetworkError)
                return IconDownloadResult.Failure(EIconDownloadStatus.NetworkError, request.error);

            if (request.responseCode == NotFoundStatusCode)
                return IconDownloadResult.Failure(EIconDownloadStatus.NotFound, request.error, request.responseCode);

            if (isFailed)
                return IconDownloadResult.Failure(EIconDownloadStatus.Failed, request.error, request.responseCode);

            return IconDownloadResult.Success(request.downloadHandler.data, request.responseCode);
        }

        #endregion

    }

}
