using System;

using NUnit.Framework;

using SideXP.Core.EditorOnly;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="IconRetryPolicy"/>.
    /// </summary>
    public class IconRetryPolicyTests
    {

        /// <summary>
        /// Key of the icon download used in the tests.
        /// </summary>
        private const string Key = "home";

        /// <summary>
        /// Key of another icon download.
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

        #region Retries

        /// <summary>
        /// Without failure, every download can be attempted.
        /// </summary>
        [Test]
        public void CanAttempt_WithoutFailure_IsTrue()
        {
            var policy = new IconRetryPolicy();

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
            var policy = new IconRetryPolicy();
            double time = StartTime;
            double expectedDelay = IconRetryPolicy.FirstRetryDelay;

            for (int i = 1; i < IconRetryPolicy.MaxAttempts; i++)
            {
                policy.ReportFailure(Key, EIconDownloadStatus.Failed, time);

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
            var policy = new IconRetryPolicy();
            for (int i = 0; i < IconRetryPolicy.MaxAttempts; i++)
                policy.ReportFailure(Key, EIconDownloadStatus.Failed, StartTime);

            Assert.IsTrue(policy.IsGivenUp(Key));
            Assert.IsFalse(policy.CanAttempt(Key, double.MaxValue));
        }

        /// <summary>
        /// A missing file is given up right away.
        /// </summary>
        [Test]
        public void ReportFailure_NotFound_GivesUpRightAway()
        {
            var policy = new IconRetryPolicy();

            policy.ReportFailure(Key, EIconDownloadStatus.NotFound, StartTime);

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
            var policy = new IconRetryPolicy();

            policy.ReportFailure(Key, EIconDownloadStatus.Aborted, StartTime);

            Assert.AreEqual(0, policy.GetFailureCount(Key));
            Assert.IsTrue(policy.CanAttempt(Key, StartTime));
        }

        /// <summary>
        /// A successful download forgets the failures of that icon only.
        /// </summary>
        [Test]
        public void ReportSuccess_ForgetsFailuresOfThatIcon()
        {
            var policy = new IconRetryPolicy();
            policy.ReportFailure(Key, EIconDownloadStatus.Failed, StartTime);
            policy.ReportFailure(OtherKey, EIconDownloadStatus.Failed, StartTime);

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
            var policy = new IconRetryPolicy();

            Assert.Throws<ArgumentException>(() => policy.ReportFailure(Key, EIconDownloadStatus.Success, StartTime));
        }

        #endregion


        #region Network pause

        /// <summary>
        /// A network error pauses every download, without counting as a failure of that icon.
        /// </summary>
        [Test]
        public void ReportFailure_NetworkError_PausesEveryDownload()
        {
            var policy = new IconRetryPolicy();

            policy.ReportFailure(Key, EIconDownloadStatus.NetworkError, StartTime);

            Assert.IsTrue(policy.IsPaused(StartTime));
            Assert.IsFalse(policy.CanAttempt(OtherKey, StartTime + IconRetryPolicy.FirstNetworkPause - Epsilon));
            Assert.IsTrue(policy.CanAttempt(OtherKey, StartTime + IconRetryPolicy.FirstNetworkPause));
            Assert.AreEqual(0, policy.GetFailureCount(Key));
        }

        /// <summary>
        /// Consecutive network errors double the pause, up to its maximum.
        /// </summary>
        [Test]
        public void ReportFailure_ConsecutiveNetworkErrors_DoublePauseUpToMax()
        {
            var policy = new IconRetryPolicy();
            double time = StartTime;
            double expectedPause = IconRetryPolicy.FirstNetworkPause;

            while (expectedPause < IconRetryPolicy.MaxNetworkPause)
            {
                policy.ReportFailure(Key, EIconDownloadStatus.NetworkError, time);
                Assert.AreEqual(time + expectedPause, policy.PausedUntil);

                time = policy.PausedUntil;
                expectedPause *= 2;
            }

            policy.ReportFailure(Key, EIconDownloadStatus.NetworkError, time);
            Assert.AreEqual(time + IconRetryPolicy.MaxNetworkPause, policy.PausedUntil);
        }

        /// <summary>
        /// Network errors reported while paused come from downloads started before the pause, and don't extend it.
        /// </summary>
        [Test]
        public void ReportFailure_NetworkErrorWhilePaused_DoesNotExtendPause()
        {
            var policy = new IconRetryPolicy();
            policy.ReportFailure(Key, EIconDownloadStatus.NetworkError, StartTime);
            double pausedUntil = policy.PausedUntil;

            policy.ReportFailure(OtherKey, EIconDownloadStatus.NetworkError, StartTime + Epsilon);

            Assert.AreEqual(pausedUntil, policy.PausedUntil);
        }

        /// <summary>
        /// A successful download resets the network pause, so the next network error pauses for the first delay again.
        /// </summary>
        [Test]
        public void ReportSuccess_ResetsNetworkPause()
        {
            var policy = new IconRetryPolicy();
            policy.ReportFailure(Key, EIconDownloadStatus.NetworkError, StartTime);
            double time = policy.PausedUntil;
            policy.ReportFailure(Key, EIconDownloadStatus.NetworkError, time);
            time = policy.PausedUntil;

            policy.ReportSuccess(OtherKey);
            policy.ReportFailure(Key, EIconDownloadStatus.NetworkError, time);

            Assert.AreEqual(time + IconRetryPolicy.FirstNetworkPause, policy.PausedUntil);
        }

        /// <summary>
        /// Clearing the policy forgets every failure and the network pause.
        /// </summary>
        [Test]
        public void Clear_ForgetsEverything()
        {
            var policy = new IconRetryPolicy();
            policy.ReportFailure(Key, EIconDownloadStatus.NotFound, StartTime);
            policy.ReportFailure(OtherKey, EIconDownloadStatus.NetworkError, StartTime);

            policy.Clear();

            Assert.IsFalse(policy.IsPaused(StartTime));
            Assert.IsFalse(policy.IsGivenUp(Key));
            Assert.IsTrue(policy.CanAttempt(Key, StartTime));
        }

        #endregion

    }

}
