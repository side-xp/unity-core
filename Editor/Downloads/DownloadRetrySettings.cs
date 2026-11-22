namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Defines when failed downloads are attempted again, as used by <see cref="DownloadRetryPolicy"/>.
    /// </summary>
    /// <remarks>
    /// Every value has a default, so only the ones to change need to be set.
    /// </remarks>
    /// <example>
    /// <code>
    /// var policy = new DownloadRetryPolicy(new DownloadRetrySettings { MaxAttempts = 3 });
    /// </code>
    /// </example>
    public class DownloadRetrySettings
    {

        #region Fields

        /// <summary>
        /// Default value of <see cref="MaxAttempts"/>.
        /// </summary>
        public const int DefaultMaxAttempts = 5;

        /// <summary>
        /// Default value of <see cref="FirstRetryDelay"/>, in seconds.
        /// </summary>
        public const double DefaultFirstRetryDelay = 2;

        /// <summary>
        /// Default value of <see cref="FirstNetworkPause"/>, in seconds.
        /// </summary>
        public const double DefaultFirstNetworkPause = 10;

        /// <summary>
        /// Default value of <see cref="MaxNetworkPause"/>, in seconds. Short enough for downloads to resume soon after the connection
        /// is back.
        /// </summary>
        public const double DefaultMaxNetworkPause = 300;

        #endregion


        #region Public API

        /// <summary>
        /// Maximum number of attempts to download a file. Beyond, the download is given up. Network errors and aborted downloads are
        /// not counted. Must be at least 1.
        /// </summary>
        public int MaxAttempts { get; set; } = DefaultMaxAttempts;

        /// <summary>
        /// Delay before retrying a download after its first failure, in seconds. Doubles after each failure. Can't be negative.
        /// </summary>
        public double FirstRetryDelay { get; set; } = DefaultFirstRetryDelay;

        /// <summary>
        /// Pause of every download after a first network error, in seconds. Doubles after each consecutive network error. Can't be
        /// negative.
        /// </summary>
        public double FirstNetworkPause { get; set; } = DefaultFirstNetworkPause;

        /// <summary>
        /// Maximum pause of every download after consecutive network errors, in seconds. Can't be smaller than
        /// <see cref="FirstNetworkPause"/>.
        /// </summary>
        public double MaxNetworkPause { get; set; } = DefaultMaxNetworkPause;

        #endregion

    }

}
