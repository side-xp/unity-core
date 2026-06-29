using System.Collections.Generic;

using NUnit.Framework;

namespace SideXP.Core.Tests
{

    public class IEnumerableExtensionsTests
    {

        #region Test types

        private class Animal { }
        private class Cat : Animal { }
        private class Kitten : Cat { }

        #endregion


        #region Join (non-generic)

        [Test]
        public void Join_JoinsElementStringRepresentations()
        {
            // Regression guard: a non-generic IEnumerable must join its elements, not its own ToString().
            Assert.AreEqual("1,2,3", new[] { 1, 2, 3 }.Join(","));
        }

        [Test]
        public void Join_StringElements()
        {
            Assert.AreEqual("a-b-c", new[] { "a", "b", "c" }.Join("-"));
        }

        [Test]
        public void Join_NullElements_BecomeEmpty()
        {
            Assert.AreEqual("a,,c", new[] { "a", null, "c" }.Join(","));
        }

        [Test]
        public void Join_NullSource_ReturnsEmpty()
        {
            Assert.AreEqual(string.Empty, ((System.Collections.IEnumerable)null).Join(","));
        }

        #endregion


        #region Join (mapped)

        [Test]
        public void JoinMapped_AppliesMapFunc()
        {
            Assert.AreEqual("n1-n2-n3", new[] { 1, 2, 3 }.Join("-", i => "n" + i));
        }

        [Test]
        public void JoinMapped_NullSource_ReturnsEmpty()
        {
            Assert.AreEqual(string.Empty, ((IEnumerable<int>)null).Join(",", i => i.ToString()));
        }

        #endregion


        #region Map

        [Test]
        public void Map_TransformsEachElement()
        {
            CollectionAssert.AreEqual(new[] { 2, 4, 6 }, new[] { 1, 2, 3 }.Map(i => i * 2));
        }

        [Test]
        public void Map_EmptySource_ReturnsEmpty()
        {
            CollectionAssert.IsEmpty(new int[0].Map(i => i));
        }

        [Test]
        public void Map_NullSource_ReturnsEmpty()
        {
            CollectionAssert.IsEmpty(((IEnumerable<int>)null).Map(i => i));
        }

        #endregion


        #region Convert

        [Test]
        public void Convert_UnboxesValueTypes()
        {
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, new object[] { 1, 2, 3 }.Convert<int>());
        }

        [Test]
        public void Convert_CastsReferenceTypes()
        {
            CollectionAssert.AreEqual(new[] { "a", "b" }, new object[] { "a", "b" }.Convert<string>());
        }

        #endregion


        #region Filter

        [Test]
        public void Filter_ExactTypeOnly_ByDefault()
        {
            Animal[] source = { new Animal(), new Cat(), new Kitten() };

            Cat[] cats = source.Filter<Animal, Cat>();

            // Only the exact Cat is kept; Kitten (derived) and Animal (base) are excluded.
            Assert.AreEqual(1, cats.Length);
            Assert.AreEqual(typeof(Cat), cats[0].GetType());
        }

        [Test]
        public void Filter_IncludesDerivedTypes_WhenRequested()
        {
            Animal[] source = { new Animal(), new Cat(), new Kitten() };

            Cat[] cats = source.Filter<Animal, Cat>(true);

            // Both the Cat and the Kitten (which derives from Cat) are kept.
            Assert.AreEqual(2, cats.Length);
        }

        [Test]
        public void Filter_NullElements_AreSkipped_ExactPath()
        {
            // Regression guard: the exact-type path used to throw on null elements.
            Animal[] source = { new Cat(), null };

            Cat[] cats = source.Filter<Animal, Cat>();

            Assert.AreEqual(1, cats.Length);
        }

        [Test]
        public void Filter_NullElements_AreSkipped_DerivedPath()
        {
            Animal[] source = { new Cat(), null };

            Cat[] cats = source.Filter<Animal, Cat>(true);

            Assert.AreEqual(1, cats.Length);
        }

        [Test]
        public void Filter_NullSource_ReturnsEmpty()
        {
            CollectionAssert.IsEmpty(((IEnumerable<Animal>)null).Filter<Animal, Cat>());
        }

        #endregion


        #region PickRandom

        [Test]
        public void PickRandom_EmptySource_ReturnsDefault()
        {
            Assert.AreEqual(0, new int[0].PickRandom());
        }

        [Test]
        public void PickRandom_SingleElement_ReturnsThatElement()
        {
            Assert.AreEqual(42, new[] { 42 }.PickRandom());
        }

        [Test]
        public void PickRandom_ReturnsAnElementFromTheSource()
        {
            int[] source = { 1, 2, 3, 4, 5 };

            // Sample several times; every result must be a member of the source.
            for (int i = 0; i < 50; i++)
                CollectionAssert.Contains(source, source.PickRandom());
        }

        [Test]
        public void PickRandom_NullSource_ReturnsDefault()
        {
            Assert.AreEqual(0, ((IEnumerable<int>)null).PickRandom());
        }

        #endregion

    }

}
