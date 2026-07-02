using System;
using System.Collections.Generic;

using NUnit.Framework;

namespace SideXP.Core.Tests
{

    public class StatisticsUtilityTests
    {

        private const float Tolerance = 1e-5f;

        #region Sum

        [Test]
        public void Sum_Float_AddsAllValues()
        {
            var values = new List<float> { 1.5f, 2.5f, 3f };
            Assert.AreEqual(7f, values.Sum(), Tolerance);
        }

        [Test]
        public void Sum_Float_Empty_IsZero()
        {
            Assert.AreEqual(0f, new List<float>().Sum(), Tolerance);
        }

        [Test]
        public void Sum_Int_AddsAllValues()
        {
            var values = new List<int> { 1, 2, 3 };
            Assert.AreEqual(6, values.Sum());
        }

        [Test]
        public void Sum_Int_Empty_IsZero()
        {
            Assert.AreEqual(0, new List<int>().Sum());
        }

        #endregion


        #region Average

        [Test]
        public void Average_Float_ReturnsMean()
        {
            var values = new List<float> { 2f, 4f, 6f };
            Assert.AreEqual(4f, values.Average(), Tolerance);
        }

        [Test]
        public void Average_Int_UsesFloatDivision()
        {
            // The int overload returns a float, so an odd sum keeps its fractional part.
            var values = new List<int> { 1, 2 };
            Assert.AreEqual(1.5f, values.Average(), Tolerance);
        }

        [Test]
        public void Average_Float_Empty_IsZero()
        {
            // Safe variant: an empty sequence averages to 0 instead of NaN (or throwing, like LINQ).
            Assert.AreEqual(0f, new List<float>().Average(), Tolerance);
        }

        [Test]
        public void Average_Int_Empty_IsZero()
        {
            Assert.AreEqual(0f, new List<int>().Average(), Tolerance);
        }

        #endregion


        #region Null guards

        [Test]
        public void Sum_Float_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => ((IEnumerable<float>)null).Sum());
        }

        [Test]
        public void Sum_Int_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => ((IEnumerable<int>)null).Sum());
        }

        [Test]
        public void Average_Float_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => ((IEnumerable<float>)null).Average());
        }

        [Test]
        public void Average_Int_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => ((IEnumerable<int>)null).Average());
        }

        #endregion

    }

}
