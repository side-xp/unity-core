namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Defines how an <see cref="EditorDownloadQueue"/> runs its downloads.
    /// </summary>
    /// <remarks>
    /// Every value has a default, so only the ones to change need to be set. The settings are read when the queue is created.
    /// </remarks>
    /// <example>
    /// <code>
    /// var settings = new EditorDownloadQueueSettings
    /// {
    ///     MaxConcurrentDownloads = 2,
    ///     RetrySettings = new DownloadRetrySettings { MaxAttempts = 3 },
    /// };
    /// var queue = new EditorDownloadQueue("Download thumbnails", settings);
    /// </code>
    /// </example>
    public class EditorDownloadQueueSettings
    {

        #region Fields

        /// <summary>
        /// Default value of <see cref="MaxConcurrentDownloads"/>. A few parallel downloads hide the latency of each request, without
        /// flooding the server when many resources are requested at once.
        /// </summary>
        public const int DefaultMaxConcurrentDownloads = DownloadQueue.DefaultMaxConcurrentDownloads;

        #endregion


        #region Public API

        /// <summary>
        /// Maximum number of downloads of this queue in progress at the same time. Must be at least 1. All the queues also share
        /// <see cref="EditorDownloadQueue.MaxTotalConcurrentDownloads"/>.
        /// </summary>
        public int MaxConcurrentDownloads { get; set; } = DefaultMaxConcurrentDownloads;

        /// <summary>
        /// Defines when failed downloads are attempted again. If null, the default settings are used.
        /// </summary>
        public DownloadRetrySettings RetrySettings { get; set; } = null;

        /// <summary>
        /// If enabled, the downloads of the queue are displayed as a task in the Background Tasks window (from Unity 2020.1), which
        /// ends when the queue is empty, and fails if some downloads have been given up.
        /// </summary>
        public bool ShouldReportProgress { get; set; } = true;

        /// <summary>
        /// If enabled, the given up downloads are remembered until the editor is closed (with <see cref="UnityEditor.SessionState"/>),
        /// so they're not attempted again after each domain reload. The keys of the requests must not contain line breaks.
        /// </summary>
        public bool ShouldRememberGivenUp { get; set; } = true;

        #endregion

    }

}
