using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Downloads requested icons, processes them, and stores their files in the shared folder.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Requesting an icon also downloads its other style (when it exists), since icons are often used in both styles.</item>
    /// <item>Each style of an icon is a separate item with its own retries, so a failing filled style doesn't hold back the outline
    /// one. Requests for the same item share the same download.</item>
    /// <item>Icons are downloaded with the Tabler version of the index they come from, so their files match it.</item>
    /// <item>Failed downloads are retried as decided by <see cref="IconRetryPolicy"/>.</item>
    /// </list>
    /// The queue doesn't run by itself: its owner must call <see cref="Update(double)"/> regularly (as from editor updates).
    /// Callbacks are called from <see cref="Update(double)"/>, or right away if the result is already known.
    /// </remarks>
    internal class IconDownloadQueue
    {

        #region Subclasses

        /// <summary>
        /// A style of an icon to download.
        /// </summary>
        private class Item
        {

            /// <summary>
            /// The key of the item, unique per version, icon and style (see <see cref="GetKey"/>).
            /// </summary>
            public string Key = null;

            /// <summary>
            /// The Tabler version to download the icon from.
            /// </summary>
            public string Version = null;

            /// <summary>
            /// The name of the icon.
            /// </summary>
            public string Name = null;

            /// <summary>
            /// The category of the icon, as written in the index.
            /// </summary>
            public string Category = null;

            /// <summary>
            /// The style to download.
            /// </summary>
            public EIconStyle Style = EIconStyle.Outline;

            /// <summary>
            /// The download in progress, or null if the item is waiting.
            /// </summary>
            public IIconDownload Download = null;

            /// <summary>
            /// The functions to call when the item is stored (true) or given up (false).
            /// </summary>
            public List<Action<bool>> Callbacks = new List<Action<bool>>();

        }

        #endregion


        #region Fields

        /// <summary>
        /// Maximum number of downloads in progress at the same time. A few parallel downloads hide the latency of each request,
        /// without flooding the server when many icons are requested at once (as when opening the picker window).
        /// </summary>
        internal const int MaxConcurrentDownloads = 4;

        /// <summary>
        /// Decides when failed downloads can be attempted again.
        /// </summary>
        private readonly IconRetryPolicy _policy = null;

        /// <summary>
        /// Called with the key of an item when it's given up, or null.
        /// </summary>
        private readonly Action<string> _onGivenUp = null;

        /// <summary>
        /// The items to download, in the order they've been requested.
        /// </summary>
        private readonly List<Item> _items = new List<Item>();

        #endregion


        #region Lifecycle

        /// <summary>
        /// Creates a download queue.
        /// </summary>
        /// <param name="policy">Decides when failed downloads can be attempted again. Its failures are shared with the queue.</param>
        /// <param name="onGivenUp">Called with the key of an item when it's given up, as to remember it. Can be null.</param>
        /// <exception cref="ArgumentNullException">Thrown if the policy is null.</exception>
        internal IconDownloadQueue(IconRetryPolicy policy, Action<string> onGivenUp = null)
        {
            _policy = policy ?? throw new ArgumentNullException(nameof(policy));
            _onGivenUp = onGivenUp;
        }

        #endregion


        #region Public API

        /// <summary>
        /// The number of items waiting or downloading.
        /// </summary>
        internal int Count => _items.Count;

        /// <summary>
        /// The number of downloads in progress.
        /// </summary>
        internal int DownloadingCount => _items.FindAll(item => item.Download != null).Count;

        /// <summary>
        /// Describes the first download in progress, as <c>home (filled)</c>, or null if none.
        /// </summary>
        internal string CurrentDownloadName
        {
            get
            {
                Item item = _items.Find(i => i.Download != null);
                return item != null ? $"{item.Name} ({IconStorage.GetStyleName(item.Style)})" : null;
            }
        }

        /// <summary>
        /// Gets the key of an item, unique per version, icon and style.
        /// </summary>
        /// <example><c>GetKey("3.52.0", "home", EIconStyle.Filled)</c> returns <c>"3.52.0/home/filled"</c>.</example>
        /// <param name="version">The Tabler version.</param>
        /// <param name="name">The name of the icon.</param>
        /// <param name="style">The style of the icon.</param>
        /// <returns>Returns the key of the item.</returns>
        internal static string GetKey(string version, string name, EIconStyle style)
        {
            return $"{version}/{name}/{IconStorage.GetStyleName(style)}";
        }

        /// <summary>
        /// Requests an icon: downloads its files if they're not stored yet, along with the files of its other style.
        /// </summary>
        /// <param name="version">The Tabler version of the index the entry comes from.</param>
        /// <param name="entry">The index entry of the icon.</param>
        /// <param name="style">
        /// The requested style. <see cref="EIconStyle.Filled"/> is replaced by <see cref="EIconStyle.Outline"/> if the icon has no
        /// filled style.
        /// </param>
        /// <param name="onComplete">
        /// Called with true when the files of the requested style are stored, or with false if they won't be (the download has been
        /// given up for this session). Not called while failed downloads are retried. Can be null.
        /// </param>
        /// <exception cref="ArgumentException">Thrown if the version or the index entry is not valid.</exception>
        internal void Request(string version, IconIndexEntry entry, EIconStyle style, Action<bool> onComplete)
        {
            if (!TablerSource.IsValidVersion(version))
                throw new ArgumentException($"Invalid Tabler version \"{version}\".", nameof(version));

            if (entry == null || !IconStorage.IsValidIconName(entry.name))
                throw new ArgumentException("Invalid index entry.", nameof(entry));

            if (!entry.hasFilled)
                style = EIconStyle.Outline;

            Enqueue(version, entry, style, onComplete);
            if (entry.hasFilled)
                Enqueue(version, entry, style == EIconStyle.Outline ? EIconStyle.Filled : EIconStyle.Outline, null);
        }

        /// <summary>
        /// Handles the completed downloads, and starts new ones.
        /// </summary>
        /// <param name="time">The current time, in seconds (as <see cref="UnityEditor.EditorApplication.timeSinceStartup"/>).</param>
        /// <returns>Returns the number of items that have been stored or given up during this update.</returns>
        internal int Update(double time)
        {
            int completedCount = 0;
            foreach (Item item in _items.ToArray())
            {
                if (item.Download != null && item.Download.IsDone && HandleDownload(item, time))
                    completedCount++;
            }

            StartDownloads(time);
            return completedCount;
        }

        /// <summary>
        /// Aborts the downloads in progress, as before a domain reload. The items stay in the queue, and are downloaded again by the
        /// next updates.
        /// </summary>
        internal void AbortDownloads()
        {
            foreach (Item item in _items)
            {
                if (item.Download == null)
                    continue;

                item.Download.Abort();
                item.Download = null;
            }
        }

        #endregion


        #region Private API

        /// <summary>
        /// Adds a style of an icon to the queue, unless its result is already known.
        /// </summary>
        /// <param name="version">The Tabler version to download the icon from.</param>
        /// <param name="entry">The index entry of the icon.</param>
        /// <param name="style">The style to download.</param>
        /// <param name="onComplete">Called when the item is stored or given up. Can be null.</param>
        private void Enqueue(string version, IconIndexEntry entry, EIconStyle style, Action<bool> onComplete)
        {
            if (IconStorage.IsStored(entry.name, entry.category, style))
            {
                Notify(onComplete, true);
                return;
            }

            string key = GetKey(version, entry.name, style);
            if (_policy.IsGivenUp(key))
            {
                Notify(onComplete, false);
                return;
            }

            Item item = _items.Find(i => i.Key == key);
            if (item == null)
            {
                item = new Item { Key = key, Version = version, Name = entry.name, Category = entry.category, Style = style };
                _items.Add(item);
            }

            if (onComplete != null)
                item.Callbacks.Add(onComplete);
        }

        /// <summary>
        /// Starts downloading the waiting items that can be attempted, in order, up to <see cref="MaxConcurrentDownloads"/>.
        /// </summary>
        /// <param name="time">The current time, in seconds.</param>
        private void StartDownloads(double time)
        {
            int downloadingCount = DownloadingCount;
            foreach (Item item in _items)
            {
                if (downloadingCount >= MaxConcurrentDownloads)
                    return;

                if (item.Download != null || !_policy.CanAttempt(item.Key, time))
                    continue;

                item.Download = IconDownloader.Start(TablerSource.GetIconUrl(item.Version, item.Name, item.Style));
                downloadingCount++;
            }
        }

        /// <summary>
        /// Handles the completed download of an item: stores its files, or reports the failure to the retry policy.
        /// </summary>
        /// <param name="item">The item of which the download is done.</param>
        /// <param name="time">The current time, in seconds.</param>
        /// <returns>Returns true if the item has been stored or given up, and removed from the queue.</returns>
        private bool HandleDownload(Item item, double time)
        {
            IconDownloadResult result = item.Download.Result;
            item.Download = null;

            EIconDownloadStatus status = result.Status;
            if (result.IsSuccess)
            {
                if (TryStore(item, result.Data))
                {
                    _policy.ReportSuccess(item.Key);
                    Complete(item, true);
                    return true;
                }

                // The downloaded file couldn't be processed or written, which may work on the next attempt
                status = EIconDownloadStatus.Failed;
            }

            _policy.ReportFailure(item.Key, status, time);
            if (!_policy.IsGivenUp(item.Key))
                return false;

            _onGivenUp?.Invoke(item.Key);
            Complete(item, false);
            return true;
        }

        /// <summary>
        /// Processes a downloaded PNG, and writes the files of each stored size.
        /// </summary>
        /// <param name="item">The downloaded item.</param>
        /// <param name="pngData">The content of the downloaded PNG file.</param>
        /// <returns>Returns true if every file has been stored.</returns>
        private static bool TryStore(Item item, byte[] pngData)
        {
            if (!IconProcessor.TryProcess(pngData, IconStorage.StoredSizes, out Dictionary<int, byte[]> pngBySize, out _))
                return false;

            try
            {
                // Files already written (as by another editor) are kept, since they have the same content
                foreach (KeyValuePair<int, byte[]> png in pngBySize)
                {
                    string path = IconStorage.GetIconFilePath(item.Name, item.Category, png.Key, item.Style);
                    IconStorage.WriteFileAtomic(path, png.Value, false);
                }

                return true;
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                return false;
            }
        }

        /// <summary>
        /// Removes an item from the queue, and calls its callbacks.
        /// </summary>
        /// <param name="item">The item to complete.</param>
        /// <param name="isStored">Whether the files of the item are stored, or it has been given up.</param>
        private void Complete(Item item, bool isStored)
        {
            _items.Remove(item);
            foreach (Action<bool> callback in item.Callbacks)
                Notify(callback, isStored);
        }

        /// <summary>
        /// Calls a callback, and logs its exceptions, so a faulty callback doesn't break the queue.
        /// </summary>
        /// <param name="callback">The callback to call. Can be null.</param>
        /// <param name="isStored">Whether the files of the item are stored, or it has been given up.</param>
        private static void Notify(Action<bool> callback, bool isStored)
        {
            if (callback == null)
                return;

            try
            {
                callback(isStored);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        #endregion

    }

}
