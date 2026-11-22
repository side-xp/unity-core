using System;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Downloads a file, and handles it with the given functions, without writing a <see cref="DownloadRequest"/> subclass.
    /// </summary>
    /// <example>
    /// <code>
    /// var request = new DelegateDownloadRequest(
    ///     "my-tool/logo",
    ///     "https://example.com/logo.png",
    ///     data => { s_logo = LoadTexture(data); return EDownloadHandlingResult.Success; },
    ///     () => s_logo != null
    /// );
    /// queue.Request(request);
    /// </code>
    /// </example>
    public class DelegateDownloadRequest : DownloadRequest
    {

        #region Fields

        /// <summary>
        /// Handles the downloaded file.
        /// </summary>
        private readonly Func<byte[], EDownloadHandlingResult> _handle = null;

        /// <summary>
        /// Checks if the requested resource is already available, or null if it never is.
        /// </summary>
        private readonly Func<bool> _isAvailable = null;

        #endregion


        #region Lifecycle

        /// <summary>
        /// Creates a request handled by functions.
        /// </summary>
        /// <param name="key"><inheritdoc cref="DownloadRequest.Key" path="/summary"/></param>
        /// <param name="url">The URL of the file to download.</param>
        /// <param name="handle">
        /// Handles the downloaded file, and returns the outcome. Exceptions are logged, and count as
        /// <see cref="EDownloadHandlingResult.Retry"/>.
        /// </param>
        /// <param name="isAvailable">
        /// Checks if the requested resource is already available, so it doesn't need to be downloaded. If null, it never is.
        /// </param>
        /// <exception cref="ArgumentException">Thrown if the key or the URL is null or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown if the handling function is null.</exception>
        public DelegateDownloadRequest(string key, string url, Func<byte[], EDownloadHandlingResult> handle, Func<bool> isAvailable = null)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("The key of a request can't be null or empty.", nameof(key));

            if (string.IsNullOrEmpty(url))
                throw new ArgumentException("The URL of a download can't be null or empty.", nameof(url));

            Key = key;
            Url = url;
            _handle = handle ?? throw new ArgumentNullException(nameof(handle));
            _isAvailable = isAvailable;
        }

        #endregion


        #region Public API

        /// <inheritdoc/>
        public override string Key { get; }

        /// <inheritdoc/>
        public override string Url { get; }

        /// <inheritdoc/>
        public override bool IsAvailable()
        {
            return _isAvailable != null && _isAvailable();
        }

        #endregion


        #region Protected API

        /// <inheritdoc/>
        protected internal override EDownloadHandlingResult Handle(byte[] data, out string error)
        {
            // The queue uses a generic error if the file couldn't be handled
            error = null;
            return _handle(data);
        }

        #endregion

    }

}
