using System;
using System.IO;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.UIElements;

using SideXP.Core.EditorOnly;

using Object = UnityEngine.Object;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for the <see cref="Icon"/> UI Toolkit element.
    /// </summary>
    /// <remarks>
    /// Every test starts from a clean state: a temporary folder set as <see cref="IconStorage.RootFolderOverride"/>, no texture in
    /// memory, an empty download queue, no automatic index download, and fake downloads that never complete. The custom USS size is
    /// not tested here, since it needs the element to be in a panel with a style sheet.
    /// </remarks>
    public class IconElementTests
    {

        private string _tempFolder = null;

        /// <summary>
        /// Creates a clean state, with downloads that never complete.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            string tempFolderName = $"SideXP-{nameof(IconElementTests)}-{Guid.NewGuid():N}";
            _tempFolder = IconStorage.NormalizePath(Path.Combine(Path.GetTempPath(), tempFolderName));
            Directory.CreateDirectory(_tempFolder);
            IconStorage.RootFolderOverride = _tempFolder;
            IconDownloader.DownloadOverride = url => new FakeIconDownload(url, IconDownloadResult.Success(new byte[0]), true);

            ResetAll();
            EditorIcons.IsAutoRefreshEnabled = false;
        }

        /// <summary>
        /// Restores the default state, and deletes the temporary folder.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            ResetAll();
            EditorIcons.IsAutoRefreshEnabled = true;
            IconDownloader.DownloadOverride = null;
            IconStorage.RootFolderOverride = null;
            if (Directory.Exists(_tempFolder))
                Directory.Delete(_tempFolder, true);
        }

        /// <summary>
        /// Forgets the state of every part of the editor icons.
        /// </summary>
        private static void ResetAll()
        {
            IconDownloads.Reset();
            IconTextures.Clear();
            IconIndexStore.ClearMemory();
            EditorIcons.ResetState();
        }

        /// <summary>
        /// Writes an index with the <c>home</c> icon, and stores its outline files in every size.
        /// </summary>
        private static void StoreHome()
        {
            IconIndexStore.Write(new IconIndex
            {
                version = "3.52.0",
                icons = new[] { new IconIndexEntry { name = "home", category = "Buildings" } },
            });

            foreach (int size in IconStorage.StoredSizes)
            {
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                string path = IconStorage.GetIconFilePath("home", "Buildings", size, EIconStyle.Outline);
                FileEditorUtility.WriteFileAtomic(path, texture.EncodeToPNG(), false);
                Object.DestroyImmediate(texture);
            }
        }

        /// <summary>
        /// Checks the size of an element, in points.
        /// </summary>
        /// <param name="icon">The element to check.</param>
        /// <param name="expected">The expected size, in points.</param>
        private static void AssertSize(Icon icon, float expected)
        {
            Assert.AreEqual(expected, icon.PointSize);
            Assert.AreEqual(expected, icon.style.width.value.value);
            Assert.AreEqual(expected, icon.style.height.value.value);
        }

        /// <summary>
        /// A new element has no icon, the default style, color and size, and the USS classes of its size.
        /// </summary>
        [Test]
        public void Constructor_Default_UsesDefaults()
        {
            var icon = new Icon();

            Assert.IsNull(icon.IconName);
            Assert.AreEqual(EIconStyle.Outline, icon.IconStyle);
            Assert.AreEqual(EIconColor.Auto, icon.IconColor);
            Assert.AreEqual(EIconSize.Small, icon.IconSize);
            AssertSize(icon, 16f);
            Assert.IsTrue(icon.ClassListContains(Icon.UssClassName));
            Assert.IsTrue(icon.ClassListContains(Icon.SmallUssClassName));
            Assert.AreEqual(StyleKeyword.None, icon.style.backgroundImage.keyword);
        }

        /// <summary>
        /// The given icon, style, color and size are used.
        /// </summary>
        [Test]
        public void Constructor_WithValues_UsesValues()
        {
            var icon = new Icon("home", EIconStyle.Filled, EIconColor.White, EIconSize.Medium);

            Assert.AreEqual("home", icon.IconName);
            Assert.AreEqual(EIconStyle.Filled, icon.IconStyle);
            Assert.AreEqual(EIconColor.White, icon.IconColor);
            AssertSize(icon, 24f);
        }

        /// <summary>
        /// Changing the size changes the size of the element, and its size USS class.
        /// </summary>
        [TestCase(EIconSize.Small, 16f, Icon.SmallUssClassName)]
        [TestCase(EIconSize.Medium, 24f, Icon.MediumUssClassName)]
        [TestCase(EIconSize.Large, 32f, Icon.LargeUssClassName)]
        public void IconSize_SetsSizeAndUssClass(EIconSize size, float expectedPointSize, string expectedClass)
        {
            var icon = new Icon { IconSize = EIconSize.Large };

            icon.IconSize = size;

            AssertSize(icon, expectedPointSize);
            foreach (string sizeClass in new[] { Icon.SmallUssClassName, Icon.MediumUssClassName, Icon.LargeUssClassName })
                Assert.AreEqual(sizeClass == expectedClass, icon.ClassListContains(sizeClass), sizeClass);

            Assert.IsTrue(icon.ClassListContains(Icon.UssClassName));
        }

        /// <summary>
        /// A stored icon is displayed as background image.
        /// </summary>
        [Test]
        public void IconName_Stored_DisplaysIcon()
        {
            StoreHome();

            var icon = new Icon("home");

            Assert.AreSame(EditorIcons.GetIcon("home"), icon.style.backgroundImage.value.texture);
        }

        /// <summary>
        /// An icon not stored yet displays the loading placeholder, of the size of the element.
        /// </summary>
        [Test]
        public void IconName_NotStored_DisplaysLoadingPlaceholder()
        {
            StoreHome();
            IconTextures.Clear();
            IconStorage.DeleteDownloadedIcons();

            var icon = new Icon("home", iconSize: EIconSize.Large);

            Texture2D texture = icon.style.backgroundImage.value.texture;
            int pixelSize = IconSizes.GetPixelSize(EIconSize.Large, UnityEditor.EditorGUIUtility.pixelsPerPoint);
            Assert.AreSame(IconTextures.GetLoadingPlaceholder(pixelSize, EditorIcons.GetPointSize(EIconSize.Large)), texture);
        }

        /// <summary>
        /// Clearing the icon name removes the background image.
        /// </summary>
        [Test]
        public void IconName_Cleared_RemovesImage()
        {
            StoreHome();
            var icon = new Icon("home");

            icon.IconName = null;

            Assert.AreEqual(StyleKeyword.None, icon.style.backgroundImage.keyword);
        }

    }

}
