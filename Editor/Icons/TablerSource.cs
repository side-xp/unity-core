using System;
using System.Text.RegularExpressions;

using UnityEngine;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Locations of the Tabler files, served by jsDelivr from the npm packages.
    /// </summary>
    internal static class TablerSource
    {

        #region Subclasses

        // Field names are the keys of the jsDelivr response, read with JsonUtility, so they don't follow the naming rules
#pragma warning disable IDE1006

        /// <summary>
        /// The response of the jsDelivr data API when resolving a package version.
        /// </summary>
        [Serializable]
        private class ResolvedVersionResponse
        {

            /// <summary>
            /// The resolved version of the package.
            /// </summary>
            public string version = null;

        }

#pragma warning restore IDE1006

        #endregion


        #region Fields

        /// <summary>
        /// URL to get the latest version of the Tabler PNG package. The PNG package is used (rather than the one that contains the
        /// catalog), since the PNG files are what must exist for the version of the index.
        /// </summary>
        internal const string LatestVersionUrl = "https://data.jsdelivr.com/v1/packages/npm/@tabler/icons-png/resolved?specifier=latest";

        /// <summary>
        /// Base URL of the npm packages served by jsDelivr.
        /// </summary>
        private const string CdnBaseUrl = "https://cdn.jsdelivr.net/npm";

        /// <summary>
        /// Name of the npm package that contains the Tabler catalog.
        /// </summary>
        private const string CatalogPackage = "@tabler/icons";

        /// <summary>
        /// Path to the catalog in its package.
        /// </summary>
        private const string CatalogPath = "icons.json";

        /// <summary>
        /// Name of the npm package that contains the Tabler PNG files.
        /// </summary>
        private const string PngPackage = "@tabler/icons-png";

        /// <summary>
        /// Path to the folder of the PNG files in their package, which contains a folder per style.
        /// </summary>
        private const string PngFolder = "icons";

        /// <summary>
        /// Matches a valid package version (as <c>3.52.0</c>, or <c>3.52.0-beta.1</c>). Versions are inserted in URLs and stored in the
        /// index, so anything else is rejected.
        /// </summary>
        private static readonly Regex VersionRegex = new Regex(@"^\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?\z");

        #endregion


        #region Public API

        /// <summary>
        /// Checks if a string is a valid package version.
        /// </summary>
        /// <param name="version">The string to check.</param>
        /// <returns>Returns true if the string is a valid version.</returns>
        internal static bool IsValidVersion(string version)
        {
            return !string.IsNullOrEmpty(version) && VersionRegex.IsMatch(version);
        }

        /// <summary>
        /// Gets the URL of the Tabler catalog for a given version.
        /// </summary>
        /// <param name="version">The Tabler version.</param>
        /// <returns>Returns the URL of the catalog.</returns>
        /// <exception cref="ArgumentException">Thrown if the version is not valid.</exception>
        internal static string GetCatalogUrl(string version)
        {
            if (!IsValidVersion(version))
                throw new ArgumentException($"Invalid Tabler version \"{version}\".", nameof(version));

            return $"{CdnBaseUrl}/{CatalogPackage}@{version}/{CatalogPath}";
        }

        /// <summary>
        /// Gets the URL of the PNG file of an icon.
        /// </summary>
        /// <param name="version">The Tabler version, which must be the version of the index.</param>
        /// <param name="name">The name of the icon.</param>
        /// <param name="style">The style of the icon.</param>
        /// <returns>Returns the URL of the PNG file.</returns>
        /// <exception cref="ArgumentException">Thrown if the version or the icon name is not valid.</exception>
        internal static string GetIconUrl(string version, string name, EIconStyle style)
        {
            if (!IsValidVersion(version))
                throw new ArgumentException($"Invalid Tabler version \"{version}\".", nameof(version));

            if (!IconStorage.IsValidIconName(name))
                throw new ArgumentException($"Invalid icon name \"{name}\".", nameof(name));

            string styleFolder = IconStorage.GetStyleName(style);
            return $"{CdnBaseUrl}/{PngPackage}@{version}/{PngFolder}/{styleFolder}/{name}{IconStorage.IconFileExtension}";
        }

        /// <summary>
        /// Reads the version from the response of <see cref="LatestVersionUrl"/>.
        /// </summary>
        /// <param name="json">The content of the response.</param>
        /// <param name="version">Outputs the resolved version, or null if it can't be read.</param>
        /// <returns>Returns true if a valid version has been read.</returns>
        internal static bool TryParseLatestVersion(string json, out string version)
        {
            version = null;
            if (string.IsNullOrWhiteSpace(json))
                return false;

            ResolvedVersionResponse response;
            try
            {
                response = JsonUtility.FromJson<ResolvedVersionResponse>(json);
            }
            catch (ArgumentException)
            {
                return false;
            }

            if (response == null || !IsValidVersion(response.version))
                return false;

            version = response.version;
            return true;
        }

        #endregion

    }

}
