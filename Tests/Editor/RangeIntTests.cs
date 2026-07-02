using NUnit.Framework;

namespace SideXP.Core.Tests
{

    public class RangeIntTests
    {

        [Test]
        public void Constructor_MinMax_SetsBounds()
        {
            var range = new RangeInt(2, 5);
            Assert.AreEqual(2, range.Min);
            Assert.AreEqual(5, range.Max);
        }

        [Test]
        public void Constructor_MaxOnly_StartsAtZero()
        {
            var range = new RangeInt(5);
            Assert.AreEqual(0, range.Min);
            Assert.AreEqual(5, range.Max);
        }

        [Test]
        public void Constructor_Copy_CopiesBounds()
        {
            var original = new RangeInt(2, 5);
            var copy = new RangeInt(original);
            Assert.AreEqual(original.Min, copy.Min);
            Assert.AreEqual(original.Max, copy.Max);
        }

        [Test]
        public void MinMax_AreNormalized_WhenConstructedInverted()
        {
            var range = new RangeInt(5, 2);
            Assert.AreEqual(2, range.Min);
            Assert.AreEqual(5, range.Max);
        }

        [Test]
        public void Delta_IsAlwaysNonNegative()
        {
            Assert.AreEqual(3, new RangeInt(2, 5).Delta);
            Assert.AreEqual(3, new RangeInt(5, 2).Delta);
            Assert.AreEqual(0, new RangeInt(4, 4).Delta);
        }

        [Test]
        public void Setter_NormalizesOnGet()
        {
            var range = new RangeInt(2, 5);
            range.Min = 10;
            Assert.AreEqual(5, range.Min);   // min(10, 5)
            Assert.AreEqual(10, range.Max);  // max(10, 5)
        }

        [Test]
        public void Random_WithinInclusiveBounds()
        {
            UnityEngine.Random.InitState(123);
            var range = new RangeInt(2, 5);

            for (int i = 0; i < 200; i++)
            {
                int value = range.Random;
                Assert.GreaterOrEqual(value, 2);
                Assert.LessOrEqual(value, 5);
            }
        }

        [Test]
        public void Random_IncludesBothEnds()
        {
            // Over enough samples, both Min (2) and Max (5) must appear (the range is inclusive).
            UnityEngine.Random.InitState(123);
            var range = new RangeInt(2, 5);

            bool sawMin = false, sawMax = false;
            for (int i = 0; i < 500; i++)
            {
                int value = range.Random;
                if (value == 2) sawMin = true;
                if (value == 5) sawMax = true;
            }

            Assert.IsTrue(sawMin, "The minimum bound was never produced.");
            Assert.IsTrue(sawMax, "The maximum bound was never produced.");
        }

        [Test]
        public void Random_SinglePointRange_ReturnsThatValue()
        {
            // Regression guard: a [3,3] range used to return 0 instead of 3.
            Assert.AreEqual(3, new RangeInt(3, 3).Random);
        }

    }

}
