using System.Collections.Generic;

using NUnit.Framework;

using UnityEngine;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="SpriteExtensions"/>.
    /// </summary>
    public class SpriteExtensionsTests
    {

        // Textures and sprites created during a test, destroyed in TearDown.
        private readonly List<Object> _objects = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object obj in _objects)
            {
                if (obj != null)
                    Object.DestroyImmediate(obj);
            }
            _objects.Clear();
        }

        /// <summary>
        /// Creates a readable texture whose pixels are all black except the ones set through <paramref name="paint"/>.
        /// </summary>
        private Texture2D NewTexture(int width, int height, System.Action<Texture2D> paint = null)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            _objects.Add(texture);

            var pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.black;
            texture.SetPixels(pixels);

            paint?.Invoke(texture);
            texture.Apply();
            return texture;
        }

        private Sprite NewSprite(Texture2D texture, Rect rect)
        {
            Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
            _objects.Add(sprite);
            return sprite;
        }

        private static void AssertColor(Color expected, Color actual, string message = null)
        {
            // RGBA32 quantizes channels to 8 bits, so allow ~1/255 per channel.
            float delta = Mathf.Abs(expected.r - actual.r) + Mathf.Abs(expected.g - actual.g)
                        + Mathf.Abs(expected.b - actual.b) + Mathf.Abs(expected.a - actual.a);
            Assert.Less(delta, 0.02f, message ?? $"Expected {expected}, got {actual}.");
        }

        [Test]
        public void ToTexture_FullRect_MatchesSpriteSize()
        {
            Texture2D source = NewTexture(4, 4);
            Sprite sprite = NewSprite(source, new Rect(0, 0, 4, 4));

            Texture2D result = NewTextureFromSprite(sprite);

            Assert.AreEqual(4, result.width);
            Assert.AreEqual(4, result.height);
        }

        [Test]
        public void ToTexture_SubRect_ExtractsRegionSizeAndPixels()
        {
            // 4x4 source; paint the 2x2 block at (2,1) with four distinct colors.
            Texture2D source = NewTexture(4, 4, t =>
            {
                t.SetPixel(2, 1, Color.red);
                t.SetPixel(3, 1, Color.green);
                t.SetPixel(2, 2, Color.blue);
                t.SetPixel(3, 2, Color.white);
            });
            Sprite sprite = NewSprite(source, new Rect(2, 1, 2, 2));

            Texture2D result = NewTextureFromSprite(sprite);

            // Size matches the sub-rect.
            Assert.AreEqual(2, result.width);
            Assert.AreEqual(2, result.height);

            // Pixels map from the source region with a bottom-left origin: result(0,0) == source(2,1), etc.
            AssertColor(Color.red, result.GetPixel(0, 0));
            AssertColor(Color.green, result.GetPixel(1, 0));
            AssertColor(Color.blue, result.GetPixel(0, 1));
            AssertColor(Color.white, result.GetPixel(1, 1));
        }

        // Extracts and tracks the produced texture for teardown.
        private Texture2D NewTextureFromSprite(Sprite sprite)
        {
            Texture2D result = sprite.ToTexture();
            _objects.Add(result);
            return result;
        }

    }

}
