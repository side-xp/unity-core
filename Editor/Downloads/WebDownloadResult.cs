using System;
using System.Text;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// The outcome of a completed web download, with its data if it succeeded.
    /// </summary>
    public readonly struct WebDownloadResult
    {

        #region Fields

        /// <summary>
        /// The HTTP status code of a successful response.
        /// </summary>
        public const long OkResponseCode = 200;

        /// <summary>
        /// The outcome of the download.
        /// </summary>
        public EWebDownloadStatus Status { get; }

        /// <summary>
        /// The HTTP status code of the response, or 0 if there was no response.
        /// </summary>
        public long ResponseCode { get; }

        /// <summary>
        /// The downloaded data, or null if the download failed.
        /// </summary>
        public byte[] Data { get; }

        /// <summary>
        /// The reason why the download failed, or null if it succeeded.
        /// </summary>
        public string Error { get; }

        #endregion


        #region Lifecycle

        /// <summary>
        /// Creates the outcome of a download.
        /// </summary>
        /// <param name="status"><inheritdoc cref="Status" path="/summary"/></param>
        /// <param name="responseCode"><inheritdoc cref="ResponseCode" path="/summary"/></param>
        /// <param name="data"><inheritdoc cref="Data" path="/summary"/></param>
        /// <param name="error"><inheritdoc cref="Error" path="/summary"/></param>
        private WebDownloadResult(EWebDownloadStatus status, long responseCode, byte[] data, string error)
        {
            Status = status;
            ResponseCode = responseCode;
            Data = data;
            Error = error;
        }

        /// <summary>
        /// Creates the outcome of a successful download.
        /// </summary>
        /// <remarks>
        /// Meant for custom <see cref="IWebDownload"/> implementations, as fake downloads in tests.
        /// </remarks>
        /// <param name="data">The downloaded data. Null is replaced by an empty array.</param>
        /// <param name="responseCode">The HTTP status code of the response.</param>
        /// <returns>Returns the created outcome.</returns>
        public static WebDownloadResult Success(byte[] data, long responseCode = OkResponseCode)
        {
            return new WebDownloadResult(EWebDownloadStatus.Success, responseCode, data ?? Array.Empty<byte>(), null);
        }

        /// <summary>
        /// Creates the outcome of a failed download.
        /// </summary>
        /// <remarks>
        /// Meant for custom <see cref="IWebDownload"/> implementations, as fake downloads in tests.
        /// </remarks>
        /// <param name="status">The outcome of the download. Must not be <see cref="EWebDownloadStatus.Success"/>.</param>
        /// <param name="error">The reason why the download failed. If null or empty, the name of the status is used.</param>
        /// <param name="responseCode">The HTTP status code of the response, or 0 if there was no response.</param>
        /// <returns>Returns the created outcome.</returns>
        /// <exception cref="ArgumentException">Thrown if the status is <see cref="EWebDownloadStatus.Success"/>.</exception>
        public static WebDownloadResult Failure(EWebDownloadStatus status, string error, long responseCode = 0)
        {
            if (status == EWebDownloadStatus.Success)
                throw new ArgumentException($"Use {nameof(Success)}() for a successful download.", nameof(status));

            return new WebDownloadResult(status, responseCode, null, string.IsNullOrEmpty(error) ? status.ToString() : error);
        }

        #endregion


        #region Public API

        /// <summary>
        /// Checks if the download succeeded.
        /// </summary>
        public bool IsSuccess => Status == EWebDownloadStatus.Success;

        /// <summary>
        /// Gets the downloaded data as UTF-8 text.
        /// </summary>
        /// <returns>Returns the downloaded text, or null if the download failed.</returns>
        public string GetText()
        {
            return Data != null ? Encoding.UTF8.GetString(Data) : null;
        }

        #endregion

    }

}
