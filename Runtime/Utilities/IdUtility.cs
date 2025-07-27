using System;
using System.Text.RegularExpressions;

namespace SideXP.Core
{

    /// <summary>
    /// Miscellaneous functions for working with unique identifiers.
    /// </summary>
    [System.Serializable]
    public static class IdUtility
    {

        /// <summary>
        /// Creates a new GUID using <see cref="System.Guid.NewGuid()"/>, and returns its string value.
        /// </summary>
        /// <returns>Returns the generated GUID.</returns>
        public static string GetGUID()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Creates a GUID of 22 characters using C# GUID converted to Base64.
        /// </summary>
        /// <returns><inheritdoc cref="GetGUID()"/></returns>
        public static string GetShortGUID()
        {
            return
                Regex.Replace(
                    Convert.ToBase64String(
                        Guid.NewGuid().ToByteArray()),
                    "[/+=]", "");
        }

    }

}