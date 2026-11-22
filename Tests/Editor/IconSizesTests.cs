using NUnit.Framework;

using SideXP.Core.EditorOnly;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="IconSizes"/>.
    /// </summary>
    public class IconSizesTests
    {

        /// <summary>
        /// Each icon size has its size on screen.
        /// </summary>
        [TestCase(EIconSize.Small, 16f)]
        [TestCase(EIconSize.Medium, 24f)]
        [TestCase(EIconSize.Large, 32f)]
        public void GetPointSize_MatchesIconSize(EIconSize size, float expected)
        {
            Assert.AreEqual(expected, IconSizes.GetPointSize(size));
        }

        /// <summary>
        /// The stored file matches the point size multiplied by the display scale, or is the smallest larger one.
        /// </summary>
        [TestCase(EIconSize.Small, 1f, 16)]
        [TestCase(EIconSize.Small, 1.25f, 24)]
        [TestCase(EIconSize.Small, 1.5f, 24)]
        [TestCase(EIconSize.Small, 2f, 32)]
        [TestCase(EIconSize.Medium, 1f, 24)]
        [TestCase(EIconSize.Medium, 1.25f, 32)]
        [TestCase(EIconSize.Medium, 1.5f, 48)]
        [TestCase(EIconSize.Medium, 2f, 48)]
        [TestCase(EIconSize.Large, 1f, 32)]
        [TestCase(EIconSize.Large, 1.5f, 48)]
        [TestCase(EIconSize.Large, 2f, 64)]
        public void GetPixelSize_UsesSmallestLargeEnoughFile(EIconSize size, float pixelsPerPoint, int expected)
        {
            Assert.AreEqual(expected, IconSizes.GetPixelSize(size, pixelsPerPoint));
        }

        /// <summary>
        /// Without stored file large enough, the largest one is used.
        /// </summary>
        [Test]
        public void GetPixelSize_ScaleTooLarge_UsesLargestFile()
        {
            Assert.AreEqual(64, IconSizes.GetPixelSize(EIconSize.Large, 3f));
        }

        /// <summary>
        /// Float imprecision on the display scale doesn't select the next stored size.
        /// </summary>
        [Test]
        public void GetPixelSize_ImpreciseScale_KeepsMatchingFile()
        {
            Assert.AreEqual(32, IconSizes.GetPixelSize(EIconSize.Small, 2.0001f));
        }

        /// <summary>
        /// An invalid display scale is considered as 100%.
        /// </summary>
        [TestCase(0f)]
        [TestCase(-1f)]
        public void GetPixelSize_InvalidScale_UsesDefaultScale(float pixelsPerPoint)
        {
            Assert.AreEqual(16, IconSizes.GetPixelSize(EIconSize.Small, pixelsPerPoint));
        }

        /// <summary>
        /// Automatic icons are white on the dark theme and black on the light one, and explicit colors are kept.
        /// </summary>
        [TestCase(EIconColor.Auto, true, false)]
        [TestCase(EIconColor.Auto, false, true)]
        [TestCase(EIconColor.White, false, false)]
        [TestCase(EIconColor.Black, true, true)]
        public void IsBlack_ResolvesColor(EIconColor color, bool isDarkTheme, bool expected)
        {
            Assert.AreEqual(expected, IconSizes.IsBlack(color, isDarkTheme));
        }

    }

}
