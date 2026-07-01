using NUnit.Framework;

namespace SideXP.Core.Tests
{

    public class FloatExtensionsTests
    {

        private const float Tolerance = 1e-5f;

        #region Approximately

        [Test]
        public void Approximately_EqualValues_ReturnsTrue()
        {
            Assert.IsTrue((0.1f).Approximately(0.1f));
        }

        [Test]
        public void Approximately_DifferentValues_ReturnsFalse()
        {
            Assert.IsFalse((0.1f).Approximately(0.2f));
        }

        // Now forwards to MathUtility: bounds are inclusive, so a value exactly epsilon away matches.
        [Test]
        public void Approximately_WithEpsilon_BoundaryIsInclusive()
        {
            Assert.IsTrue((5.1f).Approximately(5f, 0.1f));
        }

        [Test]
        public void Approximately_WithEpsilon_OutsideRange_ReturnsFalse()
        {
            Assert.IsFalse((5.2f).Approximately(5f, 0.1f));
        }

        // MathUtility ignores the sign of epsilon; the old inline implementation returned false here.
        [Test]
        public void Approximately_NegativeEpsilon_IsTreatedAsAbsolute()
        {
            Assert.IsTrue((5.05f).Approximately(5f, -0.1f));
        }

        [Test]
        public void Approximately_ZeroEpsilon_MatchesExactValues()
        {
            Assert.IsTrue((5f).Approximately(5f, 0f));
        }

        #endregion


        #region Ratio / Percents (forwarding)

        [Test]
        public void Ratio_ReturnsNormalizedPosition()
        {
            Assert.AreEqual(0f, (0f).Ratio(0f, 10f), Tolerance);
            Assert.AreEqual(0.5f, (5f).Ratio(0f, 10f), Tolerance);
            Assert.AreEqual(1f, (10f).Ratio(0f, 10f), Tolerance);
        }

        [Test]
        public void Percents_ReturnsRatioTimesHundred()
        {
            Assert.AreEqual(50f, (5f).Percents(0f, 10f), Tolerance);
        }

        #endregion


        #region Clamp

        [Test]
        public void Clamp_WithinRange_ReturnsValue()
        {
            Assert.AreEqual(5f, (5f).Clamp(0f, 10f), Tolerance);
        }

        [Test]
        public void Clamp_BelowMin_ReturnsMin()
        {
            Assert.AreEqual(0f, (-3f).Clamp(0f, 10f), Tolerance);
        }

        [Test]
        public void Clamp_AboveMax_ReturnsMax()
        {
            Assert.AreEqual(10f, (15f).Clamp(0f, 10f), Tolerance);
        }

        #endregion


        #region Pad

        // Pad uses culture-dependent ToString, so pin the decimal separator with an invariant-culture scope.

        [Test]
        public void Pad_NoDecimals_PadsIntegerString()
        {
            using (CultureScope.Invariant)
            {
                Assert.AreEqual("005", (5f).Pad(3));
            }
        }

        [Test]
        public void Pad_WithDecimals_FormatsThenPads()
        {
            using (CultureScope.Invariant)
            {
                // "5.50" (length 4) padded to length 6 with '0'.
                Assert.AreEqual("005.50", (5.5f).Pad(6, 2));
            }
        }

        [Test]
        public void Pad_NegativeDecimals_ClampedToZero()
        {
            using (CultureScope.Invariant)
            {
                // decimals < 0 is clamped to 0, so "F0" formatting is used.
                Assert.AreEqual("008", (7.8f).Pad(3, -2));
            }
        }

        #endregion

    }

}
