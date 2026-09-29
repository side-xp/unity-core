using System;
using System.Collections.Generic;

using NUnit.Framework;

namespace SideXP.Core.Tests
{

    public class ValueModifierTests
    {

        private const float Tolerance = 0.0001f;

        #region Constructor

        [Test]
        public void Constructor_SetsOperatorAndValue()
        {
            ValueModifier modifier = new ValueModifier(EOperator.Multiply, 1.5f);
            Assert.AreEqual(EOperator.Multiply, modifier.Operator);
            Assert.AreEqual(1.5f, modifier.Value);
        }

        [Test]
        public void Default_LeavesValueUnchanged()
        {
            Assert.AreEqual(42f, default(ValueModifier).ApplyTo(42f));
        }

        #endregion


        #region ApplyTo

        [Test]
        public void ApplyTo_ModifierIsRightHandOperand()
        {
            Assert.AreEqual(90f, new ValueModifier(EOperator.Subtract, 10).ApplyTo(100), Tolerance);
            Assert.AreEqual(50f, new ValueModifier(EOperator.Divide, 2).ApplyTo(100), Tolerance);
            Assert.AreEqual(10000f, new ValueModifier(EOperator.Power, 2).ApplyTo(100), Tolerance);
        }

        [Test]
        public void ApplyTo_Set_ReturnsModifierValue()
        {
            Assert.AreEqual(3f, new ValueModifier(EOperator.Set, 3).ApplyTo(100));
        }

        [Test]
        public void ApplyTo_DivideByZero_ReturnsInputUnchanged()
        {
            Assert.AreEqual(100f, new ValueModifier(EOperator.Divide, 0).ApplyTo(100));
        }

        [Test]
        public void ApplyTo_UndefinedOperator_Throws()
        {
            ValueModifier modifier = new ValueModifier((EOperator)999, 2);
            Assert.Throws<ArgumentOutOfRangeException>(() => modifier.ApplyTo(100));
        }

        #endregion


        #region ApplyAll

        [Test]
        public void ApplyAll_NoModifiers_ReturnsBaseValue()
        {
            Assert.AreEqual(100f, ValueModifierUtility.ApplyAll(100));
            Assert.AreEqual(100f, ValueModifierUtility.ApplyAll(100, new List<ValueModifier>()));
        }

        [Test]
        public void ApplyAll_NullModifiers_ReturnsBaseValue()
        {
            Assert.AreEqual(100f, ValueModifierUtility.ApplyAll(100, (ValueModifier[])null));
            Assert.AreEqual(100f, ValueModifierUtility.ApplyAll(100, (IEnumerable<ValueModifier>)null));
        }

        [Test]
        public void ApplyAll_AppliesModifiersInOrder()
        {
            ValueModifier bonus = new ValueModifier(EOperator.Add, 10);
            ValueModifier multiplier = new ValueModifier(EOperator.Multiply, 2);

            Assert.AreEqual(220f, ValueModifierUtility.ApplyAll(100, bonus, multiplier), Tolerance);
            Assert.AreEqual(210f, ValueModifierUtility.ApplyAll(100, multiplier, bonus), Tolerance);
        }

        [Test]
        public void ApplyAll_EnumerableAndArray_GiveSameResult()
        {
            ValueModifier[] modifiers = new ValueModifier[]
            {
                new ValueModifier(EOperator.Add, 10),
                new ValueModifier(EOperator.Multiply, 2),
                new ValueModifier(EOperator.Subtract, 20),
            };

            Assert.AreEqual(200f, ValueModifierUtility.ApplyAll(100, modifiers), Tolerance);
            Assert.AreEqual(200f, ValueModifierUtility.ApplyAll(100, new List<ValueModifier>(modifiers)), Tolerance);
        }

        [Test]
        public void ApplyAll_Set_DiscardsPreviousResult()
        {
            float result = ValueModifierUtility.ApplyAll(
                5,
                new ValueModifier(EOperator.Multiply, 1.5f),
                new ValueModifier(EOperator.Set, 0),
                new ValueModifier(EOperator.Add, 1)
            );
            Assert.AreEqual(1f, result, Tolerance);
        }

        [Test]
        public void ApplyAll_IgnoresDivisionsByZero()
        {
            float result = ValueModifierUtility.ApplyAll(
                100,
                new ValueModifier(EOperator.Divide, 0),
                new ValueModifier(EOperator.Modulo, 0),
                new ValueModifier(EOperator.Divide, 4)
            );
            Assert.AreEqual(25f, result, Tolerance);
        }

        #endregion

    }

}
