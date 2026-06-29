using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using NUnit.Framework;

namespace SideXP.Core.Tests
{

    public class IdUtilityTests
    {

        #region GetGUID

        [Test]
        public void GetGUID_IsParseableGuid()
        {
            string id = IdUtility.GetGUID();
            Assert.IsTrue(Guid.TryParse(id, out _), $"'{id}' should be a valid GUID string.");
        }

        [Test]
        public void GetGUID_ProducesUniqueValues()
        {
            var set = new HashSet<string>();
            for (int i = 0; i < 1000; i++)
                set.Add(IdUtility.GetGUID());
            Assert.AreEqual(1000, set.Count);
        }

        #endregion


        #region GetShortGUID

        [Test]
        public void GetShortGUID_IsAlways22Characters()
        {
            for (int i = 0; i < 1000; i++)
                Assert.AreEqual(22, IdUtility.GetShortGUID().Length);
        }

        [Test]
        public void GetShortGUID_IsUrlSafe()
        {
            // Only URL-safe Base64 characters, never '/', '+' or '=' .
            var urlSafe = new Regex("^[A-Za-z0-9_-]+$");
            for (int i = 0; i < 1000; i++)
            {
                string id = IdUtility.GetShortGUID();
                Assert.IsTrue(urlSafe.IsMatch(id), $"'{id}' contains non-URL-safe characters.");
            }
        }

        [Test]
        public void GetShortGUID_PreservesFullEntropy()
        {
            // Reversing the transform must yield the original 16 GUID bytes (no data lost).
            string id = IdUtility.GetShortGUID();
            string base64 = id.Replace('_', '/').Replace('-', '+') + "==";
            byte[] bytes = Convert.FromBase64String(base64);
            Assert.AreEqual(16, bytes.Length);
            Assert.DoesNotThrow(() => _ = new Guid(bytes));
        }

        [Test]
        public void GetShortGUID_ProducesUniqueValues()
        {
            var set = new HashSet<string>();
            for (int i = 0; i < 1000; i++)
                set.Add(IdUtility.GetShortGUID());
            Assert.AreEqual(1000, set.Count);
        }

        #endregion

    }

}
