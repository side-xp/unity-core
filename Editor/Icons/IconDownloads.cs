using System;

using UnityEditorInternal;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Downloads the editor icons with a shared <see cref="EditorDownloadQueue"/>.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Requesting an icon also downloads its other style (when it exists), since icons are often used in both styles. Each
    /// style is a separate request with its own retries, so a failing filled style doesn't hold back the outline one.</item>
    /// <item>The editor views are repainted when icons are stored, so the IMGUI code waiting for them draws them.</item>
    /// <item>The queue displays the downloads in the Background Tasks window, aborts them before a domain reload, and remembers the
    /// given up ones until the editor is closed (see <see cref="EditorDownloadQueue"/>).</item>
    /// </list>
    /// Downloads don't need to be paused while the shared folder is moved or the downloaded icons are deleted: their files are
    /// written from editor updates, which don't run during these operations, and their paths are resolved when writing.
    /// </remarks>
    internal static class IconDownloads
    {

        #region Fields

        /// <summary>
        /// Name of the shared queue, displayed in the Background Tasks window.
        /// </summary>
        private const string QueueName = "Download editor icons";

        /// <summary>
        /// The shared queue, or null if it has not been created yet.
        /// </summary>
        private static EditorDownloadQueue s_queue = null;

        #endregion


        #region Public API

        /// <summary>
        /// The number of icon styles waiting or downloading in the shared queue.
        /// </summary>
        internal static int Count => s_queue != null ? s_queue.Count : 0;

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
        internal static void Request(string version, IconIndexEntry entry, EIconStyle style, Action<bool> onComplete)
        {
            if (entry != null && !entry.hasFilled)
                style = EIconStyle.Outline;

            Action<DownloadRequest> callback = null;
            if (onComplete != null)
                callback = request => onComplete(request.Status == EDownloadRequestStatus.Succeeded);

            EditorDownloadQueue queue = GetQueue();
            queue.Request(new IconDownloadRequest(version, entry, style), callback);

            if (entry.hasFilled)
            {
                EIconStyle otherStyle = style == EIconStyle.Outline ? EIconStyle.Filled : EIconStyle.Outline;
                queue.Request(new IconDownloadRequest(version, entry, otherStyle));
            }
        }

        /// <summary>
        /// Checks if a style of an icon is waiting or downloading.
        /// </summary>
        /// <param name="version">The Tabler version.</param>
        /// <param name="name">The name of the icon.</param>
        /// <param name="style">The style of the icon.</param>
        /// <returns>Returns true if the icon style is in the shared queue.</returns>
        internal static bool IsPending(string version, string name, EIconStyle style)
        {
            return s_queue != null && s_queue.IsPending(IconDownloadRequest.GetKey(version, name, style));
        }

        /// <summary>
        /// Checks if the download of a style of an icon has been given up for this session.
        /// </summary>
        /// <param name="version">The Tabler version.</param>
        /// <param name="name">The name of the icon.</param>
        /// <param name="style">The style of the icon.</param>
        /// <returns>Returns true if requesting this icon style fails right away.</returns>
        internal static bool IsGivenUp(string version, string name, EIconStyle style)
        {
            return GetQueue().IsGivenUp(IconDownloadRequest.GetKey(version, name, style));
        }

        /// <summary>
        /// Aborts the downloads in progress, and forgets the shared queue and the given up downloads, without calling the pending
        /// callbacks. Meant for tests.
        /// </summary>
        internal static void Reset()
        {
            // Created if needed, so the given up downloads remembered in the session are also forgotten
            EditorDownloadQueue queue = GetQueue();
            queue.ClearFailures();
            queue.Dispose();
            s_queue = null;
        }

        #endregion


        #region Private API

        /// <summary>
        /// Gets the shared queue, or creates it with the downloads given up before the last domain reload.
        /// </summary>
        /// <returns>Returns the shared queue.</returns>
        private static EditorDownloadQueue GetQueue()
        {
            if (s_queue == null)
            {
                s_queue = new EditorDownloadQueue(QueueName);
                s_queue.RequestCompleted += OnRequestCompleted;
            }

            return s_queue;
        }

        /// <summary>
        /// Called when an icon style has been stored or given up: repaints the views, so the IMGUI code waiting for it draws the
        /// icon, or the empty placeholder instead of the loading one.
        /// </summary>
        /// <param name="request">The completed request.</param>
        private static void OnRequestCompleted(DownloadRequest request)
        {
            InternalEditorUtility.RepaintAllViews();
        }

        #endregion

    }

}
