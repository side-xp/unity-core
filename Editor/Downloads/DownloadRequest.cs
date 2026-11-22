using System;
using System.Collections.Generic;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// A resource to download on demand with an <see cref="EditorDownloadQueue"/>: where to download it from, how to check if it's
    /// already available, and what to do with the downloaded file.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Requests are identified by their <see cref="Key"/>. Requesting a key that is already in the queue doesn't download it
    /// again: the request already in the queue is used instead.</item>
    /// <item><see cref="IsAvailable"/> is checked when the resource is requested, and again before each download, so a resource
    /// made available in the meantime (as by another editor) is not downloaded.</item>
    /// <item><see cref="Handle(byte[], out string)"/> is called on the main thread from editor updates, so keep it short.</item>
    /// <item>A request can only be in one queue at a time. Once done, it can be requested again.</item>
    /// </list>
    /// For common cases, use <see cref="FileDownloadRequest"/>, <see cref="MemoryDownloadRequest"/> or
    /// <see cref="DelegateDownloadRequest"/>.
    /// </remarks>
    /// <example>
    /// <code>
    /// public class ThumbnailRequest : DownloadRequest
    /// {
    ///     public ThumbnailRequest(string id) { Id = id; }
    ///
    ///     public string Id { get; }
    ///     public override string Key => Id;
    ///     public override string Url => $"https://example.com/thumbnails/{Id}.png";
    ///     public override bool IsAvailable() => File.Exists(GetPath(Id));
    ///
    ///     protected override EDownloadHandlingResult Handle(byte[] data, out string error)
    ///     {
    ///         // Process and store the downloaded file here
    ///         error = null;
    ///         return EDownloadHandlingResult.Success;
    ///     }
    /// }
    /// </code>
    /// </example>
    public abstract class DownloadRequest
    {

        #region Public API

        /// <summary>
        /// Identifies the requested resource, as its URL or the path where it's stored. Requests with the same key share the same
        /// download, and the same failures. Can't be null or empty.
        /// </summary>
        public abstract string Key { get; }

        /// <summary>
        /// The URL of the file to download. Can't be null or empty.
        /// </summary>
        public abstract string Url { get; }

        /// <summary>
        /// Describes the requested resource, as displayed in the progress of the downloads. Uses <see cref="Url"/> by default.
        /// </summary>
        public virtual string DisplayName => Url;

        /// <summary>
        /// The time after which the download fails, in seconds, or 0 for no timeout. Uses
        /// <see cref="WebDownloader.DefaultTimeoutSeconds"/> by default.
        /// </summary>
        public virtual int TimeoutSeconds => WebDownloader.DefaultTimeoutSeconds;

        /// <summary>
        /// The state of the request.
        /// </summary>
        public EDownloadRequestStatus Status { get; internal set; } = EDownloadRequestStatus.None;

        /// <summary>
        /// Checks if the request succeeded or failed.
        /// </summary>
        public bool IsDone => Status == EDownloadRequestStatus.Succeeded || Status == EDownloadRequestStatus.Failed;

        /// <summary>
        /// The progress of the current download between 0 and 1, or 1 if the request succeeded.
        /// </summary>
        public float Progress
        {
            get
            {
                if (Status == EDownloadRequestStatus.Succeeded)
                    return 1f;

                return Download != null ? Download.Progress : 0f;
            }
        }

        /// <summary>
        /// The reason why the last attempt failed, or null if none failed. Set while the download is waiting to be retried, and when
        /// it has been given up.
        /// </summary>
        public string Error { get; internal set; } = null;

        /// <summary>
        /// Checks if the requested resource is already available, so it doesn't need to be downloaded. Never available by default.
        /// </summary>
        /// <remarks>
        /// Called when the resource is requested, and before each download. Exceptions are logged, and count as not available.
        /// </remarks>
        /// <returns>Returns true if the resource is available.</returns>
        public virtual bool IsAvailable()
        {
            return false;
        }

        #endregion


        #region Protected API

        /// <summary>
        /// Handles the downloaded file: processes it, stores it, or keeps it in memory.
        /// </summary>
        /// <remarks>
        /// Called on the main thread, from editor updates. Exceptions are logged, and count as <see cref="EDownloadHandlingResult.Retry"/>.
        /// </remarks>
        /// <param name="data">The content of the downloaded file.</param>
        /// <param name="error">Outputs the reason why the file couldn't be handled, or null if it has been.</param>
        /// <returns>Returns the outcome of the handling.</returns>
        protected internal abstract EDownloadHandlingResult Handle(byte[] data, out string error);

        #endregion


        #region Internal API

        /// <summary>
        /// The queue the request is in, or null if it's not in a queue.
        /// </summary>
        internal DownloadQueue Queue { get; set; } = null;

        /// <summary>
        /// The download in progress, or null if the request is not downloading.
        /// </summary>
        internal IWebDownload Download { get; set; } = null;

        /// <summary>
        /// The functions to call when the request succeeds or fails.
        /// </summary>
        internal List<Action<DownloadRequest>> Callbacks { get; } = new List<Action<DownloadRequest>>();

        #endregion

    }

}
