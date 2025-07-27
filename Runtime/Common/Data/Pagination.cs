using System.Collections.Generic;

using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Utility class for pagination system.
    /// </summary>
    [System.Serializable]
    public struct Pagination
    {

        #region Fields

        public const int DefaultElementsCountPerPage = 25;

        [SerializeField]
        [Tooltip("The index of the current page (starting at 0).")]
        private int _pageIndex;

        [SerializeField]
        [Tooltip("The number of elements per page.")]
        private int _nbElementsPerPage;

        /// <summary>
        /// The total number of elements.
        /// </summary>
        private int _elementsCount;

        #endregion


        #region Lifecycle

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="page">The current page index.</param>
        public Pagination(int page)
        {
            _pageIndex = page;
            _nbElementsPerPage = DefaultElementsCountPerPage;
            _elementsCount = 0;
        }

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="page">The current page index.</param>
        /// <param name="nbElementsPerPage">The number of elements per page.</param>
        public Pagination(int page, int nbElementsPerPage)
        {
            _pageIndex = page;
            _nbElementsPerPage = nbElementsPerPage;
            _elementsCount = 0;
        }

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="page">The current page index.</param>
        /// <param name="nbElementsPerPage">The number of elements displayed per page.</param>
        /// <param name="nbElements">The number of elements in your paginated ensemble.</param>
        public Pagination(int page, int nbElementsPerPage, int nbElements)
        {
            _pageIndex = page;
            _nbElementsPerPage = nbElementsPerPage;
            _elementsCount = nbElements;
        }

        #endregion


        #region Public API

        /// <summary>
        /// Gets/sets the current page index (starting at 0).
        /// </summary>
        public int Page
        {
            get => Mathf.Clamp(_pageIndex, 0, Mathf.Max(0, PagesCount - 1));
            set => _pageIndex = value;
        }

        /// <summary>
        /// Gets the number of pages.
        /// </summary>
        public int PagesCount => CountPages(_elementsCount, _nbElementsPerPage);

        /// <summary>
        /// Gets/sets the total number of elements.
        /// </summary>
        public int ElementsCount
        {
            get => _elementsCount;
            set => _elementsCount = value;
        }

        /// <summary>
        /// Gets/sets the number of elements per page.
        /// </summary>
        public int NbElementsPerPage
        {
            get => Mathf.Max(1, _nbElementsPerPage);
            set => _nbElementsPerPage = value;
        }

        /// <summary>
        /// Gets the first index (inclusive) in the paginated range.
        /// </summary>
        public int FirstIndex => Page * NbElementsPerPage;

        /// <summary>
        /// Gets the last index (exclusive) in the paginated range.
        /// </summary>
        public int LastIndex => Mathf.Min(FirstIndex + NbElementsPerPage, ElementsCount);

        /// <inheritdoc cref="Paginate{T}(IList{T})"/>
        /// <param name="page">The current page index.</param>
        /// <param name="nbElementsPerPage">The number of elements per page.</param>
        public static T[] Paginate<T>(IList<T> list, int page, int nbElementsPerPage = DefaultElementsCountPerPage)
        {
            Pagination pagination = new Pagination(list.Count, page, nbElementsPerPage);
            return pagination.Paginate(list);
        }

        /// <inheritdoc cref="Paginate{T}(IList{T}, int, int)"/>
        /// <param name="pagination">Outputs the <see cref="Pagination"/> infos of the operation.</param>
        public static T[] Paginate<T>(IList<T> list, out Pagination pagination, int page, int nbElementsPerPage = DefaultElementsCountPerPage)
        {
            pagination = new Pagination(list.Count, page, nbElementsPerPage);
            return pagination.Paginate(list);
        }

        /// <summary>
        /// Creates a sub-list of the given one that contains only the elements in the paginated range.
        /// </summary>
        /// <typeparam name="T">The type of elements in the given list.</typeparam>
        /// <param name="list">The list that is paginated.</param>
        /// <returns>Returns the sub-list of the elements to display.</returns>
        public T[] Paginate<T>(IList<T> list)
        {
            ElementsCount = list.Count;
            List<T> subList = new List<T>();
            for (int i = FirstIndex; i < LastIndex; i++)
                subList.Add(list[i]);
            return subList.ToArray();
        }

        /// <summary>
        /// Computes the number of pages, given the total number of elements and the number of elements per page.
        /// </summary>
        /// <typeparam name="T">The types of elements in the given list.</typeparam>
        /// <param name="list">The list that is paginated.</param>
        /// <param name="nbElementsPerPage">The number of elements displayed per page.</param>
        /// <returns>Returns the computed number of pages.</returns>
        public static int CountPages<T>(IList<T> list, int nbElementsPerPage = DefaultElementsCountPerPage)
        {
            return Mathf.CeilToInt(list.Count / (float)nbElementsPerPage);
        }

        /// <inheritdoc cref="CountPages{T}(IList{T}, int)"/>
        /// <param name="elementsCount">The total number of elements.</param>
        public static int CountPages(int elementsCount, int nbElementsPerPage = DefaultElementsCountPerPage)
        {
            return Mathf.CeilToInt(elementsCount / (float)nbElementsPerPage);
        }

        #endregion


        #region Operators

        /// <summary>
        /// Checks if the given <see cref="Pagination"/> values are equal.
        /// </summary>
        public static bool operator ==(Pagination a, Pagination b)
        {
            return
                a.Page == b.Page &&
                a.ElementsCount == b.ElementsCount &&
                a.NbElementsPerPage == b.NbElementsPerPage;
        }

        /// <summary>
        /// Checks if the given <see cref="Pagination"/> are different.
        /// </summary>
        public static bool operator !=(Pagination a, Pagination b)
        {
            return
                a.Page != b.Page ||
                a.ElementsCount != b.ElementsCount ||
                a.NbElementsPerPage == b.NbElementsPerPage;
        }

        /// <summary>
        /// Increment the current page index.
        /// </summary>
        public static Pagination operator ++(Pagination pagination)
        {
            pagination.Page++;
            return pagination;
        }

        /// <summary>
        /// Decrement the current page index.
        /// </summary>
        public static Pagination operator --(Pagination pagination)
        {
            pagination.Page--;
            return pagination;
        }

        /// <summary>
        /// Increment the current page index by the given value.
        /// </summary>
        public static Pagination operator +(Pagination pagination, int nbPagesNext)
        {
            pagination.Page += nbPagesNext;
            return pagination;
        }

        /// <summary>
        /// Decrement the current page index by the given value.
        /// </summary>
        public static Pagination operator -(Pagination pagination, int nbPagesPrevious)
        {
            pagination.Page -= nbPagesPrevious;
            return pagination;
        }

        /// <summary>
        /// Checks if the given object is equal to this <see cref="Pagination"/> object.
        /// </summary>
        public override bool Equals(object other)
        {
            return
                other != null &&
                !GetType().Equals(other.GetType()) &&
                this == (Pagination)other;
        }

        /// <summary>
        /// Gets the hash code of this <see cref="Pagination"/> object.
        /// </summary>
        public override int GetHashCode()
        {
            return Page ^ ElementsCount ^ NbElementsPerPage;
        }

        /// <summary>
        /// Converts this <see cref="Pagination"/> object into a string.
        /// </summary>
        public override string ToString()
        {
            return $"Pagination (page: {Page}/{PagesCount}, elements: {ElementsCount}, elements per page: {NbElementsPerPage})";
        }

        #endregion

    }

}