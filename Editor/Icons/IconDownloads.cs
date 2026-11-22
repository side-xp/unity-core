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
    /// <item>From Unity 2020.1, the downloads are displayed as a single task in the Background Tasks window, which ends when the queue
    /// is empty. The task fails if some icons couldn't be downloaded.</item>
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
        /// Name of the task displayed in the Background Tasks window.
        /// </summary>
        private const string TaskName = "Download editor icons";

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

        /// <summary>
        /// The task of the downloads in the Background Tasks window, or null if no download is in progress.
        /// </summary>
        private static EditorProgressTask s_task = null;

        /// <summary>
        /// The number of icon styles stored or given up since the task started.
        /// </summary>
        private static int s_completedCount = 0;

        /// <summary>
        /// The number of icon styles given up since the task started.
        /// </summary>
        private static int s_givenUpCount = 0;

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
                StartTask();
            }
        }

        /// <summary>
        /// Aborts the downloads in progress, and forgets the shared queue and the given up downloads, without calling the pending
        /// callbacks. Meant for tests.
        /// </summary>
        internal static void Reset()
        {
            if (s_queue != null)
                s_queue.AbortDownloads();

            StopUpdating();
            CancelTask();
            s_queue = null;
            s_policy = null;
            SessionState.EraseString(GivenUpSessionKey);
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

            s_queue = new IconDownloadQueue(s_policy, _ => OnGivenUp());
        }

        /// <summary>
        /// Updates the shared queue and its task, and repaints the views if icons have been stored.
        /// </summary>
        private static void Update()
        {
            double time = EditorApplication.timeSinceStartup;
            int completedCount = s_queue.Update(time);
            s_completedCount += completedCount;
            if (completedCount > 0)
                InternalEditorUtility.RepaintAllViews();

            if (s_queue.Count > 0)
            {
                ReportTask(time);
                return;
            }

            StopUpdating();
            FinishTask();
        }

        /// <summary>
        /// Aborts the downloads in progress before scripts are reloaded, since they can't survive a domain reload.
        /// </summary>
        private static void OnBeforeAssemblyReload()
        {
            s_queue.AbortDownloads();
            StopUpdating();
            CancelTask();
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
        /// Called when the shared queue gives up a download: remembers it, and counts it for the task.
        /// </summary>
        private static void OnGivenUp()
        {
            s_givenUpCount++;
            SessionState.SetString(GivenUpSessionKey, string.Join(GivenUpSeparator.ToString(), s_policy.GivenUpKeys));
        }

        #endregion


        #region Task

        /// <summary>
        /// Starts the task of the downloads.
        /// </summary>
        private static void StartTask()
        {
            s_completedCount = 0;
            s_givenUpCount = 0;
            s_task = new EditorProgressTask(TaskName, $"0 / {s_queue.Count}");
        }

        /// <summary>
        /// Reports the progress of the downloads: the number of icon styles done out of all the requested ones, and the current one.
        /// </summary>
        /// <param name="time">The current time, in seconds.</param>
        private static void ReportTask(double time)
        {
            if (s_task == null)
                return;

            int totalCount = s_completedCount + s_queue.Count;
            string progress = $"{s_completedCount} / {totalCount}";
            string currentName = s_queue.CurrentDownloadName;

            string description;
            if (s_policy.IsPaused(time))
                description = $"{progress}: waiting for the network";
            else if (currentName != null)
                description = $"{progress}: {currentName}";
            else
                description = $"{progress}: waiting to retry";

            s_task.Report((float)s_completedCount / totalCount, description);
        }

        /// <summary>
        /// Ends the task of the downloads once the queue is empty: failed if some icons couldn't be downloaded.
        /// </summary>
        private static void FinishTask()
        {
            if (s_task == null)
                return;

            if (s_givenUpCount > 0)
                s_task.Fail($"{s_givenUpCount} of {s_completedCount} icons couldn't be downloaded.");
            else
                s_task.Succeed($"{s_completedCount} icons downloaded.");

            s_task = null;
        }

        /// <summary>
        /// Ends the task of the downloads as cancelled, as before a domain reload.
        /// </summary>
        private static void CancelTask()
        {
            if (s_task == null)
                return;

            s_task.Cancel();
            s_task = null;
        }

        #endregion

    }

}
