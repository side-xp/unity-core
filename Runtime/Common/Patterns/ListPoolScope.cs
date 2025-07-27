using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Pool;

namespace SideXP.Core
{

    /// <summary>
    /// Gets the instance of a list from <see cref="ListPool{T}"/>, and release it whenever it's no more used.
    /// <example>
    /// <code>
    /// string[] letters = null;
    /// using (var scope = new ListPoolScope{string}())
    /// {
    ///     scope.List.Add("A");
    ///     scope.List.Add("B");
    ///     scope.List.Add("C");
    ///     letters = scope.List.ToArray();
    /// }
    /// 
    /// UnityEngine.Debug.Log(string.Join(", ", letters));
    /// </code>
    /// </example>
    /// </summary>
    /// <typeparam name="T">The type of the elements of the list.</typeparam>
    public class ListPoolScope<T> : IDisposable, IList<T>
    {

        /// <summary>
        /// The recycled or created instance of the list.
        /// </summary>
        private List<T> _list = null;

        /// <summary>
        /// Flag enabled after calling <see cref="Dispose()"/> once.
        /// </summary>
        private bool _disposed = false;

        /// <inheritdoc cref="ListPoolScope{T}"/>
        public ListPoolScope()
        {
            _list = ListPool<T>.Get();
        }

        /// <inheritdoc cref="ListPoolScope{T}"/>
        /// <param name="collection">The initial items in the list </param>
        public ListPoolScope(IEnumerable<T> collection)
            : this()
        {
            _list.AddRange(collection);
        }

        /// <inheritdoc cref="ListPoolScope{T}"/>
        /// <param name="capacity">The initial capacity of the list </param>
        public ListPoolScope(int capacity)
            : this()
        {
            _list.Capacity = capacity;
        }

        /// <inheritdoc cref="_list"/>
        public List<T> List
        {
            get
            {
                if (_disposed)
                    Debug.LogError($"Failed to get the list instance from a {nameof(ListPoolScope<T>)}: That scope has been disposed.");

                return _list;
            }
        }

        /// <inheritdoc cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            if (_disposed)
                return;

            ListPool<T>.Release(_list);
            _list = null;
        }


        #region List implementation

        /// <inheritdoc cref="IList{T}[int]"/>
        public T this[int index]
        {
            get => _list[index];
            set => _list[index] = value;
        }

        /// <inheritdoc cref="ICollection{T}.Count"/>
        public int Count => _list.Count;

        /// <inheritdoc cref="List{T}.Capacity"/>
        public int Capacity { get; set; }

        /// <inheritdoc cref="ICollection{T}.IsReadOnly"/>
        public bool IsReadOnly => ((ICollection<T>)_list).IsReadOnly;

        /// <inheritdoc cref="ICollection{T}.Add(T)"/>
        public void Add(T item)
        {
            _list.Add(item);
        }

        /// <inheritdoc cref="List{T}.AddRange(IEnumerable{T})"/>
        public void AddRange(IEnumerable<T> collection)
        {
            _list.AddRange(collection);
        }

        /// <inheritdoc cref="List{T}.AsReadOnly"/>
        public System.Collections.ObjectModel.ReadOnlyCollection<T> AsReadOnly()
        {
            return _list.AsReadOnly();
        }

        /// <inheritdoc cref="List{T}.BinarySearch(int, int, T, IComparer{T})"/>
        public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
        {
            return _list.BinarySearch(index, count, item, comparer);
        }

        /// <inheritdoc cref="List{T}.BinarySearch(T)"/>
        public int BinarySearch(T item)
        {
            return _list.BinarySearch(item);
        }

        /// <inheritdoc cref="List{T}.BinarySearch(T, IComparer{T})"/>
        public int BinarySearch(T item, IComparer<T> comparer)
        {
            return _list.BinarySearch(item, comparer);
        }

        /// <inheritdoc cref="ICollection{T}.Clear"/>
        public void Clear()
        {
            _list.Clear();
        }

        /// <inheritdoc cref="ICollection{T}.Contains(T)"/>
        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        /// <inheritdoc cref="List{T}.ConvertAll{TOutput}(Converter{T, TOutput})"/>
        public List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
        {
            return _list.ConvertAll(converter);
        }

        /// <inheritdoc cref="ICollection{T}.CopyTo(T[], int)"/>
        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc cref="List{T}.CopyTo(T[])"/>
        public void CopyTo(T[] array)
        {
            _list.CopyTo(array);
        }

        /// <inheritdoc cref="List{T}.CopyTo(int, T[], int, int)"/>
        public void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            _list.CopyTo(index, array, arrayIndex, count);
        }

        /// <inheritdoc cref="ICollection.CopyTo(Array, int)"/>
        public void CopyTo(Array array, int index)
        {
            ((ICollection)_list).CopyTo(array, index);
        }

        /// <inheritdoc cref="List{T}.Exists(Predicate{T})"/>
        public bool Exists(Predicate<T> match)
        {
            return _list.Exists(match);
        }

        /// <inheritdoc cref="List{T}.Find(Predicate{T})"/>
        public T Find(Predicate<T> match)
        {
            return _list.Find(match);
        }

        /// <inheritdoc cref="List{T}.FindAll(Predicate{T})"/>
        public List<T> FindAll(Predicate<T> match)
        {
            return _list.FindAll(match);
        }

        /// <inheritdoc cref="List{T}.FindIndex(int, int, Predicate{T})"/>
        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            return _list.FindIndex(startIndex, count, match);
        }

        /// <inheritdoc cref="List{T}.FindIndex(int, Predicate{T})"/>
        public int FindIndex(int startIndex, Predicate<T> match)
        {
            return _list.FindIndex(startIndex, match);
        }

        /// <inheritdoc cref="List{T}.FindIndex(Predicate{T})"/>
        public int FindIndex(Predicate<T> match)
        {
            return _list.FindIndex(match);
        }

        /// <inheritdoc cref="List{T}.FindLast(Predicate{T})"/>
        public T FindLast(Predicate<T> match)
        {
            return _list.FindLast(match);
        }

        /// <inheritdoc cref="List{T}.FindLastIndex(int, int, Predicate{T})"/>
        public int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            return _list.FindLastIndex(startIndex, count, match);
        }

        /// <inheritdoc cref="List{T}.FindLastIndex(int, Predicate{T})"/>
        public int FindLastIndex(int startIndex, Predicate<T> match)
        {
            return _list.FindLastIndex(startIndex, match);
        }

        /// <inheritdoc cref="List{T}.FindLastIndex(Predicate{T})"/>
        public int FindLastIndex(Predicate<T> match)
        {
            return _list.FindLastIndex(match);
        }

        /// <inheritdoc cref="List{T}.ForEach(Action{T})"/>
        public void ForEach(Action<T> action)
        {
            _list.ForEach(action);
        }

        /// <inheritdoc cref="List{T}.GetRange(int, int))"/>
        public List<T> GetRange(int index, int count)
        {
            return _list.GetRange(index, count);
        }

        /// <inheritdoc cref="List{T}.IndexOf(T, int, int))"/>
        public int IndexOf(T item, int index, int count)
        {
            return _list.IndexOf(item, index, count);
        }

        /// <inheritdoc cref="List{T}.IndexOf(T, int))"/>
        public int IndexOf(T item, int index)
        {
            return _list.IndexOf(item, index);
        }

        /// <inheritdoc cref="IList{T}.IndexOf(T)"/>
        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        /// <inheritdoc cref="IList{T}.Insert(int, T)"/>
        public void Insert(int index, T item)
        {
            _list.Insert(index, item);
        }

        /// <inheritdoc cref="List{T}.InsertRange(int, IEnumerable{T})"/>
        public void InsertRange(int index, IEnumerable<T> collection)
        {
            _list.InsertRange(index, collection);
        }

        /// <inheritdoc cref="List{T}.LastIndexOf(T)"/>
        public int LastIndexOf(T item)
        {
            return _list.LastIndexOf(item);
        }

        /// <inheritdoc cref="List{T}.LastIndexOf(T, int)"/>
        public int LastIndexOf(T item, int index)
        {
            return _list.LastIndexOf(item, index);
        }

        /// <inheritdoc cref="List{T}.LastIndexOf(T, int, int)"/>
        public int LastIndexOf(T item, int index, int count)
        {
            return _list.LastIndexOf(item, index, count);
        }

        /// <inheritdoc cref="ICollection{T}.Remove(T)"/>
        public bool Remove(T item)
        {
            return _list.Remove(item);
        }

        /// <inheritdoc cref="List{T}.RemoveAll(Predicate{T})"/>
        public int RemoveAll(Predicate<T> match)
        {
            return _list.RemoveAll(match);
        }

        /// <inheritdoc cref="IList.RemoveAt(int)"/>
        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        /// <inheritdoc cref="List{T}.RemoveRange(int, int)"/>
        public void RemoveRange(int index, int count)
        {
            _list.RemoveRange(index, count);
        }

        /// <inheritdoc cref="List{T}.Reverse(int, int)"/>
        public void Reverse(int index, int count)
        {
            _list.Reverse(index, count);
        }

        /// <inheritdoc cref="List{T}.Reverse()"/>
        public void Reverse()
        {
            _list.Reverse();
        }

        /// <inheritdoc cref="List{T}.Sort(Comparison{T})"/>
        public void Sort(Comparison<T> comparison)
        {
            _list.Sort(comparison);
        }

        /// <inheritdoc cref="List{T}.Sort(int, int, IComparer{T})"/>
        public void Sort(int index, int count, IComparer<T> comparer)
        {
            _list.Sort(index, count, comparer);
        }

        /// <inheritdoc cref="List{T}.Sort()"/>
        public void Sort()
        {
            _list.Sort();
        }

        /// <inheritdoc cref="List{T}.Sort(IComparer{T})"/>
        public void Sort(IComparer<T> comparer)
        {
            _list.Sort(comparer);
        }

        /// <inheritdoc cref="List{T}.ToArray"/>
        public T[] ToArray()
        {
            return _list.ToArray();
        }

        /// <inheritdoc cref="List{T}.TrimExcess"/>
        public void TrimExcess()
        {
            _list.TrimExcess();
        }

        /// <inheritdoc cref="List{T}.TrueForAll(Predicate{T})"/>
        public bool TrueForAll(Predicate<T> match)
        {
            return _list.TrueForAll(match);
        }

        /// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)_list).GetEnumerator();
        }

        /// <inheritdoc cref="GetEnumerator"/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_list).GetEnumerator();
        }

        #endregion

    }

}
