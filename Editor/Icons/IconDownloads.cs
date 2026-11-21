using System;

using UnityEditor;
using UnityEditorInternal;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Runs the shared icon download queue from editor updates.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>The editor views are repainted when icons are stored, so the IMGUI code waiting for them draws them.</item>
    /// <item>Downloads in progress are aborted before a domain reload, and the queue is lost with the other static data. Code that
    /// still needs its icons requests them again after the reload.</item>
    /// <item>Given up downloads are remembered in <see cref="SessionState"/>, so a missing file is not requested again after each
    /// domain reload, until the editor is closed.</item>
    /// </list>
    /// Downloads don't need to be paused while the shared folder is moved or the downloaded icons are deleted: their files are
    /// written from editor updates, which don't run during these operations, and their paths are resolved when writing.
    /// </remarks>
    internal static class IconDownloads
    {

        #region Fields

        /// <summary>
        /// Key of the given up downloads in <see cref="SessionState"/>.
        /// </summary>
        private const string GivenUpSessionKey = "SideXP.Core.EditorOnly.IconDownloads.GivenUp";

        /// <summary>
        /// Separator of the given up download keys in <see cref="SessionState"/>. Keys never contain it.
        /// </summary>
        private const char GivenUpSeparator = '\n';

        /// <summary>
        /// The retry policy of the shared queue, or null if the queue has not been created yet.
        /// </summary>
        private static IconRetryPolicy s_policy = null;

        /// <summary>
        /// The shared queue, or null if it has not been created yet.
        /// </summary>
        private static IconDownloadQueue s_queue = null;

        /// <summary>
        /// Whether the shared queue is updated from editor updates.
        /// </summary>
        private static bool s_isUpdating = false;

        #endregion


        #region Public API

        /// <summary>
        /// The number of icon styles waiting or downloading in the shared queue.
        /// </summary>
        internal static int Count => s_queue != null ? s_queue.Count : 0;

        /// <inheritdoc cref="IconDownloadQueue.Request"/>
        internal static void Request(string version, IconIndexEntry entry, EIconStyle style, Action<bool> onComplete)
        {
            EnsureQueue();
            s_queue.Request(version, entry, style, onComplete);

            if (s_queue.Count > 0 && !s_isUpdating)
            {
                EditorApplication.update += Update;
                AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
                s_isUpdating = true;
            }
        }

        #endregion


        #region Private API

        /// <summary>
        /// Creates the shared queue if it doesn't exist yet, with the downloads given up before the last domain reload.
        /// </summary>
        private static void EnsureQueue()
        {
            if (s_queue != null)
                return;

            s_policy = new IconRetryPolicy();
            foreach (string key in SessionState.GetString(GivenUpSessionKey, string.Empty).Split(GivenUpSeparator))
            {
                if (!string.IsNullOrEmpty(key))
                    s_policy.GiveUp(key);
            }

            s_queue = new IconDownloadQueue(s_policy, _ => SaveGivenUp());
        }

        /// <summary>
        /// Updates the shared queue, and repaints the views if icons have been stored.
        /// </summary>
        private static void Update()
        {
            if (s_queue.Update(EditorApplication.timeSinceStartup) > 0)
                InternalEditorUtility.RepaintAllViews();

            if (s_queue.Count == 0)
                StopUpdating();
        }

        /// <summary>
        /// Aborts the downloads in progress before scripts are reloaded, since they can't survive a domain reload.
        /// </summary>
        private static void OnBeforeAssemblyReload()
        {
            s_queue.AbortDownloads();
            StopUpdating();
        }

        /// <summary>
        /// Stops updating the shared queue.
        /// </summary>
        private static void StopUpdating()
        {
            EditorApplication.update -= Update;
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            s_isUpdating = false;
        }

        /// <summary>
        /// Saves the given up downloads in <see cref="SessionState"/>.
        /// </summary>
        private static void SaveGivenUp()
        {
            SessionState.SetString(GivenUpSessionKey, string.Join(GivenUpSeparator.ToString(), s_policy.GivenUpKeys));
        }

        #endregion

    }

}
