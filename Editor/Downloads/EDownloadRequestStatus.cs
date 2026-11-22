namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Defines the state of a <see cref="DownloadRequest"/>.
    /// </summary>
    public enum EDownloadRequestStatus
    {

        /// <summary>
        /// The request has not been passed to a queue yet.
        /// </summary>
        None,

        /// <summary>
        /// The request is in a queue, waiting for a free download slot, or for the delay before its next attempt.
        /// </summary>
        Waiting,

        /// <summary>
        /// The file is being downloaded.
        /// </summary>
        Downloading,

        /// <summary>
        /// The resource is available: it already was when requested, or it has been downloaded and handled.
        /// </summary>
        Succeeded,

        /// <summary>
        /// The download has been given up (missing file, too many failures, or the handling asked to give up). It won't be
        /// attempted again by the same queue, until its failures are cleared.
        /// </summary>
        Failed,

    }

}
