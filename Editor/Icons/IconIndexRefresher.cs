using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Runs the refresh of the icons index from editor updates, and reports its outcome.
    /// </summary>
    /// <remarks>
    /// Only one refresh runs at a time: asking for a refresh while one is running waits for that one instead of starting another.
    /// From Unity 2020.1, every refresh is displayed as a cancellable task in the Background Tasks window. On older versions, only
    /// interactive refreshes display their progress, in a modal progress bar.
    /// </remarks>
    internal static class IconIndexRefresher
    {

        #region Fields

        /// <summary>
        /// Title of the progress bar and dialogs of a refresh.
        /// </summary>
        private const string Title = "Refresh icons index";

        /// <summary>
        /// The refresh in progress, or null if none.
        /// </summary>
        private static IconIndexRefresh s_job = null;

        /// <summary>
        /// Whether the refresh in progress displays its outcome in a dialog, and a progress bar if Background Tasks are not supported.
        /// </summary>
        private static bool s_isInteractive = false;

        /// <summary>
        /// The task of the refresh in progress in the Background Tasks window, or null if none.
        /// </summary>
        private static EditorProgressTask s_task = null;

        /// <summary>
        /// The functions to call when the refresh in progress is done.
        /// </summary>
        private static List<Action<IconIndexRefresh>> s_callbacks = new List<Action<IconIndexRefresh>>();

        #endregion


        #region Public API

        /// <summary>
        /// Checks if a refresh is in progress.
        /// </summary>
        internal static bool IsRefreshing => s_job != null;

        /// <summary>
        /// Starts refreshing the icons index, or waits for the refresh in progress.
        /// </summary>
        /// <param name="isInteractive">
        /// If enabled, displays the outcome in a dialog, and a cancellable progress bar if Background Tasks are not supported. Meant
        /// for refreshes asked by the user. Otherwise, only failures are logged. If a refresh is already running, this makes it
        /// interactive.
        /// </param>
        /// <param name="onComplete">Called when the refresh is done, whether it succeeded or not. Can be null.</param>
        internal static void Refresh(bool isInteractive, Action<IconIndexRefresh> onComplete = null)
        {
            if (onComplete != null)
                s_callbacks.Add(onComplete);

            s_isInteractive |= isInteractive;
            if (s_job != null)
                return;

            s_job = new IconIndexRefresh();
            s_job.Start();
            s_task = new EditorProgressTask(Title, s_job.Description, OnTaskCancelled);
            EditorApplication.update += Update;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
        }

        #endregion


        #region Private API

        /// <summary>
        /// Moves the refresh forward, and updates its progress display.
        /// </summary>
        private static void Update()
        {
            s_job.Update();

            if (!s_job.IsDone)
            {
                s_task.Report(s_job.Progress, s_job.Description);
                if (IsUsingProgressBar(s_isInteractive)
                    && EditorUtility.DisplayCancelableProgressBar(Title, s_job.Description, s_job.Progress))
                {
                    s_job.Cancel();
                }
            }

            if (s_job.IsDone)
                Finish();
        }

        /// <summary>
        /// Called when the user cancels the refresh from the Background Tasks window.
        /// </summary>
        /// <returns>Returns true, since the refresh can always be cancelled.</returns>
        private static bool OnTaskCancelled()
        {
            // Ended by the next update, which reports the outcome and notifies the callbacks
            if (s_job != null)
                s_job.Cancel();

            return true;
        }

        /// <summary>
        /// Checks if the progress of a refresh is displayed in a modal progress bar.
        /// </summary>
        /// <param name="isInteractive">Whether the refresh is interactive.</param>
        /// <returns>Returns true if the refresh is interactive, and Background Tasks are not supported.</returns>
        private static bool IsUsingProgressBar(bool isInteractive)
        {
            return isInteractive && !EditorProgressTask.IsSupported;
        }

        /// <summary>
        /// Cancels the refresh before scripts are reloaded, since it can't survive a domain reload.
        /// </summary>
        private static void OnBeforeAssemblyReload()
        {
            if (s_job == null)
                return;

            s_job.Cancel();
            Finish();
        }

        /// <summary>
        /// Ends the refresh: stops the updates, reports the outcome, and notifies the callbacks.
        /// </summary>
        private static void Finish()
        {
            EditorApplication.update -= Update;
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;

            IconIndexRefresh job = s_job;
            EditorProgressTask task = s_task;
            bool isInteractive = s_isInteractive;
            Action<IconIndexRefresh>[] callbacks = s_callbacks.ToArray();
            s_job = null;
            s_task = null;
            s_isInteractive = false;
            s_callbacks.Clear();

            if (IsUsingProgressBar(isInteractive))
                EditorUtility.ClearProgressBar();

            if (job.IsCancelled)
                task.Cancel();
            else if (job.IsSuccess)
                task.Succeed(job.Description);
            else
                task.Fail(job.Error);

            Report(job, isInteractive);
            foreach (Action<IconIndexRefresh> callback in callbacks)
                callback(job);
        }

        /// <summary>
        /// Reports the outcome of a refresh.
        /// </summary>
        /// <param name="job">The completed refresh.</param>
        /// <param name="isInteractive">If enabled, displays the outcome in a dialog. Otherwise, only failures are logged.</param>
        private static void Report(IconIndexRefresh job, bool isInteractive)
        {
            if (job.CleanUpErrors.Count > 0)
            {
                Debug.LogWarning("Some downloaded icons couldn't be cleaned up after the icons index refresh. They're harmless, and "
                    + $"deleted with the other downloaded icons.\n{string.Join("\n", job.CleanUpErrors)}");
            }

            // A cancelled refresh has been stopped by the user or by a domain reload, there's nothing to report
            if (job.IsCancelled)
                return;

            if (!job.IsSuccess)
            {
                if (isInteractive)
                    EditorUtility.DisplayDialog(Title, job.Error, "OK");
                else
                    Debug.LogWarning($"The icons index couldn't be refreshed: {job.Error}");

                return;
            }

            if (!isInteractive)
                return;

            string message = job.IsUpToDate
                ? $"The icons index is already up to date (Tabler {job.Version})."
                : $"The icons index has been updated to Tabler {job.Version}.";
            EditorUtility.DisplayDialog(Title, message, "OK");
        }

        #endregion

    }

}
