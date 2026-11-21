using System;
using System.Collections.Generic;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Decides when a failed icon download can be attempted again, and when the whole download queue must wait.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>A failed download of an icon is retried after a delay that doubles after each failure, up to
    /// <see cref="MaxAttempts"/> attempts.</item>
    /// <item>A network error (no connection, proxy, timeout) pauses the whole queue instead, since the other downloads would fail
    /// the same way. The pause doubles after each consecutive network error, up to <see cref="MaxNetworkPause"/>.</item>
    /// <item>A missing file (404) is never retried.</item>
    /// </list>
    /// Times are given by the caller (in seconds, from any fixed origin), so the policy doesn't depend on a clock.
    /// </remarks>
    internal class IconRetryPolicy
    {

        #region Subclasses

        /// <summary>
        /// The failures of an icon download.
        /// </summary>
        private class IconFailures
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
        /// Maximum number of attempts to download an icon. Beyond, the icon is given up for this session.
        /// </summary>
        internal const int MaxAttempts = 5;

        /// <summary>
        /// Delay before retrying the download of an icon after its first failure, in seconds. Doubles after each failure.
        /// </summary>
        internal const double FirstRetryDelay = 2;

        /// <summary>
        /// Pause of the whole queue after a first network error, in seconds. Doubles after each consecutive network error.
        /// </summary>
        internal const double FirstNetworkPause = 10;

        /// <summary>
        /// Maximum pause of the whole queue after network errors, in seconds, so downloads resume soon after the connection is back.
        /// </summary>
        internal const double MaxNetworkPause = 300;

        /// <summary>
        /// The failures of each icon download, by key.
        /// </summary>
        private Dictionary<string, IconFailures> _failures = new Dictionary<string, IconFailures>();

        /// <summary>
        /// The number of consecutive network errors, reset by any successful download.
        /// </summary>
        private int _networkErrorCount = 0;

        #endregion


        #region Public API

        /// <summary>
        /// The time until which the whole queue is paused after network errors.
        /// </summary>
        internal double PausedUntil { get; private set; } = 0;

        /// <summary>
        /// Checks if the whole queue is paused after network errors.
        /// </summary>
        /// <param name="time">The current time.</param>
        /// <returns>Returns true if no download should be started.</returns>
        internal bool IsPaused(double time)
        {
            return time < PausedUntil;
        }

        /// <summary>
        /// Checks if the download of an icon has been given up for this session (missing file, or too many failures).
        /// </summary>
        /// <param name="key">The key of the icon download.</param>
        /// <returns>Returns true if the download won't be attempted again.</returns>
        internal bool IsGivenUp(string key)
        {
            return _failures.TryGetValue(key, out IconFailures failures) && failures.IsGivenUp;
        }

        /// <summary>
        /// Checks if the download of an icon can be attempted now.
        /// </summary>
        /// <param name="key">The key of the icon download.</param>
        /// <param name="time">The current time.</param>
        /// <returns>Returns true if the queue is not paused, the icon is not given up, and its retry delay is over.</returns>
        internal bool CanAttempt(string key, double time)
        {
            if (IsPaused(time))
                return false;

            return !_failures.TryGetValue(key, out IconFailures failures) || (!failures.IsGivenUp && time >= failures.NextAttemptTime);
        }

        /// <summary>
        /// Gets the number of failed attempts to download an icon. Network errors and aborted downloads are not counted.
        /// </summary>
        /// <param name="key">The key of the icon download.</param>
        /// <returns>Returns the number of failed attempts.</returns>
        internal int GetFailureCount(string key)
        {
            return _failures.TryGetValue(key, out IconFailures failures) ? failures.Count : 0;
        }

        /// <summary>
        /// Reports a successful download: forgets the failures of that icon, and resets the network pause.
        /// </summary>
        /// <param name="key">The key of the icon download.</param>
        internal void ReportSuccess(string key)
        {
            _failures.Remove(key);
            _networkErrorCount = 0;
        }

        /// <summary>
        /// Reports a failed download.
        /// </summary>
        /// <param name="key">The key of the icon download.</param>
        /// <param name="status">The outcome of the download.</param>
        /// <param name="time">The time of the failure.</param>
        /// <exception cref="ArgumentException">Thrown if the status is <see cref="EIconDownloadStatus.Success"/>.</exception>
        internal void ReportFailure(string key, EIconDownloadStatus status, double time)
        {
            switch (status)
            {
                case EIconDownloadStatus.Success:
                    throw new ArgumentException($"Use {nameof(ReportSuccess)}() for a successful download.", nameof(status));

                // Not a failure of the download itself
                case EIconDownloadStatus.Aborted:
                    return;

                case EIconDownloadStatus.NotFound:
                    GetOrAddFailures(key).IsGivenUp = true;
                    return;

                case EIconDownloadStatus.NetworkError:
                    // Downloads started before the pause may fail while it's running, and must not extend it again
                    if (IsPaused(time))
                        return;

                    _networkErrorCount++;
                    PausedUntil = time + GetDoubledDelay(FirstNetworkPause, _networkErrorCount, MaxNetworkPause);
                    return;

                default:
                    IconFailures failures = GetOrAddFailures(key);
                    failures.Count++;
                    if (failures.Count >= MaxAttempts)
                        failures.IsGivenUp = true;
                    else
                        failures.NextAttemptTime = time + GetDoubledDelay(FirstRetryDelay, failures.Count, double.MaxValue);
                    return;
            }
        }

        /// <summary>
        /// The keys of the icon downloads that have been given up.
        /// </summary>
        internal IEnumerable<string> GivenUpKeys
        {
            get
            {
                foreach (KeyValuePair<string, IconFailures> failures in _failures)
                {
                    if (failures.Value.IsGivenUp)
                        yield return failures.Key;
                }
            }
        }

        /// <summary>
        /// Gives up the download of an icon, as when restoring the downloads given up before a domain reload.
        /// </summary>
        /// <param name="key">The key of the icon download.</param>
        internal void GiveUp(string key)
        {
            GetOrAddFailures(key).IsGivenUp = true;
        }

        /// <summary>
        /// Forgets all failures and the network pause, so every download can be attempted again.
        /// </summary>
        internal void Clear()
        {
            _failures.Clear();
            _networkErrorCount = 0;
            PausedUntil = 0;
        }

        #endregion


        #region Private API

        /// <summary>
        /// Gets the failures of an icon download, or creates them.
        /// </summary>
        /// <param name="key">The key of the icon download.</param>
        /// <returns>Returns the failures of the icon download.</returns>
        private IconFailures GetOrAddFailures(string key)
        {
            if (!_failures.TryGetValue(key, out IconFailures failures))
            {
                failures = new IconFailures();
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

        #endregion

    }

}
