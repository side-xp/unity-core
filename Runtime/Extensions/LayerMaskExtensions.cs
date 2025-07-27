using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Miscellaneous extension functions for working with <see cref="LayerMask"/> values.
    /// </summary>
    public static class LayerMaskExtensions
    {

        /// <inheritdoc cref="GetLayerIndices(int)"/>
        public static int[] GetLayerIndices(this LayerMask mask)
        {
            return GetLayerIndices(mask.value);
        }

        /// <summary>
        /// Gets the indices of the layers (from 0 to 31 included) as defined in the project settings from a given mask value.
        /// </summary>
        /// <param name="mask">The mask value, which may represent several layers.</param>
        /// <returns>Returns the indices extracted from the given mask value.</returns>
        public static int[] GetLayerIndices(int mask)
        {
            using (var scope = new ListPoolScope<int>())
            {
                for (int i = 0; i < 32; i++)
                {
                    if ((mask & (1 << i)) != 0)
                        scope.List.Add(i);
                }
                return scope.List.ToArray();
            }
        }

    }

}