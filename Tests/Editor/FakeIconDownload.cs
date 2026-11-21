using SideXP.Core.EditorOnly;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// A download that doesn't use the network, with an outcome set by the test.
    /// </summary>
    /// <remarks>
    /// Use it through <see cref="IconDownloader.DownloadOverride"/>. A download created as pending stays in progress until
    /// <see cref="Complete"/> is called.
    /// </remarks>
    internal class FakeIconDownload : IIconDownload
    {

        #region Fields

        /// <summary>
        /// Progress reported while the download is pending.
        /// </summary>
        private const float PendingProgress = 0.5f;

        /// <summary>
        /// The outcome of the download.
        /// </summary>
        private IconDownloadResult _result;

        #endregion


        #region Lifecycle

        /// <summary>
        /// Creates a fake download.
        /// </summary>
        /// <param name="url">The URL of the downloaded file.</param>
        /// <param name="result">The outcome of the download.</param>
        /// <param name="isPending">If enabled, the download stays in progress until <see cref="Complete"/> is called.</param>
        public FakeIconDownload(string url, IconDownloadResult result, bool isPending = false)
        {
            Url = url;
            _result = result;
            IsDone = !isPending;
        }

        #endregion


        #region Public API

        /// <summary>
        /// The URL of the downloaded file.
        /// </summary>
        public string Url { get; }

        /// <summary>
        /// Checks if <see cref="Abort"/> has been called before the download was done.
        /// </summary>
        public bool IsAborted { get; private set; } = false;

        /// <inheritdoc/>
        public bool IsDone { get; private set; }

        /// <inheritdoc/>
        public float Progress => IsDone ? 1f : PendingProgress;

        /// <inheritdoc/>
        public IconDownloadResult Result => _result;

        /// <summary>
        /// Ends a pending download with its outcome.
        /// </summary>
        public void Complete()
        {
            IsDone = true;
        }

        /// <inheritdoc/>
        public void Abort()
        {
            if (IsDone)
                return;

            IsAborted = true;
            IsDone = true;
            _result = IconDownloadResult.Failure(EIconDownloadStatus.Aborted, "Aborted by the test.");
        }

        #endregion

    }

}
