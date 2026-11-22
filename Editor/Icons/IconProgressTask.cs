using System;

using UnityEngine;
#if UNITY_2020_1_OR_NEWER
using UnityEditor;
#endif

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// A task displayed in the Background Tasks window and in the progress bar at the bottom right of the editor.
    /// </summary>
    /// <remarks>
    /// Uses the <c>UnityEditor.Progress</c> API, available from Unity 2020.1. On older versions, this class does nothing, and
    /// <see cref="IsSupported"/> is false so callers can display progress another way.
    /// </remarks>
    internal class IconProgressTask
    {

        #region Fields

#if UNITY_2020_1_OR_NEWER
        /// <summary>
        /// The identifier of the task in the <see cref="Progress"/> API.
        /// </summary>
        private readonly int _id = -1;
#endif

        /// <summary>
        /// The last reported progress, so the same progress is not reported again on each update.
        /// </summary>
        private float _lastProgress = -1f;

        /// <summary>
        /// The last reported description, so the same description is not reported again on each update.
        /// </summary>
        private string _lastDescription = null;

        #endregion


        #region Lifecycle

        /// <summary>
        /// Starts a task.
        /// </summary>
        /// <param name="name">The name of the task.</param>
        /// <param name="description">The description of the current state of the task.</param>
        /// <param name="onCancel">
        /// Called when the user cancels the task from the Background Tasks window. Returns true if the task has been cancelled. If
        /// null, the task can't be cancelled.
        /// </param>
        internal IconProgressTask(string name, string description, Func<bool> onCancel = null)
        {
#if UNITY_2020_1_OR_NEWER
            _id = Progress.Start(name, description);
            if (onCancel != null)
                Progress.RegisterCancelCallback(_id, onCancel);
#endif
            _lastDescription = description;
        }

        #endregion


        #region Public API

        /// <summary>
        /// Checks if tasks can be displayed in this version of Unity.
        /// </summary>
        internal static bool IsSupported
        {
            get
            {
#if UNITY_2020_1_OR_NEWER
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Checks if the task is still running, as it may have been cancelled from the Background Tasks window.
        /// </summary>
        internal bool IsRunning
        {
            get
            {
#if UNITY_2020_1_OR_NEWER
                return Progress.Exists(_id) && Progress.GetStatus(_id) == Progress.Status.Running;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Reports the progress of the task, if it changed.
        /// </summary>
        /// <param name="progress">The progress of the task, between 0 and 1.</param>
        /// <param name="description">The description of the current state of the task.</param>
        internal void Report(float progress, string description)
        {
            progress = Mathf.Clamp01(progress);
            if (Mathf.Approximately(progress, _lastProgress) && description == _lastDescription)
                return;

            _lastProgress = progress;
            _lastDescription = description;
#if UNITY_2020_1_OR_NEWER
            if (IsRunning)
                Progress.Report(_id, progress, description);
#endif
        }

        /// <summary>
        /// Ends the task successfully.
        /// </summary>
        /// <param name="description">The final description of the task.</param>
        internal void Succeed(string description)
        {
#if UNITY_2020_1_OR_NEWER
            Finish(Progress.Status.Succeeded, description);
#endif
        }

        /// <summary>
        /// Ends the task with an error.
        /// </summary>
        /// <param name="error">The reason why the task failed, displayed as its description.</param>
        internal void Fail(string error)
        {
#if UNITY_2020_1_OR_NEWER
            Finish(Progress.Status.Failed, error);
#endif
        }

        /// <summary>
        /// Ends the task as cancelled.
        /// </summary>
        internal void Cancel()
        {
#if UNITY_2020_1_OR_NEWER
            Finish(Progress.Status.Canceled, _lastDescription);
#endif
        }

        #endregion


        #region Private API

#if UNITY_2020_1_OR_NEWER
        /// <summary>
        /// Ends the task, unless it's already ended (as when cancelled from the Background Tasks window).
        /// </summary>
        /// <param name="status">The final status of the task.</param>
        /// <param name="description">The final description of the task.</param>
        private void Finish(Progress.Status status, string description)
        {
            if (!IsRunning)
                return;

            if (!string.IsNullOrEmpty(description))
                Progress.SetDescription(_id, description);

            Progress.Finish(_id, status);
        }
#endif

        #endregion

    }

}
