using System;
using System.IO;

using NUnit.Framework;

using UnityEngine;

using SideXP.Core.EditorOnly;

using Object = UnityEngine.Object;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="IconTextures"/>.
    /// </summary>
    /// <remarks>
    /// Every test works in a temporary folder set as <see cref="IconStorage.RootFolderOverride"/>, and clears the textures kept in
    /// memory before and after running. Placeholders use the GPU, so these tests work in the editor, but not in batch mode with
    /// <c>-nographics</c>.
    /// </remarks>
    public class IconTexturesTests
    {

        /// <summary>
        /// Size of the generated icon files, in pixels.
        /// </summary>
        private const int PixelSize = 16;

        /// <summary>
        /// Alpha of the semi-transparent pixel of the generated icon.
        /// </summary>
        private const byte HalfAlpha = 128;

        private string _tempFolder = null;

        /// <summary>
        /// Creates an empty temporary folder as the shared folder, and clears the textures kept in memory.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            string tempFolderName = $"SideXP-{nameof(IconTexturesTests)}-{Guid.NewGuid():N}";
            _tempFolder = IconStorage.NormalizePath(Path.Combine(Path.GetTempPath(), tempFolderName));
            Directory.CreateDirectory(_tempFolder);
            IconStorage.RootFolderOverride = _tempFolder;
            IconTextures.Clear();
        }

        /// <summary>
        /// Clears the textures kept in memory, restores the default shared folder, and deletes the temporary folder.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            IconTextures.Clear();
            IconStorage.RootFolderOverride = null;
            if (Directory.Exists(_tempFolder))
                Directory.Delete(_tempFolder, true);
        }

        /// <summary>
        /// Makes a white icon file like the stored ones: opaque in the first pixel, half transparent in the second one, and fully
        /// transparent elsewhere.
        /// </summary>
        /// <returns>Returns the content of the PNG file.</returns>
        private static byte[] MakeWhitePng()
        {
            var texture = new Texture2D(PixelSize, PixelSize, TextureFormat.RGBA32, false);
            try
            {
                var pixels = new Color32[PixelSize * PixelSize];
                for (int i = 0; i < pixels.Length; i++)
                    pixels[i] = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, 0);

                pixels[0].a = byte.MaxValue;
                pixels[1].a = HalfAlpha;
                texture.SetPixels32(pixels);
                return texture.EncodeToPNG();
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }
        }

        /// <summary>
        /// Stores a white icon file for the <c>home</c> icon.
        /// </summary>
        /// <returns>Returns the path to the stored file.</returns>
        private static string StoreHome()
        {
            string path = IconStorage.GetIconFilePath("home", "Buildings", PixelSize, EIconStyle.Outline);
            FileEditorUtility.WriteFileAtomic(path, MakeWhitePng(), false);
            return path;
        }

        #region Icon textures

        /// <summary>
        /// The white version of an icon keeps the pixels of the stored file.
        /// </summary>
        [Test]
        public void CreateTexture_White_KeepsPixels()
        {
            Texture2D texture = IconTextures.CreateTexture(MakeWhitePng(), false, true);
            try
            {
                Color32[] pixels = texture.GetPixels32();
                Assert.AreEqual(new Color32(255, 255, 255, 255), pixels[0]);
                Assert.AreEqual(new Color32(255, 255, 255, HalfAlpha), pixels[1]);
                Assert.AreEqual(HideFlags.HideAndDontSave, texture.hideFlags);
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }
        }

        /// <summary>
        /// The black version of an icon has every pixel set to black, and keeps the alpha of the stored file.
        /// </summary>
        [Test]
        public void CreateTexture_Black_KeepsAlpha()
        {
            Texture2D texture = IconTextures.CreateTexture(MakeWhitePng(), true, true);
            try
            {
                Color32[] pixels = texture.GetPixels32();
                Assert.AreEqual(new Color32(0, 0, 0, 255), pixels[0]);
                Assert.AreEqual(new Color32(0, 0, 0, HalfAlpha), pixels[1]);
                Assert.AreEqual(new Color32(0, 0, 0, 0), pixels[2]);
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }
        }

        /// <summary>
        /// Data that is not a valid image makes no texture.
        /// </summary>
        [Test]
        public void CreateTexture_InvalidImage_ReturnsNull()
        {
            Assert.IsNull(IconTextures.CreateTexture(new byte[] { 1, 2, 3 }, false, false));
            Assert.IsNull(IconTextures.CreateTexture(null, false, false));
        }

        /// <summary>
        /// A stored file is loaded once, then kept in memory.
        /// </summary>
        [Test]
        public void TryGet_StoredFile_LoadsOnceAndKeepsInMemory()
        {
            StoreHome();

            Assert.IsTrue(IconTextures.TryGet("home", "Buildings", EIconStyle.Outline, PixelSize, false, out Texture2D first));
            Assert.IsTrue(IconTextures.TryGet("home", "Buildings", EIconStyle.Outline, PixelSize, false, out Texture2D second));

            Assert.AreSame(first, second);
            Assert.AreEqual(PixelSize, first.width);
            Assert.AreEqual(1, IconTextures.Count);
        }

        /// <summary>
        /// The white and black versions of an icon are separate textures.
        /// </summary>
        [Test]
        public void TryGet_WhiteAndBlack_AreSeparateTextures()
        {
            StoreHome();

            Assert.IsTrue(IconTextures.TryGet("home", "Buildings", EIconStyle.Outline, PixelSize, false, out Texture2D white));
            Assert.IsTrue(IconTextures.TryGet("home", "Buildings", EIconStyle.Outline, PixelSize, true, out Texture2D black));

            Assert.AreNotSame(white, black);
        }

        /// <summary>
        /// A file that is not stored makes no texture.
        /// </summary>
        [Test]
        public void TryGet_MissingFile_ReturnsFalse()
        {
            Assert.IsFalse(IconTextures.TryGet("home", "Buildings", EIconStyle.Outline, PixelSize, false, out Texture2D texture));
            Assert.IsNull(texture);
        }

        /// <summary>
        /// A stored file that is not a valid image is deleted, so it's downloaded again.
        /// </summary>
        [Test]
        public void TryGet_InvalidFile_DeletesIt()
        {
            string path = IconStorage.GetIconFilePath("home", "Buildings", PixelSize, EIconStyle.Outline);
            FileEditorUtility.WriteFileAtomic(path, new byte[] { 1, 2, 3 }, false);

            Assert.IsFalse(IconTextures.TryGet("home", "Buildings", EIconStyle.Outline, PixelSize, false, out _));
            Assert.IsFalse(File.Exists(path));
        }

        /// <summary>
        /// Clearing the textures destroys them, and the next request loads them again.
        /// </summary>
        [Test]
        public void Clear_DestroysTextures()
        {
            StoreHome();
            IconTextures.TryGet("home", "Buildings", EIconStyle.Outline, PixelSize, false, out Texture2D texture);

            IconTextures.Clear();

            Assert.IsTrue(texture == null, "The texture has not been destroyed.");
            Assert.AreEqual(0, IconTextures.Count);
            Assert.IsTrue(IconTextures.TryGet("home", "Buildings", EIconStyle.Outline, PixelSize, false, out Texture2D reloaded));
            Assert.IsTrue(reloaded != null);
        }

        #endregion


        #region Placeholders

        /// <summary>
        /// The empty placeholder has the size of the icon, and is made once per size.
        /// </summary>
        [Test]
        public void GetEmptyPlaceholder_HasIconSize_AndIsReused()
        {
            Texture2D placeholder = IconTextures.GetEmptyPlaceholder(32);

            Assert.AreEqual(32, placeholder.width);
            Assert.AreEqual(32, placeholder.height);
            Assert.AreSame(placeholder, IconTextures.GetEmptyPlaceholder(32));
            Assert.AreNotSame(placeholder, IconTextures.GetEmptyPlaceholder(16));
        }

        /// <summary>
        /// The loading placeholder has the size of the icon, and is made once per size.
        /// </summary>
        [TestCase(16, 16f)]
        [TestCase(48, 24f)]
        [TestCase(64, 32f)]
        public void GetLoadingPlaceholder_HasIconSize_AndIsReused(int pixelSize, float pointSize)
        {
            Texture2D placeholder = IconTextures.GetLoadingPlaceholder(pixelSize, pointSize);

            Assert.AreEqual(pixelSize, placeholder.width);
            Assert.AreEqual(pixelSize, placeholder.height);
            Assert.AreSame(placeholder, IconTextures.GetLoadingPlaceholder(pixelSize, pointSize));
        }

        #endregion

    }

}
