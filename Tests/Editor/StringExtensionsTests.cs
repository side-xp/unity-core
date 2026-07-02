using System;

using NUnit.Framework;

namespace SideXP.Core.Tests
{

    public class StringExtensionsTests
    {

        #region Split

        [Test]
        public void Split_SingleCharSeparator_SplitsParts()
        {
            CollectionAssert.AreEqual(new[] { "a", "b", "c" }, "a,b,c".Split(","));
        }

        [Test]
        public void Split_MultiCharSeparator_SplitsParts()
        {
            CollectionAssert.AreEqual(new[] { "a", "b", "c" }, "a-->b-->c".Split("-->"));
        }

        [Test]
        public void Split_KeepsEmptyEntriesByDefault()
        {
            // The default options are StringSplitOptions.None (matching the built-in method), so empties are kept.
            CollectionAssert.AreEqual(new[] { "a", "", "b" }, "a,,b".Split(","));
        }

        [Test]
        public void Split_RemovesEmptyEntries_WhenRequested()
        {
            CollectionAssert.AreEqual(new[] { "a", "b" }, "a,,b".Split(",", StringSplitOptions.RemoveEmptyEntries));
        }

        #endregion


        #region SplitLines

        [Test]
        public void SplitLines_SplitsOnBothNewlineStyles()
        {
            CollectionAssert.AreEqual(new[] { "a", "b", "c" }, "a\nb\r\nc".SplitLines());
        }

        [Test]
        public void SplitLines_RemovesEmptyLinesByDefault()
        {
            CollectionAssert.AreEqual(new[] { "a", "b" }, "a\n\nb".SplitLines());
        }

        #endregion


        #region Repeat

        [TestCase("ab", 3, "ababab")]
        [TestCase("x", 1, "x")]
        public void Repeat_PositiveIterations_RepeatsString(string str, int iterations, string expected)
        {
            Assert.AreEqual(expected, str.Repeat(iterations));
        }

        [TestCase("x", 0)]
        [TestCase("x", -2)]
        [TestCase("", 5)]
        public void Repeat_NonPositiveOrEmpty_ReturnsEmpty(string str, int iterations)
        {
            Assert.AreEqual(string.Empty, str.Repeat(iterations));
        }

        #endregion


        #region Occurrences

        [TestCase("banana", "a", 3)]
        [TestCase("hello", "l", 2)]
        [TestCase("hello", "z", 0)]
        public void Occurrences_CountsMatches(string str, string pattern, int expected)
        {
            Assert.AreEqual(expected, str.Occurrences(pattern));
        }

        [Test]
        public void Occurrences_IsNonOverlapping()
        {
            Assert.AreEqual(2, "aaaa".Occurrences("aa"));
        }

        [Test]
        public void Occurrences_TreatsPatternLiterally_NotAsRegex()
        {
            // A literal dot should match only actual dots, not "any character".
            Assert.AreEqual(2, "a.b.c".Occurrences("."));
        }

        [Test]
        public void Occurrences_RegexSpecialChars_DoNotThrow()
        {
            // A lone "(" is an invalid regex; literal matching must not throw.
            Assert.AreEqual(1, "a(b)c".Occurrences("("));
        }

        [TestCase("", "a")]
        [TestCase("abc", "")]
        [TestCase(null, "a")]
        public void Occurrences_EmptyOrNullInput_ReturnsZero(string str, string pattern)
        {
            Assert.AreEqual(0, str.Occurrences(pattern));
        }

        #endregion


        #region Slice

        [Test]
        public void Slice_StartOnly_ReturnsToEnd()
        {
            Assert.AreEqual("llo", "hello".Slice(2));
        }

        [Test]
        public void Slice_StartAndEnd_ReturnsRange()
        {
            Assert.AreEqual("el", "hello".Slice(1, 3));
        }

        [Test]
        public void Slice_NegativeStart_CountsFromEnd()
        {
            Assert.AreEqual("lo", "hello".Slice(-2));
        }

        [Test]
        public void Slice_NegativeEnd_CountsFromEnd()
        {
            Assert.AreEqual("hell", "hello".Slice(0, -1));
        }

        [Test]
        public void Slice_StartGreaterThanEnd_SwapsBounds()
        {
            Assert.AreEqual("el", "hello".Slice(3, 1));
        }

        [Test]
        public void Slice_EndBeyondLength_ClampsToLength()
        {
            Assert.AreEqual("hello", "hello".Slice(0, 100));
        }

        [Test]
        public void Slice_StartBeyondLength_ReturnsEmpty()
        {
            Assert.AreEqual(string.Empty, "hello".Slice(10));
        }

        #endregion


        #region RemoveDiacritics

        [Test]
        public void RemoveDiacritics_StripsAccents_KeepsBaseLetters()
        {
            // Accented characters are built from code points so the source stays pure ASCII.
            string eAcute = ((char)0x00E9).ToString();      // e-acute
            string iDiaeresis = ((char)0x00EF).ToString();  // i-diaeresis
            string eGrave = ((char)0x00E8).ToString();      // e-grave
            string uCircumflex = ((char)0x00FB).ToString(); // u-circumflex

            Assert.AreEqual("cafe", ("caf" + eAcute).RemoveDiacritics());
            Assert.AreEqual("naive", ("na" + iDiaeresis + "ve").RemoveDiacritics());
            Assert.AreEqual("Creme Brulee", ("Cr" + eGrave + "me Br" + uCircumflex + "l" + eAcute + "e").RemoveDiacritics());
            Assert.AreEqual("hello", "hello".RemoveDiacritics());
        }

        #endregion


        #region RemoveSpecialChars

        [Test]
        public void RemoveSpecialChars_RemovesNonAlphanumeric()
        {
            Assert.AreEqual("abc", "a!b@c#".RemoveSpecialChars());
        }

        [Test]
        public void RemoveSpecialChars_KeepsAlphanumeric()
        {
            Assert.AreEqual("abc123", "abc123".RemoveSpecialChars());
        }

        [Test]
        public void RemoveSpecialChars_RemovesWhitespace()
        {
            Assert.AreEqual("ab", " a b ".RemoveSpecialChars());
        }

        [Test]
        public void RemoveSpecialChars_KeepsAllowedChars()
        {
            Assert.AreEqual("a-b_c", "a-b_c".RemoveSpecialChars("-_"));
            Assert.AreEqual("a-bc", "a-b.c".RemoveSpecialChars("-"));
        }

        [Test]
        public void RemoveSpecialChars_KeepsUnicodeLetters()
        {
            // "caf" + e-acute, then "!": the accented letter is a Unicode letter and must be preserved,
            // while "!" is stripped. (The old ASCII-only implementation wrongly dropped the accented letter.)
            string cafe = "caf" + (char)0x00E9;
            Assert.AreEqual(cafe, (cafe + "!").RemoveSpecialChars());
        }

        [Test]
        public void RemoveSpecialChars_EmptyInput_ReturnsEmpty()
        {
            Assert.AreEqual(string.Empty, "".RemoveSpecialChars());
        }

        #endregion

    }

}
