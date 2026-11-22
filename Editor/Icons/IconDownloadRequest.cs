using System;
using System.Collections.Generic;
using System.IO;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Downloads a style of an icon, processes it, and stores its files in the shared folder.
    /// </summary>
    /// <remarks>
    /// The icon is downloaded with the Tabler version of the index it comes from, so its files match it. It's available once its
    /// files are stored in every size (see <see cref="IconStorage.IsStored(string, string, EIconStyle)"/>).
    /// </remarks>
    internal class IconDownloadRequest : DownloadRequest
    {

        #region Lifecycle

        /// <summary>
        /// Creates a request to download a style of an icon.
        /// </summary>
        /// <param name="version">The Tabler version of the index the entry comes from.</param>
        /// <param name="entry">The index entry of the icon.</param>
        /// <param name="style">The style to download.</param>
        /// <exception cref="ArgumentException">Thrown if the version or the index entry is not valid.</exception>
        internal IconDownloadRequest(string version, IconIndexEntry entry, EIconStyle style)
        {
            if (!TablerSource.IsValidVersion(version))
                throw new ArgumentException($"Invalid Tabler version \"{version}\".", nameof(version));

            if (entry == null || !IconStorage.IsValidIconName(entry.name))
                throw new ArgumentException("Invalid index entry.", nameof(entry));

            Version = version;
            Name = entry.name;
            Category = entry.category;
            Style = style;
            Key = GetKey(version, Name, style);
            Url = TablerSource.GetIconUrl(version, Name, style);
        }

        #endregion


        #region Public API

        /// <summary>
        /// The Tabler version to download the icon from.
        /// </summary>
        internal string Version { get; }

        /// <summary>
        /// The name of the icon.
        /// </summary>
        internal string Name { get; }

        /// <summary>
        /// The category of the icon, as written in the index.
        /// </summary>
        internal string Category { get; }

        /// <summary>
        /// The style to download.
        /// </summary>
        internal EIconStyle Style { get; }

        /// <summary>
        /// The key of the request, unique per version, icon and style (see <see cref="GetKey"/>).
        /// </summary>
        public override string Key { get; }

        /// <inheritdoc/>
        public override string Url { get; }

        /// <summary>
        /// Describes the icon, as <c>home (filled)</c>.
        /// </summary>
        public override string DisplayName => $"{Name} ({IconStorage.GetStyleName(Style)})";

        /// <summary>
        /// Gets the key of a request, unique per version, icon and style.
        /// </summary>
        /// <example><c>GetKey("3.52.0", "home", EIconStyle.Filled)</c> returns <c>"3.52.0/home/filled"</c>.</example>
        /// <param name="version">The Tabler version.</param>
        /// <param name="name">The name of the icon.</param>
        /// <param name="style">The style of the icon.</param>
        /// <returns>Returns the key of the request.</returns>
        internal static string GetKey(string version, string name, EIconStyle style)
        {
            return $"{version}/{name}/{IconStorage.GetStyleName(style)}";
        }

        /// <summary>
        /// Checks if the files of the icon are stored in every size.
        /// </summary>
        /// <returns>Returns true if the icon doesn't need to be downloaded.</returns>
        public override bool IsAvailable()
        {
            return IconStorage.IsStored(Name, Category, Style);
        }

        #endregion


        #region Protected API

        /// <summary>
        /// Processes the downloaded PNG, and writes the files of each stored size.
        /// </summary>
        /// <param name="data">The content of the downloaded PNG file.</param>
        /// <param name="error">Outputs the reason why the icon couldn't be stored, or null if it has been.</param>
        /// <returns>
        /// Returns <see cref="EDownloadHandlingResult.Success"/> if every file has been stored,
        /// <see cref="EDownloadHandlingResult.GiveUp"/> if the downloaded file is not a valid icon image, or
        /// <see cref="EDownloadHandlingResult.Retry"/> if a file couldn't be written.
        /// </returns>
        protected internal override EDownloadHandlingResult Handle(byte[] data, out string error)
        {
            // The same file would be served again, so there's no point in retrying
            if (!IconProcessor.TryProcess(data, IconStorage.StoredSizes, out Dictionary<int, byte[]> pngBySize, out error))
                return EDownloadHandlingResult.GiveUp;

            try
            {
                // Files already written (as by another editor) are kept, since they have the same content
                foreach (KeyValuePair<int, byte[]> png in pngBySize)
                {
                    string path = IconStorage.GetIconFilePath(Name, Category, png.Key, Style);
                    FileEditorUtility.WriteFileAtomic(path, png.Value, false);
                }

                error = null;
                return EDownloadHandlingResult.Success;
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                error = $"The files of the icon couldn't be written: {exception.Message}";
                return EDownloadHandlingResult.Retry;
            }
        }

        #endregion

    }

}
