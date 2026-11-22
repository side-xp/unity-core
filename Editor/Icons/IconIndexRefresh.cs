using System;
using System.Collections.Generic;
using System.IO;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Downloads the latest Tabler catalog, converts it into the index, cleans up the icon files that don't match it anymore, and
    /// writes it to the shared folder.
    /// </summary>
    /// <remarks>
    /// The job doesn't run by itself: its owner must call <see cref="Update"/> regularly (as from editor updates) until it's done.
    /// The conversion, the clean-up and the write are done in the same update, right after the catalog is downloaded.
    /// </remarks>
    internal class IconIndexRefresh
    {

        #region Subclasses

        /// <summary>
        /// Defines the current step of the job.
        /// </summary>
        internal enum EStep
        {

            /// <summary>
            /// The job has not been started yet.
            /// </summary>
            NotStarted,

            /// <summary>
            /// Getting the latest Tabler version.
            /// </summary>
            ResolvingVersion,

            /// <summary>
            /// Downloading the catalog for that version.
            /// </summary>
            DownloadingCatalog,

            /// <summary>
            /// The job is done, whether it succeeded or not.
            /// </summary>
            Done,

        }

        #endregion


        #region Fields

        /// <summary>
        /// Share of the overall progress done once the version is resolved. The catalog download takes most of the time.
        /// </summary>
        private const float VersionProgressShare = 0.1f;

        /// <summary>
        /// Time after which the catalog download fails, in seconds. Larger than <see cref="WebDownloader.DefaultTimeoutSeconds"/>,
        /// since the catalog weighs a few megabytes and may be downloaded on a slow connection.
        /// </summary>
        private const int CatalogTimeoutSeconds = 60;

        /// <summary>
        /// The download in progress, or null if none.
        /// </summary>
        private IWebDownload _download = null;

        #endregion


        #region Public API

        /// <summary>
        /// The current step of the job.
        /// </summary>
        internal EStep Step { get; private set; } = EStep.NotStarted;

        /// <summary>
        /// Checks if the job is done, whether it succeeded or not.
        /// </summary>
        internal bool IsDone => Step == EStep.Done;

        /// <summary>
        /// Checks if the job succeeded. Only meaningful once <see cref="IsDone"/> is true.
        /// </summary>
        internal bool IsSuccess { get; private set; } = false;

        /// <summary>
        /// Checks if the job has been cancelled.
        /// </summary>
        internal bool IsCancelled { get; private set; } = false;

        /// <summary>
        /// Checks if the stored index was already up to date, so nothing has been downloaded but the version.
        /// </summary>
        internal bool IsUpToDate { get; private set; } = false;

        /// <summary>
        /// The reason why the job failed, or null if it didn't.
        /// </summary>
        internal string Error { get; private set; } = null;

        /// <summary>
        /// Why the job failed, so the caller can decide if and when to try again: the status of the failed download,
        /// <see cref="EWebDownloadStatus.Aborted"/> if the job has been cancelled, or <see cref="EWebDownloadStatus.Failed"/> if the
        /// downloaded files couldn't be read or written. <see cref="EWebDownloadStatus.Success"/> if the job didn't fail.
        /// </summary>
        internal EWebDownloadStatus FailureStatus { get; private set; } = EWebDownloadStatus.Success;

        /// <summary>
        /// The latest Tabler version, or null if it's not resolved yet.
        /// </summary>
        internal string Version { get; private set; } = null;

        /// <summary>
        /// The errors of the clean-up changes that failed. They don't fail the job, since the files left behind are harmless.
        /// </summary>
        internal List<string> CleanUpErrors { get; private set; } = new List<string>();

        /// <summary>
        /// The overall progress of the job, between 0 and 1.
        /// </summary>
        internal float Progress
        {
            get
            {
                switch (Step)
                {
                    case EStep.ResolvingVersion:
                        return GetDownloadProgress() * VersionProgressShare;

                    case EStep.DownloadingCatalog:
                        return VersionProgressShare + (GetDownloadProgress() * (1f - VersionProgressShare));

                    case EStep.Done:
                        return 1f;

                    default:
                        return 0f;
                }
            }
        }

        /// <summary>
        /// Describes the current step of the job, to display it.
        /// </summary>
        internal string Description
        {
            get
            {
                switch (Step)
                {
                    case EStep.ResolvingVersion:
                        return "Getting the latest Tabler version...";

                    case EStep.DownloadingCatalog:
                        return $"Downloading the Tabler {Version} catalog...";

                    case EStep.Done:
                        return IsSuccess ? $"Icons index updated to Tabler {Version}." : Error;

                    default:
                        return string.Empty;
                }
            }
        }

        /// <summary>
        /// Starts the job.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the job has already been started.</exception>
        internal void Start()
        {
            if (Step != EStep.NotStarted)
                throw new InvalidOperationException("The icons index refresh has already been started.");

            Step = EStep.ResolvingVersion;
            _download = WebDownloader.Start(TablerSource.LatestVersionUrl);
        }

        /// <summary>
        /// Moves the job forward if the current download is done.
        /// </summary>
        internal void Update()
        {
            if (IsDone || _download == null || !_download.IsDone)
                return;

            WebDownloadResult result = _download.Result;
            _download = null;

            switch (Step)
            {
                case EStep.ResolvingVersion:
                    OnVersionDownloaded(result);
                    break;

                case EStep.DownloadingCatalog:
                    OnCatalogDownloaded(result);
                    break;
            }
        }

        /// <summary>
        /// Cancels the job, and aborts the current download. The stored index and icon files are left as they are.
        /// </summary>
        internal void Cancel()
        {
            if (IsDone)
                return;

            if (_download != null)
                _download.Abort();

            _download = null;
            IsCancelled = true;
            Fail("The icons index refresh has been cancelled.", EWebDownloadStatus.Aborted);
        }

        #endregion


        #region Private API

        /// <summary>
        /// Called when the latest version has been downloaded: starts downloading the catalog, unless the stored index is already
        /// up to date.
        /// </summary>
        /// <param name="result">The outcome of the download.</param>
        private void OnVersionDownloaded(WebDownloadResult result)
        {
            if (!result.IsSuccess)
            {
                Fail($"The latest Tabler version couldn't be resolved: {result.Error}", result.Status);
                return;
            }

            if (!TablerSource.TryParseLatestVersion(result.GetText(), out string version))
            {
                Fail("The latest Tabler version couldn't be read from the jsDelivr response.");
                return;
            }

            Version = version;
            if (IconIndexStore.TryGetIndex(out IconIndex storedIndex) && storedIndex.version == version)
            {
                IsUpToDate = true;
                Succeed();
                return;
            }

            Step = EStep.DownloadingCatalog;
            _download = WebDownloader.Start(TablerSource.GetCatalogUrl(version), CatalogTimeoutSeconds);
        }

        /// <summary>
        /// Called when the catalog has been downloaded: converts it, cleans up the icon files, and writes the new index.
        /// </summary>
        /// <param name="result">The outcome of the download.</param>
        private void OnCatalogDownloaded(WebDownloadResult result)
        {
            if (!result.IsSuccess)
            {
                Fail($"The Tabler {Version} catalog couldn't be downloaded: {result.Error}", result.Status);
                return;
            }

            if (!TablerCatalogConverter.TryConvert(result.GetText(), Version, out IconIndex newIndex, out string error))
            {
                Fail(error);
                return;
            }

            // Without valid stored index, there are no known files to clean up
            IconIndexStore.TryGetIndex(out IconIndex oldIndex);
            CleanUpErrors = IconIndexDiff.Apply(IconIndexDiff.Compare(oldIndex, newIndex));

            try
            {
                IconIndexStore.Write(newIndex);
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                Fail($"The icons index couldn't be written: {exception.Message}");
                return;
            }

            Succeed();
        }

        /// <summary>
        /// Gets the progress of the current download.
        /// </summary>
        /// <returns>Returns the progress of the current download between 0 and 1, or 0 if there's none.</returns>
        private float GetDownloadProgress()
        {
            return _download != null ? _download.Progress : 0f;
        }

        /// <summary>
        /// Ends the job successfully.
        /// </summary>
        private void Succeed()
        {
            IsSuccess = true;
            Error = null;
            FailureStatus = EWebDownloadStatus.Success;
            Step = EStep.Done;
        }

        /// <summary>
        /// Ends the job with an error.
        /// </summary>
        /// <param name="error">The reason why the job failed.</param>
        /// <param name="status">
        /// Why the job failed (see <see cref="FailureStatus"/>). <see cref="EWebDownloadStatus.Success"/> is replaced by
        /// <see cref="EWebDownloadStatus.Failed"/>.
        /// </param>
        private void Fail(string error, EWebDownloadStatus status = EWebDownloadStatus.Failed)
        {
            IsSuccess = false;
            Error = error;
            FailureStatus = status != EWebDownloadStatus.Success ? status : EWebDownloadStatus.Failed;
            Step = EStep.Done;
        }

        #endregion

    }

}
