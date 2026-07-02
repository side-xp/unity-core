using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using NUnit.Framework;

namespace SideXP.Core.Tests
{

    public class CollectionWrapperTests
    {

        #region ArrayWrapper

        [Test]
        public void ArrayWrapper_Default_IsEmptyNotNull()
        {
            var wrapper = new ArrayWrapper<int>();
            Assert.IsNotNull(wrapper.Elements);
            Assert.AreEqual(0, wrapper.Elements.Length);
        }

        [Test]
        public void ArrayWrapper_Elements_RoundTripsAssignedArray()
        {
            var wrapper = new ArrayWrapper<int>();
            int[] array = { 1, 2, 3 };
            wrapper.Elements = array;
            // The wrapper exposes the very same array instance, not a copy.
            Assert.AreSame(array, wrapper.Elements);
        }

        [Test]
        public void ArrayWrapper_Enumeration_YieldsElementsInOrder()
        {
            var wrapper = new ArrayWrapper<string> { Elements = new[] { "a", "b", "c" } };

            var collected = new List<string>();
            foreach (string item in wrapper)
                collected.Add(item);

            CollectionAssert.AreEqual(new[] { "a", "b", "c" }, collected);
        }

        [Test]
        public void ArrayWrapper_NonGenericEnumeration_YieldsElementsInOrder()
        {
            var wrapper = new ArrayWrapper<int> { Elements = new[] { 10, 20 } };

            var collected = new List<object>();
            foreach (object item in (IEnumerable)wrapper)
                collected.Add(item);

            CollectionAssert.AreEqual(new object[] { 10, 20 }, collected);
        }

        #endregion


        #region ListWrapper

        [Test]
        public void ListWrapper_Default_IsEmptyNotNull()
        {
            var wrapper = new ListWrapper<int>();
            Assert.IsNotNull(wrapper.Elements);
            Assert.AreEqual(0, wrapper.Elements.Count);
        }

        [Test]
        public void ListWrapper_Elements_ExposesBackingListForInPlaceMutation()
        {
            var wrapper = new ListWrapper<int>();
            // Elements is get-only, so mutation happens through the returned list instance.
            wrapper.Elements.Add(1);
            wrapper.Elements.Add(2);
            CollectionAssert.AreEqual(new[] { 1, 2 }, wrapper.Elements);
        }

        [Test]
        public void ListWrapper_Enumeration_YieldsElementsInOrder()
        {
            var wrapper = new ListWrapper<string>();
            wrapper.Elements.AddRange(new[] { "x", "y", "z" });

            var collected = new List<string>();
            foreach (string item in wrapper)
                collected.Add(item);

            CollectionAssert.AreEqual(new[] { "x", "y", "z" }, collected);
        }

        [Test]
        public void ListWrapper_NonGenericEnumeration_YieldsElementsInOrder()
        {
            var wrapper = new ListWrapper<int>();
            wrapper.Elements.AddRange(new[] { 5, 6 });

            var collected = new List<object>();
            foreach (object item in (IEnumerable)wrapper)
                collected.Add(item);

            CollectionAssert.AreEqual(new object[] { 5, 6 }, collected);
        }

        #endregion


        #region CollectionWrapperUtility

        [Test]
        public void ElementsProp_HasExpectedName()
        {
            Assert.AreEqual("_elements", CollectionWrapperUtility.ElementsProp);
        }

        // The whole point of the const is that drawers can find the serialized field by name.
        // Pin that the actual backing field on both wrappers matches it.
        [Test]
        public void ElementsProp_MatchesArrayWrapperBackingField()
        {
            FieldInfo field = typeof(ArrayWrapper<int>).GetField(
                CollectionWrapperUtility.ElementsProp,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, "ArrayWrapper has no field named '_elements'.");
        }

        [Test]
        public void ElementsProp_MatchesListWrapperBackingField()
        {
            FieldInfo field = typeof(ListWrapper<int>).GetField(
                CollectionWrapperUtility.ElementsProp,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, "ListWrapper has no field named '_elements'.");
        }

        #endregion

    }

}
