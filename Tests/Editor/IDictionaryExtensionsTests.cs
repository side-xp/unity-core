using System.Collections.Generic;

using NUnit.Framework;

namespace SideXP.Core.Tests
{

    public class IDictionaryExtensionsTests
    {

        #region GetOrAdd

        [Test]
        public void GetOrAdd_ExistingKey_ReturnsValueWithoutModifying()
        {
            var dict = new Dictionary<string, int> { { "a", 42 } };
            dict.GetOrAdd("a", out int value, 99);

            Assert.AreEqual(42, value);        // existing value, not the default
            Assert.AreEqual(1, dict.Count);    // nothing added
            Assert.AreEqual(42, dict["a"]);    // not overwritten
        }

        [Test]
        public void GetOrAdd_MissingKey_AddsAndReturnsProvidedDefault()
        {
            var dict = new Dictionary<string, int>();
            dict.GetOrAdd("a", out int value, 99);

            Assert.AreEqual(99, value);
            Assert.AreEqual(1, dict.Count);
            Assert.AreEqual(99, dict["a"]);
        }

        [Test]
        public void GetOrAdd_MissingKey_NoDefaultOverload_AddsTypeDefault()
        {
            var dict = new Dictionary<string, int>();
            dict.GetOrAdd("a", out int value);

            Assert.AreEqual(0, value);         // default(int)
            Assert.AreEqual(0, dict["a"]);
        }

        [Test]
        public void GetOrAdd_MissingKey_ReferenceType_AddsNullDefault()
        {
            var dict = new Dictionary<string, string>();
            dict.GetOrAdd("a", out string value);

            Assert.IsNull(value);              // default(string)
            Assert.IsTrue(dict.ContainsKey("a"));
            Assert.IsNull(dict["a"]);
        }

        #endregion

    }

}
