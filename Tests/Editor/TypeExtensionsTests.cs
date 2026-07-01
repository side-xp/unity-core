using System.Collections.Generic;

using NUnit.Framework;

namespace SideXP.Core.Tests
{

    public class TypeExtensionsTests
    {

        private class Base { }
        private class Derived : Base { }
        private interface IFoo { }
        private class FooImpl : IFoo { }

        #region Inherits

        [Test]
        public void Inherits_DerivedFromBase_ReturnsTrue()
        {
            Assert.IsTrue(typeof(Derived).Inherits(typeof(Base)));
        }

        [Test]
        public void Inherits_SameType_ReturnsFalse()
        {
            // A type does not "inherit" from itself.
            Assert.IsFalse(typeof(Base).Inherits(typeof(Base)));
        }

        [Test]
        public void Inherits_BaseFromDerived_ReturnsFalse()
        {
            Assert.IsFalse(typeof(Base).Inherits(typeof(Derived)));
        }

        [Test]
        public void Inherits_ImplementedInterface_ReturnsTrue()
        {
            Assert.IsTrue(typeof(FooImpl).Inherits(typeof(IFoo)));
        }

        [Test]
        public void Inherits_Generic_MatchesNonGeneric()
        {
            Assert.IsTrue(typeof(Derived).Inherits<Base>());
            Assert.IsFalse(typeof(Base).Inherits<Base>());
        }

        #endregion


        #region Is

        [Test]
        public void Is_DerivedFromBase_ReturnsTrue()
        {
            Assert.IsTrue(typeof(Derived).Is(typeof(Base)));
        }

        [Test]
        public void Is_SameType_ReturnsTrue()
        {
            // Unlike Inherits, Is includes the type itself.
            Assert.IsTrue(typeof(Base).Is(typeof(Base)));
        }

        [Test]
        public void Is_Unrelated_ReturnsFalse()
        {
            Assert.IsFalse(typeof(Base).Is(typeof(Derived)));
        }

        [Test]
        public void Is_Generic_MatchesNonGeneric()
        {
            Assert.IsTrue(typeof(Derived).Is<Base>());
            Assert.IsTrue(typeof(Base).Is<Base>());
        }

        #endregion

    }

}
