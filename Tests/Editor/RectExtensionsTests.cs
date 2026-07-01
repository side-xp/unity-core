using NUnit.Framework;

using UnityEngine;

namespace SideXP.Core.Tests
{

    public class RectExtensionsTests
    {

        private static readonly Rect Area = new Rect(0f, 0f, 100f, 100f);

        #region HoldInArea

        [Test]
        public void HoldInArea_RectAlreadyInside_IsUnchanged()
        {
            Rect rect = new Rect(10f, 20f, 30f, 40f);
            rect.HoldInArea(Area);
            Assert.AreEqual(new Rect(10f, 20f, 30f, 40f), rect);
        }

        [Test]
        public void HoldInArea_OverflowsRight_MovesLeftToFit()
        {
            Rect rect = new Rect(90f, 10f, 30f, 10f);
            rect.HoldInArea(Area);
            // x + width must not exceed area right (100): x becomes 70.
            Assert.AreEqual(70f, rect.x);
            Assert.AreEqual(10f, rect.y);
        }

        [Test]
        public void HoldInArea_OverflowsBottom_MovesUpToFit()
        {
            Rect rect = new Rect(10f, 90f, 10f, 30f);
            rect.HoldInArea(Area);
            // y + height must not exceed area bottom (100): y becomes 70.
            Assert.AreEqual(70f, rect.y);
            Assert.AreEqual(10f, rect.x);
        }

        [Test]
        public void HoldInArea_PastTopLeft_ClampsToAreaOrigin()
        {
            Rect rect = new Rect(-20f, -30f, 10f, 10f);
            rect.HoldInArea(Area);
            Assert.AreEqual(0f, rect.x);
            Assert.AreEqual(0f, rect.y);
        }

        [Test]
        public void HoldInArea_LargerThanArea_AnchorsTopLeft()
        {
            // When the rect can't fit, the top-left clamp wins (applied after the overflow push).
            Rect rect = new Rect(50f, 50f, 200f, 200f);
            rect.HoldInArea(Area);
            Assert.AreEqual(0f, rect.x);
            Assert.AreEqual(0f, rect.y);
        }

        [Test]
        public void HoldInArea_RespectsNonZeroAreaOrigin()
        {
            Rect area = new Rect(10f, 10f, 50f, 50f);
            Rect rect = new Rect(0f, 0f, 20f, 20f);
            rect.HoldInArea(area);
            // Clamped to the area's origin, not to (0,0).
            Assert.AreEqual(10f, rect.x);
            Assert.AreEqual(10f, rect.y);
        }

        #endregion


        #region HoldInScreenSpace

        [Test]
        public void HoldInScreenSpace_ClampsOriginToNonNegative()
        {
            // The screen area starts at (0,0), so regardless of resolution the origin can't stay negative.
            Rect rect = new Rect(-50f, -50f, 20f, 20f);
            rect.HoldInScreenSpace();
            Assert.GreaterOrEqual(rect.x, 0f);
            Assert.GreaterOrEqual(rect.y, 0f);
        }

        #endregion

    }

}
