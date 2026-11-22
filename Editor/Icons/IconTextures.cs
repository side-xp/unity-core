using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEditor;

using Object = UnityEngine.Object;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Loads the stored icon files as textures, and keeps them in memory, along with the placeholders drawn while icons are missing.
    /// </summary>
    /// <remarks>
    /// Textures use <see cref="HideFlags.HideAndDontSave"/>, so Unity doesn't unload them. That also makes them survive domain
    /// reloads while the static cache doesn't, so they're destroyed before scripts are reloaded. Textures are owned by the cache:
    /// callers must not destroy them.
    /// </remarks>
    internal static class IconTextures
    {

        #region Fields

        /// <summary>
        /// Name of the built-in icon used as loading placeholder on the light editor theme: the first frame of Unity's spinner.
        /// </summary>
        /// <remarks>
        /// Built-in icon names are not a documented API. This one exists in every editor from 2021.3 to 6000.6, in 16 px only.
        /// </remarks>
        private const string SpinnerIconName = "WaitSpin00";

        /// <summary>
        /// Name of the built-in icon used as loading placeholder on the dark editor theme.
        /// </summary>
        private const string DarkSpinnerIconName = "d_" + SpinnerIconName;

        /// <summary>
        /// Size of the spinner drawn in the loading placeholder, in points, whatever the size of the placeholder.
        /// </summary>
        private const float SpinnerPointSize = 16f;

        /// <summary>
        /// The loaded textures, by key.
        /// </summary>
        private static readonly Dictionary<string, Texture2D> s_textures = new Dictionary<string, Texture2D>();

        #endregion


        #region Lifecycle

        /// <summary>
        /// Static constructor.
        /// </summary>
        static IconTextures()
        {
            AssemblyReloadEvents.beforeAssemblyReload += Clear;
        }

        #endregion


        #region Public API

        /// <summary>
        /// The number of textures kept in memory, including placeholders.
        /// </summary>
        internal static int Count => s_textures.Count;

        /// <summary>
        /// Gets the texture of a stored icon file, from memory or by loading the file.
        /// </summary>
        /// <remarks>
        /// A stored file that can't be read as an image is deleted, so it's downloaded again when requested.
        /// </remarks>
        /// <param name="name">The name of the icon.</param>
        /// <param name="category">The category of the icon, as written in the index.</param>
        /// <param name="style">The style of the icon.</param>
        /// <param name="pixelSize">The size of the stored file, in pixels.</param>
        /// <param name="isBlack">If enabled, gets the black version of the icon. Otherwise, gets the white version.</param>
        /// <param name="texture">Outputs the texture of the icon, or null if the file is not stored or can't be read.</param>
        /// <returns>Returns true if the texture is available.</returns>
        /// <exception cref="ArgumentException">Thrown if the icon name is not valid.</exception>
        internal static bool TryGet(string name, string category, EIconStyle style, int pixelSize, bool isBlack, out Texture2D texture)
        {
            string key = $"{name}/{IconStorage.GetStyleName(style)}/{pixelSize}/{(isBlack ? "black" : "white")}";
            if (s_textures.TryGetValue(key, out texture) && texture != null)
                return true;

            texture = null;
            string path = IconStorage.GetIconFilePath(name, category, pixelSize, style);
            byte[] pngData;
            try
            {
                if (!File.Exists(path))
                    return false;

                pngData = File.ReadAllBytes(path);
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                return false;
            }

            texture = CreateTexture(pngData, isBlack, false);
            if (texture == null)
            {
                FileEditorUtility.TryDeleteFile(path);
                return false;
            }

            Remember(key, texture);
            return true;
        }

        /// <summary>
        /// Gets a fully transparent texture of a given size, drawn when an icon is missing.
        /// </summary>
        /// <param name="pixelSize">The size of the texture, in pixels.</param>
        /// <returns>Returns the placeholder texture.</returns>
        internal static Texture2D GetEmptyPlaceholder(int pixelSize)
        {
            string key = $"placeholder/empty/{pixelSize}";
            if (s_textures.TryGetValue(key, out Texture2D texture) && texture != null)
                return texture;

            texture = CreateTransparentTexture(pixelSize);
            texture.Apply(false, true);
            Remember(key, texture);
            return texture;
        }

        /// <summary>
        /// Gets a transparent texture of a given size with the first frame of Unity's spinner in its center, drawn while an icon is
        /// downloaded.
        /// </summary>
        /// <remarks>
        /// The spinner is drawn at <see cref="SpinnerPointSize"/> on screen whatever the size of the icon, and matches the editor
        /// theme. If the built-in spinner icon can't be found, the placeholder is fully transparent.
        /// </remarks>
        /// <param name="pixelSize">The size of the texture, in pixels.</param>
        /// <param name="pointSize">The size on screen of the icon, in points.</param>
        /// <returns>Returns the placeholder texture.</returns>
        internal static Texture2D GetLoadingPlaceholder(int pixelSize, float pointSize)
        {
            bool isDarkTheme = EditorGUIUtility.isProSkin;
            int spinnerPixels = Mathf.Clamp(Mathf.RoundToInt(pixelSize * SpinnerPointSize / pointSize), 1, pixelSize);
            string key = $"placeholder/loading/{pixelSize}/{spinnerPixels}/{(isDarkTheme ? "dark" : "light")}";
            if (s_textures.TryGetValue(key, out Texture2D texture) && texture != null)
                return texture;

            texture = CreateTransparentTexture(pixelSize);
            Texture spinner = EditorGUIUtility.FindTexture(isDarkTheme ? DarkSpinnerIconName : SpinnerIconName);
            if (spinner != null)
                DrawCentered(spinner, texture, spinnerPixels);

            texture.Apply(false, true);
            Remember(key, texture);
            return texture;
        }

        /// <summary>
        /// Creates a texture from an icon file.
        /// </summary>
        /// <param name="pngData">The content of the icon file.</param>
        /// <param name="isBlack">
        /// If enabled, sets every pixel to black, keeping its alpha. The stored files are white, so this makes the black version.
        /// </param>
        /// <param name="isReadable">If enabled, keeps a copy of the pixels in memory, so they can be read from scripts.</param>
        /// <returns>Returns the created texture, or null if the data is not a valid image.</returns>
        internal static Texture2D CreateTexture(byte[] pngData, bool isBlack, bool isReadable)
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (pngData == null || !texture.LoadImage(pngData, false))
            {
                Object.DestroyImmediate(texture);
                return null;
            }

            if (isBlack)
            {
                Color32[] pixels = texture.GetPixels32();
                for (int i = 0; i < pixels.Length; i++)
                    pixels[i] = new Color32(0, 0, 0, pixels[i].a);

                texture.SetPixels32(pixels);
            }

            texture.Apply(false, !isReadable);
            SetUp(texture);
            return texture;
        }

        /// <summary>
        /// Destroys all the textures kept in memory. The next requests load them again.
        /// </summary>
        internal static void Clear()
        {
            foreach (Texture2D texture in s_textures.Values)
            {
                if (texture != null)
                    Object.DestroyImmediate(texture);
            }
            s_textures.Clear();
        }

        #endregion


        #region Private API

        /// <summary>
        /// Keeps a texture in memory.
        /// </summary>
        /// <param name="key">The key of the texture.</param>
        /// <param name="texture">The texture to keep.</param>
        private static void Remember(string key, Texture2D texture)
        {
            texture.name = key;
            s_textures[key] = texture;
        }

        /// <summary>
        /// Sets the options shared by every icon texture.
        /// </summary>
        /// <param name="texture">The texture to set up.</param>
        private static void SetUp(Texture2D texture)
        {
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
        }

        /// <summary>
        /// Creates a fully transparent texture, still readable so it can be drawn into.
        /// </summary>
        /// <param name="pixelSize">The size of the texture, in pixels.</param>
        /// <returns>Returns the created texture.</returns>
        private static Texture2D CreateTransparentTexture(int pixelSize)
        {
            var texture = new Texture2D(pixelSize, pixelSize, TextureFormat.RGBA32, false);
            texture.SetPixels32(new Color32[pixelSize * pixelSize]);
            SetUp(texture);
            return texture;
        }

        /// <summary>
        /// Draws a texture in the center of another, at a given size.
        /// </summary>
        /// <remarks>
        /// Uses <see cref="Graphics.Blit(Texture, RenderTexture)"/>, since built-in textures are not readable.
        /// </remarks>
        /// <param name="source">The texture to draw.</param>
        /// <param name="target">The texture to draw into. Must be readable.</param>
        /// <param name="pixelSize">The size at which to draw the source texture, in pixels.</param>
        private static void DrawCentered(Texture source, Texture2D target, int pixelSize)
        {
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture scaled = RenderTexture.GetTemporary(
                pixelSize,
                pixelSize,
                0,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.Default
            );
            try
            {
                Graphics.Blit(source, scaled);
                RenderTexture.active = scaled;

                int offset = (target.width - pixelSize) / 2;
                target.ReadPixels(new Rect(0, 0, pixelSize, pixelSize), offset, offset, false);
            }
            finally
            {
                RenderTexture.active = previousActive;
                RenderTexture.ReleaseTemporary(scaled);
            }
        }

        #endregion

    }

}
