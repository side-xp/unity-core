using System;

using NUnit.Framework;

namespace SideXP.Core.Tests
{

    public class OptionalValueTests
    {

        #region Construction

        [Test]
        public void DefaultConstructor_LeavesValueDefaultAndDisabled()
        {
            var optional = new OptionalValue<int>();
            Assert.AreEqual(0, optional.Value);
            Assert.IsFalse(optional.Enabled);
        }

        [Test]
        public void Constructor_ValueOnly_DefaultsToDisabled()
        {
            var optional = new OptionalValue<int>(42);
            Assert.AreEqual(42, optional.Value);
            Assert.IsFalse(optional.Enabled);
        }

        [Test]
        public void Constructor_ValueAndEnabled_StoresBoth()
        {
            var optional = new OptionalValue<string>("hello", true);
            Assert.AreEqual("hello", optional.Value);
            Assert.IsTrue(optional.Enabled);
        }

        #endregion


        #region Properties

        [Test]
        public void Value_Setter_UpdatesValue()
        {
            var optional = new OptionalValue<int>(1);
            optional.Value = 7;
            Assert.AreEqual(7, optional.Value);
        }

        [Test]
        public void Enabled_Setter_UpdatesFlag()
        {
            var optional = new OptionalValue<int>(1);
            Assert.IsFalse(optional.Enabled);
            optional.Enabled = true;
            Assert.IsTrue(optional.Enabled);
        }

        #endregion


        #region Non-generic IOptionalValue interface

        [Test]
        public void NonGenericValue_Get_ReturnsBoxedValue()
        {
            IOptionalValue optional = new OptionalValue<int>(99);
            Assert.AreEqual(99, optional.Value);
        }

        [Test]
        public void NonGenericValue_Set_UpdatesTypedValue()
        {
            var optional = new OptionalValue<int>(0);
            ((IOptionalValue)optional).Value = 123;
            Assert.AreEqual(123, optional.Value);
        }

        [Test]
        public void NonGenericValue_SetWrongType_ThrowsInvalidCast()
        {
            IOptionalValue optional = new OptionalValue<int>(0);
            Assert.Throws<InvalidCastException>(() => optional.Value = "not an int");
        }

        #endregion


        #region Implicit conversions

        [Test]
        public void ImplicitToValue_ReturnsUnderlyingValue()
        {
            var optional = new OptionalValue<int>(55, true);
            int value = optional;
            Assert.AreEqual(55, value);
        }

        // The conversion exposes the value regardless of Enabled; it does not gate on the flag.
        [Test]
        public void ImplicitToValue_IgnoresEnabledFlag()
        {
            var optional = new OptionalValue<int>(55, false);
            int value = optional;
            Assert.AreEqual(55, value);
        }

        [Test]
        public void ImplicitFromValue_CreatesDisabledOptional()
        {
            OptionalValue<int> optional = 88;
            Assert.AreEqual(88, optional.Value);
            // The value-to-optional conversion goes through the single-arg constructor, which defaults to disabled.
            Assert.IsFalse(optional.Enabled);
        }

        #endregion

    }

}
