using System;

namespace SideXP.Core
{

    /// <summary>
    /// Miscellaneous functions for working with unique identifiers.
    /// </summary>
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
        /// Creates a 22-character GUID by encoding a C# GUID as URL-safe Base64.
        /// </summary>
        /// <returns><inheritdoc cref="GetGUID()"/></returns>
        public static string GetShortGUID()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace('/', '_')
                .Replace('+', '-')
                .TrimEnd('=');
        }

    }

}
