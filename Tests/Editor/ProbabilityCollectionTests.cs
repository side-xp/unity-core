using NUnit.Framework;

namespace SideXP.Core.Tests
{

    public class ProbabilityCollectionTests
    {

        /// <summary>
        /// Tolerance used when comparing floating-point results.
        /// </summary>
        private const float Tolerance = 1e-4f;


        #region Test doubles

        private class TestItem : IProbabilityItem
        {
            public object Data { get; }
            public float Probability { get; }
            public string Label => Data != null ? Data.ToString() : string.Empty;

            public TestItem(object data, float probability)
            {
                Data = data;
                Probability = probability;
            }
        }

        private class TestCollection : ProbabilityCollection
        {
            private readonly IProbabilityItem[] _items;
            public override IProbabilityItem[] Items => _items;
            public override IProbabilityItem this[int index] => _items[index];

            public TestCollection(params IProbabilityItem[] items)
            {
                _items = items;
            }
        }

        private static TestItem Item(object data, float probability)
        {
            return new TestItem(data, probability);
        }

        #endregion


        #region GetProbabilityPercents (deterministic)

        [Test]
        public void GetProbabilityPercents_ReturnsWeightShare()
        {
            var collection = new TestCollection(Item("A", 0.25f), Item("B", 0.75f));

            Assert.AreEqual(25f, collection.GetProbabilityPercents("A"), Tolerance);
            Assert.AreEqual(75f, collection.GetProbabilityPercents("B"), Tolerance);
        }

        [Test]
        public void GetProbabilityPercents_ExcludesNonPositiveFromTotal()
        {
            // The -0.3 item is excluded from the total (0.2 + 0.5 = 0.7).
            var collection = new TestCollection(Item("A", 0.2f), Item("B", -0.3f), Item("C", 0.5f));

            Assert.AreEqual(0.2f / 0.7f * 100f, collection.GetProbabilityPercents("A"), Tolerance);
            Assert.AreEqual(0.5f / 0.7f * 100f, collection.GetProbabilityPercents("C"), Tolerance);
            Assert.AreEqual(0f, collection.GetProbabilityPercents("B"), Tolerance);
        }

        [Test]
        public void GetProbabilityPercents_PositiveSharesSumTo100()
        {
            var collection = new TestCollection(Item("A", 0.1f), Item("B", 0.4f), Item("C", 0.5f));

            float sum = collection.GetProbabilityPercents("A")
                + collection.GetProbabilityPercents("B")
                + collection.GetProbabilityPercents("C");

            Assert.AreEqual(100f, sum, Tolerance);
        }

        [Test]
        public void GetProbabilityPercents_UnknownItem_ReturnsZero()
        {
            var collection = new TestCollection(Item("A", 1f));
            Assert.AreEqual(0f, collection.GetProbabilityPercents("missing"), Tolerance);
        }

        [Test]
        public void GetProbabilityPercents_AllNonPositive_ReturnsZero()
        {
            var collection = new TestCollection(Item("A", 0f), Item("B", -1f));
            Assert.AreEqual(0f, collection.GetProbabilityPercents("A"), Tolerance);
        }

        #endregion


        #region Get (randomized, seeded for determinism)

        [Test]
        public void Get_DistributesAccordingToWeights()
        {
            UnityEngine.Random.InitState(12345);
            var collection = new TestCollection(Item("A", 0.25f), Item("B", 0.75f));

            const int samples = 20000;
            int countA = 0, countB = 0;
            for (int i = 0; i < samples; i++)
            {
                Assert.IsTrue(collection.Get(out object data));
                if ((string)data == "A") countA++;
                else if ((string)data == "B") countB++;
            }

            Assert.AreEqual(samples, countA + countB);
            Assert.AreEqual(0.25f, (float)countA / samples, 0.03f);
            Assert.AreEqual(0.75f, (float)countB / samples, 0.03f);
        }

        [Test]
        public void Get_FrequencyMatches_GetProbabilityPercents()
        {
            // The heart of the fix: actual pick frequency must agree with the reported percentage,
            // even with a negative (excluded) item present.
            UnityEngine.Random.InitState(2024);
            var collection = new TestCollection(Item("A", 0.2f), Item("B", -0.3f), Item("C", 0.5f));

            const int samples = 20000;
            int countA = 0, countC = 0;
            for (int i = 0; i < samples; i++)
            {
                collection.Get(out object data);
                if ((string)data == "A") countA++;
                else if ((string)data == "C") countC++;
            }

            Assert.AreEqual(collection.GetProbabilityPercents("A") / 100f, (float)countA / samples, 0.03f);
            Assert.AreEqual(collection.GetProbabilityPercents("C") / 100f, (float)countC / samples, 0.03f);
        }

        [Test]
        public void Get_NeverPicksNonPositiveProbabilityItems()
        {
            UnityEngine.Random.InitState(987);
            var collection = new TestCollection(Item("A", 1f), Item("B", -1f), Item("C", 0f));

            for (int i = 0; i < 1000; i++)
            {
                Assert.IsTrue(collection.Get(out object data));
                Assert.AreEqual("A", data);
            }
        }

        [Test]
        public void Get_SinglePositiveItem_AlwaysReturnsIt()
        {
            UnityEngine.Random.InitState(1);
            var collection = new TestCollection(Item("only", 0.42f));

            for (int i = 0; i < 100; i++)
            {
                Assert.IsTrue(collection.Get(out object data));
                Assert.AreEqual("only", data);
            }
        }

        [Test]
        public void Get_AllNonPositive_ReturnsFalse()
        {
            var collection = new TestCollection(Item("A", 0f), Item("B", -2f));

            Assert.IsFalse(collection.Get(out object data));
            Assert.IsNull(data);
        }

        [Test]
        public void Get_EmptyCollection_ReturnsFalse()
        {
            var collection = new TestCollection();

            Assert.IsFalse(collection.Get(out object data));
            Assert.IsNull(data);
        }

        #endregion


        #region Get (no-arg)

        [Test]
        public void GetNoArg_ReturnsValue_WhenPickable()
        {
            UnityEngine.Random.InitState(5);
            var collection = new TestCollection(Item("A", 1f));

            Assert.AreEqual("A", collection.Get());
        }

        [Test]
        public void GetNoArg_ReturnsNull_WhenNothingPickable()
        {
            var collection = new TestCollection(Item("A", 0f));

            Assert.IsNull(collection.Get());
        }

        #endregion

    }

}
