using System;
using System.Collections.Generic;

using NUnit.Framework;

using UnityEngine;

using SideXP.Core.EditorOnly;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="IconIndex"/> and <see cref="IconInfo"/>.
    /// </summary>
    public class IconIndexTests
    {

        /// <summary>
        /// Makes an index entry for the tests.
        /// </summary>
        /// <returns>Returns the created entry.</returns>
        private static IconIndexEntry MakeEntry()
        {
            return new IconIndexEntry
            {
                name = "home",
                category = "Buildings",
                tags = new[] { "house", "dashboard" },
                hasFilled = true,
            };
        }

        /// <summary>
        /// The index is written and read back with <see cref="JsonUtility"/> without losing data.
        /// </summary>
        [Test]
        public void Index_JsonRoundTrip_KeepsData()
        {
            var index = new IconIndex { version = "3.52.0", icons = new[] { MakeEntry() } };

            IconIndex readIndex = JsonUtility.FromJson<IconIndex>(JsonUtility.ToJson(index));

            Assert.AreEqual("3.52.0", readIndex.version);
            Assert.AreEqual(1, readIndex.icons.Length);
            Assert.AreEqual("home", readIndex.icons[0].name);
            Assert.AreEqual("Buildings", readIndex.icons[0].category);
            CollectionAssert.AreEqual(new[] { "house", "dashboard" }, readIndex.icons[0].tags);
            Assert.IsTrue(readIndex.icons[0].hasFilled);
        }

        /// <summary>
        /// The index is written with the keys of the documented format.
        /// </summary>
        [Test]
        public void Index_ToJson_UsesDocumentedKeys()
        {
            var index = new IconIndex { version = "3.52.0", icons = new[] { MakeEntry() } };

            string json = JsonUtility.ToJson(index);

            StringAssert.Contains("\"version\":\"3.52.0\"", json);
            StringAssert.Contains("\"icons\":[", json);
            StringAssert.Contains("\"name\":\"home\"", json);
            StringAssert.Contains("\"category\":\"Buildings\"", json);
            StringAssert.Contains("\"tags\":[\"house\",\"dashboard\"]", json);
            StringAssert.Contains("\"hasFilled\":true", json);
        }

        /// <summary>
        /// The informations about an icon are copied from its index entry.
        /// </summary>
        [Test]
        public void IconInfo_FromEntry_CopiesData()
        {
            var info = new IconInfo(MakeEntry());

            Assert.AreEqual("home", info.Name);
            Assert.AreEqual("Buildings", info.Category);
            CollectionAssert.AreEqual(new[] { "house", "dashboard" }, info.Tags);
            Assert.IsTrue(info.HasFilled);
        }

        /// <summary>
        /// The tags can't be changed through the informations about an icon.
        /// </summary>
        [Test]
        public void IconInfo_Tags_AreReadOnly()
        {
            var info = new IconInfo(MakeEntry());

            Assert.Throws<NotSupportedException>(() => ((IList<string>)info.Tags)[0] = "changed");
        }

        /// <summary>
        /// A missing category or tags list is exposed as an empty value.
        /// </summary>
        [Test]
        public void IconInfo_MissingCategoryAndTags_AreEmpty()
        {
            var info = new IconInfo(new IconIndexEntry { name = "home", category = null, tags = null });

            Assert.AreEqual(string.Empty, info.Category);
            Assert.IsNotNull(info.Tags);
            Assert.AreEqual(0, info.Tags.Count);
        }

    }

}
