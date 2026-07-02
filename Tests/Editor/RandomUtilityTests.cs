using NUnit.Framework;

namespace SideXP.Core.Tests
{

    public class RandomUtilityTests
    {

        [Test]
        public void RandomAmong_ReturnsOneOfTheValues()
        {
            UnityEngine.Random.InitState(123);
            int[] values = { 1, 2, 3, 4, 5 };

            for (int i = 0; i < 50; i++)
                CollectionAssert.Contains(values, RandomUtility.RandomAmong(values));
        }

        [Test]
        public void RandomAmong_SingleValue_ReturnsIt()
        {
            Assert.AreEqual(42, RandomUtility.RandomAmong(42));
        }

        [Test]
        public void RandomAmong_NoValues_ReturnsDefault()
        {
            // An empty argument list yields default instead of throwing.
            Assert.AreEqual(0, RandomUtility.RandomAmong<int>());
        }

        [Test]
        public void RandomAmong_NoReferenceValues_ReturnsNull()
        {
            Assert.IsNull(RandomUtility.RandomAmong<string>());
        }

        [Test]
        public void RandomAmong_MixedTypes_ReturnsOneOfTheValues()
        {
            UnityEngine.Random.InitState(7);
            object[] values = { 1, "a", 2.0 };

            for (int i = 0; i < 20; i++)
                CollectionAssert.Contains(values, RandomUtility.RandomAmong(1, "a", 2.0));
        }

    }

}
