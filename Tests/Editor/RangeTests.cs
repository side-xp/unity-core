using NUnit.Framework;

namespace SideXP.Core.Tests
{

    public class RangeTests
    {

        private const float Tolerance = 1e-5f;

        [Test]
        public void Constructor_MinMax_SetsBounds()
        {
            var range = new Range(2f, 5f);
            Assert.AreEqual(2f, range.Min, Tolerance);
            Assert.AreEqual(5f, range.Max, Tolerance);
        }

        [Test]
        public void Constructor_MaxOnly_StartsAtZero()
        {
            var range = new Range(5f);
            Assert.AreEqual(0f, range.Min, Tolerance);
            Assert.AreEqual(5f, range.Max, Tolerance);
        }

        [Test]
        public void Constructor_Copy_CopiesBounds()
        {
            var original = new Range(2f, 5f);
            var copy = new Range(original);
            Assert.AreEqual(original.Min, copy.Min, Tolerance);
            Assert.AreEqual(original.Max, copy.Max, Tolerance);
        }

        [Test]
        public void MinMax_AreNormalized_WhenConstructedInverted()
        {
            // Min/Max always return the smaller/larger bound, regardless of argument order.
            var range = new Range(5f, 2f);
            Assert.AreEqual(2f, range.Min, Tolerance);
            Assert.AreEqual(5f, range.Max, Tolerance);
        }

        [Test]
        public void Delta_IsAlwaysNonNegative()
        {
            Assert.AreEqual(3f, new Range(2f, 5f).Delta, Tolerance);
            Assert.AreEqual(3f, new Range(5f, 2f).Delta, Tolerance);
            Assert.AreEqual(0f, new Range(4f, 4f).Delta, Tolerance);
        }

        [Test]
        public void Setter_NormalizesOnGet()
        {
            // The setters write the raw fields, but the getters normalize: setting Min above the current
            // Max effectively swaps which bound is returned as Min/Max.
            var range = new Range(2f, 5f);
            range.Min = 10f;
            Assert.AreEqual(5f, range.Min, Tolerance);   // min(10, 5)
            Assert.AreEqual(10f, range.Max, Tolerance);  // max(10, 5)
        }

        [Test]
        public void Random_WithinInclusiveBounds()
        {
            UnityEngine.Random.InitState(123);
            var range = new Range(2f, 5f);

            for (int i = 0; i < 100; i++)
            {
                float value = range.Random;
                Assert.GreaterOrEqual(value, 2f);
                Assert.LessOrEqual(value, 5f);
            }
        }

        [Test]
        public void Random_SinglePointRange_ReturnsThatValue()
        {
            Assert.AreEqual(3f, new Range(3f, 3f).Random, Tolerance);
        }

    }

}
