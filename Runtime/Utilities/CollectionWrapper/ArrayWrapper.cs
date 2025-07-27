using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Wraps an array in a container. This is useful to create custom property drawers for an entire array instead of each of its entries.
    /// </summary>
    [System.Serializable]
    public class ArrayWrapper<T> : ICollectionWrapper<T>
    {

        [SerializeField]
        [Tooltip("Elements in this collection.")]
        private T[] _elements = { };

        /// <inheritdoc cref="_elements"/>
        public T[] Elements
        {
            get => _elements;
            set => _elements = value;
        }

        /// <summary>
        /// Iterates through items in this collection.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)_elements).GetEnumerator();
        }

        /// <inheritdoc cref="GetEnumerator"/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _elements.GetEnumerator();
        }

    }

}