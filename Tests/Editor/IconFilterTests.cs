using System.Collections.Generic;

using NUnit.Framework;

using SideXP.Core.EditorOnly;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="IconFilter"/>.
    /// </summary>
    public class IconFilterTests
    {

        /// <summary>
        /// The icons to filter: <c>home</c> (filled), <c>arrow-left</c> and <c>git-branch</c>.
        /// </summary>
        private static readonly IconInfo[] Icons =
        {
            new IconInfo(new IconIndexEntry
            {
                name = "home",
                category = "Buildings",
                tags = new[] { "house", "Dashboard" },
                hasFilled = true,
            }),
            new IconInfo(new IconIndexEntry { name = "arrow-left", category = "Arrows", tags = new[] { "back", "previous" } }),
            new IconInfo(new IconIndexEntry { name = "git-branch", category = "Version control", tags = new[] { "code" } }),
        };

        /// <summary>
        /// Filters the test icons, and gets the names of the matching ones.
        /// </summary>
        /// <param name="search">The search text.</param>
        /// <param name="category">The category of the icons to keep, or null for all categories.</param>
        /// <param name="isFilledOnly">If enabled, only keeps the icons that exist in the filled style.</param>
        /// <returns>Returns the names of the matching icons.</returns>
        private static List<string> FilterNames(string search, string category = null, bool isFilledOnly = false)
        {
            return IconFilter.Filter(Icons, search, category, isFilledOnly).ConvertAll(icon => icon.Name);
        }

        /// <summary>
        /// An empty search keeps every icon, in order.
        /// </summary>
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Filter_EmptySearch_KeepsEveryIcon(string search)
        {
            CollectionAssert.AreEqual(new[] { "home", "arrow-left", "git-branch" }, FilterNames(search));
        }

        /// <summary>
        /// Every word of the search must be found in the name or a tag, ignoring case.
        /// </summary>
        [TestCase("home", new[] { "home" })]
        [TestCase("HOUSE", new[] { "home" })]
        [TestCase("dash", new[] { "home" })]
        [TestCase("arrow left", new[] { "arrow-left" })]
        [TestCase("arrow  back", new[] { "arrow-left" })]
        [TestCase("arrow right", new string[0])]
        [TestCase("-", new[] { "arrow-left", "git-branch" })]
        public void Filter_Search_MatchesNameAndTags(string search, string[] expected)
        {
            CollectionAssert.AreEqual(expected, FilterNames(search));
        }

        /// <summary>
        /// Only the icons of the selected category are kept.
        /// </summary>
        [Test]
        public void Filter_Category_KeepsThatCategoryOnly()
        {
            CollectionAssert.AreEqual(new[] { "git-branch" }, FilterNames(null, "Version control"));
            Assert.IsEmpty(FilterNames("home", "Arrows"));
        }

        /// <summary>
        /// Only the icons with a filled style are kept when filtering filled icons.
        /// </summary>
        [Test]
        public void Filter_FilledOnly_KeepsFilledIcons()
        {
            CollectionAssert.AreEqual(new[] { "home" }, FilterNames(null, null, true));
        }

        /// <summary>
        /// Search words are split on spaces, without empty words.
        /// </summary>
        [Test]
        public void GetSearchWords_SplitsOnSpaces()
        {
            CollectionAssert.AreEqual(new[] { "arrow", "left" }, IconFilter.GetSearchWords("  arrow \t left "));
            Assert.IsEmpty(IconFilter.GetSearchWords(null));
        }

    }

}
