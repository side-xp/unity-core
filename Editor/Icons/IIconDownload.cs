namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// A download in progress, polled from editor updates until it's done.
    /// </summary>
    /// <remarks>
    /// Started with <see cref="IconDownloader.Start(string)"/>. Polling (instead of callbacks) lets the caller show progress, and
    /// abort the download at any time.
    /// </remarks>
    internal interface IIconDownload
    {

        /// <summary>
        /// Checks if the download is done, whether it succeeded or not.
        /// </summary>
        bool IsDone { get; }

        /// <summary>
        /// The progress of the download, between 0 and 1.
        /// </summary>
        float Progress { get; }

        /// <summary>
        /// The outcome of the download, only available once <see cref="IsDone"/> is true.
        /// </summary>
        IconDownloadResult Result { get; }

        /// <summary>
        /// Aborts the download if it's not done yet. Its outcome is then <see cref="EIconDownloadStatus.Aborted"/>.
        /// </summary>
        void Abort();

    }

}
