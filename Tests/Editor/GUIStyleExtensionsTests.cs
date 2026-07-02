using NUnit.Framework;

using UnityEngine;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="GUIStyleExtensions"/> — a fluent builder where every method returns a copy of the style with a single
    /// property changed, leaving the original untouched.
    /// </summary>
    public class GUIStyleExtensionsTests
    {

        private static void AssertRectOffset(int left, int right, int top, int bottom, RectOffset actual)
        {
            Assert.AreEqual(left, actual.left, "left");
            Assert.AreEqual(right, actual.right, "right");
            Assert.AreEqual(top, actual.top, "top");
            Assert.AreEqual(bottom, actual.bottom, "bottom");
        }

        #region Copy semantics

        [Test]
        public void Margin_ReturnsCopy_LeavesOriginalUntouched()
        {
            var style = new GUIStyle();

            GUIStyle result = style.Margin(1, 2, 3, 4);

            Assert.AreNotSame(style, result);
            AssertRectOffset(1, 2, 3, 4, result.margin);
            AssertRectOffset(0, 0, 0, 0, style.margin); // original unchanged
        }

        [Test]
        public void FontColor_DoesNotMutateOriginalState()
        {
            var style = new GUIStyle();
            style.normal.textColor = Color.white;

            GUIStyle result = style.FontColor(Color.red);

            // Verifies the copy constructor deep-copies GUIStyleState (otherwise the original would change too).
            Assert.AreEqual(Color.red, result.normal.textColor);
            Assert.AreEqual(Color.white, style.normal.textColor);
        }

        #endregion


        #region Margin / Padding

        [Test]
        public void Margin_HorizontalVertical_MirrorsValues()
        {
            AssertRectOffset(5, 5, 6, 6, new GUIStyle().Margin(5, 6).margin);
        }

        [Test]
        public void Margin_RectOffset_SetsIt()
        {
            AssertRectOffset(1, 2, 3, 4, new GUIStyle().Margin(new RectOffset(1, 2, 3, 4)).margin);
        }

        [Test]
        public void Padding_FourArgs_SetsPadding()
        {
            AssertRectOffset(1, 2, 3, 4, new GUIStyle().Padding(1, 2, 3, 4).padding);
        }

        [Test]
        public void Padding_HorizontalVertical_MirrorsValues()
        {
            AssertRectOffset(7, 7, 8, 8, new GUIStyle().Padding(7, 8).padding);
        }

        [Test]
        public void Padding_RectOffset_SetsIt()
        {
            AssertRectOffset(4, 3, 2, 1, new GUIStyle().Padding(new RectOffset(4, 3, 2, 1)).padding);
        }

        #endregion


        #region Boolean toggles

        [Test]
        public void WordWrap_SetsFlag()
        {
            Assert.IsTrue(new GUIStyle().WordWrap(true).wordWrap);
            Assert.IsFalse(new GUIStyle { wordWrap = true }.WordWrap(false).wordWrap);
        }

        [Test]
        public void RichText_SetsFlag()
        {
            Assert.IsTrue(new GUIStyle().RichText(true).richText);
        }

        [Test]
        public void StretchWidth_And_StretchHeight_SetFlags()
        {
            Assert.IsTrue(new GUIStyle { stretchWidth = false }.StretchWidth(true).stretchWidth);
            Assert.IsFalse(new GUIStyle { stretchHeight = true }.StretchHeight(false).stretchHeight);
        }

        [Test]
        public void Stretch_SetsBothFlags()
        {
            GUIStyle result = new GUIStyle().Stretch(true, false);
            Assert.IsTrue(result.stretchWidth);
            Assert.IsFalse(result.stretchHeight);
        }

        #endregion


        #region Font size

        [Test]
        public void FontSize_WithinRange_SetsValue()
        {
            Assert.AreEqual(20, new GUIStyle().FontSize(20).fontSize);
        }

        [Test]
        public void FontSize_OutOfRange_IsClamped()
        {
            Assert.AreEqual(GUIStyleExtensions.MaxFontSize, new GUIStyle().FontSize(1000).fontSize);
            Assert.AreEqual(GUIStyleExtensions.MinFontSize, new GUIStyle().FontSize(-5).fontSize);
        }

        [Test]
        public void FontSizeDiff_AddsToOriginalSize()
        {
            var style = new GUIStyle { fontSize = 20 };
            Assert.AreEqual(25, style.FontSizeDiff(5).fontSize);
            Assert.AreEqual(15, style.FontSizeDiff(-5).fontSize);
        }

        [Test]
        public void FontSizeDiff_ClampsResult()
        {
            var style = new GUIStyle { fontSize = 250 };
            Assert.AreEqual(GUIStyleExtensions.MaxFontSize, style.FontSizeDiff(100).fontSize);
        }

        #endregion


        #region Font style / color / alignment

        [Test]
        public void FontColor_SetsNormalTextColor()
        {
            Assert.AreEqual(Color.green, new GUIStyle().FontColor(Color.green).normal.textColor);
        }

        [Test]
        public void FontStyle_SetsStyle()
        {
            Assert.AreEqual(UnityEngine.FontStyle.Italic, new GUIStyle().FontStyle(UnityEngine.FontStyle.Italic).fontStyle);
        }

        [Test]
        public void TextAlignment_SetsAlignment()
        {
            Assert.AreEqual(TextAnchor.UpperRight, new GUIStyle().TextAlignment(TextAnchor.UpperRight).alignment);
        }

        [Test]
        public void AlignShortcuts_SetMiddleAnchors()
        {
            Assert.AreEqual(TextAnchor.MiddleLeft, new GUIStyle().AlignLeft().alignment);
            Assert.AreEqual(TextAnchor.MiddleCenter, new GUIStyle().AlignCenter().alignment);
            Assert.AreEqual(TextAnchor.MiddleRight, new GUIStyle().AlignRight().alignment);
        }

        [Test]
        public void BoldItalicShortcuts_SetFontStyles()
        {
            Assert.AreEqual(UnityEngine.FontStyle.Bold, new GUIStyle().Bold().fontStyle);
            Assert.AreEqual(UnityEngine.FontStyle.Italic, new GUIStyle().Italic().fontStyle);
            Assert.AreEqual(UnityEngine.FontStyle.BoldAndItalic, new GUIStyle().BoldItalic().fontStyle);
        }

        #endregion

    }

}
