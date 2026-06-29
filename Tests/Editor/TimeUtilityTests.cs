using System;

using NUnit.Framework;

namespace SideXP.Core.Tests
{

    public class TimeUtilityTests
    {

        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Allow a generous window for the clock-based tests so they don't flake on slow CI.
        private const long ToleranceMs = 5000;
        private const long ToleranceSeconds = 5;

        #region ToTimestamp (deterministic)

        [Test]
        public void ToTimestamp_Epoch_IsZero()
        {
            Assert.AreEqual(0, TimeUtility.ToTimestamp(Epoch));
            Assert.AreEqual(0, TimeUtility.ToTimestampSeconds(Epoch));
        }

        [Test]
        public void ToTimestamp_KnownDate_MatchesUnixValue()
        {
            // 2021-01-01T00:00:00Z == 1609459200 seconds since the epoch.
            var date = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            Assert.AreEqual(1609459200L, TimeUtility.ToTimestampSeconds(date));
            Assert.AreEqual(1609459200000L, TimeUtility.ToTimestamp(date));
        }

        [Test]
        public void ToTimestamp_OneDayAfterEpoch()
        {
            var date = Epoch.AddDays(1);
            Assert.AreEqual(86_400_000L, TimeUtility.ToTimestamp(date));
            Assert.AreEqual(86_400L, TimeUtility.ToTimestampSeconds(date));
        }

        [Test]
        public void ToTimestampSeconds_TruncatesFraction()
        {
            // 1500 ms past the epoch is 1 whole second.
            var date = Epoch.AddMilliseconds(1500);
            Assert.AreEqual(1500L, TimeUtility.ToTimestamp(date));
            Assert.AreEqual(1L, TimeUtility.ToTimestampSeconds(date));
        }

        #endregion


        #region GetTimestamp (clock-based, windowed)

        [Test]
        public void GetTimestamp_Default_TracksUtcNow()
        {
            // Regression guard: the default (non-local) branch must use UtcNow, not Now.
            long expected = TimeUtility.ToTimestamp(DateTime.UtcNow);
            Assert.That(TimeUtility.GetTimestamp(), Is.EqualTo(expected).Within(ToleranceMs));
        }

        [Test]
        public void GetTimestamp_Local_TracksLocalNow()
        {
            // Regression guard: the local branch must use Now, not UtcNow.
            long expected = TimeUtility.ToTimestamp(DateTime.Now);
            Assert.That(TimeUtility.GetTimestamp(local: true), Is.EqualTo(expected).Within(ToleranceMs));
        }

        [Test]
        public void GetTimestampSeconds_Default_TracksUtcNow()
        {
            long expected = TimeUtility.ToTimestampSeconds(DateTime.UtcNow);
            Assert.That(TimeUtility.GetTimestampSeconds(), Is.EqualTo(expected).Within(ToleranceSeconds));
        }

        [Test]
        public void GetTimestampSeconds_Local_TracksLocalNow()
        {
            long expected = TimeUtility.ToTimestampSeconds(DateTime.Now);
            Assert.That(TimeUtility.GetTimestampSeconds(local: true), Is.EqualTo(expected).Within(ToleranceSeconds));
        }

        #endregion

    }

}
