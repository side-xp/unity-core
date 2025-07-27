using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Represents a list of items bound to a probabiility value, from which those items can be picked randomly by taking account of their
    /// probability.
    /// </summary>
    /// <typeparam name="T">The type of data bound to probability values.</typeparam>
    [System.Serializable]
    public class ProbabilityList<T> : ProbabilityCollection, IList<ProbabilityList<T>.ProbabilityItem>
    {

        #region Subclasses

        /// <summary>
        /// Groups an item of the list and its probability value.
        /// </summary>
        [System.Serializable]
        public class ProbabilityItem : IProbabilityItem<T>
        {

            [SerializeField]
            [Tooltip("The data item itself.")]
            private T _data = default;

            [SerializeField, Range(0, 1)]
            [Tooltip("The probability value assigned to the item.")]
            private float _probability = 1f;

            /// <inheritdoc cref="IProbabilityItem{T}.Data"/>
            public T Data => _data;

            /// <inheritdoc cref="IProbabilityItem.Probability"/>
            public float Probability => _probability;

            /// <inheritdoc cref="IProbabilityItem.Data"/>
            object IProbabilityItem.Data => _data;

            /// <inheritdoc cref="IProbabilityItem.Label"/>
            public string Label => Data != null ? Data.ToString() : string.Empty;

        }

        #endregion


        #region Fields

        [SerializeField]
        [Tooltip("The items in this probability collection.")]
        private List<ProbabilityItem> _items = new List<ProbabilityItem>();

        #endregion


        #region Public API

        /// <inheritdoc cref="ProbabilityCollection.Items"/>
        public override IProbabilityItem[] Items => _items.ToArray();

        /// <inheritdoc cref="ICollection{T}.Count"/>
        public int Count => _items.Count;

        /// <inheritdoc cref="ICollection{T}.IsReadOnly"/>
        public bool IsReadOnly => ((ICollection<ProbabilityItem>)_items).IsReadOnly;

        /// <inheritdoc cref="ICollection{T}[int]"/>
        ProbabilityItem IList<ProbabilityItem>.this[int index]
        {
            get => _items[index];
            set => _items[index] = value;
        }

        /// <inheritdoc cref="ProbabilityCollection[int]"/>
        public override IProbabilityItem this[int index] => _items[index];

        /// <inheritdoc cref="ProbabilityCollection.Get(out object)"/>
        public bool Get(out T data)
        {
            bool success = Get(out object dataObj);
            data = success ? (T)dataObj : default;
            return success;
        }

        /// <inheritdoc cref="ProbabilityCollection.GetProbabilityPercents(object)"/>
        public float GetProbabilityPercents(T data)
        {
            return GetProbabilityPercents((object)data);
        }

        /// <inheritdoc cref="IList{T}.IndexOf(T)"/>
        public int IndexOf(ProbabilityItem item)
        {
            return _items.IndexOf(item);
        }

        /// <inheritdoc cref="IList{T}.Insert(int, T)"/>
        public void Insert(int index, ProbabilityItem item)
        {
            _items.Insert(index, item);
        }

        /// <inheritdoc cref="IList{T}.RemoveAt(int)"/>
        public void RemoveAt(int index)
        {
            _items.RemoveAt(index);
        }

        /// <inheritdoc cref="ICollection{T}.Add(T)"/>
        public void Add(ProbabilityItem item)
        {
            _items.Add(item);
        }

        /// <inheritdoc cref="ICollection{T}.Clear()"/>
        public void Clear()
        {
            _items.Clear();
        }

        /// <inheritdoc cref="ICollection{T}.Contains(T)"/>
        public bool Contains(ProbabilityItem item)
        {
            return _items.Contains(item);
        }

        /// <inheritdoc cref="ICollection{T}.CopyTo(T[], int)"/>
        public void CopyTo(ProbabilityItem[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc cref="ICollection{T}.Remove(T)"/>
        public bool Remove(ProbabilityItem item)
        {
            return _items.Remove(item);
        }

        /// <summary>
        /// Iterates through the items in this collection.
        /// </summary>
        public IEnumerator<ProbabilityItem> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        /// <inheritdoc cref="GetEnumerator()"/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

    }

}