using NUnit.Framework;

using UnityEngine;

namespace SideXP.Core.Tests
{

    public class ColorExtensionsTests
    {

        private const float Tolerance = 1e-3f;

        private static void AssertColorApprox(Color expected, Color actual)
        {
            Assert.AreEqual(expected.r, actual.r, Tolerance, "r channel");
            Assert.AreEqual(expected.g, actual.g, Tolerance, "g channel");
            Assert.AreEqual(expected.b, actual.b, Tolerance, "b channel");
            Assert.AreEqual(expected.a, actual.a, Tolerance, "a channel");
        }

        #region Set (Color)

        [Test]
        public void Set_Color_AllChannels_AssignsEachChannel()
        {
            Color color = Color.black;
            color.Set(0.1f, 0.2f, 0.3f, 0.4f);
            AssertColorApprox(new Color(0.1f, 0.2f, 0.3f, 0.4f), color);
        }

        [Test]
        public void Set_Color_ValueAndAlpha_AssignsRgbToValue()
        {
            Color color = Color.black;
            color.Set(0.5f, 0.25f);
            AssertColorApprox(new Color(0.5f, 0.5f, 0.5f, 0.25f), color);
        }

        [Test]
        public void Set_Color_SingleValue_AssignsAllChannelsIncludingAlpha()
        {
            Color color = Color.black;
            color.Set(0.5f);
            AssertColorApprox(new Color(0.5f, 0.5f, 0.5f, 0.5f), color);
        }

        #endregion


        #region Set (Color32)

        [Test]
        public void Set_Color32_AllChannels_AssignsEachChannel()
        {
            Color32 color = new Color32(0, 0, 0, 0);
            color.Set((byte)10, (byte)20, (byte)30, (byte)40);
            Assert.AreEqual(new Color32(10, 20, 30, 40), color);
        }

        [Test]
        public void Set_Color32_ValueAndAlpha_AssignsRgbToValue()
        {
            Color32 color = new Color32(0, 0, 0, 0);
            color.Set((byte)128, (byte)64);
            Assert.AreEqual(new Color32(128, 128, 128, 64), color);
        }

        [Test]
        public void Set_Color32_SingleValue_AssignsAllChannelsIncludingAlpha()
        {
            Color32 color = new Color32(0, 0, 0, 0);
            color.Set((byte)200);
            Assert.AreEqual(new Color32(200, 200, 200, 200), color);
        }

        #endregion


        #region ToHex (forwarding)

        [Test]
        public void ToHexRGB_Color_ForwardsToUtility()
        {
            Assert.AreEqual("FF0000", new Color(1f, 0f, 0f).ToHexRGB());
        }

        [Test]
        public void ToHexRGBA_Color_ForwardsToUtility()
        {
            Assert.AreEqual("FF0000FF", new Color(1f, 0f, 0f, 1f).ToHexRGBA());
        }

        [Test]
        public void ToHexRGB_Color32_ForwardsToUtility()
        {
            Assert.AreEqual("123456", new Color32(0x12, 0x34, 0x56, 0x78).ToHexRGB());
        }

        [Test]
        public void ToHexRGBA_Color32_ForwardsToUtility()
        {
            Assert.AreEqual("12345678", new Color32(0x12, 0x34, 0x56, 0x78).ToHexRGBA());
        }

        #endregion


        #region FromHex (regression: must mutate the caller)

        // The extension used to take the color by value, so the parsed result was discarded and only the
        // bool was useful. It's now `this ref`, so a successful parse writes back into the caller's color.
        [Test]
        public void FromHex_ValidString_ReturnsTrueAndWritesBackColor()
        {
            Color color = Color.black;
            bool ok = color.FromHex("#00FF00");
            Assert.IsTrue(ok);
            AssertColorApprox(new Color(0f, 1f, 0f, 1f), color);
        }

        [Test]
        public void FromHex_InvalidString_ReturnsFalseAndLeavesColorUntouched()
        {
            Color color = Color.black;
            bool ok = color.FromHex("not-a-color");
            Assert.IsFalse(ok);
            AssertColorApprox(Color.black, color);
        }

        [Test]
        public void FromHex32_ValidString_ReturnsTrueAndWritesBackColor()
        {
            Color32 color = new Color32(0, 0, 0, 255);
            bool ok = color.FromHex32("#00FF00");
            Assert.IsTrue(ok);
            Assert.AreEqual(new Color32(0, 255, 0, 255), color);
        }

        [Test]
        public void FromHex32_InvalidString_ReturnsFalseAndLeavesColorUntouched()
        {
            Color32 color = new Color32(1, 2, 3, 4);
            bool ok = color.FromHex32("not-a-color");
            Assert.IsFalse(ok);
            Assert.AreEqual(new Color32(1, 2, 3, 4), color);
        }

        #endregion


        #region GetOverlayTint (forwarding)

        [Test]
        public void GetOverlayTint_Color_BrightColor_ReturnsBlack()
        {
            Assert.AreEqual(Color.black, Color.white.GetOverlayTint());
        }

        [Test]
        public void GetOverlayTint_Color_DarkColor_ReturnsWhite()
        {
            Assert.AreEqual(Color.white, Color.black.GetOverlayTint());
        }

        [Test]
        public void GetOverlayTint_Color32_ForwardsToUtility()
        {
            Color32 result = new Color32(255, 255, 255, 255).GetOverlayTint();
            Assert.AreEqual(new Color32(0, 0, 0, 255), result);
        }

        #endregion

    }

}
