using System;

using NUnit.Framework;

namespace SideXP.Core.Tests
{

    // Thin forwarders to TimeUtility (covered separately). Tests confirm delegation and a couple of
    // concrete anchor values around the Unix epoch.
    public class DateTimeExtensionsTests
    {

        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        [Test]
        public void ToTimestamp_Epoch_IsZero()
        {
            Assert.AreEqual(0L, Epoch.ToTimestamp());
        }

        [Test]
        public void ToTimestampSeconds_Epoch_IsZero()
        {
            Assert.AreEqual(0L, Epoch.ToTimestampSeconds());
        }

        [Test]
        public void ToTimestamp_OneSecondAfterEpoch_IsThousandMilliseconds()
        {
            DateTime time = Epoch.AddSeconds(1);
            Assert.AreEqual(1000L, time.ToTimestamp());
        }

        [Test]
        public void ToTimestampSeconds_OneSecondAfterEpoch_IsOne()
        {
            DateTime time = Epoch.AddSeconds(1);
            Assert.AreEqual(1L, time.ToTimestampSeconds());
        }

        [Test]
        public void ToTimestamp_ForwardsToUtility()
        {
            DateTime time = new DateTime(2020, 6, 15, 12, 0, 0, DateTimeKind.Utc);
            Assert.AreEqual(TimeUtility.ToTimestamp(time), time.ToTimestamp());
        }

        [Test]
        public void ToTimestampSeconds_ForwardsToUtility()
        {
            DateTime time = new DateTime(2020, 6, 15, 12, 0, 0, DateTimeKind.Utc);
            Assert.AreEqual(TimeUtility.ToTimestampSeconds(time), time.ToTimestampSeconds());
        }

    }

}
