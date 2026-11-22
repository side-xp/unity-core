using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;

using SideXP.Core.EditorOnly;

using Object = UnityEngine.Object;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="EditorIcons"/>.
    /// </summary>
    /// <remarks>
    /// Every test starts from a clean state: a temporary folder set as <see cref="IconStorage.RootFolderOverride"/>, no texture in
    /// memory, an empty download queue, and no automatic index download. Downloads are replaced by <see cref="FakeIconDownload"/>.
    /// Textures and processing use the GPU, so these tests work in the editor, but not in batch mode with <c>-nographics</c>.
    /// </remarks>
    public class EditorIconsTests
    {

        /// <summary>
        /// Tabler version of the test index.
        /// </summary>
        private const string Version = "3.52.0";

        /// <summary>
        /// Name of an icon that is not in the test index.
        /// </summary>
        private const string UnknownName = "not-an-icon";

        /// <summary>
        /// Maximum number of editor frames to wait for downloads, so a broken download fails the test instead of waiting forever.
        /// </summary>
        private const int MaxFrames = 200;

        private string _tempFolder = null;

        /// <summary>
        /// Whether the fake server answers with a 404 instead of a valid PNG.
        /// </summary>
        private bool _isServerMissingFiles = false;

        /// <summary>
        /// Creates a clean state, and replaces the downloads by a fake server answering with a valid PNG.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            string tempFolderName = $"SideXP-{nameof(EditorIconsTests)}-{Guid.NewGuid():N}";
            _tempFolder = IconStorage.NormalizePath(Path.Combine(Path.GetTempPath(), tempFolderName));
            Directory.CreateDirectory(_tempFolder);
            IconStorage.RootFolderOverride = _tempFolder;

            _isServerMissingFiles = false;
            IconDownloader.DownloadOverride = url => _isServerMissingFiles
                ? new FakeIconDownload(url, IconDownloadResult.Failure(EIconDownloadStatus.NotFound, "Not Found", 404))
                : new FakeIconDownload(url, IconDownloadResult.Success(MakePng(32)));

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
        /// Writes the test index: <c>home</c> in both styles, <c>user</c> in outline only, and <c>git-branch</c>.
        /// </summary>
        private static void WriteIndex()
        {
            IconIndexStore.Write(new IconIndex
            {
                version = Version,
                icons = new[]
                {
                    new IconIndexEntry { name = "home", category = "Buildings", tags = new[] { "house" }, hasFilled = true },
                    new IconIndexEntry { name = "user", category = "System", tags = new[] { "person" } },
                    new IconIndexEntry { name = "git-branch", category = "Version control", tags = new[] { "code" } },
                },
            });
        }

        /// <summary>
        /// Makes a white square PNG.
        /// </summary>
        /// <param name="pixelSize">The size of the image, in pixels.</param>
        /// <returns>Returns the content of the PNG file.</returns>
        private static byte[] MakePng(int pixelSize)
        {
            var texture = new Texture2D(pixelSize, pixelSize, TextureFormat.RGBA32, false);
            try
            {
                var pixels = new Color32[pixelSize * pixelSize];
                for (int i = 0; i < pixels.Length; i++)
                    pixels[i] = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

                texture.SetPixels32(pixels);
                return texture.EncodeToPNG();
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }
        }

        /// <summary>
        /// Stores the files of a style of an icon in every size, as if it had been downloaded before.
        /// </summary>
        /// <param name="name">The name of the icon.</param>
        /// <param name="category">The category of the icon.</param>
        /// <param name="style">The style to store.</param>
        private static void Store(string name, string category, EIconStyle style)
        {
            foreach (int size in IconStorage.StoredSizes)
                IconStorage.WriteFileAtomic(IconStorage.GetIconFilePath(name, category, size, style), MakePng(size), false);
        }

        /// <summary>
        /// Gets the size of the file used for small icons at the current display scale.
        /// </summary>
        private static int SmallPixelSize => IconSizes.GetPixelSize(EIconSize.Small, EditorGUIUtility.pixelsPerPoint);

        /// <summary>
        /// Waits for editor frames until the download queue is empty.
        /// </summary>
        /// <returns>Returns the coroutine that waits.</returns>
        private static IEnumerator WaitForDownloads()
        {
            for (int i = 0; i < MaxFrames && IconDownloads.Count > 0; i++)
                yield return null;

            Assert.AreEqual(0, IconDownloads.Count, "The downloads didn't complete.");
        }

        #region Index

        /// <summary>
        /// Without index, no name can be checked, and the lists are empty.
        /// </summary>
        [Test]
        public void Exists_WithoutIndex_ReturnsFalse()
        {
            Assert.IsFalse(EditorIcons.IsIndexAvailable);
            Assert.IsFalse(EditorIcons.Exists("home"));
            Assert.IsEmpty(EditorIcons.Categories);
            Assert.AreEqual(0, EditorIcons.Icons.Count);
        }

        /// <summary>
        /// With index, names are checked, along with the styles they exist in.
        /// </summary>
        [Test]
        public void Exists_WithIndex_ChecksNameAndStyle()
        {
            WriteIndex();

            Assert.IsTrue(EditorIcons.IsIndexAvailable);
            Assert.IsTrue(EditorIcons.Exists("home"));
            Assert.IsTrue(EditorIcons.Exists("home", EIconStyle.Filled));
            Assert.IsTrue(EditorIcons.Exists("user"));
            Assert.IsFalse(EditorIcons.Exists("user", EIconStyle.Filled));
            Assert.IsFalse(EditorIcons.Exists(UnknownName));
            Assert.IsFalse(EditorIcons.Exists(null));
        }

        /// <summary>
        /// The categories are listed once each, sorted by name, and every icon is listed.
        /// </summary>
        [Test]
        public void CategoriesAndIcons_ListIndexContent()
        {
            WriteIndex();

            CollectionAssert.AreEqual(new[] { "Buildings", "System", "Version control" }, EditorIcons.Categories);
            Assert.AreEqual(3, EditorIcons.Icons.Count);
            Assert.AreEqual("home", EditorIcons.Icons[0].Name);
            Assert.IsTrue(EditorIcons.Icons[0].HasFilled);
        }

        /// <summary>
        /// Each size has its size on screen.
        /// </summary>
        [Test]
        public void GetPointSize_MatchesIconSize()
        {
            Assert.AreEqual(16f, EditorIcons.GetPointSize(EIconSize.Small));
            Assert.AreEqual(32f, EditorIcons.GetPointSize(EIconSize.Large));
        }

        #endregion


        #region Icons

        /// <summary>
        /// A stored icon is returned with the file size matching the display scale, and the same texture is returned each time.
        /// </summary>
        [Test]
        public void GetIcon_Stored_ReturnsIconTexture()
        {
            WriteIndex();
            Store("home", "Buildings", EIconStyle.Outline);

            Texture2D icon = EditorIcons.GetIcon("home");

            Assert.AreEqual(SmallPixelSize, icon.width);
            Assert.AreSame(icon, EditorIcons.GetIcon("home"));
            Assert.IsTrue(EditorIcons.TryGetIcon("home", out Texture2D available));
            Assert.AreSame(icon, available);
        }

        /// <summary>
        /// The white and black versions of an icon are different textures.
        /// </summary>
        [Test]
        public void GetIcon_WhiteAndBlack_AreDifferentTextures()
        {
            WriteIndex();
            Store("home", "Buildings", EIconStyle.Outline);

            Assert.AreNotSame(EditorIcons.GetIcon("home", color: EIconColor.White), EditorIcons.GetIcon("home", color: EIconColor.Black));
        }

        /// <summary>
        /// The filled style of an icon without filled version falls back to its outline style.
        /// </summary>
        [Test]
        public void GetIcon_FilledWithoutFilledStyle_FallsBackToOutline()
        {
            WriteIndex();
            Store("user", "System", EIconStyle.Outline);

            Assert.AreSame(EditorIcons.GetIcon("user"), EditorIcons.GetIcon("user", EIconStyle.Filled));
        }

        /// <summary>
        /// An icon not stored yet returns the loading placeholder, and is not available.
        /// </summary>
        [Test]
        public void GetIcon_NotStored_ReturnsLoadingPlaceholder()
        {
            WriteIndex();

            Texture2D icon = EditorIcons.GetIcon("home");

            Assert.AreSame(IconTextures.GetLoadingPlaceholder(SmallPixelSize, EditorIcons.GetPointSize(EIconSize.Small)), icon);
            Assert.IsFalse(EditorIcons.TryGetIcon("home", out Texture2D available));
            Assert.IsNull(available);
        }

        /// <summary>
        /// An icon not stored yet is downloaded, then returned.
        /// </summary>
        /// <returns>Returns the test coroutine.</returns>
        [UnityTest]
        public IEnumerator GetIcon_NotStored_IsDownloaded()
        {
            WriteIndex();
            EditorIcons.GetIcon("home");

            yield return WaitForDownloads();

            Assert.IsTrue(EditorIcons.TryGetIcon("home", out Texture2D icon));
            Assert.AreEqual(SmallPixelSize, icon.width);
            Assert.IsTrue(IconStorage.IsStored("home", "Buildings", EIconStyle.Filled), "The other style has not been downloaded.");
        }

        /// <summary>
        /// An icon that can't be downloaded returns the empty placeholder once given up.
        /// </summary>
        /// <returns>Returns the test coroutine.</returns>
        [UnityTest]
        public IEnumerator GetIcon_MissingOnServer_ReturnsEmptyPlaceholder()
        {
            WriteIndex();
            _isServerMissingFiles = true;
            EditorIcons.GetIcon("user");

            yield return WaitForDownloads();

            Assert.AreSame(IconTextures.GetEmptyPlaceholder(SmallPixelSize), EditorIcons.GetIcon("user"));
        }

        /// <summary>
        /// An unknown name logs a warning once, and returns the empty placeholder.
        /// </summary>
        [Test]
        public void GetIcon_UnknownName_WarnsOnce()
        {
            WriteIndex();
            LogAssert.Expect(LogType.Warning, new Regex(UnknownName));

            Texture2D icon = EditorIcons.GetIcon(UnknownName);
            EditorIcons.GetIcon(UnknownName);

            Assert.AreSame(IconTextures.GetEmptyPlaceholder(SmallPixelSize), icon);
            LogAssert.NoUnexpectedReceived();
        }

        /// <summary>
        /// An invalid name logs a warning once, and returns the empty placeholder without throwing.
        /// </summary>
        [Test]
        public void GetIcon_InvalidName_WarnsOnceWithoutThrowing()
        {
            WriteIndex();
            LogAssert.Expect(LogType.Warning, new Regex("Invalid editor icon name"));
            LogAssert.Expect(LogType.Warning, new Regex("Invalid editor icon name"));

            Texture2D icon = EditorIcons.GetIcon("../home");
            EditorIcons.GetIcon("../home");
            EditorIcons.GetIcon(null);

            Assert.AreSame(IconTextures.GetEmptyPlaceholder(SmallPixelSize), icon);
            LogAssert.NoUnexpectedReceived();
        }

        /// <summary>
        /// Names requested before the index is available are checked, and warned about, once it's loaded.
        /// </summary>
        [Test]
        public void GetIcon_BeforeIndex_WarnsOnceIndexIsLoaded()
        {
            EditorIcons.GetIcon(UnknownName);
            LogAssert.NoUnexpectedReceived();

            WriteIndex();
            LogAssert.Expect(LogType.Warning, new Regex(UnknownName));
            EditorIcons.GetIcon("home");
        }

        #endregion


        #region Asynchronous loading

        /// <summary>
        /// Loading a stored icon calls the callback right away.
        /// </summary>
        [Test]
        public void LoadIcon_Stored_CallsBackRightAway()
        {
            WriteIndex();
            Store("home", "Buildings", EIconStyle.Outline);

            Texture2D loadedIcon = null;
            EditorIcons.LoadIcon("home", icon => loadedIcon = icon);

            Assert.AreSame(EditorIcons.GetIcon("home"), loadedIcon);
        }

        /// <summary>
        /// Loading an icon not stored yet calls the callback once it's downloaded.
        /// </summary>
        /// <returns>Returns the test coroutine.</returns>
        [UnityTest]
        public IEnumerator LoadIcon_NotStored_CallsBackOnceDownloaded()
        {
            WriteIndex();
            Texture2D loadedIcon = null;
            EditorIcons.LoadIcon("home", icon => loadedIcon = icon, EIconStyle.Filled, EIconColor.White, EIconSize.Large);
            Assert.IsNull(loadedIcon);

            yield return WaitForDownloads();

            Assert.IsNotNull(loadedIcon);
            Assert.AreEqual(IconSizes.GetPixelSize(EIconSize.Large, EditorGUIUtility.pixelsPerPoint), loadedIcon.width);
        }

        /// <summary>
        /// Loading an icon that can't be downloaded never calls the callback.
        /// </summary>
        /// <returns>Returns the test coroutine.</returns>
        [UnityTest]
        public IEnumerator LoadIcon_MissingOnServer_NeverCallsBack()
        {
            WriteIndex();
            _isServerMissingFiles = true;
            bool isCalled = false;
            EditorIcons.LoadIcon("user", _ => isCalled = true);

            yield return WaitForDownloads();

            Assert.IsFalse(isCalled);
        }

        /// <summary>
        /// The callback is required.
        /// </summary>
        [Test]
        public void LoadIcon_NullCallback_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => EditorIcons.LoadIcon("home", null));
        }

        #endregion


        #region IMGUI

        /// <summary>
        /// The content has the given text and tooltip, and the icon as image.
        /// </summary>
        [Test]
        public void IconContent_HasTextTooltipAndIcon()
        {
            WriteIndex();
            Store("home", "Buildings", EIconStyle.Outline);

            GUIContent content = EditorIcons.IconContent("home", "Home", "Go home");

            Assert.AreEqual("Home", content.text);
            Assert.AreEqual("Go home", content.tooltip);
            Assert.AreSame(EditorIcons.GetIcon("home"), content.image);
        }

        /// <summary>
        /// The content of an unknown icon has the empty placeholder as image, so it's never null.
        /// </summary>
        [Test]
        public void IconContent_UnknownName_HasPlaceholder()
        {
            WriteIndex();
            LogAssert.Expect(LogType.Warning, new Regex(UnknownName));

            GUIContent content = EditorIcons.IconContent(UnknownName);

            Assert.AreSame(IconTextures.GetEmptyPlaceholder(SmallPixelSize), content.image);
        }

        /// <summary>
        /// The icon size scope sets the size of the icons drawn in contents, and restores the previous one, even when nested.
        /// </summary>
        [Test]
        public void IconSizeScope_SetsAndRestoresIconSize()
        {
            Vector2 previousSize = EditorGUIUtility.GetIconSize();

            using (new IconSizeScope(EIconSize.Large))
            {
                Assert.AreEqual(new Vector2(32f, 32f), EditorGUIUtility.GetIconSize());

                using (new IconSizeScope(EIconSize.Small))
                    Assert.AreEqual(new Vector2(16f, 16f), EditorGUIUtility.GetIconSize());

                Assert.AreEqual(new Vector2(32f, 32f), EditorGUIUtility.GetIconSize());
            }

            Assert.AreEqual(previousSize, EditorGUIUtility.GetIconSize());
        }

        #endregion

    }

}
