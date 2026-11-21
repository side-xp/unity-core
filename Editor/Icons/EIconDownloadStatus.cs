namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Defines the outcome of a download made for the editor icons.
    /// </summary>
    internal enum EIconDownloadStatus
    {

        /// <summary>
        /// The file has been downloaded.
        /// </summary>
        Success,

        /// <summary>
        /// The server answered that the file doesn't exist (HTTP 404). Retrying won't help.
        /// </summary>
        NotFound,

        /// <summary>
        /// The server couldn't be reached (no connection, proxy, timeout). Other downloads are likely to fail the same way.
        /// </summary>
        NetworkError,

        /// <summary>
        /// The server answered with another error, or the data couldn't be processed. Retrying later may help.
        /// </summary>
        Failed,

        /// <summary>
        /// The download has been aborted before it completed.
        /// </summary>
        Aborted,

    }

}
