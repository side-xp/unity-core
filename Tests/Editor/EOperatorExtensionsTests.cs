using System;

using NUnit.Framework;

namespace SideXP.Core.Tests
{

    public class EOperatorExtensionsTests
    {

        private const float Tolerance = 0.0001f;
        private const EOperator UndefinedOperator = (EOperator)999;

        #region Apply (int)

        [Test]
        public void ApplyInt_Add()
        {
            Assert.AreEqual(9, EOperator.Add.Apply(7, 2));
        }

        [Test]
        public void ApplyInt_Subtract()
        {
            Assert.AreEqual(5, EOperator.Subtract.Apply(7, 2));
        }

        [Test]
        public void ApplyInt_Multiply()
        {
            Assert.AreEqual(14, EOperator.Multiply.Apply(7, 2));
        }

        [Test]
        public void ApplyInt_Divide_IsTruncated()
        {
            Assert.AreEqual(3, EOperator.Divide.Apply(7, 2));
        }

        [Test]
        public void ApplyInt_DivideByZero_ReturnsLeftOperand()
        {
            Assert.AreEqual(7, EOperator.Divide.Apply(7, 0));
        }

        [Test]
        public void ApplyInt_Modulo()
        {
            Assert.AreEqual(1, EOperator.Modulo.Apply(7, 2));
        }

        [Test]
        public void ApplyInt_ModuloByZero_ReturnsLeftOperand()
        {
            Assert.AreEqual(7, EOperator.Modulo.Apply(7, 0));
        }

        [Test]
        public void ApplyInt_Set_ReturnsRightOperand()
        {
            Assert.AreEqual(2, EOperator.Set.Apply(7, 2));
        }

        [Test]
        public void ApplyInt_Power()
        {
            Assert.AreEqual(1024, EOperator.Power.Apply(2, 10));
            Assert.AreEqual(1, EOperator.Power.Apply(7, 0));
        }

        [Test]
        public void ApplyInt_PowerWithNegativeExponent_IsTruncated()
        {
            Assert.AreEqual(0, EOperator.Power.Apply(2, -1));
        }

        [Test]
        public void ApplyInt_PowerOfZeroWithNegativeExponent_ReturnsLeftOperand()
        {
            Assert.AreEqual(0, EOperator.Power.Apply(0, -1));
        }

        [Test]
        public void ApplyInt_UndefinedOperator_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => UndefinedOperator.Apply(7, 2));
        }

        #endregion


        #region Apply (float)

        [Test]
        public void ApplyFloat_Add()
        {
            Assert.AreEqual(9.5f, EOperator.Add.Apply(7f, 2.5f), Tolerance);
        }

        [Test]
        public void ApplyFloat_Subtract()
        {
            Assert.AreEqual(4.5f, EOperator.Subtract.Apply(7f, 2.5f), Tolerance);
        }

        [Test]
        public void ApplyFloat_Multiply()
        {
            Assert.AreEqual(17.5f, EOperator.Multiply.Apply(7f, 2.5f), Tolerance);
        }

        [Test]
        public void ApplyFloat_Divide()
        {
            Assert.AreEqual(3.5f, EOperator.Divide.Apply(7f, 2f), Tolerance);
        }

        [Test]
        public void ApplyFloat_DivideByZero_ReturnsLeftOperand()
        {
            Assert.AreEqual(7f, EOperator.Divide.Apply(7f, 0f));
        }

        [Test]
        public void ApplyFloat_Modulo()
        {
            Assert.AreEqual(2f, EOperator.Modulo.Apply(7f, 2.5f), Tolerance);
        }

        [Test]
        public void ApplyFloat_ModuloByZero_ReturnsLeftOperand()
        {
            Assert.AreEqual(7f, EOperator.Modulo.Apply(7f, 0f));
        }

        [Test]
        public void ApplyFloat_Set_ReturnsRightOperand()
        {
            Assert.AreEqual(2.5f, EOperator.Set.Apply(7f, 2.5f));
        }

        [Test]
        public void ApplyFloat_Power()
        {
            Assert.AreEqual(1024f, EOperator.Power.Apply(2f, 10f), Tolerance);
            Assert.AreEqual(0.5f, EOperator.Power.Apply(2f, -1f), Tolerance);
            Assert.AreEqual(3f, EOperator.Power.Apply(9f, 0.5f), Tolerance);
        }

        [Test]
        public void ApplyFloat_PowerOfZeroWithNegativeExponent_ReturnsLeftOperand()
        {
            Assert.AreEqual(0f, EOperator.Power.Apply(0f, -1f));
        }

        [Test]
        public void ApplyFloat_UndefinedOperator_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => UndefinedOperator.Apply(7f, 2f));
        }

        #endregion


        #region Apply (variadic)

        [Test]
        public void ApplyVariadic_NullOrEmpty_ReturnsZero()
        {
            Assert.AreEqual(0, EOperator.Add.Apply(new int[0]));
            Assert.AreEqual(0, EOperator.Add.Apply((int[])null));
            Assert.AreEqual(0f, EOperator.Add.Apply(new float[0]));
            Assert.AreEqual(0f, EOperator.Add.Apply((float[])null));
        }

        [Test]
        public void ApplyVariadic_SingleValue_ReturnsThatValue()
        {
            Assert.AreEqual(7, EOperator.Subtract.Apply(new int[] { 7 }));
            Assert.AreEqual(7f, EOperator.Subtract.Apply(new float[] { 7f }));
        }

        [Test]
        public void ApplyVariadic_IsComputedFromLeftToRight()
        {
            Assert.AreEqual(5, EOperator.Subtract.Apply(10, 3, 2));
            Assert.AreEqual(5f, EOperator.Subtract.Apply(10f, 3f, 2f), Tolerance);
            Assert.AreEqual(64, EOperator.Power.Apply(2, 3, 2));
        }

        [Test]
        public void ApplyVariadic_IgnoresDivisionsByZero()
        {
            Assert.AreEqual(20, EOperator.Divide.Apply(100, 0, 5));
            Assert.AreEqual(25f, EOperator.Divide.Apply(100f, 0f, 4f), Tolerance);
        }

        [Test]
        public void ApplyVariadic_UndefinedOperator_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => UndefinedOperator.Apply(1, 2, 3));
            Assert.Throws<ArgumentOutOfRangeException>(() => UndefinedOperator.Apply(1f, 2f, 3f));
        }

        #endregion

    }

}
