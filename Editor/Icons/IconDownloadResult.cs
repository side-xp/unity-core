using System;
using System.Text;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// The outcome of a completed download, with its data if it succeeded.
    /// </summary>
    internal readonly struct IconDownloadResult
    {

        #region Fields

        /// <summary>
        /// The outcome of the download.
        /// </summary>
        public EIconDownloadStatus Status { get; }

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
        private IconDownloadResult(EIconDownloadStatus status, long responseCode, byte[] data, string error)
        {
            Status = status;
            ResponseCode = responseCode;
            Data = data;
            Error = error;
        }

        /// <summary>
        /// Creates the outcome of a successful download.
        /// </summary>
        /// <param name="data">The downloaded data.</param>
        /// <param name="responseCode">The HTTP status code of the response.</param>
        /// <returns>Returns the created outcome.</returns>
        internal static IconDownloadResult Success(byte[] data, long responseCode = 200)
        {
            return new IconDownloadResult(EIconDownloadStatus.Success, responseCode, data ?? Array.Empty<byte>(), null);
        }

        /// <summary>
        /// Creates the outcome of a failed download.
        /// </summary>
        /// <param name="status">The outcome of the download. Must not be <see cref="EIconDownloadStatus.Success"/>.</param>
        /// <param name="error">The reason why the download failed.</param>
        /// <param name="responseCode">The HTTP status code of the response, or 0 if there was no response.</param>
        /// <returns>Returns the created outcome.</returns>
        internal static IconDownloadResult Failure(EIconDownloadStatus status, string error, long responseCode = 0)
        {
            return new IconDownloadResult(status, responseCode, null, string.IsNullOrEmpty(error) ? status.ToString() : error);
        }

        #endregion


        #region Public API

        /// <summary>
        /// Checks if the download succeeded.
        /// </summary>
        public bool IsSuccess => Status == EIconDownloadStatus.Success;

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
