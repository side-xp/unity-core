using System;
using System.Collections.Generic;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Decides when a failed download can be attempted again, and when every download must wait.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>A failed download is retried after a delay that doubles after each failure, up to
    /// <see cref="DownloadRetrySettings.MaxAttempts"/> attempts. Beyond, it's given up.</item>
    /// <item>A network error (no connection, proxy, timeout) pauses every download instead, since the others would fail the same
    /// way. The pause doubles after each consecutive network error, up to <see cref="DownloadRetrySettings.MaxNetworkPause"/>.</item>
    /// <item>A missing file (404) is given up right away.</item>
    /// <item>An aborted download is not a failure, and can be attempted again right away.</item>
    /// </list>
    /// Downloads are identified by a key chosen by the caller (as a URL, or a file path). Times are also given by the caller (in
    /// seconds, from any fixed origin, as <see cref="UnityEditor.EditorApplication.timeSinceStartup"/>), so the policy doesn't
    /// depend on a clock.
    /// </remarks>
    /// <example>
    /// <code>
    /// var policy = new DownloadRetryPolicy();
    /// double time = EditorApplication.timeSinceStartup;
    /// if (policy.CanAttempt(url, time))
    ///     download = WebDownloader.Start(url);
    ///
    /// // Once the download is done:
    /// if (download.Result.IsSuccess)
    ///     policy.ReportSuccess(url);
    /// else
    ///     policy.ReportFailure(url, download.Result.Status, EditorApplication.timeSinceStartup);
    /// </code>
    /// </example>
    public class DownloadRetryPolicy
    {

        #region Subclasses

        /// <summary>
        /// The failures of a download.
        /// </summary>
        private class Failures
        {

            /// <summary>
            /// The number of failed attempts.
            /// </summary>
            public int Count = 0;

            /// <summary>
            /// The time from which the next attempt can be made.
            /// </summary>
            public double NextAttemptTime = 0;

            /// <summary>
            /// Whether the download won't be attempted again.
            /// </summary>
            public bool IsGivenUp = false;

        }

        #endregion


        #region Fields

        /// <summary>
        /// The failures of each download, by key.
        /// </summary>
        private readonly Dictionary<string, Failures> _failures = new Dictionary<string, Failures>();

        /// <summary>
        /// The number of consecutive network errors, reset by any successful download.
        /// </summary>
        private int _networkErrorCount = 0;

        #endregion


        #region Lifecycle

        /// <summary>
        /// Creates a retry policy.
        /// </summary>
        /// <param name="settings">Defines when failed downloads are attempted again. If null, the default settings are used.</param>
        /// <exception cref="ArgumentException">Thrown if a value of the settings is not valid.</exception>
        public DownloadRetryPolicy(DownloadRetrySettings settings = null)
        {
            Settings = settings ?? new DownloadRetrySettings();
            ThrowIfInvalid(Settings);
        }

        #endregion


        #region Public API

        /// <summary>
        /// The settings of this policy.
        /// </summary>
        public DownloadRetrySettings Settings { get; }

        /// <summary>
        /// The time until which every download is paused after network errors.
        /// </summary>
        public double PausedUntil { get; private set; } = 0;

        /// <summary>
        /// The keys of the downloads that have been given up.
        /// </summary>
        public IEnumerable<string> GivenUpKeys
        {
            get
            {
                foreach (KeyValuePair<string, Failures> failures in _failures)
                {
                    if (failures.Value.IsGivenUp)
                        yield return failures.Key;
                }
            }
        }

        /// <summary>
        /// Checks if every download is paused after network errors.
        /// </summary>
        /// <param name="time">The current time, in seconds.</param>
        /// <returns>Returns true if no download should be started.</returns>
        public bool IsPaused(double time)
        {
            return time < PausedUntil;
        }

        /// <summary>
        /// Checks if a download has been given up (missing file, or too many failures).
        /// </summary>
        /// <param name="key">The key of the download.</param>
        /// <returns>Returns true if the download won't be attempted again.</returns>
        public bool IsGivenUp(string key)
        {
            return _failures.TryGetValue(key, out Failures failures) && failures.IsGivenUp;
        }

        /// <summary>
        /// Checks if a download can be attempted now.
        /// </summary>
        /// <param name="key">The key of the download.</param>
        /// <param name="time">The current time, in seconds.</param>
        /// <returns>Returns true if downloads are not paused, this one is not given up, and its retry delay is over.</returns>
        public bool CanAttempt(string key, double time)
        {
            if (IsPaused(time))
                return false;

            return !_failures.TryGetValue(key, out Failures failures) || (!failures.IsGivenUp && time >= failures.NextAttemptTime);
        }

        /// <summary>
        /// Gets the number of failed attempts of a download. Network errors and aborted downloads are not counted.
        /// </summary>
        /// <param name="key">The key of the download.</param>
        /// <returns>Returns the number of failed attempts.</returns>
        public int GetFailureCount(string key)
        {
            return _failures.TryGetValue(key, out Failures failures) ? failures.Count : 0;
        }

        /// <summary>
        /// Reports a successful download: forgets its failures, and resets the network pause.
        /// </summary>
        /// <param name="key">The key of the download.</param>
        public void ReportSuccess(string key)
        {
            _failures.Remove(key);
            _networkErrorCount = 0;
        }

        /// <summary>
        /// Reports a failed download.
        /// </summary>
        /// <param name="key">The key of the download.</param>
        /// <param name="status">The outcome of the download.</param>
        /// <param name="time">The time of the failure, in seconds.</param>
        /// <exception cref="ArgumentException">Thrown if the status is <see cref="EWebDownloadStatus.Success"/>.</exception>
        public void ReportFailure(string key, EWebDownloadStatus status, double time)
        {
            switch (status)
            {
                case EWebDownloadStatus.Success:
                    throw new ArgumentException($"Use {nameof(ReportSuccess)}() for a successful download.", nameof(status));

                // Not a failure of the download itself
                case EWebDownloadStatus.Aborted:
                    return;

                case EWebDownloadStatus.NotFound:
                    GiveUp(key);
                    return;

                case EWebDownloadStatus.NetworkError:
                    // Downloads started before the pause may fail while it's running, and must not extend it again
                    if (IsPaused(time))
                        return;

                    _networkErrorCount++;
                    PausedUntil = time + GetDoubledDelay(Settings.FirstNetworkPause, _networkErrorCount, Settings.MaxNetworkPause);
                    return;

                default:
                    Failures failures = GetOrAddFailures(key);
                    failures.Count++;
                    if (failures.Count >= Settings.MaxAttempts)
                        failures.IsGivenUp = true;
                    else
                        failures.NextAttemptTime = time + GetDoubledDelay(Settings.FirstRetryDelay, failures.Count, double.MaxValue);
                    return;
            }
        }

        /// <summary>
        /// Gives up a download, as when restoring the downloads given up before a domain reload.
        /// </summary>
        /// <param name="key">The key of the download.</param>
        public void GiveUp(string key)
        {
            GetOrAddFailures(key).IsGivenUp = true;
        }

        /// <summary>
        /// Forgets all failures and the network pause, so every download can be attempted again.
        /// </summary>
        public void Clear()
        {
            _failures.Clear();
            _networkErrorCount = 0;
            PausedUntil = 0;
        }

        #endregion


        #region Private API

        /// <summary>
        /// Gets the failures of a download, or creates them.
        /// </summary>
        /// <param name="key">The key of the download.</param>
        /// <returns>Returns the failures of the download.</returns>
        private Failures GetOrAddFailures(string key)
        {
            if (!_failures.TryGetValue(key, out Failures failures))
            {
                failures = new Failures();
                _failures[key] = failures;
            }
            return failures;
        }

        /// <summary>
        /// Computes a delay that doubles after each failure.
        /// </summary>
        /// <example><c>GetDoubledDelay(2, 3, 60)</c> returns <c>8</c>.</example>
        /// <param name="firstDelay">The delay after the first failure.</param>
        /// <param name="failureCount">The number of failures, starting at 1.</param>
        /// <param name="maxDelay">The maximum delay.</param>
        /// <returns>Returns the delay after the given number of failures.</returns>
        private static double GetDoubledDelay(double firstDelay, int failureCount, double maxDelay)
        {
            // Qualified, since SideXP.Core.Math (obsolete) hides System.Math in this namespace
            return System.Math.Min(firstDelay * System.Math.Pow(2, failureCount - 1), maxDelay);
        }

        /// <summary>
        /// Throws an exception if a value of the settings is not valid.
        /// </summary>
        /// <param name="settings">The settings to check.</param>
        /// <exception cref="ArgumentException">Thrown if a value of the settings is not valid.</exception>
        private static void ThrowIfInvalid(DownloadRetrySettings settings)
        {
            if (settings.MaxAttempts < 1)
                throw new ArgumentException($"{nameof(settings.MaxAttempts)} must be at least 1.", nameof(settings));

            if (settings.FirstRetryDelay < 0 || settings.FirstNetworkPause < 0)
                throw new ArgumentException("Retry delays and network pauses can't be negative.", nameof(settings));

            if (settings.MaxNetworkPause < settings.FirstNetworkPause)
            {
                string message = $"{nameof(settings.MaxNetworkPause)} can't be smaller than {nameof(settings.FirstNetworkPause)}.";
                throw new ArgumentException(message, nameof(settings));
            }
        }

        #endregion

    }

}
