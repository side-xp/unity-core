using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Represents a list that can be filled by assets created from the inspector and attached to the main asset.<br/>
    /// This type of property can only be used in <see cref="ScriptableObject"/> implementations.
    /// </summary>
    /// <remarks>This class implements <see cref="IList{T}"/>, so you can use it just like you would use a native <see cref="List{T}"/>
    /// instance.</remarks>
    /// <typeparam name="T">The type of the subassets contained in this list.</typeparam>
    [System.Serializable]
    public sealed class SubassetsList<T> : SubassetsListBase, IList<T>
        where T : ScriptableObject
    {

        #region Fields

        [SerializeField]
        [Tooltip("The subassets referenced in this list.")]
        private List<T> _subassetsList = new List<T>();

        #endregion


        #region Lifecycle

        /// <inheritdoc cref="SubassetsList{T}"/>
        public SubassetsList() { }

        /// <inheritdoc cref="SubassetsList{T}"/>
        /// <param name="collection">The collection whose elements are copied to the new list.</param>
        public SubassetsList(IEnumerable<T> collection)
        {
            _subassetsList = new List<T>(collection);
        }
        /// <inheritdoc cref="SubassetsList{T}"/>
        /// <param name="capacity">The number of elements that the new list can initially store.</param>
        public SubassetsList(int capacity)
        {
            _subassetsList = new List<T>(capacity);
        }

        #endregion


        #region Public API

        /// <summary>
        /// Gets the type of the subassets in this list.
        /// </summary>
        public Type SubassetsType => typeof(T);

        #endregion


        #region List Implementation

        /// <inheritdoc cref="IList{T}[int]"/>
        public T this[int index]
        {
            get => _subassetsList[index];
            set => _subassetsList[index] = value;
        }

        /// <inheritdoc cref="ICollection{T}.Count"/>
        public int Count => _subassetsList.Count;

        /// <inheritdoc cref="List{T}.Capacity"/>
        public int Capacity { get; set; }

        /// <inheritdoc cref="ICollection{T}.IsReadOnly"/>
        public bool IsReadOnly => ((ICollection<T>)_subassetsList).IsReadOnly;

        /// <inheritdoc cref="ICollection{T}.Add(T)"/>
        public void Add(T item)
        {
            _subassetsList.Add(item);
        }

        /// <inheritdoc cref="List{T}.AddRange(IEnumerable{T})"/>
        public void AddRange(IEnumerable<T> collection)
        {
            _subassetsList.AddRange(collection);
        }

        /// <inheritdoc cref="List{T}.AsReadOnly"/>
        public System.Collections.ObjectModel.ReadOnlyCollection<T> AsReadOnly()
        {
            return _subassetsList.AsReadOnly();
        }

        /// <inheritdoc cref="List{T}.BinarySearch(int, int, T, IComparer{T})"/>
        public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
        {
            return _subassetsList.BinarySearch(index, count, item, comparer);
        }

        /// <inheritdoc cref="List{T}.BinarySearch(T)"/>
        public int BinarySearch(T item)
        {
            return _subassetsList.BinarySearch(item);
        }

        /// <inheritdoc cref="List{T}.BinarySearch(T, IComparer{T})"/>
        public int BinarySearch(T item, IComparer<T> comparer)
        {
            return _subassetsList.BinarySearch(item, comparer);
        }

        /// <inheritdoc cref="ICollection{T}.Clear"/>
        public void Clear()
        {
            _subassetsList.Clear();
        }

        /// <inheritdoc cref="ICollection{T}.Contains(T)"/>
        public bool Contains(T item)
        {
            return _subassetsList.Contains(item);
        }

        /// <inheritdoc cref="List{T}.ConvertAll{TOutput}(Converter{T, TOutput})"/>
        public List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
        {
            return _subassetsList.ConvertAll(converter);
        }

        /// <inheritdoc cref="ICollection{T}.CopyTo(T[], int)"/>
        public void CopyTo(T[] array, int arrayIndex)
        {
            _subassetsList.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc cref="List{T}.CopyTo(T[])"/>
        public void CopyTo(T[] array)
        {
            _subassetsList.CopyTo(array);
        }

        /// <inheritdoc cref="List{T}.CopyTo(int, T[], int, int)"/>
        public void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            _subassetsList.CopyTo(index, array, arrayIndex, count);
        }

        /// <inheritdoc cref="ICollection.CopyTo(Array, int)"/>
        public void CopyTo(Array array, int index)
        {
            ((ICollection)_subassetsList).CopyTo(array, index);
        }

        /// <inheritdoc cref="List{T}.Exists(Predicate{T})"/>
        public bool Exists(Predicate<T> match)
        {
            return _subassetsList.Exists(match);
        }

        /// <inheritdoc cref="List{T}.Find(Predicate{T})"/>
        public T Find(Predicate<T> match)
        {
            return _subassetsList.Find(match);
        }

        /// <inheritdoc cref="List{T}.FindAll(Predicate{T})"/>
        public List<T> FindAll(Predicate<T> match)
        {
            return _subassetsList.FindAll(match);
        }

        /// <inheritdoc cref="List{T}.FindIndex(int, int, Predicate{T})"/>
        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            return _subassetsList.FindIndex(startIndex, count, match);
        }

        /// <inheritdoc cref="List{T}.FindIndex(int, Predicate{T})"/>
        public int FindIndex(int startIndex, Predicate<T> match)
        {
            return _subassetsList.FindIndex(startIndex, match);
        }

        /// <inheritdoc cref="List{T}.FindIndex(Predicate{T})"/>
        public int FindIndex(Predicate<T> match)
        {
            return _subassetsList.FindIndex(match);
        }

        /// <inheritdoc cref="List{T}.FindLast(Predicate{T})"/>
        public T FindLast(Predicate<T> match)
        {
            return _subassetsList.FindLast(match);
        }

        /// <inheritdoc cref="List{T}.FindLastIndex(int, int, Predicate{T})"/>
        public int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            return _subassetsList.FindLastIndex(startIndex, count, match);
        }

        /// <inheritdoc cref="List{T}.FindLastIndex(int, Predicate{T})"/>
        public int FindLastIndex(int startIndex, Predicate<T> match)
        {
            return _subassetsList.FindLastIndex(startIndex, match);
        }

        /// <inheritdoc cref="List{T}.FindLastIndex(Predicate{T})"/>
        public int FindLastIndex(Predicate<T> match)
        {
            return _subassetsList.FindLastIndex(match);
        }

        /// <inheritdoc cref="List{T}.ForEach(Action{T})"/>
        public void ForEach(Action<T> action)
        {
            _subassetsList.ForEach(action);
        }

        /// <inheritdoc cref="List{T}.GetRange(int, int))"/>
        public List<T> GetRange(int index, int count)
        {
            return _subassetsList.GetRange(index, count);
        }

        /// <inheritdoc cref="List{T}.IndexOf(T, int, int))"/>
        public int IndexOf(T item, int index, int count)
        {
            return _subassetsList.IndexOf(item, index, count);
        }

        /// <inheritdoc cref="List{T}.IndexOf(T, int))"/>
        public int IndexOf(T item, int index)
        {
            return _subassetsList.IndexOf(item, index);
        }

        /// <inheritdoc cref="IList{T}.IndexOf(T)"/>
        public int IndexOf(T item)
        {
            return _subassetsList.IndexOf(item);
        }

        /// <inheritdoc cref="IList{T}.Insert(int, T)"/>
        public void Insert(int index, T item)
        {
            _subassetsList.Insert(index, item);
        }

        /// <inheritdoc cref="List{T}.InsertRange(int, IEnumerable{T})"/>
        public void InsertRange(int index, IEnumerable<T> collection)
        {
            _subassetsList.InsertRange(index, collection);
        }

        /// <inheritdoc cref="List{T}.LastIndexOf(T)"/>
        public int LastIndexOf(T item)
        {
            return _subassetsList.LastIndexOf(item);
        }

        /// <inheritdoc cref="List{T}.LastIndexOf(T, int)"/>
        public int LastIndexOf(T item, int index)
        {
            return _subassetsList.LastIndexOf(item, index);
        }

        /// <inheritdoc cref="List{T}.LastIndexOf(T, int, int)"/>
        public int LastIndexOf(T item, int index, int count)
        {
            return _subassetsList.LastIndexOf(item, index, count);
        }

        /// <inheritdoc cref="ICollection{T}.Remove(T)"/>
        public bool Remove(T item)
        {
            return _subassetsList.Remove(item);
        }

        /// <inheritdoc cref="List{T}.RemoveAll(Predicate{T})"/>
        public int RemoveAll(Predicate<T> match)
        {
            return _subassetsList.RemoveAll(match);
        }

        /// <inheritdoc cref="IList.RemoveAt(int)"/>
        public void RemoveAt(int index)
        {
            _subassetsList.RemoveAt(index);
        }

        /// <inheritdoc cref="List{T}.RemoveRange(int, int)"/>
        public void RemoveRange(int index, int count)
        {
            _subassetsList.RemoveRange(index, count);
        }

        /// <inheritdoc cref="List{T}.Reverse(int, int)"/>
        public void Reverse(int index, int count)
        {
            _subassetsList.Reverse(index, count);
        }

        /// <inheritdoc cref="List{T}.Reverse()"/>
        public void Reverse()
        {
            _subassetsList.Reverse();
        }

        /// <inheritdoc cref="List{T}.Sort(Comparison{T})"/>
        public void Sort(Comparison<T> comparison)
        {
            _subassetsList.Sort(comparison);
        }

        /// <inheritdoc cref="List{T}.Sort(int, int, IComparer{T})"/>
        public void Sort(int index, int count, IComparer<T> comparer)
        {
            _subassetsList.Sort(index, count, comparer);
        }

        /// <inheritdoc cref="List{T}.Sort()"/>
        public void Sort()
        {
            _subassetsList.Sort();
        }

        /// <inheritdoc cref="List{T}.Sort(IComparer{T})"/>
        public void Sort(IComparer<T> comparer)
        {
            _subassetsList.Sort(comparer);
        }

        /// <inheritdoc cref="List{T}.ToArray"/>
        public T[] ToArray()
        {
            return _subassetsList.ToArray();
        }

        /// <inheritdoc cref="List{T}.TrimExcess"/>
        public void TrimExcess()
        {
            _subassetsList.TrimExcess();
        }

        /// <inheritdoc cref="List{T}.TrueForAll(Predicate{T})"/>
        public bool TrueForAll(Predicate<T> match)
        {
            return _subassetsList.TrueForAll(match);
        }

        /// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)_subassetsList).GetEnumerator();
        }

        /// <inheritdoc cref="GetEnumerator"/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_subassetsList).GetEnumerator();
        }

        #endregion

    }

}