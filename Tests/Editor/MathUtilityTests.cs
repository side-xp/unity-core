using NUnit.Framework;

namespace SideXP.Core.Tests
{

    public class MathUtilityTests
    {

        /// <summary>
        /// Tolerance used when comparing floating-point results.
        /// </summary>
        private const float Tolerance = 1e-5f;


        #region Approximately

        [TestCase(1f, 1f)]
        [TestCase(0f, 0f)]
        [TestCase(-3.5f, -3.5f)]
        public void Approximately_EqualValues_ReturnsTrue(float a, float b)
        {
            Assert.IsTrue(MathUtility.Approximately(a, b));
        }

        [Test]
        public void Approximately_VeryCloseValues_ReturnsTrue()
        {
            Assert.IsTrue(MathUtility.Approximately(1f, 1f + 1e-7f));
        }

        [TestCase(1f, 2f)]
        [TestCase(0f, 0.5f)]
        public void Approximately_DistinctValues_ReturnsFalse(float a, float b)
        {
            Assert.IsFalse(MathUtility.Approximately(a, b));
        }

        [Test]
        public void Approximately_UsesRelativeTolerance_ScalingWithMagnitude()
        {
            // The two-argument overload uses Unity's relative tolerance (~1e-6 of the magnitude),
            // so a 0.05 difference is "close enough" at a magnitude of 100000.
            Assert.IsTrue(MathUtility.Approximately(100000f, 100000.05f));
        }

        [TestCase(5f, 5f, 0.1f)]      // Exactly equal, within epsilon
        [TestCase(5.05f, 5f, 0.1f)]   // Inside the epsilon window
        [TestCase(4.95f, 5f, 0.1f)]   // Inside the epsilon window (below)
        public void Approximately_WithEpsilon_InsideWindow_ReturnsTrue(float a, float b, float epsilon)
        {
            Assert.IsTrue(MathUtility.Approximately(a, b, epsilon));
        }

        [TestCase(5.2f, 5f, 0.1f)]    // Outside the epsilon window
        [TestCase(4.8f, 5f, 0.1f)]    // Outside the epsilon window (below)
        public void Approximately_WithEpsilon_OutsideWindow_ReturnsFalse(float a, float b, float epsilon)
        {
            Assert.IsFalse(MathUtility.Approximately(a, b, epsilon));
        }

        [TestCase(5.1f, 5f, 0.1f)]    // a == b + epsilon
        [TestCase(4.9f, 5f, 0.1f)]    // a == b - epsilon
        public void Approximately_WithEpsilon_OnBoundary_IsInclusive(float a, float b, float epsilon)
        {
            // The window uses inclusive bounds, so the exact boundary is considered approximately equal.
            Assert.IsTrue(MathUtility.Approximately(a, b, epsilon));
        }

        [Test]
        public void Approximately_WithZeroEpsilon_EqualValues_ReturnsTrue()
        {
            // An epsilon of 0 still treats strictly equal values as approximately equal.
            Assert.IsTrue(MathUtility.Approximately(5f, 5f, 0f));
        }

        [Test]
        public void Approximately_WithNegativeEpsilon_BehavesLikePositive()
        {
            // The sign of epsilon is ignored (its absolute value is used).
            Assert.IsTrue(MathUtility.Approximately(5.05f, 5f, -0.1f));
            Assert.IsFalse(MathUtility.Approximately(5.2f, 5f, -0.1f));
        }

        [Test]
        public void Approximately_WithEpsilon_UsesAbsoluteTolerance()
        {
            // Unlike the two-argument overload, the epsilon overload uses an absolute tolerance and does
            // not scale with magnitude: a 0.05 difference exceeds an absolute epsilon of 0.001.
            Assert.IsFalse(MathUtility.Approximately(100000f, 100000.05f, 0.001f));
        }

        #endregion


        #region Remap (int)

        // Cases mirrored from the XML documentation examples on MathUtility.Remap.
        [TestCase(5, 0, 10, 100, 200, 150)]
        [TestCase(0, -10, 10, -100, 100, 0)]
        [TestCase(-10, -10, 10, -100, 100, -100)]
        [TestCase(10, -10, 10, -100, 100, 100)]
        [TestCase(0, -10, 10, 100, 200, 150)]
        public void RemapInt_DocumentedCases_ReturnExpected(int value, int fromMin, int fromMax, int toMin, int toMax, int expected)
        {
            Assert.AreEqual(expected, MathUtility.Remap(value, fromMin, fromMax, toMin, toMax));
        }

        [Test]
        public void RemapInt_OutOfRangeValue_ProducesOutOfRangeResult()
        {
            // Documented behavior: an out-of-range input yields an out-of-range output.
            Assert.AreEqual(250, MathUtility.Remap(15, 0, 10, 100, 200));
        }

        [Test]
        public void RemapInt_UsesIntegerDivision_TruncatesTowardZero()
        {
            // 1 * 10 / 3 == 3 (positive result truncates the .33 remainder).
            Assert.AreEqual(3, MathUtility.Remap(1, 0, 3, 0, 10));
            // 2 * -10 / 3 == -6 (negative result truncates toward zero, not toward -infinity).
            Assert.AreEqual(-6, MathUtility.Remap(2, 0, 3, 0, -10));
        }

        [Test]
        public void RemapInt_InvertedOutputRange_ProducesDescendingMapping()
        {
            // An inverted output range is supported and maps downward.
            Assert.AreEqual(150, MathUtility.Remap(5, 0, 10, 200, 100));
        }

        [Test]
        public void RemapInt_LargeRange_DoesNotOverflow()
        {
            // The intermediate multiplication (1e6 * 2e6) overflows int, but is computed as long internally.
            Assert.AreEqual(1_000_000, MathUtility.Remap(1_000_000, 0, 2_000_000, 0, 2_000_000));
        }

        [Test]
        public void RemapInt_ZeroWidthInputRange_ReturnsZero()
        {
            Assert.AreEqual(0, MathUtility.Remap(5, 5, 5, 0, 100));
        }

        [Test]
        public void RemapInt_InvertedInputRange_ReturnsZero()
        {
            // fromMax - fromMin is negative, so the guard returns 0 (current behavior).
            Assert.AreEqual(0, MathUtility.Remap(5, 10, 0, 0, 100));
        }

        #endregion


        #region Remap (float)

        [TestCase(5f, 0f, 10f, 100f, 200f, 150f)]
        [TestCase(0f, -10f, 10f, -100f, 100f, 0f)]
        [TestCase(-10f, -10f, 10f, -100f, 100f, -100f)]
        [TestCase(10f, -10f, 10f, -100f, 100f, 100f)]
        public void RemapFloat_DocumentedCases_ReturnExpected(float value, float fromMin, float fromMax, float toMin, float toMax, float expected)
        {
            Assert.AreEqual(expected, MathUtility.Remap(value, fromMin, fromMax, toMin, toMax), Tolerance);
        }

        [Test]
        public void RemapFloat_KeepsFractionalPrecision()
        {
            // Unlike the int overload, the float version preserves the fractional result.
            Assert.AreEqual(3.3333333f, MathUtility.Remap(1f, 0f, 3f, 0f, 10f), Tolerance);
        }

        [Test]
        public void RemapFloat_OutOfRangeValue_ProducesOutOfRangeResult()
        {
            Assert.AreEqual(250f, MathUtility.Remap(15f, 0f, 10f, 100f, 200f), Tolerance);
        }

        [Test]
        public void RemapFloat_InvertedOutputRange_ProducesDescendingMapping()
        {
            Assert.AreEqual(150f, MathUtility.Remap(5f, 0f, 10f, 200f, 100f), Tolerance);
        }

        [Test]
        public void RemapFloat_ZeroWidthInputRange_ReturnsZero()
        {
            Assert.AreEqual(0f, MathUtility.Remap(5f, 5f, 5f, 0f, 100f), Tolerance);
        }

        [Test]
        public void RemapFloat_InvertedInputRange_ReturnsZero()
        {
            Assert.AreEqual(0f, MathUtility.Remap(5f, 10f, 0f, 0f, 100f), Tolerance);
        }

        #endregion


        #region Ratio

        [TestCase(5f, 0f, 10f, 0.5f)]
        [TestCase(0f, 0f, 10f, 0f)]
        [TestCase(10f, 0f, 10f, 1f)]
        [TestCase(2.5f, 0f, 10f, 0.25f)]
        public void Ratio_WithinRange_ReturnsNormalizedValue(float value, float min, float max, float expected)
        {
            Assert.AreEqual(expected, MathUtility.Ratio(value, min, max), Tolerance);
        }

        [TestCase(-5f, 0f, 10f, 0f)]    // Below range clamps to 0
        [TestCase(15f, 0f, 10f, 1f)]    // Above range clamps to 1
        public void Ratio_OutOfRange_IsClamped01(float value, float min, float max, float expected)
        {
            Assert.AreEqual(expected, MathUtility.Ratio(value, min, max), Tolerance);
        }

        [Test]
        public void Ratio_ZeroWidthRange_ReturnsZero()
        {
            Assert.AreEqual(0f, MathUtility.Ratio(5f, 5f, 5f), Tolerance);
        }

        #endregion


        #region Percents

        [TestCase(5f, 0f, 10f, 50f)]
        [TestCase(0f, 0f, 10f, 0f)]
        [TestCase(10f, 0f, 10f, 100f)]
        public void Percents_WithinRange_ReturnsPercentage(float value, float min, float max, float expected)
        {
            Assert.AreEqual(expected, MathUtility.Percents(value, min, max), Tolerance);
        }

        [TestCase(15f, 0f, 10f, 150f)]   // Not clamped, unlike Ratio
        [TestCase(-5f, 0f, 10f, -50f)]
        public void Percents_OutOfRange_IsNotClamped(float value, float min, float max, float expected)
        {
            Assert.AreEqual(expected, MathUtility.Percents(value, min, max), Tolerance);
        }

        [Test]
        public void Percents_ZeroWidthRange_ReturnsZero()
        {
            Assert.AreEqual(0f, MathUtility.Percents(5f, 5f, 5f), Tolerance);
        }

        #endregion


        #region Ratio vs Percents

        [Test]
        public void RatioAndPercents_WithinRange_Agree()
        {
            // Within range, Percents is simply Ratio scaled to 0..100.
            Assert.AreEqual(MathUtility.Ratio(5f, 0f, 10f) * 100f, MathUtility.Percents(5f, 0f, 10f), Tolerance);
        }

        [Test]
        public void RatioAndPercents_OutOfRange_Diverge()
        {
            // Out of range they part ways: Ratio clamps (to 1 -> 100), Percents does not (150).
            Assert.AreEqual(100f, MathUtility.Ratio(15f, 0f, 10f) * 100f, Tolerance);
            Assert.AreEqual(150f, MathUtility.Percents(15f, 0f, 10f), Tolerance);
        }

        #endregion

    }

}
