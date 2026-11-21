using System.Text.RegularExpressions;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;

using SideXP.Core.EditorOnly;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="TablerCatalogConverter"/>.
    /// </summary>
    public class TablerCatalogConverterTests
    {

        /// <summary>
        /// Tabler version used for the conversions.
        /// </summary>
        private const string Version = "3.52.0";

        /// <summary>
        /// A small catalog, pretty-printed like the real one: an icon without filled style, an icon with a filled style and a numeric
        /// tag, and an icon in a category with a space in its name.
        /// </summary>
        private const string PrettyCatalog = @"{
  ""a-b-2"": {
    ""name"": ""a-b-2"",
    ""category"": ""Text"",
    ""tags"": [
      ""ab-test"",
      ""a"",
      ""b""
    ],
    ""styles"": {
      ""outline"": {
        ""version"": ""1.76"",
        ""unicode"": ""f25f""
      }
    }
  },
  ""ambulance"": {
    ""name"": ""ambulance"",
    ""category"": ""Vehicles"",
    ""tags"": [
      ""vehicle"",
      911,
      ""help""
    ],
    ""styles"": {
      ""outline"": {
        ""version"": ""1.0"",
        ""unicode"": ""ebf5""
      },
      ""filled"": {
        ""version"": ""2.0"",
        ""unicode"": ""fa00""
      }
    }
  },
  ""git-branch"": {
    ""name"": ""git-branch"",
    ""category"": ""Version control"",
    ""tags"": [
      ""code""
    ],
    ""styles"": {
      ""outline"": {
        ""version"": ""1.0"",
        ""unicode"": ""eab2""
      }
    }
  }
}";

        /// <summary>
        /// Converts a catalog, and checks that the conversion succeeded.
        /// </summary>
        /// <param name="catalogJson">The catalog to convert.</param>
        /// <returns>Returns the converted index.</returns>
        private static IconIndex Convert(string catalogJson)
        {
            bool isConverted = TablerCatalogConverter.TryConvert(catalogJson, Version, out IconIndex index, out string error);
            Assert.IsTrue(isConverted, error);
            Assert.IsNull(error);
            return index;
        }

        /// <summary>
        /// Converts a catalog, and checks that the conversion failed with an error.
        /// </summary>
        /// <param name="catalogJson">The catalog to convert.</param>
        /// <param name="version">The Tabler version of the catalog.</param>
        private static void AssertConversionFails(string catalogJson, string version = Version)
        {
            bool isConverted = TablerCatalogConverter.TryConvert(catalogJson, version, out IconIndex index, out string error);
            Assert.IsFalse(isConverted);
            Assert.IsNull(index);
            Assert.IsFalse(string.IsNullOrEmpty(error));
        }

        #region Conversion

        /// <summary>
        /// Every icon of a pretty-printed catalog is converted, in order, with the given version.
        /// </summary>
        [Test]
        public void TryConvert_PrettyCatalog_ConvertsEveryIcon()
        {
            IconIndex index = Convert(PrettyCatalog);

            Assert.AreEqual(Version, index.version);
            Assert.AreEqual(3, index.icons.Length);
            Assert.AreEqual("a-b-2", index.icons[0].name);
            Assert.AreEqual("Text", index.icons[0].category);
            Assert.AreEqual("ambulance", index.icons[1].name);
            Assert.AreEqual("git-branch", index.icons[2].name);
            Assert.AreEqual("Version control", index.icons[2].category);
        }

        /// <summary>
        /// Bare numbers in tags are converted to strings, and the other tags are kept as is.
        /// </summary>
        [Test]
        public void TryConvert_NumericTags_BecomeStrings()
        {
            IconIndex index = Convert(PrettyCatalog);

            CollectionAssert.AreEqual(new[] { "vehicle", "911", "help" }, index.icons[1].tags);
            CollectionAssert.AreEqual(new[] { "ab-test", "a", "b" }, index.icons[0].tags);
        }

        /// <summary>
        /// Only the icons with a filled style are marked as having one.
        /// </summary>
        [Test]
        public void TryConvert_FilledStyle_SetsHasFilled()
        {
            IconIndex index = Convert(PrettyCatalog);

            Assert.IsFalse(index.icons[0].hasFilled);
            Assert.IsTrue(index.icons[1].hasFilled);
            Assert.IsFalse(index.icons[2].hasFilled);
        }

        /// <summary>
        /// A catalog on a single line is converted too, including negative and decimal numeric tags.
        /// </summary>
        [Test]
        public void TryConvert_CompactCatalog_ConvertsEveryIcon()
        {
            string catalog = @"{""home"":{""name"":""home"",""category"":""Buildings"",""tags"":[""house"",1.5,-2],"
                + @"""styles"":{""outline"":{""version"":""1.0"",""unicode"":""eac1""}}}}";

            IconIndex index = Convert(catalog);

            Assert.AreEqual(1, index.icons.Length);
            Assert.AreEqual("home", index.icons[0].name);
            CollectionAssert.AreEqual(new[] { "house", "1.5", "-2" }, index.icons[0].tags);
        }

        /// <summary>
        /// Bare literals in tags are converted to strings, as for the <c>null</c> tag of <c>number-0</c> in Tabler 3.52.0.
        /// </summary>
        [Test]
        public void TryConvert_LiteralTags_BecomeStrings()
        {
            string catalog = @"{""number-0"":{""name"":""number-0"",""category"":""Numbers"",""tags"":[""zero"",null,""nil"",true,false],"
                + @"""styles"":{""outline"":{""version"":""1.0"",""unicode"":""f0b0""}}}}";

            IconIndex index = Convert(catalog);

            CollectionAssert.AreEqual(new[] { "zero", "null", "nil", "true", "false" }, index.icons[0].tags);
        }

        /// <summary>
        /// Tags containing commas, brackets, escaped quotes or digits are kept as is.
        /// </summary>
        [Test]
        public void TryConvert_TagsWithSpecialCharacters_AreKept()
        {
            string catalog = @"{""home"":{""name"":""home"",""category"":""Buildings"",""tags"":[""a,b"",""c]d"",""e\""f 42""],"
                + @"""styles"":{""outline"":{""version"":""1.0"",""unicode"":""eac1""}}}}";

            IconIndex index = Convert(catalog);

            CollectionAssert.AreEqual(new[] { "a,b", "c]d", "e\"f 42" }, index.icons[0].tags);
        }

        /// <summary>
        /// A missing category or tags list is converted to an empty value.
        /// </summary>
        [Test]
        public void TryConvert_MissingCategoryAndTags_UsesEmptyValues()
        {
            string catalog = @"{""home"":{""name"":""home"",""styles"":{""outline"":{""version"":""1.0"",""unicode"":""eac1""}}}}";

            IconIndex index = Convert(catalog);

            Assert.AreEqual(string.Empty, index.icons[0].category);
            Assert.IsNotNull(index.icons[0].tags);
            Assert.IsEmpty(index.icons[0].tags);
        }

        /// <summary>
        /// An icon with a name that can't be used safely in paths is skipped, with a warning.
        /// </summary>
        [Test]
        public void TryConvert_InvalidIconName_IsSkippedWithWarning()
        {
            string catalog = @"{""home"":{""name"":""home"",""category"":""Buildings""},"
                + @"""Bad_Name"":{""name"":""Bad_Name"",""category"":""Buildings""}}";

            LogAssert.Expect(LogType.Warning, new Regex("Bad_Name"));
            IconIndex index = Convert(catalog);

            Assert.AreEqual(1, index.icons.Length);
            Assert.AreEqual("home", index.icons[0].name);
        }

        #endregion


        #region Failures

        /// <summary>
        /// The conversion fails if an entry doesn't start with its name, instead of producing a broken index.
        /// </summary>
        [Test]
        public void TryConvert_NameNotFirst_Fails()
        {
            string catalog = @"{""home"":{""name"":""home"",""category"":""Buildings""},"
                + @"""user"":{""category"":""System"",""name"":""user""}}";

            AssertConversionFails(catalog);
        }

        /// <summary>
        /// The conversion fails if a nested object starts with a name field, which would be taken for an entry.
        /// </summary>
        [Test]
        public void TryConvert_NestedObjectStartingWithName_Fails()
        {
            string catalog = @"{""home"":{""name"":""home"",""styles"":{""outline"":{""name"":""home-outline""}}}}";

            AssertConversionFails(catalog);
        }

        /// <summary>
        /// The conversion fails if the catalog is not a JSON object with at least one entry.
        /// </summary>
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("{}")]
        [TestCase("[]")]
        [TestCase("not json")]
        [TestCase(@"{""home"":{""name"":""home""}")]
        [TestCase(@"{""home"":{""name"":""home""}}{}")]
        public void TryConvert_InvalidCatalog_Fails(string catalog)
        {
            AssertConversionFails(catalog);
        }

        /// <summary>
        /// The conversion fails without Tabler version, since the icons must be downloaded with the version of the index.
        /// </summary>
        [TestCase(null)]
        [TestCase("")]
        public void TryConvert_MissingVersion_Fails(string version)
        {
            AssertConversionFails(PrettyCatalog, version);
        }

        #endregion

    }

}
