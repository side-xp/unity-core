using System.Collections.Generic;

using NUnit.Framework;

namespace SideXP.Core.Tests
{

    public class PaginationTests
    {

        #region Constructors

        [Test]
        public void Constructor_PageOnly_UsesDefaultsForRest()
        {
            var pagination = new Pagination(2);
            Assert.AreEqual(Pagination.DefaultElementsCountPerPage, pagination.NbElementsPerPage);
            Assert.AreEqual(0, pagination.ElementsCount);
            // With no elements there are 0 pages, so the Page getter clamps the stored index (2) down to 0.
            Assert.AreEqual(0, pagination.Page);
        }

        [Test]
        public void Constructor_PageAndPerPage_DefaultsElementsCountToZero()
        {
            var pagination = new Pagination(0, 10);
            Assert.AreEqual(10, pagination.NbElementsPerPage);
            Assert.AreEqual(0, pagination.ElementsCount);
        }

        [Test]
        public void Constructor_AllArgs_SetsEveryField()
        {
            var pagination = new Pagination(1, 10, 35);
            Assert.AreEqual(1, pagination.Page);
            Assert.AreEqual(10, pagination.NbElementsPerPage);
            Assert.AreEqual(35, pagination.ElementsCount);
        }

        #endregion


        #region Page clamping

        [Test]
        public void Page_ClampsToValidRange()
        {
            // 10 elements, 3 per page => 4 pages (indices 0..3).
            var pagination = new Pagination(0, 3, 10);

            pagination.Page = 100;
            Assert.AreEqual(3, pagination.Page, "Page above the last index should clamp to the last page.");

            pagination.Page = -5;
            Assert.AreEqual(0, pagination.Page, "Negative page should clamp to 0.");
        }

        [Test]
        public void Page_WithNoElements_ClampsToZero()
        {
            var pagination = new Pagination(5, 10, 0);
            Assert.AreEqual(0, pagination.Page);
        }

        #endregion


        #region PagesCount

        [Test]
        public void PagesCount_RoundsUp()
        {
            Assert.AreEqual(4, new Pagination(0, 3, 10).PagesCount, "10 / 3 rounds up to 4.");
            Assert.AreEqual(1, new Pagination(0, 25, 25).PagesCount, "An exact fill is a single page.");
            Assert.AreEqual(2, new Pagination(0, 25, 26).PagesCount, "One extra element spills onto a second page.");
        }

        [Test]
        public void PagesCount_WithNoElements_IsZero()
        {
            Assert.AreEqual(0, new Pagination(0, 25, 0).PagesCount);
        }

        [Test]
        public void PagesCount_WithNonPositivePerPage_UsesClampedValue()
        {
            // PagesCount routes through the clamped NbElementsPerPage (min 1), so a per-page count of 0
            // does not divide by zero — it behaves as 1 element per page.
            var pagination = new Pagination(0, 5, 10);
            pagination.NbElementsPerPage = 0;
            Assert.AreEqual(10, pagination.PagesCount);
        }

        #endregion


        #region NbElementsPerPage

        [Test]
        public void NbElementsPerPage_ClampsToAtLeastOne()
        {
            var pagination = new Pagination(0, 5, 10);

            pagination.NbElementsPerPage = 0;
            Assert.AreEqual(1, pagination.NbElementsPerPage);

            pagination.NbElementsPerPage = -3;
            Assert.AreEqual(1, pagination.NbElementsPerPage);

            pagination.NbElementsPerPage = 8;
            Assert.AreEqual(8, pagination.NbElementsPerPage);
        }

        #endregion


        #region First / Last index

        [Test]
        public void FirstAndLastIndex_FirstPage()
        {
            var pagination = new Pagination(0, 3, 10);
            Assert.AreEqual(0, pagination.FirstIndex);
            Assert.AreEqual(3, pagination.LastIndex);
        }

        [Test]
        public void FirstAndLastIndex_MiddlePage()
        {
            var pagination = new Pagination(1, 3, 10);
            Assert.AreEqual(3, pagination.FirstIndex);
            Assert.AreEqual(6, pagination.LastIndex);
        }

        [Test]
        public void LastIndex_OnPartialLastPage_ClampsToElementsCount()
        {
            // Page 3 of a 10-element set with 3 per page contains only index 9.
            var pagination = new Pagination(3, 3, 10);
            Assert.AreEqual(9, pagination.FirstIndex);
            Assert.AreEqual(10, pagination.LastIndex);
        }

        #endregion


        #region Paginate (instance)

        [Test]
        public void Paginate_Instance_ReturnsPageSlice()
        {
            var list = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var pagination = new Pagination(1, 3);

            int[] page = pagination.Paginate(list);

            Assert.AreEqual(new[] { 3, 4, 5 }, page);
            Assert.AreEqual(10, pagination.ElementsCount, "Paginate should sync ElementsCount with the list size.");
        }

        [Test]
        public void Paginate_Instance_LastPartialPage()
        {
            var list = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var pagination = new Pagination(3, 3);
            Assert.AreEqual(new[] { 9 }, pagination.Paginate(list));
        }

        [Test]
        public void Paginate_Instance_PageBeyondRange_ClampsToLastPage()
        {
            var list = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var pagination = new Pagination(100, 3);
            Assert.AreEqual(new[] { 9 }, pagination.Paginate(list));
        }

        #endregion


        #region Paginate (static)

        [Test]
        public void Paginate_Static_ReturnsPageSlice()
        {
            var list = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            // Regression guard: the static overloads previously passed constructor args in the wrong order,
            // scrambling the page index and elements-per-page.
            int[] page = Pagination.Paginate(list, 1, 3);
            Assert.AreEqual(new[] { 3, 4, 5 }, page);
        }

        [Test]
        public void Paginate_Static_OutPagination_ReportsState()
        {
            var list = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            int[] page = Pagination.Paginate(list, out Pagination pagination, 1, 3);

            Assert.AreEqual(new[] { 3, 4, 5 }, page);
            Assert.AreEqual(1, pagination.Page);
            Assert.AreEqual(3, pagination.NbElementsPerPage);
            Assert.AreEqual(10, pagination.ElementsCount);
            Assert.AreEqual(4, pagination.PagesCount);
        }

        #endregion


        #region CountPages

        [Test]
        public void CountPages_FromCount_RoundsUp()
        {
            Assert.AreEqual(4, Pagination.CountPages(10, 3));
            Assert.AreEqual(0, Pagination.CountPages(0, 25));
            Assert.AreEqual(1, Pagination.CountPages(25, 25));
            Assert.AreEqual(2, Pagination.CountPages(26, 25));
        }

        [Test]
        public void CountPages_FromList_MatchesCountOverload()
        {
            var list = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            Assert.AreEqual(Pagination.CountPages(list.Count, 3), Pagination.CountPages(list, 3));
        }

        #endregion


        #region Operators

        [Test]
        public void IncrementOperator_AdvancesPage()
        {
            var pagination = new Pagination(0, 3, 10);
            pagination++;
            Assert.AreEqual(1, pagination.Page);
        }

        [Test]
        public void DecrementOperator_GoesBackPage()
        {
            var pagination = new Pagination(2, 3, 10);
            pagination--;
            Assert.AreEqual(1, pagination.Page);
        }

        [Test]
        public void AddOperator_ClampsToLastPage()
        {
            var pagination = new Pagination(0, 3, 10); // 4 pages (0..3)
            pagination += 100;
            Assert.AreEqual(3, pagination.Page);
        }

        [Test]
        public void SubtractOperator_ClampsToFirstPage()
        {
            var pagination = new Pagination(2, 3, 10);
            pagination -= 100;
            Assert.AreEqual(0, pagination.Page);
        }

        [Test]
        public void EqualityOperators_CompareByValue()
        {
            var a = new Pagination(1, 3, 10);
            var b = new Pagination(1, 3, 10);
            var c = new Pagination(2, 3, 10);

            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);
            Assert.IsTrue(a != c);
            Assert.IsFalse(a == c);
        }

        [Test]
        public void Inequality_IsTrue_WhenPerPageDiffers()
        {
            // Regression guard: operator != previously compared NbElementsPerPage with == .
            var a = new Pagination(0, 3, 10);
            var b = new Pagination(0, 5, 10);
            Assert.IsTrue(a != b);
            Assert.IsFalse(a == b);
        }

        [Test]
        public void Equals_SameValue_IsTrue()
        {
            var a = new Pagination(1, 3, 10);
            object b = new Pagination(1, 3, 10);
            Assert.IsTrue(a.Equals(b));
        }

        [Test]
        public void Equals_DifferentType_IsFalse()
        {
            // Regression guard: the inverted type check used to throw an InvalidCastException here.
            var a = new Pagination(1, 3, 10);
            Assert.IsFalse(a.Equals(42));
            Assert.IsFalse(a.Equals(null));
        }

        [Test]
        public void GetHashCode_EqualValues_MatchingHashes()
        {
            var a = new Pagination(1, 3, 10);
            var b = new Pagination(1, 3, 10);
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }

        #endregion

    }

}
