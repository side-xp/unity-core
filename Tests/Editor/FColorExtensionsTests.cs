using NUnit.Framework;

using UnityEngine;

namespace SideXP.Core.Tests
{

    public class FColorExtensionsTests
    {

        private const float Tolerance = 1e-3f;

        private static void AssertColorApprox(Color expected, Color actual)
        {
            Assert.AreEqual(expected.r, actual.r, Tolerance, "r channel");
            Assert.AreEqual(expected.g, actual.g, Tolerance, "g channel");
            Assert.AreEqual(expected.b, actual.b, Tolerance, "b channel");
            Assert.AreEqual(expected.a, actual.a, Tolerance, "a channel");
        }

        // Most tests use explicit primitive flags (Red/Maroon/Green/Lime/Blue/Navy + Alpha*) to pin the
        // resolution logic; the "Named tints" region covers the composite aliases and the opaque-by-default
        // behavior (a color with no Alpha* flag resolves to full alpha; only Clear and explicit Alpha0 are
        // transparent).

        #region ToColor

        [Test]
        public void ToColor_None_ReturnsClear()
        {
            AssertColorApprox(Color.clear, FColor.Clear.ToColor());
        }

        [Test]
        public void ToColor_FullComponents_MapToOne()
        {
            Color result = (FColor.Red | FColor.Green | FColor.Blue | FColor.Alpha100).ToColor();
            AssertColorApprox(new Color(1f, 1f, 1f, 1f), result);
        }

        [Test]
        public void ToColor_HalfComponents_MapToHalf()
        {
            Color result = (FColor.Maroon | FColor.Lime | FColor.Navy | FColor.Alpha100).ToColor();
            AssertColorApprox(new Color(0.5f, 0.5f, 0.5f, 1f), result);
        }

        [Test]
        public void ToColor_FullComponentTakesPrecedenceOverHalf()
        {
            // Per channel the full flag is checked before the half flag (if/else if), so Red wins over Maroon.
            Color result = (FColor.Red | FColor.Maroon | FColor.Alpha100).ToColor();
            Assert.AreEqual(1f, result.r, Tolerance);
        }

        [Test]
        public void ToColor_AlphaFlag_MapsToFraction()
        {
            Assert.AreEqual(0.5f, (FColor.Red | FColor.Alpha50).ToColor().a, Tolerance);
            Assert.AreEqual(0.25f, (FColor.Red | FColor.Alpha25).ToColor().a, Tolerance);
            Assert.AreEqual(0f, (FColor.Red | FColor.Alpha0).ToColor().a, Tolerance);
        }

        [Test]
        public void ToColor_IgnoreAlpha_ForcesOpaque()
        {
            // Even with an explicit Alpha50 flag, ignoreAlpha overrides to full alpha.
            Color result = (FColor.Red | FColor.Alpha50).ToColor(ignoreAlpha: true);
            Assert.AreEqual(1f, result.a, Tolerance);
        }

        [Test]
        public void ToColor_FloatAlpha_OverridesEnumAlpha()
        {
            // The float overload ignores the enum alpha and applies the given 0..1 value.
            Color result = (FColor.Red | FColor.Alpha50).ToColor(0.25f);
            AssertColorApprox(new Color(1f, 0f, 0f, 0.25f), result);
        }

        #endregion


        #region ToColor32

        [Test]
        public void ToColor32_None_ReturnsTransparentBlack()
        {
            Assert.AreEqual(new Color32(0, 0, 0, 0), FColor.Clear.ToColor32());
        }

        [Test]
        public void ToColor32_FullComponents_MapTo255()
        {
            Color32 result = (FColor.Red | FColor.Green | FColor.Blue | FColor.Alpha100).ToColor32();
            Assert.AreEqual(new Color32(255, 255, 255, 255), result);
        }

        [Test]
        public void ToColor32_HalfComponents_MapTo127()
        {
            Color32 result = (FColor.Maroon | FColor.Lime | FColor.Navy | FColor.Alpha100).ToColor32();
            Assert.AreEqual(new Color32(127, 127, 127, 255), result);
        }

        [Test]
        public void ToColor32_AlphaFlags_MapToByteFractions()
        {
            Assert.AreEqual((byte)127, (FColor.Red | FColor.Alpha50).ToColor32().a);
            Assert.AreEqual((byte)63, (FColor.Red | FColor.Alpha25).ToColor32().a);
            Assert.AreEqual((byte)31, (FColor.Red | FColor.Alpha12).ToColor32().a);
        }

        [Test]
        public void ToColor32_ByteAlpha_OverridesEnumAlpha()
        {
            Color32 result = (FColor.Red | FColor.Alpha50).ToColor32((byte)200);
            Assert.AreEqual(new Color32(255, 0, 0, 200), result);
        }

        // Regression: the float overload used to compute `255 / alpha`, which was inverted and overflowed
        // the byte cast (e.g. alpha 0.5 -> 254). It now applies `Clamp01(alpha) * 255`.
        [Test]
        public void ToColor32_FloatAlpha_ConvertsNormalizedToByte()
        {
            Assert.AreEqual((byte)127, FColor.Red.ToColor32(0.5f).a);
        }

        [Test]
        public void ToColor32_FloatAlpha_ClampsOutOfRange()
        {
            Assert.AreEqual((byte)255, FColor.Red.ToColor32(2f).a);
            Assert.AreEqual((byte)0, FColor.Red.ToColor32(-1f).a);
        }

        #endregion


        #region Named tints & opaque default

        // A color with RGB flags but no Alpha* flag defaults to opaque (full alpha).
        [Test]
        public void ToColor_NoAlphaFlag_DefaultsToOpaque()
        {
            Assert.AreEqual(1f, (FColor.Red | FColor.Green | FColor.Blue).ToColor().a, Tolerance);
        }

        [Test]
        public void ToColor32_NoAlphaFlag_DefaultsToOpaque()
        {
            Assert.AreEqual((byte)255, (FColor.Red | FColor.Green | FColor.Blue).ToColor32().a);
        }

        // Clear is the flagless value (0) and short-circuits to a fully transparent color.
        [Test]
        public void ToColor_Clear_IsTransparent()
        {
            Assert.AreEqual(0f, FColor.Clear.ToColor().a, Tolerance);
        }

        // Explicit Alpha0 keeps a flagged color transparent despite the opaque default.
        [Test]
        public void ToColor_ExplicitAlpha0_StaysTransparent()
        {
            Assert.AreEqual(0f, (FColor.Red | FColor.Alpha0).ToColor().a, Tolerance);
        }

        [Test]
        public void ToColor_White_IsOpaqueWhite()
        {
            AssertColorApprox(new Color(1f, 1f, 1f, 1f), FColor.White.ToColor());
        }

        // Grey was fixed from Maroon|Green|Navy (a light green) to Maroon|Lime|Navy, a neutral grey.
        [Test]
        public void ToColor_Grey_IsNeutralOpaqueGrey()
        {
            AssertColorApprox(new Color(0.5f, 0.5f, 0.5f, 1f), FColor.Grey.ToColor());
        }

        [Test]
        public void ToColor_Black_IsOpaqueBlack()
        {
            AssertColorApprox(new Color(0f, 0f, 0f, 1f), FColor.Black.ToColor());
        }

        #endregion

    }

}
