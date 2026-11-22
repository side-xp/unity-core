using System;

using NUnit.Framework;

using SideXP.Core.EditorOnly;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="TablerSource"/>.
    /// </summary>
    public class TablerSourceTests
    {

        /// <summary>
        /// The response of the jsDelivr data API for the latest Tabler PNG package, as received in November 2026.
        /// </summary>
        private const string LatestVersionResponse = "{\n\t\"type\": \"npm\",\n\t\"name\": \"@tabler/icons-png\",\n"
            + "\t\"version\": \"3.52.0\",\n"
            + "\t\"links\": {\n\t\t\"self\": \"https://data.jsdelivr.com/v1/packages/npm/@tabler/icons-png@3.52.0\"\n\t}\n}";

        /// <summary>
        /// Only plain versions, optionally with a pre-release suffix, are valid.
        /// </summary>
        [TestCase("3.52.0", true)]
        [TestCase("10.0.12", true)]
        [TestCase("3.52.0-beta.1", true)]
        [TestCase(null, false)]
        [TestCase("", false)]
        [TestCase("latest", false)]
        [TestCase("3.52", false)]
        [TestCase("3.52.0/../../x", false)]
        [TestCase("3.52.0\n", false)]
        public void IsValidVersion_ChecksFormat(string version, bool expected)
        {
            Assert.AreEqual(expected, TablerSource.IsValidVersion(version));
        }

        /// <summary>
        /// The catalog URL uses the given version of the Tabler icons package.
        /// </summary>
        [Test]
        public void GetCatalogUrl_UsesVersion()
        {
            Assert.AreEqual("https://cdn.jsdelivr.net/npm/@tabler/icons@3.52.0/icons.json", TablerSource.GetCatalogUrl("3.52.0"));
        }

        /// <summary>
        /// An invalid version can't be used in a URL.
        /// </summary>
        [Test]
        public void GetCatalogUrl_InvalidVersion_Throws()
        {
            Assert.Throws<ArgumentException>(() => TablerSource.GetCatalogUrl("../evil"));
        }

        /// <summary>
        /// The PNG URL uses the given version of the Tabler PNG package, and a folder per style.
        /// </summary>
        [Test]
        public void GetIconUrl_UsesVersionAndStyleFolder()
        {
            Assert.AreEqual(
                "https://cdn.jsdelivr.net/npm/@tabler/icons-png@3.52.0/icons/outline/home.png",
                TablerSource.GetIconUrl("3.52.0", "home", EIconStyle.Outline)
            );
            Assert.AreEqual(
                "https://cdn.jsdelivr.net/npm/@tabler/icons-png@3.52.0/icons/filled/home.png",
                TablerSource.GetIconUrl("3.52.0", "home", EIconStyle.Filled)
            );
        }

        /// <summary>
        /// An invalid version or icon name can't be used in a URL.
        /// </summary>
        [Test]
        public void GetIconUrl_InvalidVersionOrName_Throws()
        {
            Assert.Throws<ArgumentException>(() => TablerSource.GetIconUrl("latest", "home", EIconStyle.Outline));
            Assert.Throws<ArgumentException>(() => TablerSource.GetIconUrl("3.52.0", "../home", EIconStyle.Outline));
        }

        /// <summary>
        /// The version is read from a response shaped like the real one.
        /// </summary>
        [Test]
        public void TryParseLatestVersion_RealResponse_ReadsVersion()
        {
            Assert.IsTrue(TablerSource.TryParseLatestVersion(LatestVersionResponse, out string version));
            Assert.AreEqual("3.52.0", version);
        }

        /// <summary>
        /// A response that can't be read, or without valid version, is rejected.
        /// </summary>
        [TestCase(null)]
        [TestCase("")]
        [TestCase("not json")]
        [TestCase(@"{""name"":""@tabler/icons-png""}")]
        [TestCase(@"{""version"":""latest""}")]
        public void TryParseLatestVersion_InvalidResponse_ReturnsFalse(string json)
        {
            Assert.IsFalse(TablerSource.TryParseLatestVersion(json, out string version));
            Assert.IsNull(version);
        }

    }

}
