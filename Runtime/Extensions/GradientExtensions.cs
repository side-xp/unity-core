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

            // Cache the source arrays: Unity's colorKeys/alphaKeys getters allocate a new array on every
            // access, so reading them inside the loop would churn an array per iteration.
            GradientColorKey[] colorKeys = gradient.colorKeys;
            GradientColorKey[] newColorKeys = new GradientColorKey[colorKeys.Length];
            for (int i = 0; i < colorKeys.Length; i++)
            {
                newColorKeys[i] = new GradientColorKey(colorKeys[i].color, 1f - colorKeys[i].time);
            }
            newGradient.colorKeys = newColorKeys;

            GradientAlphaKey[] alphaKeys = gradient.alphaKeys;
            GradientAlphaKey[] newAlphaKeys = new GradientAlphaKey[alphaKeys.Length];
            for (int i = 0; i < alphaKeys.Length; i++)
            {
                newAlphaKeys[i] = new GradientAlphaKey(alphaKeys[i].alpha, 1f - alphaKeys[i].time);
            }
            newGradient.alphaKeys = newAlphaKeys;

            return newGradient;
        }

    }

}