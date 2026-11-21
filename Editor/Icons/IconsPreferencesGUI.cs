using System;
using System.IO;

using UnityEngine;
using UnityEditor;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Draws the editor icons section of <see cref="CorePreferences"/>, and moves the icons when their folder changes.
    /// </summary>
    internal static class IconsPreferencesGUI
    {

        #region Fields

        /// <summary>
        /// Title of the dialogs and progress bar displayed when the icons folder changes.
        /// </summary>
        private const string MoveTitle = "Move icons";

        #endregion


        #region UI

        /// <summary>
        /// Draws the editor icons section, using automatic layout.
        /// </summary>
        /// <param name="preferences">The preferences to edit.</param>
        /// <param name="iconsFolderProperty">The serialized property of the icons folder, drawn with its folder path drawer.</param>
        internal static void DrawLayout(CorePreferences preferences, SerializedProperty iconsFolderProperty)
        {
            bool isDefaultFolder = string.IsNullOrEmpty(preferences.IconsFolder);
            EditorGUILayout.LabelField("Icons", EditorStyles.boldLabel);

            Rect fieldRect = EditorGUILayout.GetControlRect(true);
            Rect resetButtonRect = new Rect(fieldRect.xMax - MoreGUI.WidthS, fieldRect.y, MoreGUI.WidthS, fieldRect.height);
            fieldRect.width -= MoreGUI.WidthS + MoreGUI.HMargin;

            string previousFolder = iconsFolderProperty.stringValue;
            EditorGUI.PropertyField(fieldRect, iconsFolderProperty);
            string selectedFolder = iconsFolderProperty.stringValue;

            // The drawer applies the new value right away, but it must only be saved once the icons have been moved
            if (selectedFolder != previousFolder)
            {
                iconsFolderProperty.stringValue = previousFolder;
                iconsFolderProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                Undo.ClearUndo(preferences);

                // An emptied field means the default folder
                ChangeFolder(preferences, string.IsNullOrWhiteSpace(selectedFolder) ? IconStorage.DefaultRootFolder : selectedFolder);

                // The folder panel and dialogs break the current layout
                GUIUtility.ExitGUI();
            }

            using (new EditorGUI.DisabledScope(isDefaultFolder))
            {
                if (GUI.Button(resetButtonRect, new GUIContent("Reset", "Move the icons back to the default folder.")))
                {
                    ChangeFolder(preferences, IconStorage.DefaultRootFolder);
                    GUIUtility.ExitGUI();
                }
            }

            if (isDefaultFolder)
                EditorGUILayout.LabelField($"Empty: the default folder is used ({IconStorage.DefaultRootFolder}).", EditorStyles.miniLabel);
        }

        #endregion


        #region Private API

        /// <summary>
        /// Moves the icons to another folder, and saves it in the preferences if the move succeeds.
        /// </summary>
        /// <param name="preferences">The preferences to update.</param>
        /// <param name="targetFolder">The absolute path to the folder where to move the icons.</param>
        private static void ChangeFolder(CorePreferences preferences, string targetFolder)
        {
            // @todo Pause icon downloads while moving, once the download queue exists.
            string sourceFolder = IconStorage.ConfiguredRootFolder;
            if (!IconFolderMover.CanMove(sourceFolder, targetFolder, out string error))
            {
                EditorUtility.DisplayDialog(MoveTitle, error, "OK");
                return;
            }

            targetFolder = IconStorage.NormalizePath(targetFolder);
            if (Directory.Exists(sourceFolder))
            {
                string message = $"The icons will be moved from:\n{sourceFolder}\n\nTo:\n{targetFolder}\n\n"
                    + "Other open editors will use the new folder after their next domain reload.";
                if (!EditorUtility.DisplayDialog(MoveTitle, message, "Move", "Cancel"))
                    return;
            }

            // The parent folders of the default folder are only created for it, so they are deleted if empty after the move
            bool isSourceDefault = IsSameFolder(sourceFolder, IconStorage.DefaultRootFolder);
            int emptyParentsToDelete = isSourceDefault
                ? IconStorage.DefaultRelativeFolder.Split(PathUtility.DefaultDirectorySeparator).Length - 1
                : 0;

            bool isMoved;
            try
            {
                isMoved = IconFolderMover.Move(sourceFolder, targetFolder, emptyParentsToDelete, ReportProgress, out error);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            if (!isMoved)
            {
                EditorUtility.DisplayDialog(MoveTitle, $"{error}\n\nThe icons are still in:\n{sourceFolder}", "OK");
                return;
            }

            // Store an empty path for the default folder, so it follows the OS settings
            bool isTargetDefault = IsSameFolder(targetFolder, IconStorage.DefaultRootFolder);
            if (!preferences.SetIconsFolder(isTargetDefault ? null : targetFolder))
                Debug.LogError($"The icons have been moved to {targetFolder}, but the preferences couldn't be saved.");
        }

        /// <summary>
        /// Displays the progress of a move in a progress bar.
        /// </summary>
        /// <param name="copiedCount">The number of copied files.</param>
        /// <param name="totalCount">The total number of files to copy.</param>
        /// <returns>Returns true if the user cancelled the move.</returns>
        private static bool ReportProgress(int copiedCount, int totalCount)
        {
            float progress = totalCount > 0 ? (float)copiedCount / totalCount : 0;
            return EditorUtility.DisplayCancelableProgressBar(MoveTitle, $"Copying files ({copiedCount} / {totalCount})", progress);
        }

        /// <summary>
        /// Checks if two paths point to the same folder.
        /// </summary>
        /// <param name="a">The first path.</param>
        /// <param name="b">The second path.</param>
        /// <returns>Returns true if both paths point to the same folder.</returns>
        private static bool IsSameFolder(string a, string b)
        {
            return string.Equals(IconStorage.NormalizePath(a), IconStorage.NormalizePath(b), StringComparison.OrdinalIgnoreCase);
        }

        #endregion

    }

}
