using NUnit.Framework;

using UnityEngine;

namespace SideXP.Core.Tests
{

    public class LayerMaskExtensionsTests
    {

        #region GetLayerIndices (int)

        [Test]
        public void GetLayerIndices_Zero_ReturnsEmpty()
        {
            Assert.AreEqual(new int[0], LayerMaskExtensions.GetLayerIndices(0));
        }

        [Test]
        public void GetLayerIndices_SingleBit_ReturnsThatIndex()
        {
            Assert.AreEqual(new[] { 5 }, LayerMaskExtensions.GetLayerIndices(1 << 5));
        }

        [Test]
        public void GetLayerIndices_MultipleBits_ReturnsAscendingIndices()
        {
            int mask = (1 << 0) | (1 << 3) | (1 << 8);
            Assert.AreEqual(new[] { 0, 3, 8 }, LayerMaskExtensions.GetLayerIndices(mask));
        }

        [Test]
        public void GetLayerIndices_HighestBit_IsIncluded()
        {
            // Bit 31 is the sign bit (1 << 31 is negative); it must still be detected.
            Assert.AreEqual(new[] { 31 }, LayerMaskExtensions.GetLayerIndices(1 << 31));
        }

        [Test]
        public void GetLayerIndices_AllBits_ReturnsZeroToThirtyOne()
        {
            int[] result = LayerMaskExtensions.GetLayerIndices(~0);
            Assert.AreEqual(32, result.Length);
            for (int i = 0; i < 32; i++)
                Assert.AreEqual(i, result[i]);
        }

        #endregion


        #region GetLayerIndices (LayerMask)

        [Test]
        public void GetLayerIndices_LayerMask_UsesMaskValue()
        {
            LayerMask mask = (1 << 2) | (1 << 4); // implicit int -> LayerMask
            Assert.AreEqual(new[] { 2, 4 }, mask.GetLayerIndices());
        }

        #endregion

    }

}
