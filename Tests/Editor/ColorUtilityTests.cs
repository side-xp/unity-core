using NUnit.Framework;

using UnityEngine;

namespace SideXP.Core.Tests
{

    public class ColorUtilityTests
    {

        private const float Tolerance = 1e-3f;

        private static void AssertColorApprox(Color expected, Color actual)
        {
            Assert.AreEqual(expected.r, actual.r, Tolerance, "r channel");
            Assert.AreEqual(expected.g, actual.g, Tolerance, "g channel");
            Assert.AreEqual(expected.b, actual.b, Tolerance, "b channel");
            Assert.AreEqual(expected.a, actual.a, Tolerance, "a channel");
        }

        private static void AssertColor32Equal(Color32 expected, Color32 actual)
        {
            Assert.AreEqual(expected.r, actual.r, "r channel");
            Assert.AreEqual(expected.g, actual.g, "g channel");
            Assert.AreEqual(expected.b, actual.b, "b channel");
            Assert.AreEqual(expected.a, actual.a, "a channel");
        }

        #region ToHex

        [Test]
        public void ToHexRGB_Color_FormatsRRGGBB()
        {
            Assert.AreEqual("FF0000", ColorUtility.ToHexRGB(new Color(1f, 0f, 0f)));
        }

        [Test]
        public void ToHexRGBA_Color_FormatsRRGGBBAA()
        {
            Assert.AreEqual("FF0000FF", ColorUtility.ToHexRGBA(new Color(1f, 0f, 0f, 1f)));
        }

        [Test]
        public void ToHexRGB_Color32_FormatsRRGGBB()
        {
            // Regression guard: this overload used to recurse into itself (StackOverflow).
            Assert.AreEqual("123456", ColorUtility.ToHexRGB(new Color32(0x12, 0x34, 0x56, 0x78)));
        }

        [Test]
        public void ToHexRGBA_Color32_FormatsRRGGBBAA()
        {
            // Regression guard: this overload used to recurse into itself (StackOverflow).
            Assert.AreEqual("12345678", ColorUtility.ToHexRGBA(new Color32(0x12, 0x34, 0x56, 0x78)));
        }

        #endregion


        #region FromHex (Color)

        [Test]
        public void FromHex_Ref_ValidRRGGBB_ReturnsTrueAndParses()
        {
            Color color = Color.clear;
            bool ok = ColorUtility.FromHex(ref color, "#FF0000");
            Assert.IsTrue(ok);
            AssertColorApprox(new Color(1f, 0f, 0f, 1f), color);
        }

        [Test]
        public void FromHex_Ref_ShortForm_Parses()
        {
            Color color = Color.clear;
            bool ok = ColorUtility.FromHex(ref color, "#F00");
            Assert.IsTrue(ok);
            AssertColorApprox(new Color(1f, 0f, 0f, 1f), color);
        }

        [Test]
        public void FromHex_Ref_Invalid_ReturnsFalseAndLeavesColorUntouched()
        {
            Color original = new Color(0.1f, 0.2f, 0.3f, 0.4f);
            Color color = original;
            bool ok = ColorUtility.FromHex(ref color, "not-a-color");
            Assert.IsFalse(ok);
            AssertColorApprox(original, color); // not clobbered on failure (matches FromHex32)
        }

        [Test]
        public void FromHex_FromString_ReturnsParsedColor()
        {
            AssertColorApprox(new Color(0f, 1f, 0f, 1f), ColorUtility.FromHex("#00FF00"));
        }

        #endregion


        #region FromHex32 (Color32)

        [Test]
        public void FromHex32_Ref_ValidRRGGBB_ParsesOpaque()
        {
            Color32 color = new Color32(0, 0, 0, 0);
            bool ok = ColorUtility.FromHex32(ref color, "#FF0000");
            Assert.IsTrue(ok);
            AssertColor32Equal(new Color32(255, 0, 0, 255), color);
        }

        [Test]
        public void FromHex32_Ref_ValidRRGGBBAA_ParsesAlpha()
        {
            Color32 color = new Color32(0, 0, 0, 0);
            bool ok = ColorUtility.FromHex32(ref color, "#12345678");
            Assert.IsTrue(ok);
            AssertColor32Equal(new Color32(0x12, 0x34, 0x56, 0x78), color);
        }

        [Test]
        public void FromHex32_Ref_ShortForm_NowSupported()
        {
            // After delegating to Unity's parser, the short forms documented on the method actually work.
            Color32 color = new Color32(0, 0, 0, 0);
            bool ok = ColorUtility.FromHex32(ref color, "#F00");
            Assert.IsTrue(ok);
            AssertColor32Equal(new Color32(255, 0, 0, 255), color);
        }

        [Test]
        public void FromHex32_Ref_Invalid_ReturnsFalseAndLeavesColorUntouched()
        {
            Color32 original = new Color32(10, 20, 30, 40);
            Color32 color = original;
            bool ok = ColorUtility.FromHex32(ref color, "not-a-color");
            Assert.IsFalse(ok);
            AssertColor32Equal(original, color); // not clobbered on failure
        }

        [Test]
        public void FromHex32_Ref_BareHexWithoutHash_NowReturnsFalse()
        {
            // Behavior change: the old custom parser stripped '#' and accepted bare hex.
            // Unity's parser requires the '#' prefix (consistent with FromHex), so bare hex now fails.
            Color32 color = new Color32(0, 0, 0, 0);
            Assert.IsFalse(ColorUtility.FromHex32(ref color, "FF0000"));
        }

        [Test]
        public void FromHex32_FromString_ReturnsParsedColor()
        {
            AssertColor32Equal(new Color32(0, 255, 0, 255), ColorUtility.FromHex32("#00FF00"));
        }

        #endregion


        #region GetOverlayTint

        [Test]
        public void GetOverlayTint_LightColor_ReturnsBlack()
        {
            Assert.AreEqual(Color.black, ColorUtility.GetOverlayTint(Color.white));
        }

        [Test]
        public void GetOverlayTint_DarkColor_ReturnsWhite()
        {
            Assert.AreEqual(Color.white, ColorUtility.GetOverlayTint(Color.black));
        }

        [Test]
        public void GetOverlayTint_Color32_LightColor_ReturnsBlack()
        {
            // Regression guard: this overload used to recurse into itself (StackOverflow).
            AssertColor32Equal(new Color32(0, 0, 0, 255), ColorUtility.GetOverlayTint(new Color32(255, 255, 255, 255)));
        }

        [Test]
        public void GetOverlayTint_Color32_DarkColor_ReturnsWhite()
        {
            AssertColor32Equal(new Color32(255, 255, 255, 255), ColorUtility.GetOverlayTint(new Color32(0, 0, 0, 255)));
        }

        #endregion


        #region GenerateRandomColor

        [Test]
        public void GenerateRandomColor_ChannelsAreQuantizedAndInRange()
        {
            Random.InitState(12345);
            const int subdivs = 4;

            for (int i = 0; i < 50; i++)
            {
                Color color = ColorUtility.GenerateRandomColor(subdivs);

                foreach (float channel in new[] { color.r, color.g, color.b })
                {
                    Assert.GreaterOrEqual(channel, 0f);
                    Assert.LessOrEqual(channel, 1f);
                    // Each channel is a multiple of 1/subdivs.
                    float steps = channel * subdivs;
                    Assert.AreEqual(Mathf.Round(steps), steps, Tolerance, "channel should land on a subdivision");
                }

                Assert.AreEqual(1f, color.a, Tolerance, "alpha stays opaque");
            }
        }

        [Test]
        public void GenerateRandomColor_SingleSubdiv_ChannelsAreZeroOrOne()
        {
            Random.InitState(999);

            for (int i = 0; i < 50; i++)
            {
                Color color = ColorUtility.GenerateRandomColor(1);
                foreach (float channel in new[] { color.r, color.g, color.b })
                    Assert.IsTrue(channel == 0f || channel == 1f, $"unexpected channel value {channel}");
            }
        }

        [Test]
        public void GenerateRandomColor_SubdivsBelowOne_ClampedNoDivideByZero([Values(0, -3)] int subdivs)
        {
            Random.InitState(2024);

            for (int i = 0; i < 50; i++)
            {
                Color color = ColorUtility.GenerateRandomColor(subdivs);
                foreach (float channel in new[] { color.r, color.g, color.b })
                {
                    Assert.IsFalse(float.IsNaN(channel), "channel must not be NaN");
                    Assert.IsFalse(float.IsInfinity(channel), "channel must be finite");
                    // Clamped to a single subdiv, so channels are 0 or 1.
                    Assert.IsTrue(channel == 0f || channel == 1f, $"unexpected channel value {channel}");
                }
            }
        }

        #endregion

    }

}
