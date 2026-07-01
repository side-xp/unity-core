using System.Collections.Generic;

using NUnit.Framework;

using Random = UnityEngine.Random;

namespace SideXP.Core.Tests
{

    public class ListExtensionsTests
    {

        #region AddOnce

        [Test]
        public void AddOnce_NewItem_AddsAndReturnsTrue()
        {
            var list = new List<int> { 1, 2 };
            bool added = list.AddOnce(3);
            Assert.IsTrue(added);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, list);
        }

        [Test]
        public void AddOnce_ExistingItem_DoesNotAddAndReturnsFalse()
        {
            var list = new List<int> { 1, 2 };
            bool added = list.AddOnce(2);
            Assert.IsFalse(added);
            CollectionAssert.AreEqual(new[] { 1, 2 }, list);
        }

        #endregion


        #region Move

        [Test]
        public void Move_Forward_ShiftsItemsBetween()
        {
            var list = new List<string> { "A", "B", "C", "D" };
            bool moved = list.Move(0, 2);
            Assert.IsTrue(moved);
            CollectionAssert.AreEqual(new[] { "B", "C", "A", "D" }, list);
        }

        [Test]
        public void Move_Backward_ShiftsItemsBetween()
        {
            var list = new List<string> { "A", "B", "C", "D" };
            bool moved = list.Move(3, 1);
            Assert.IsTrue(moved);
            CollectionAssert.AreEqual(new[] { "A", "D", "B", "C" }, list);
        }

        [Test]
        public void Move_SameIndex_ReturnsFalse()
        {
            var list = new List<int> { 1, 2, 3 };
            Assert.IsFalse(list.Move(1, 1));
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, list);
        }

        [Test]
        public void Move_EmptyList_ReturnsFalse()
        {
            var list = new List<int>();
            Assert.IsFalse(list.Move(0, 1));
        }

        [Test]
        public void Move_OutOfRangeIndices_AreClamped()
        {
            var list = new List<string> { "A", "B", "C" };
            // targetIndex clamps to Count - 1 (2).
            bool moved = list.Move(0, 99);
            Assert.IsTrue(moved);
            CollectionAssert.AreEqual(new[] { "B", "C", "A" }, list);
        }

        #endregion


        #region IsInRange

        [Test]
        public void IsInRange_ValidIndex_ReturnsTrue()
        {
            var list = new List<int> { 1, 2, 3 };
            Assert.IsTrue(list.IsInRange(0));
            Assert.IsTrue(list.IsInRange(2));
        }

        [Test]
        public void IsInRange_OutOfBounds_ReturnsFalse()
        {
            var list = new List<int> { 1, 2, 3 };
            Assert.IsFalse(list.IsInRange(-1));
            Assert.IsFalse(list.IsInRange(3));
        }

        #endregion


        #region Shuffle

        [Test]
        public void Shuffle_IsPermutationOfOriginal()
        {
            Random.InitState(12345);
            var list = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 };
            list.Shuffle();
            CollectionAssert.AreEquivalent(new[] { 1, 2, 3, 4, 5, 6, 7, 8 }, list);
        }

        [Test]
        public void Shuffle_SameSeed_ProducesSameResult()
        {
            var a = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 };
            var b = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 };

            Random.InitState(999);
            a.Shuffle();
            Random.InitState(999);
            b.Shuffle();

            CollectionAssert.AreEqual(a, b); // deterministic for a given seed
        }

        [Test]
        public void ShuffleCrypto_IsPermutationOfOriginal()
        {
            var list = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 };
            list.ShuffleCrypto();
            CollectionAssert.AreEquivalent(new[] { 1, 2, 3, 4, 5, 6, 7, 8 }, list);
        }

        #endregion


        #region Slice

        [Test]
        public void Slice_StartAndEnd_ExtractsRangeExcludingEnd()
        {
            var list = new List<int> { 10, 20, 30, 40, 50 };
            CollectionAssert.AreEqual(new[] { 20, 30 }, list.Slice(1, 3));
        }

        [Test]
        public void Slice_StartOnly_ExtractsToEnd()
        {
            var list = new List<int> { 10, 20, 30, 40, 50 };
            CollectionAssert.AreEqual(new[] { 30, 40, 50 }, list.Slice(2));
        }

        [Test]
        public void Slice_NegativeStart_CountsFromEnd()
        {
            var list = new List<int> { 10, 20, 30, 40, 50 };
            CollectionAssert.AreEqual(new[] { 40, 50 }, list.Slice(-2));
        }

        [Test]
        public void Slice_NegativeEnd_CountsFromEnd()
        {
            var list = new List<int> { 10, 20, 30, 40, 50 };
            CollectionAssert.AreEqual(new[] { 20, 30, 40 }, list.Slice(1, -1));
        }

        [Test]
        public void Slice_EndBeforeStart_ReturnsEmpty()
        {
            var list = new List<int> { 10, 20, 30, 40, 50 };
            Assert.AreEqual(0, list.Slice(3, 1).Length);
        }

        #endregion


        #region RemoveDoubles

        [Test]
        public void RemoveDoubles_Comparable_KeepsFirstOccurrence()
        {
            var list = new List<int> { 1, 1, 2, 3, 3, 3 };
            int removed = list.RemoveDoubles();
            Assert.AreEqual(3, removed);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, list);
        }

        [Test]
        public void RemoveDoubles_NoDuplicates_RemovesNothing()
        {
            var list = new List<int> { 1, 2, 3 };
            int removed = list.RemoveDoubles();
            Assert.AreEqual(0, removed);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, list);
        }

        [Test]
        public void RemoveDoubles_CustomComparator_UsesIt()
        {
            var list = new List<string> { "a", "A", "b", "B", "a" };
            // Case-insensitive similarity: only the first of each letter is kept.
            int removed = list.RemoveDoubles((x, y) => string.Equals(x, y, System.StringComparison.OrdinalIgnoreCase));
            Assert.AreEqual(3, removed);
            CollectionAssert.AreEqual(new[] { "a", "b" }, list);
        }

        #endregion

    }

}
