using System.Collections.Generic;

using NUnit.Framework;

using UnityEngine;

using SideXP.Core.EditorOnly;

using Object = UnityEngine.Object;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="IconProcessor"/>.
    /// </summary>
    /// <remarks>
    /// The processing uses the GPU, so these tests work in the editor, but not in batch mode with <c>-nographics</c>.
    /// </remarks>
    public class IconProcessorTests
    {

        /// <summary>
        /// Size of the Tabler PNG files, in pixels.
        /// </summary>
        private const int SourceSize = 240;

        /// <summary>
        /// Radius of the circle drawn in the generated icon, in pixels.
        /// </summary>
        private const float CircleRadius = 80f;

        /// <summary>
        /// Maximum difference between the average alpha of a processed icon and the source one. Large enough for the anti-aliasing
        /// at the circle's edge, small enough to catch a filter that skips source pixels.
        /// </summary>
        private const float AlphaTolerance = 0.03f;

        /// <summary>
        /// Makes a PNG like a Tabler icon: a black filled circle on a transparent background.
        /// </summary>
        /// <param name="width">The width of the image, in pixels.</param>
        /// <param name="height">The height of the image, in pixels.</param>
        /// <returns>Returns the content of the PNG file.</returns>
        private static byte[] MakeIconPng(int width = SourceSize, int height = SourceSize)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false, true);
            try
            {
                var center = new Vector2(width / 2f, height / 2f);
                var pixels = new Color32[width * height];
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        bool isInCircle = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center) <= CircleRadius;
                        pixels[(y * width) + x] = new Color32(0, 0, 0, isInCircle ? byte.MaxValue : byte.MinValue);
                    }
                }

                texture.SetPixels32(pixels);
                return texture.EncodeToPNG();
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }
        }

        /// <summary>
        /// Reads the pixels of a PNG.
        /// </summary>
        /// <param name="pngData">The content of the PNG file.</param>
        /// <param name="size">Outputs the size of the image, in pixels.</param>
        /// <returns>Returns the pixels of the image, from the bottom-left corner.</returns>
        private static Color32[] ReadPixels(byte[] pngData, out Vector2Int size)
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
            try
            {
                Assert.IsTrue(texture.LoadImage(pngData), "The processed file is not a valid PNG.");
                size = new Vector2Int(texture.width, texture.height);
                return texture.GetPixels32();
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }
        }

        /// <summary>
        /// Processes a PNG into every stored size, and checks that the processing succeeded.
        /// </summary>
        /// <param name="pngData">The content of the PNG file to process.</param>
        /// <returns>Returns the content of the PNG file made for each size.</returns>
        private static Dictionary<int, byte[]> Process(byte[] pngData)
        {
            int[] sizes = IconStorage.StoredSizes;
            bool isProcessed = IconProcessor.TryProcess(pngData, sizes, out Dictionary<int, byte[]> pngBySize, out string error);
            Assert.IsTrue(isProcessed, error);
            Assert.IsNull(error);
            return pngBySize;
        }

        /// <summary>
        /// Checks if a pixel is white, whatever its alpha.
        /// </summary>
        /// <param name="pixel">The pixel to check.</param>
        /// <returns>Returns true if the pixel is white.</returns>
        private static bool IsWhite(Color32 pixel)
        {
            return pixel.r == byte.MaxValue && pixel.g == byte.MaxValue && pixel.b == byte.MaxValue;
        }

        /// <summary>
        /// Computes the average alpha of pixels.
        /// </summary>
        /// <param name="pixels">The pixels.</param>
        /// <returns>Returns the average alpha, between 0 and 1.</returns>
        private static float GetAverageAlpha(Color32[] pixels)
        {
            double sum = 0;
            foreach (Color32 pixel in pixels)
                sum += pixel.a;

            return (float)(sum / pixels.Length / byte.MaxValue);
        }

        #region Downscale chain

        /// <summary>
        /// Each size is reached by steps that at most halve the size, all of them exact halvings but the first one.
        /// </summary>
        [TestCase(240, 64, new[] { 128, 64 })]
        [TestCase(240, 32, new[] { 128, 64, 32 })]
        [TestCase(240, 16, new[] { 128, 64, 32, 16 })]
        [TestCase(240, 48, new[] { 192, 96, 48 })]
        [TestCase(240, 24, new[] { 192, 96, 48, 24 })]
        [TestCase(240, 120, new[] { 120 })]
        [TestCase(240, 200, new[] { 200 })]
        public void GetDownscaleChain_StepsAtMostHalve(int sourceSize, int targetSize, int[] expected)
        {
            CollectionAssert.AreEqual(expected, IconProcessor.GetDownscaleChain(sourceSize, targetSize));
        }

        /// <summary>
        /// A target size that is not smaller than the source is reached in a single step.
        /// </summary>
        [TestCase(240, 240)]
        [TestCase(16, 64)]
        public void GetDownscaleChain_TargetNotSmaller_IsSingleStep(int sourceSize, int targetSize)
        {
            CollectionAssert.AreEqual(new[] { targetSize }, IconProcessor.GetDownscaleChain(sourceSize, targetSize));
        }

        #endregion


        #region Processing

        /// <summary>
        /// A file is made for each stored size, with the expected size.
        /// </summary>
        [Test]
        public void TryProcess_MakesEveryStoredSize()
        {
            Dictionary<int, byte[]> pngBySize = Process(MakeIconPng());

            CollectionAssert.AreEquivalent(IconStorage.StoredSizes, pngBySize.Keys);
            foreach (KeyValuePair<int, byte[]> png in pngBySize)
            {
                ReadPixels(png.Value, out Vector2Int size);
                Assert.AreEqual(new Vector2Int(png.Key, png.Key), size);
            }
        }

        /// <summary>
        /// Every pixel is white, the center of the circle is opaque, and the corners are transparent.
        /// </summary>
        [Test]
        public void TryProcess_MakesWhiteIconWithSameShape()
        {
            Dictionary<int, byte[]> pngBySize = Process(MakeIconPng());

            foreach (KeyValuePair<int, byte[]> png in pngBySize)
            {
                Color32[] pixels = ReadPixels(png.Value, out Vector2Int size);
                int center = (size.y / 2 * size.x) + (size.x / 2);
                int lastCorner = pixels.Length - 1;

                Assert.IsTrue(System.Array.TrueForAll(pixels, IsWhite), $"Not white at {png.Key} px.");
                Assert.AreEqual(byte.MaxValue, pixels[center].a, $"Center not opaque at {png.Key} px.");
                Assert.AreEqual(0, pixels[0].a, $"Corner not transparent at {png.Key} px.");
                Assert.AreEqual(0, pixels[lastCorner].a, $"Corner not transparent at {png.Key} px.");
            }
        }

        /// <summary>
        /// Downscaling averages all the source pixels, so the share of covered pixels stays the same at every size.
        /// </summary>
        [Test]
        public void TryProcess_KeepsAverageAlpha()
        {
            byte[] sourcePng = MakeIconPng();
            float sourceAlpha = GetAverageAlpha(ReadPixels(sourcePng, out _));

            Dictionary<int, byte[]> pngBySize = Process(sourcePng);

            foreach (KeyValuePair<int, byte[]> png in pngBySize)
                Assert.AreEqual(sourceAlpha, GetAverageAlpha(ReadPixels(png.Value, out _)), AlphaTolerance, $"At {png.Key} px.");
        }

        /// <summary>
        /// Data that is not a square image is rejected.
        /// </summary>
        [Test]
        public void TryProcess_InvalidImage_Fails()
        {
            Assert.IsFalse(IconProcessor.TryProcess(null, IconStorage.StoredSizes, out _, out string error));
            Assert.IsNotNull(error);
            Assert.IsFalse(IconProcessor.TryProcess(new byte[] { 1, 2, 3 }, IconStorage.StoredSizes, out _, out error));
            Assert.IsNotNull(error);
            Assert.IsFalse(IconProcessor.TryProcess(MakeIconPng(SourceSize, SourceSize / 2), IconStorage.StoredSizes, out _, out error));
            Assert.IsNotNull(error);
        }

        /// <summary>
        /// An invalid size is rejected.
        /// </summary>
        [Test]
        public void TryProcess_InvalidSize_Fails()
        {
            int[] sizes = { 16, 0 };
            bool isProcessed = IconProcessor.TryProcess(MakeIconPng(), sizes, out Dictionary<int, byte[]> pngBySize, out string error);

            Assert.IsFalse(isProcessed);
            Assert.IsNull(pngBySize);
            Assert.IsNotNull(error);
        }

        #endregion

    }

}
