using System;
using System.Collections.Generic;
using System.Security.Cryptography;

using Random = UnityEngine.Random;

namespace SideXP.Core
{

    /// <summary>
    /// Extension functions for <see cref="IList{T}"/> or <see cref="List{T}"/> instances.
    /// </summary>
    public static class ListExtensions
    {

        #region Delegates

        /// <summary>
        /// Called to check if the given items are similar.
        /// </summary>
        /// <typeparam name="T">The type of the items to compare.</typeparam>
        /// <param name="item">The first item to compare.</param>
        /// <param name="other">The second item to compare.</param>
        /// <returns>Returns true if the items are similar.</returns>
        public delegate bool IsSimilarDelegate<T>(T item, T other);

        #endregion


        #region Public API

        /// <summary>
        /// Adds a given item in the list only if it's not already in.
        /// </summary>
        /// <typeparam name="T">The type of the items in the list.</typeparam>
        /// <param name="list">The collection to process.</param>
        /// <param name="item">The item to add.</param>
        /// <returns>Returns true if the item has been added, or false of the item is already in the list.</returns>
        public static bool AddOnce<T>(this List<T> list, T item)
        {
            if (!list.Contains(item))
            {
                list.Add(item);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Moves an item in the list in-place.
        /// </summary>
        /// <param name="movedIndex">The index of the item to move in the list, clamped between 0 and list.Count - 1.</param>
        /// <param name="targetIndex">The new index of the item, clamped between 0 and list.Count - 1.</param>
        /// <returns>Returns true if an item has been moved in the list.</returns>
        /// <inheritdoc cref="AddOnce{T}(List{T}, T)"/>
        public static bool Move<T>(this IList<T> list, int movedIndex, int targetIndex)
        {
            // Cancel if the list is empty
            if (list.Count <= 0)
                return false;

            // Clamp indexes
            movedIndex = movedIndex.Clamp(0, list.Count - 1);
            targetIndex = targetIndex.Clamp(0, list.Count - 1);

            // Cancel if an item is moved at the same position
            if (movedIndex == targetIndex)
                return false;

            int direction = movedIndex < targetIndex ? 1 : -1;
            T item = list[movedIndex];

            for (int i = movedIndex; movedIndex < targetIndex ? i < targetIndex : i > targetIndex; i += direction)
                list[i] = list[i + direction];

            list[targetIndex] = item;

            return true;
        }

        /// <summary>
        /// Checks if the given index is in this list's range.
        /// </summary>
        /// <param name="index">The index you want to check.</param>
        /// <returns>Returns true if the given index in the list's range.</returns>
        /// <inheritdoc cref="AddOnce{T}(List{T}, T)"/>
        public static bool IsInRange<T>(this IList<T> list, int index)
        {
            return index >= 0 && index < list.Count;
        }

        /// <summary>
        /// Shuffles the list in-place, using <see cref="Random.Range(int, int)"/>. Original version at https://stackoverflow.com/questions/273313/randomize-a-listt
        /// </summary>
        /// <inheritdoc cref="AddOnce{T}(List{T}, T)"/>
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// <summary>
        /// Shuffles the list in-place, using Cryptography random number generators. This function is slower than <see cref="Shuffle{T}(IList{T})"/>, but provides a better randomness quality. Original version at https://stackoverflow.com/questions/273313/randomize-a-listt
        /// </summary>
        /// <inheritdoc cref="AddOnce{T}(List{T}, T)"/>
        public static void ShuffleCrypto<T>(this IList<T> list)
        {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            int n = list.Count;
            while (n > 1)
            {
                byte[] box = new byte[1];
                do
                    provider.GetBytes(box);
                while (!(box[0] < n * (Byte.MaxValue / n)));
                int k = (box[0] % n);
                n--;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// <inheritdoc cref="Slice{T}(IList{T}, int, int)"/>
        public static T[] Slice<T>(this IList<T> list, int start)
        {
            return Slice(list, start, list.Count);
        }

        /// <summary>
        /// Extracts a range of items from a list.
        /// </summary>
        /// <param name="start">The index at which to start extraction (included). If negative value given, the index is count from the end of the list.</param>
        /// <param name="end">The index at which to end extraction (excluded). If negative value given, the index is count from the end of the list.</param>
        /// <returns>Returns the extracted items.</returns>
        /// <inheritdoc cref="AddOnce{T}(List{T}, T)"/>
        public static T[] Slice<T>(this IList<T> list, int start, int end)
        {
            // Resolve negative indexes
            if (start < 0)
                start = list.Count + start;
            if (end < 0)
                end = list.Count + end;

            // Cancel if end is lower than start or start is higher than count
            if (end < start || start > list.Count)
                return new T[0];

            int i = 0;
            using (var scope = new ListPoolScope<T>())
            {
                foreach (T item in list)
                {
                    if (i >= start && i < end)
                        scope.List.Add(item);
                    i++;
                }

                return scope.List.ToArray();
            }
        }

        /// <summary>
        /// Removes the duplicates in a list by comparing them using <see cref="IComparable.CompareTo(object)"/>, so only the first occurrence of an item is kept in the list.
        /// </summary>
        /// <inheritdoc cref="RemoveDoubles{T}(IList{T}, IsSimilarDelegate{T})"/>
        public static int RemoveDoubles<T>(this IList<T> list)
            where T : IComparable<T>
        {
            return RemoveDoubles(list, (a, b) => a.CompareTo(b) == 0);
        }

        /// <summary>
        /// Removes the duplicates in a list by comparing them using the given function, so only the first occurrence of an item is kept in the list.
        /// </summary>
        /// <param name="comparator">The function that compares an item to another to check if they are similar.</param>
        /// <returns>Returns the number of doubles removed from the list.</returns>
        /// <inheritdoc cref="AddOnce{T}(List{T}, T)"/>
        public static int RemoveDoubles<T>(this IList<T> list, IsSimilarDelegate<T> comparator)
        {
            int doublesCount = 0;

            // For each item in the list
            for (int i = 0; i < list.Count; i++)
            {
                // For each "other" item in the list
                for (int j = 0; j < list.Count; j++)
                {
                    if (j == i)
                        continue;

                    // If the compared items are the same
                    if (comparator(list[i], list[j]))
                    {
                        // Remove the other item
                        list.RemoveAt(j);
                        doublesCount++;
                        j--;
                    }
                }
            }

            return doublesCount;
        }

        #endregion

    }

}