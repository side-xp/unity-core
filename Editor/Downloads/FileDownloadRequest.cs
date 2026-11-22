using System;
using System.IO;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Downloads a file, and stores it at a given path, unless it's already there.
    /// </summary>
    /// <remarks>
    /// The file is written with <see cref="FileEditorUtility.WriteFileAtomic(string, byte[], bool)"/>, so several editors can
    /// download it at the same time. An existing file is kept, so delete it to download it again. Override
    /// <see cref="Process(byte[], out byte[], out string)"/> to change the content before it's written.
    /// </remarks>
    /// <example>
    /// <code>
    /// var request = new FileDownloadRequest("https://example.com/data.json", $"{PathEditorUtility.UserCachePath}/MyTool/data.json");
    /// queue.Request(request, r => Debug.Log(r.Status == EDownloadRequestStatus.Succeeded ? "Stored" : r.Error));
    /// </code>
    /// </example>
    public class FileDownloadRequest : DownloadRequest
    {

        #region Lifecycle

        /// <summary>
        /// Creates a request to download a file.
        /// </summary>
        /// <param name="url">The URL of the file to download.</param>
        /// <param name="filePath">The absolute path where to store the file. Also used as the key of the request.</param>
        /// <exception cref="ArgumentException">Thrown if the URL is empty, or if the path is empty or relative.</exception>
        public FileDownloadRequest(string url, string filePath)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentException("The URL of a download can't be null or empty.", nameof(url));

            if (string.IsNullOrEmpty(filePath) || !Path.IsPathRooted(filePath))
                throw new ArgumentException($"The path of a downloaded file must be absolute (\"{filePath}\").", nameof(filePath));

            Url = url;
            FilePath = filePath;
        }

        #endregion


        #region Public API

        /// <summary>
        /// The absolute path where the file is stored.
        /// </summary>
        public string FilePath { get; }

        /// <inheritdoc/>
        public override string Key => FilePath;

        /// <inheritdoc/>
        public override string Url { get; }

        /// <summary>
        /// The name of the stored file.
        /// </summary>
        public override string DisplayName => Path.GetFileName(FilePath);

        /// <summary>
        /// Checks if the file is already stored.
        /// </summary>
        /// <returns>Returns true if a file exists at <see cref="FilePath"/>.</returns>
        public override bool IsAvailable()
        {
            return File.Exists(FilePath);
        }

        #endregion


        #region Protected API

        /// <summary>
        /// Processes the downloaded file, then writes it at <see cref="FilePath"/>.
        /// </summary>
        /// <param name="data">The content of the downloaded file.</param>
        /// <param name="error">Outputs the reason why the file couldn't be processed or written, or null if it has been.</param>
        /// <returns>
        /// Returns the outcome of <see cref="Process(byte[], out byte[], out string)"/> if it didn't succeed. Otherwise, returns
        /// <see cref="EDownloadHandlingResult.Retry"/> if the file can't be written, or <see cref="EDownloadHandlingResult.Success"/>.
        /// </returns>
        protected internal override EDownloadHandlingResult Handle(byte[] data, out string error)
        {
            EDownloadHandlingResult result = Process(data, out byte[] content, out error);
            if (result != EDownloadHandlingResult.Success)
                return result;

            try
            {
                // An existing file has been written by another editor in the meantime, with the same content
                FileEditorUtility.WriteFileAtomic(FilePath, content, false);
                error = null;
                return EDownloadHandlingResult.Success;
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                error = $"The downloaded file couldn't be written to {FilePath}: {exception.Message}";
                return EDownloadHandlingResult.Retry;
            }
        }

        /// <summary>
        /// Processes the downloaded file before it's written. Writes the downloaded content as is by default.
        /// </summary>
        /// <param name="data">The content of the downloaded file.</param>
        /// <param name="content">Outputs the content to write, or null if the processing didn't succeed.</param>
        /// <param name="error">Outputs the reason why the processing didn't succeed, or null if it did.</param>
        /// <returns>Returns the outcome of the processing. The file is written only on success.</returns>
        protected virtual EDownloadHandlingResult Process(byte[] data, out byte[] content, out string error)
        {
            content = data;
            error = null;
            return EDownloadHandlingResult.Success;
        }

        #endregion

    }

}
