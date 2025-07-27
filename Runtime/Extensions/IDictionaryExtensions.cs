using System.Collections.Generic;

namespace SideXP.Core
{

    /// <summary>
    /// Extension functions for <see cref="IDictionary{TKey, TValue}"/> instances.
    /// </summary>
    public static class IDictionaryExtensions
    {

        /// <inheritdoc cref="GetOrAdd{TKey, TValue}(IDictionary{TKey, TValue}, TKey, out TValue, TValue)"/>
        public static void GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, out TValue value)
        {
            GetOrAdd(dictionary, key, out value, default);
        }

        /// <summary>
        /// Gets a value in a dictionary, or add a new entry if it doesn't exist.
        /// </summary>
        /// <typeparam name="TKey">The type of the dictionary's key.</typeparam>
        /// <typeparam name="TValue">The type of the dictionary's value.</typeparam>
        /// <param name="dictionary">The dictionary's value.</param>
        /// <param name="key">The dictionary's key.</param>
        /// <param name="value">Outputs the found (or added) value.</param>
        /// <param name="defaultValue">The default value to set if the key doesn't exist in the dictionary.</param>
        public static void GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, out TValue value, TValue defaultValue)
        {
            if (!dictionary.TryGetValue(key, out value))
            {
                value = defaultValue;
                dictionary.Add(key, value);
            }
        }

    }

}