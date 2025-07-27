using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Extension functions for <see cref="IEnumerable{T}"/> instances.
    /// </summary>
    public static class IEnumerableExtensions
    {

        #region Delegates

        /// <summary>
        /// Called for each item when using the <see cref="Map{TInput, TOutput}(IEnumerable{TInput}, MapPredicateDelegate{TInput, TOutput})"/> function.
        /// </summary>
        /// <typeparam name="TInput">The type of an item in the list.</typeparam>
        /// <typeparam name="TOutput">The type of the output data for each item.</typeparam>
        /// <param name="input">The original list item.</param>
        /// <returns>Returns the "mapped" value.</returns>
        public delegate TOutput MapPredicateDelegate<TInput, TOutput>(TInput input);

        #endregion


        #region Public API

        /// <summary>
        /// Joins the items in the given collection into a single string using a separator.
        /// </summary>
        /// <param name="enumerable">The collection to pack into a single string.</param>
        /// <param name="separator">The character(s) that separates each elements in the output text.</param>
        /// <returns>Returns the processed string.</returns>
        public static string Join(this IEnumerable enumerable, string separator)
        {
            return string.Join(separator, enumerable);
        }

        /// <param name="mapFunc">The function called for every item to convert it into a string.</param>
        /// <inheritdoc cref="Join(IEnumerable, string)"/>
        public static string Join<T>(this IEnumerable<T> enumerable, string separator, MapPredicateDelegate<T, string> mapFunc)
        {
            return string.Join(separator, Map(enumerable, mapFunc));
        }

        /// <summary>
        /// Creates a new array populated with the results of calling a given function every item in the given collection.
        /// </summary>
        /// <typeparam name="TInput">The type of an item in the input collection.</typeparam>
        /// <typeparam name="TOutput">The type of the output data when calling the action function on each item.</typeparam>
        /// <param name="enumerable">The collection you want to "map".</param>
        /// <param name="mapFunc">The function to call to provide a data for each item.</param>
        /// <returns>Returns the mapped items array.</returns>
        public static TOutput[] Map<TInput, TOutput>(this IEnumerable<TInput> enumerable, MapPredicateDelegate<TInput, TOutput> mapFunc)
        {
            using (var scope = new ListPoolScope<TOutput>())
            {
                foreach (TInput item in enumerable)
                    scope.List.Add(mapFunc(item));

                return scope.List.ToArray();
            }
        }

        /// <summary>
        /// Converts all the items of a given collection into a given type.
        /// </summary>
        /// <typeparam name="T">The type to which you want to convert every items.</typeparam>
        /// <param name="enumerable">The collection you want to convert.</param>
        /// <returns>Returns the converted items array.</returns>
        public static T[] Convert<T>(this IEnumerable<object> enumerable)
        {
            return Map(enumerable, i => (T)i);
        }

        /// <summary>
        /// Filters all the objects from a collection that have a given type.
        /// </summary>
        /// <typeparam name="TSource">The type of the source collection.</typeparam>
        /// <typeparam name="TTarget">The type of the objects to filter.</typeparam>
        /// <param name="source">The source collection.</param>
        /// <param name="includeDerivedTypes">If enabled, this function will filter all the objects that have the expected type OR the
        /// objects that derive from it.</param>
        /// <returns>Returns the filtered elements.</returns>
        public static TTarget[] Filter<TSource, TTarget>(this IEnumerable<TSource> source, bool includeDerivedTypes = false)
            where TTarget : TSource
        {
            using (var scope = new ListPoolScope<TTarget>())
            {
                if (includeDerivedTypes)
                {
                    foreach (TSource i in source)
                    {
                        if (i is TTarget target)
                            scope.List.Add(target);
                    }
                }
                else
                {
                    foreach (TSource i in source)
                    {
                        if (i.GetType() == typeof(TTarget))
                            scope.List.Add((TTarget)i);
                    }
                }

                return scope.List.ToArray();
            }
        }

        /// <summary>
        /// Gets a random element of a given collection.
        /// </summary>
        /// <typeparam name="T">The type of the source collection.</typeparam>
        /// <param name="source">The source collection.</param>
        /// <returns>Returns the picked element, or a default value if the collection is empty.</returns>
        public static T PickRandom<T>(this IEnumerable<T> source)
        {
            using (var list = new ListPoolScope<T>(source))
            {
                return list.Count > 0
                    ? list[Random.Range(0, list.Count)]
                    : default;
            }
        }

        #endregion

    }

}