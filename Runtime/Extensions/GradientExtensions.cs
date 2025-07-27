using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Extension functions for <see cref="Gradient"/> instances.
    /// </summary>
    public static class GradientExtensions
    {

        /// <summary>
        /// Clones this gradient.
        /// </summary>
        /// <param name="gradient">The gradient to clone.</param>
        /// <returns>Returns the gradient's clone.</returns>
        public static Gradient Clone(this Gradient gradient)
        {
            Gradient newGradient = new Gradient
            {
                alphaKeys = (GradientAlphaKey[])gradient.alphaKeys.Clone(),
                colorKeys = (GradientColorKey[])gradient.colorKeys.Clone(),
                mode = gradient.mode
            };
            return newGradient;
        }

        /// <summary>
        /// Reverses the <see cref="Gradient"/> color and alpha keys.
        /// </summary>
        /// <param name="gradient">The original gradient you want to reverse.</param>
        /// <returns>Returns a new <see cref="Gradient"/> instance with the reversed keys of the input <see cref="Gradient"/>.</returns>
        public static Gradient Reverse(this Gradient gradient)
        {
            Gradient newGradient = gradient.Clone();

            newGradient.colorKeys = new GradientColorKey[gradient.colorKeys.Length];
            for (int i = 0; i < gradient.colorKeys.Length; i++)
            {
                newGradient.colorKeys[i] = new GradientColorKey(gradient.colorKeys[i].color, 1f - gradient.colorKeys[i].time);
            }

            newGradient.alphaKeys = new GradientAlphaKey[gradient.alphaKeys.Length];
            for (int i = 0; i < gradient.alphaKeys.Length; i++)
            {
                newGradient.alphaKeys[i] = new GradientAlphaKey(gradient.alphaKeys[i].alpha, 1f - gradient.alphaKeys[i].time);
            }

            return newGradient;
        }

    }

}