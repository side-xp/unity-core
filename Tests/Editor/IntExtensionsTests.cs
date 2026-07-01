using NUnit.Framework;

namespace SideXP.Core.Tests
{

    public class IntExtensionsTests
    {

        private const float Tolerance = 1e-5f;

        #region Ratio / Percents (forwarding)

        [Test]
        public void Ratio_ReturnsNormalizedPosition()
        {
            Assert.AreEqual(0f, (0).Ratio(0, 10), Tolerance);
            Assert.AreEqual(0.5f, (5).Ratio(0, 10), Tolerance);
            Assert.AreEqual(1f, (10).Ratio(0, 10), Tolerance);
        }

        [Test]
        public void Percents_ReturnsRatioTimesHundred()
        {
            Assert.AreEqual(50f, (5).Percents(0, 10), Tolerance);
        }

        #endregion


        #region Clamp

        [Test]
        public void Clamp_WithinRange_ReturnsValue()
        {
            Assert.AreEqual(5, (5).Clamp(0, 10));
        }

        [Test]
        public void Clamp_BelowMin_ReturnsMin()
        {
            Assert.AreEqual(0, (-3).Clamp(0, 10));
        }

        [Test]
        public void Clamp_AboveMax_ReturnsMax()
        {
            Assert.AreEqual(10, (15).Clamp(0, 10));
        }

        #endregion


        #region Pad

        [Test]
        public void Pad_ShorterThanLength_PadsWithZeros()
        {
            Assert.AreEqual("005", (5).Pad(3));
        }

        [Test]
        public void Pad_CustomPadChar_UsesIt()
        {
            Assert.AreEqual("**5", (5).Pad(3, '*'));
        }

        [Test]
        public void Pad_LongerThanLength_IsNotTruncated()
        {
            Assert.AreEqual("123", (123).Pad(2));
        }

        // Documents a known limitation: PadLeft pads before the minus sign.
        [Test]
        public void Pad_NegativeValue_PadsBeforeSign()
        {
            Assert.AreEqual("0-5", (-5).Pad(3));
        }

        #endregion

    }

}
