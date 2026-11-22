namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Defines the outcome of the handling of a downloaded file (see <see cref="DownloadRequest.Handle(byte[], out string)"/>).
    /// </summary>
    public enum EDownloadHandlingResult
    {

        /// <summary>
        /// The downloaded file has been handled (as processed and stored). The request succeeds.
        /// </summary>
        Success,

        /// <summary>
        /// The downloaded file couldn't be handled, but it may work on the next attempt (as when a file is locked). The download is
        /// retried as decided by the retry policy of the queue.
        /// </summary>
        Retry,

        /// <summary>
        /// The downloaded file can't be handled, and downloading it again won't help (as when the file is not in the expected
        /// format). The request fails right away.
        /// </summary>
        GiveUp,

    }

}
