using System;

using NUnit.Framework;

using SideXP.Core.EditorOnly;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="DownloadRetryPolicy"/>.
    /// </summary>
    public class DownloadRetryPolicyTests
    {

        /// <summary>
        /// Key of the download used in the tests.
        /// </summary>
        private const string Key = "home";

        /// <summary>
        /// Key of another download.
        /// </summary>
        private const string OtherKey = "user";

        /// <summary>
        /// Time of the first failure in the tests, in seconds.
        /// </summary>
        private const double StartTime = 100;

        /// <summary>
        /// Small duration used to check times right before and after a delay ends, in seconds.
        /// </summary>
        private const double Epsilon = 0.01;

        #region Settings

        /// <summary>
        /// Without settings, the default ones are used.
        /// </summary>
        [Test]
        public void Constructor_WithoutSettings_UsesDefaults()
        {
            var policy = new DownloadRetryPolicy();

            Assert.AreEqual(DownloadRetrySettings.DefaultMaxAttempts, policy.Settings.MaxAttempts);
            Assert.AreEqual(DownloadRetrySettings.DefaultFirstRetryDelay, policy.Settings.FirstRetryDelay);
            Assert.AreEqual(DownloadRetrySettings.DefaultFirstNetworkPause, policy.Settings.FirstNetworkPause);
            Assert.AreEqual(DownloadRetrySettings.DefaultMaxNetworkPause, policy.Settings.MaxNetworkPause);
        }

        /// <summary>
        /// Custom settings replace the default attempts, delays and pauses.
        /// </summary>
        [Test]
        public void ReportFailure_CustomSettings_UsesThem()
        {
            var settings = new DownloadRetrySettings { MaxAttempts = 2, FirstRetryDelay = 1, FirstNetworkPause = 3, MaxNetworkPause = 3 };
            var policy = new DownloadRetryPolicy(settings);

            policy.ReportFailure(Key, EWebDownloadStatus.Failed, StartTime);
            Assert.IsFalse(policy.CanAttempt(Key, StartTime + settings.FirstRetryDelay - Epsilon));
            Assert.IsTrue(policy.CanAttempt(Key, StartTime + settings.FirstRetryDelay));

            policy.ReportFailure(Key, EWebDownloadStatus.Failed, StartTime);
            Assert.IsTrue(policy.IsGivenUp(Key));

            policy.ReportFailure(OtherKey, EWebDownloadStatus.NetworkError, StartTime);
            Assert.AreEqual(StartTime + settings.FirstNetworkPause, policy.PausedUntil);
            policy.ReportFailure(OtherKey, EWebDownloadStatus.NetworkError, policy.PausedUntil);
            Assert.AreEqual(StartTime + settings.FirstNetworkPause + settings.MaxNetworkPause, policy.PausedUntil);
        }

        /// <summary>
        /// Settings that can't work are rejected.
        /// </summary>
        [Test]
        public void Constructor_InvalidSettings_Throws()
        {
            Assert.Throws<ArgumentException>(() => new DownloadRetryPolicy(new DownloadRetrySettings { MaxAttempts = 0 }));
            Assert.Throws<ArgumentException>(() => new DownloadRetryPolicy(new DownloadRetrySettings { FirstRetryDelay = -1 }));
            Assert.Throws<ArgumentException>(() => new DownloadRetryPolicy(new DownloadRetrySettings { FirstNetworkPause = -1 }));
            Assert.Throws<ArgumentException>(
                () => new DownloadRetryPolicy(new DownloadRetrySettings { FirstNetworkPause = 10, MaxNetworkPause = 5 })
            );
        }

        #endregion


        #region Retries

        /// <summary>
        /// Without failure, every download can be attempted.
        /// </summary>
        [Test]
        public void CanAttempt_WithoutFailure_IsTrue()
        {
            var policy = new DownloadRetryPolicy();

            Assert.IsTrue(policy.CanAttempt(Key, StartTime));
            Assert.IsFalse(policy.IsGivenUp(Key));
            Assert.AreEqual(0, policy.GetFailureCount(Key));
        }

        /// <summary>
        /// A failed download is retried after a delay that doubles after each failure.
        /// </summary>
        [Test]
        public void ReportFailure_Failed_DoublesRetryDelay()
        {
            var policy = new DownloadRetryPolicy();
            double time = StartTime;
            double expectedDelay = DownloadRetrySettings.DefaultFirstRetryDelay;

            for (int i = 1; i < DownloadRetrySettings.DefaultMaxAttempts; i++)
            {
                policy.ReportFailure(Key, EWebDownloadStatus.Failed, time);

                Assert.AreEqual(i, policy.GetFailureCount(Key));
                Assert.IsFalse(policy.CanAttempt(Key, time + expectedDelay - Epsilon), $"Retried too early after {i} failures.");
                Assert.IsTrue(policy.CanAttempt(Key, time + expectedDelay), $"Not retried after {i} failures.");

                time += expectedDelay;
                expectedDelay *= 2;
            }
        }

        /// <summary>
        /// A download that failed too many times is given up.
        /// </summary>
        [Test]
        public void ReportFailure_MaxAttempts_GivesUp()
        {
            var policy = new DownloadRetryPolicy();
            for (int i = 0; i < DownloadRetrySettings.DefaultMaxAttempts; i++)
                policy.ReportFailure(Key, EWebDownloadStatus.Failed, StartTime);

            Assert.IsTrue(policy.IsGivenUp(Key));
            Assert.IsFalse(policy.CanAttempt(Key, double.MaxValue));
        }

        /// <summary>
        /// A missing file is given up right away.
        /// </summary>
        [Test]
        public void ReportFailure_NotFound_GivesUpRightAway()
        {
            var policy = new DownloadRetryPolicy();

            policy.ReportFailure(Key, EWebDownloadStatus.NotFound, StartTime);

            Assert.IsTrue(policy.IsGivenUp(Key));
            Assert.IsFalse(policy.CanAttempt(Key, double.MaxValue));
            Assert.IsTrue(policy.CanAttempt(OtherKey, StartTime));
        }

        /// <summary>
        /// An aborted download is not a failure.
        /// </summary>
        [Test]
        public void ReportFailure_Aborted_IsIgnored()
        {
            var policy = new DownloadRetryPolicy();

            policy.ReportFailure(Key, EWebDownloadStatus.Aborted, StartTime);

            Assert.AreEqual(0, policy.GetFailureCount(Key));
            Assert.IsTrue(policy.CanAttempt(Key, StartTime));
        }

        /// <summary>
        /// A successful download forgets the failures of that download only.
        /// </summary>
        [Test]
        public void ReportSuccess_ForgetsFailuresOfThatDownload()
        {
            var policy = new DownloadRetryPolicy();
            policy.ReportFailure(Key, EWebDownloadStatus.Failed, StartTime);
            policy.ReportFailure(OtherKey, EWebDownloadStatus.Failed, StartTime);

            policy.ReportSuccess(Key);

            Assert.AreEqual(0, policy.GetFailureCount(Key));
            Assert.IsTrue(policy.CanAttempt(Key, StartTime));
            Assert.AreEqual(1, policy.GetFailureCount(OtherKey));
        }

        /// <summary>
        /// A success can't be reported as a failure.
        /// </summary>
        [Test]
        public void ReportFailure_Success_Throws()
        {
            var policy = new DownloadRetryPolicy();

            Assert.Throws<ArgumentException>(() => policy.ReportFailure(Key, EWebDownloadStatus.Success, StartTime));
        }

        #endregion


        #region Network pause

        /// <summary>
        /// A network error pauses every download, without counting as a failure of that download.
        /// </summary>
        [Test]
        public void ReportFailure_NetworkError_PausesEveryDownload()
        {
            var policy = new DownloadRetryPolicy();

            policy.ReportFailure(Key, EWebDownloadStatus.NetworkError, StartTime);

            Assert.IsTrue(policy.IsPaused(StartTime));
            Assert.IsFalse(policy.CanAttempt(OtherKey, StartTime + DownloadRetrySettings.DefaultFirstNetworkPause - Epsilon));
            Assert.IsTrue(policy.CanAttempt(OtherKey, StartTime + DownloadRetrySettings.DefaultFirstNetworkPause));
            Assert.AreEqual(0, policy.GetFailureCount(Key));
        }

        /// <summary>
        /// Consecutive network errors double the pause, up to its maximum.
        /// </summary>
        [Test]
        public void ReportFailure_ConsecutiveNetworkErrors_DoublePauseUpToMax()
        {
            var policy = new DownloadRetryPolicy();
            double time = StartTime;
            double expectedPause = DownloadRetrySettings.DefaultFirstNetworkPause;

            while (expectedPause < DownloadRetrySettings.DefaultMaxNetworkPause)
            {
                policy.ReportFailure(Key, EWebDownloadStatus.NetworkError, time);
                Assert.AreEqual(time + expectedPause, policy.PausedUntil);

                time = policy.PausedUntil;
                expectedPause *= 2;
            }

            policy.ReportFailure(Key, EWebDownloadStatus.NetworkError, time);
            Assert.AreEqual(time + DownloadRetrySettings.DefaultMaxNetworkPause, policy.PausedUntil);
        }

        /// <summary>
        /// Network errors reported while paused come from downloads started before the pause, and don't extend it.
        /// </summary>
        [Test]
        public void ReportFailure_NetworkErrorWhilePaused_DoesNotExtendPause()
        {
            var policy = new DownloadRetryPolicy();
            policy.ReportFailure(Key, EWebDownloadStatus.NetworkError, StartTime);
            double pausedUntil = policy.PausedUntil;

            policy.ReportFailure(OtherKey, EWebDownloadStatus.NetworkError, StartTime + Epsilon);

            Assert.AreEqual(pausedUntil, policy.PausedUntil);
        }

        /// <summary>
        /// A successful download resets the network pause, so the next network error pauses for the first delay again.
        /// </summary>
        [Test]
        public void ReportSuccess_ResetsNetworkPause()
        {
            var policy = new DownloadRetryPolicy();
            policy.ReportFailure(Key, EWebDownloadStatus.NetworkError, StartTime);
            double time = policy.PausedUntil;
            policy.ReportFailure(Key, EWebDownloadStatus.NetworkError, time);
            time = policy.PausedUntil;

            policy.ReportSuccess(OtherKey);
            policy.ReportFailure(Key, EWebDownloadStatus.NetworkError, time);

            Assert.AreEqual(time + DownloadRetrySettings.DefaultFirstNetworkPause, policy.PausedUntil);
        }

        /// <summary>
        /// Given up downloads are listed, and can be given up again, as when restored after a domain reload.
        /// </summary>
        [Test]
        public void GiveUp_IsListedInGivenUpKeys()
        {
            var policy = new DownloadRetryPolicy();
            policy.ReportFailure(Key, EWebDownloadStatus.NotFound, StartTime);
            policy.ReportFailure("star", EWebDownloadStatus.Failed, StartTime);

            policy.GiveUp(OtherKey);

            CollectionAssert.AreEquivalent(new[] { Key, OtherKey }, policy.GivenUpKeys);
            Assert.IsFalse(policy.CanAttempt(OtherKey, StartTime));
        }

        /// <summary>
        /// Clearing the policy forgets every failure and the network pause.
        /// </summary>
        [Test]
        public void Clear_ForgetsEverything()
        {
            var policy = new DownloadRetryPolicy();
            policy.ReportFailure(Key, EWebDownloadStatus.NotFound, StartTime);
            policy.ReportFailure(OtherKey, EWebDownloadStatus.NetworkError, StartTime);

            policy.Clear();

            Assert.IsFalse(policy.IsPaused(StartTime));
            Assert.IsFalse(policy.IsGivenUp(Key));
            Assert.IsTrue(policy.CanAttempt(Key, StartTime));
        }

        #endregion

    }

}
