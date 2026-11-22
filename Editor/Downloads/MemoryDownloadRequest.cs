using System;
using System.Text;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Downloads a file, and keeps its content in memory.
    /// </summary>
    /// <remarks>
    /// Once downloaded, the content is available from <see cref="Data"/>, and the request is available: requesting it again
    /// completes right away. Create a new request to download the file again.
    /// </remarks>
    /// <example>
    /// <code>
    /// queue.Request(new MemoryDownloadRequest("https://example.com/news.json"), r =>
    /// {
    ///     if (r.Status == EDownloadRequestStatus.Succeeded)
    ///         Debug.Log(((MemoryDownloadRequest)r).GetText());
    /// });
    /// </code>
    /// </example>
    public class MemoryDownloadRequest : DownloadRequest
    {

        #region Lifecycle

        /// <summary>
        /// Creates a request to download a file in memory.
        /// </summary>
        /// <param name="url">The URL of the file to download. Also used as the key of the request.</param>
        /// <exception cref="ArgumentException">Thrown if the URL is null or empty.</exception>
        public MemoryDownloadRequest(string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentException("The URL of a download can't be null or empty.", nameof(url));

            Url = url;
        }

        #endregion


        #region Public API

        /// <inheritdoc/>
        public override string Key => Url;

        /// <inheritdoc/>
        public override string Url { get; }

        /// <summary>
        /// The content of the downloaded file, or null if it's not downloaded yet.
        /// </summary>
        public byte[] Data { get; private set; } = null;

        /// <summary>
        /// Checks if the file has already been downloaded by this request.
        /// </summary>
        /// <returns>Returns true if <see cref="Data"/> is set.</returns>
        public override bool IsAvailable()
        {
            return Data != null;
        }

        /// <summary>
        /// Gets the content of the downloaded file as UTF-8 text.
        /// </summary>
        /// <returns>Returns the downloaded text, or null if the file is not downloaded yet.</returns>
        public string GetText()
        {
            return Data != null ? Encoding.UTF8.GetString(Data) : null;
        }

        #endregion


        #region Protected API

        /// <summary>
        /// Keeps the content of the downloaded file in <see cref="Data"/>.
        /// </summary>
        /// <param name="data">The content of the downloaded file.</param>
        /// <param name="error">Outputs null, since keeping the content can't fail.</param>
        /// <returns>Returns <see cref="EDownloadHandlingResult.Success"/>.</returns>
        protected internal override EDownloadHandlingResult Handle(byte[] data, out string error)
        {
            Data = data;
            error = null;
            return EDownloadHandlingResult.Success;
        }

        #endregion

    }

}
