using System.Collections.Generic;

using UnityEngine;

using Object = UnityEngine.Object;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Converts a downloaded Tabler PNG into the white icon files stored for each size.
    /// </summary>
    /// <remarks>
    /// Tabler PNGs are black, with anti-aliased edges in the alpha channel. Every pixel is set to white first, so scaling only
    /// affects the alpha channel: there is no color fringe, and the color space (gamma or linear) doesn't change the result.<br/>
    /// Downscaling uses <see cref="Graphics.Blit(Texture, RenderTexture)"/> with a bilinear filter, which only averages all the
    /// source pixels when each step at most halves the size. So each size is reached by steps (see
    /// <see cref="GetDownscaleChain(int, int)"/>). This uses the GPU, so it doesn't work in batch mode with <c>-nographics</c>.
    /// </remarks>
    internal static class IconProcessor
    {

        #region Fields

        /// <summary>
        /// Value of a color channel at full intensity, in 32-bit colors.
        /// </summary>
        private const byte MaxChannelValue = byte.MaxValue;

        #endregion


        #region Public API

        /// <summary>
        /// Converts a PNG into white PNGs of the given sizes.
        /// </summary>
        /// <param name="pngData">The content of the source PNG file. It must be square.</param>
        /// <param name="sizes">The sizes of the files to make, in pixels.</param>
        /// <param name="pngBySize">Outputs the content of the PNG file made for each size, or null if the conversion failed.</param>
        /// <param name="error">Outputs the reason why the conversion failed, or null if it succeeded.</param>
        /// <returns>Returns true if the PNG has been converted successfully.</returns>
        internal static bool TryProcess(byte[] pngData, IEnumerable<int> sizes, out Dictionary<int, byte[]> pngBySize, out string error)
        {
            pngBySize = null;
            if (pngData == null || pngData.Length == 0)
            {
                error = "The icon file is empty.";
                return false;
            }

            var source = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
            var scaledTextures = new Dictionary<int, RenderTexture>();
            RenderTexture previousActive = RenderTexture.active;
            try
            {
                if (!source.LoadImage(pngData, false))
                {
                    error = "The icon file is not a valid image.";
                    return false;
                }

                if (source.width != source.height)
                {
                    error = $"The icon image is not square ({source.width}x{source.height}).";
                    return false;
                }

                MakeWhite(source);
                source.filterMode = FilterMode.Bilinear;
                source.wrapMode = TextureWrapMode.Clamp;

                pngBySize = new Dictionary<int, byte[]>();
                foreach (int size in sizes)
                {
                    if (size <= 0)
                    {
                        pngBySize = null;
                        error = $"Invalid icon size: {size}.";
                        return false;
                    }

                    RenderTexture scaled = Downscale(source, size, scaledTextures);
                    pngBySize[size] = Encode(scaled);
                }

                error = null;
                return true;
            }
            finally
            {
                RenderTexture.active = previousActive;
                foreach (RenderTexture texture in scaledTextures.Values)
                    RenderTexture.ReleaseTemporary(texture);

                Object.DestroyImmediate(source);
            }
        }

        /// <summary>
        /// Gets the sizes through which a texture is scaled to reach a target size, so each step at most halves the size.
        /// </summary>
        /// <remarks>
        /// The chain goes up from the target size by doubling it while that stays under the source size, so all the steps but the
        /// first one are exact halvings, and the chains of different sizes share their intermediate steps. A target that is not
        /// smaller than the source is reached in a single step.
        /// </remarks>
        /// <example>
        /// <c>GetDownscaleChain(240, 16)</c> returns <c>[128, 64, 32, 16]</c>, and <c>GetDownscaleChain(240, 24)</c> returns
        /// <c>[192, 96, 48, 24]</c>.
        /// </example>
        /// <param name="sourceSize">The size of the source texture, in pixels.</param>
        /// <param name="targetSize">The size to reach, in pixels.</param>
        /// <returns>Returns the sizes of each step, from the first one to the target size.</returns>
        internal static List<int> GetDownscaleChain(int sourceSize, int targetSize)
        {
            var chain = new List<int>();
            if (targetSize >= sourceSize)
            {
                chain.Add(targetSize);
                return chain;
            }

            int stepSize = targetSize;
            while (stepSize * 2 < sourceSize)
                stepSize *= 2;

            for (; stepSize >= targetSize; stepSize /= 2)
                chain.Add(stepSize);

            return chain;
        }

        #endregion


        #region Private API

        /// <summary>
        /// Sets every pixel of a texture to white, keeping its alpha, and uploads it to the GPU.
        /// </summary>
        /// <param name="texture">The texture to change.</param>
        private static void MakeWhite(Texture2D texture)
        {
            Color32[] pixels = texture.GetPixels32();
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = new Color32(MaxChannelValue, MaxChannelValue, MaxChannelValue, pixels[i].a);

            texture.SetPixels32(pixels);
            texture.Apply(false);
        }

        /// <summary>
        /// Scales a square texture to a given size by steps, reusing the steps already made for other sizes.
        /// </summary>
        /// <param name="source">The texture to scale.</param>
        /// <param name="size">The size to reach, in pixels.</param>
        /// <param name="scaledTextures">The textures already scaled from the same source, by size. The new steps are added to it.</param>
        /// <returns>Returns the scaled texture, owned by <paramref name="scaledTextures"/>.</returns>
        private static RenderTexture Downscale(Texture2D source, int size, Dictionary<int, RenderTexture> scaledTextures)
        {
            Texture previous = source;
            foreach (int stepSize in GetDownscaleChain(source.width, size))
            {
                if (!scaledTextures.TryGetValue(stepSize, out RenderTexture step))
                {
                    step = RenderTexture.GetTemporary(stepSize, stepSize, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                    step.filterMode = FilterMode.Bilinear;
                    step.wrapMode = TextureWrapMode.Clamp;
                    Graphics.Blit(previous, step);
                    scaledTextures[stepSize] = step;
                }
                previous = step;
            }

            return (RenderTexture)previous;
        }

        /// <summary>
        /// Reads a render texture back from the GPU, and encodes it as PNG.
        /// </summary>
        /// <param name="texture">The texture to encode.</param>
        /// <returns>Returns the content of the PNG file.</returns>
        private static byte[] Encode(RenderTexture texture)
        {
            var readable = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false, true);
            try
            {
                RenderTexture.active = texture;
                readable.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0, false);
                return readable.EncodeToPNG();
            }
            finally
            {
                Object.DestroyImmediate(readable);
            }
        }

        #endregion

    }

}
