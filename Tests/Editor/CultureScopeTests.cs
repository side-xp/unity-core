using System.Globalization;
using System.Threading;

using NUnit.Framework;

namespace SideXP.Core.Tests
{

    public class CultureScopeTests
    {

        private CultureInfo _originalCulture;

        // These tests mutate the current thread's culture; capture and restore it so a failure
        // (or the class under test misbehaving) can't leak a culture into other tests.
        [SetUp]
        public void SetUp()
        {
            _originalCulture = Thread.CurrentThread.CurrentCulture;
        }

        [TearDown]
        public void TearDown()
        {
            Thread.CurrentThread.CurrentCulture = _originalCulture;
        }

        [Test]
        public void Constructor_Default_AppliesInvariantCulture()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            using (new CultureScope())
            {
                Assert.AreEqual(CultureInfo.InvariantCulture, Thread.CurrentThread.CurrentCulture);
            }
        }

        [Test]
        public void Constructor_WithCulture_AppliesThatCulture()
        {
            var target = CultureInfo.GetCultureInfo("de-DE");
            using (new CultureScope(target))
            {
                Assert.AreEqual("de-DE", Thread.CurrentThread.CurrentCulture.Name);
            }
        }

        [Test]
        public void Dispose_RestoresPreviousCulture()
        {
            var previous = CultureInfo.GetCultureInfo("fr-FR");
            Thread.CurrentThread.CurrentCulture = previous;

            using (new CultureScope(CultureInfo.InvariantCulture))
            {
                Assert.AreEqual(CultureInfo.InvariantCulture, Thread.CurrentThread.CurrentCulture);
            }

            // After the using block, the culture in effect before the scope is back.
            Assert.AreEqual("fr-FR", Thread.CurrentThread.CurrentCulture.Name);
        }

        [Test]
        public void NestedScopes_RestoreInReverseOrder()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

            using (new CultureScope(CultureInfo.GetCultureInfo("fr-FR")))
            {
                Assert.AreEqual("fr-FR", Thread.CurrentThread.CurrentCulture.Name);
                using (new CultureScope(CultureInfo.GetCultureInfo("de-DE")))
                {
                    Assert.AreEqual("de-DE", Thread.CurrentThread.CurrentCulture.Name);
                }
                // Inner scope restored the outer scope's culture, not the very first one.
                Assert.AreEqual("fr-FR", Thread.CurrentThread.CurrentCulture.Name);
            }

            Assert.AreEqual("en-US", Thread.CurrentThread.CurrentCulture.Name);
        }

        [Test]
        public void Invariant_Property_AppliesInvariantCulture()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            using (CultureScope.Invariant)
            {
                Assert.AreEqual(CultureInfo.InvariantCulture, Thread.CurrentThread.CurrentCulture);
            }
        }

        [Test]
        public void Scope_AffectsNumberFormatting()
        {
            // A concrete, observable consequence of the culture swap: the decimal separator.
            using (new CultureScope(CultureInfo.GetCultureInfo("fr-FR")))
            {
                Assert.AreEqual("1,5", 1.5f.ToString());
            }
            using (new CultureScope(CultureInfo.InvariantCulture))
            {
                Assert.AreEqual("1.5", 1.5f.ToString());
            }
        }

    }

}
