using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Extension functions for <see cref="Sprite"/> instances.
    /// </summary>
    public static class SpriteExtensions
    {

        /// <summary>
        /// Extracts a <see cref="Texture2D"/> from a given sprite.
        /// </summary>
        /// <param name="sprite">The object that defines the original texture and size of the sprite.</param>
        /// <returns>Returns the extracted <see cref="Texture2D"/>.</returns>
        public static Texture2D ToTexture(this Sprite sprite)
        {
            Texture2D texture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
            Color[] pixels = sprite.texture.GetPixels
            (
                (int)sprite.textureRect.x,
                (int)sprite.textureRect.y,
                (int)sprite.textureRect.width,
                (int)sprite.textureRect.height
            );

            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

    }

}