using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevel;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Utility window that displays the Player Loops and subsystems registered in the engine.
    /// </summary>
    public class PlayerLoopEditorWindow : EditorWindow
    {

        #region Fields

        private const string WindowTitle = "Player Loop";
        private const string MenuItem = EditorConstants.EditorWindowMenuViewers + "/Player Loops Viewer";

        [SerializeField]
        private Vector2 _scrollPosition = Vector2.zero;

        [SerializeField]
        private List<string> _expandedPlayerLoopTypeNames = new List<string>();

        #endregion


        #region Lifecycle

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += HandlePlayModeStateChange;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChange;
        }

        #endregion


        #region Public API

        [MenuItem(MenuItem)]
        public static PlayerLoopEditorWindow Open()
        {
            PlayerLoopEditorWindow window = GetWindow<PlayerLoopEditorWindow>(false, WindowTitle, true);
            window.Show();
            return window;
        }

        #endregion


        #region UI

        private void OnGUI()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("Reset Default Player Loop"))
                {
                    if (!Application.isPlaying || EditorUtility.DisplayDialog("Reset Default Player Loop", "This operation will reset the Player Loop to its default state, and so remove any change applied at runtime.\nProceed?", "Yes, reset Player Loop", "No"))
                    {
                        PlayerLoop.SetPlayerLoop(PlayerLoop.GetDefaultPlayerLoop());
                    }
                }
            }

            using (var scope = new EditorGUILayout.ScrollViewScope(_scrollPosition))
            {
                DisplaySubsystems(PlayerLoop.GetCurrentPlayerLoop().subSystemList);
                _scrollPosition = scope.scrollPosition;
            }

            // Display the given subsystems on GUI
            void DisplaySubsystems(PlayerLoopSystem[] subsystems, int depth = 0)
            {
                // Cancel if the given subsystems array is not valid
                if (subsystems == null || subsystems.Length <= 0)
                    return;

                // Indent based on depth level
                using (new IndentedScope(depth))
                {
                    // For each given subsystem
                    foreach (PlayerLoopSystem subsystem in subsystems)
                    {
                        // Error if the current subsystem type is not valid
                        if (subsystem.type == null)
                        {
                            Rect rect = EditorGUILayout.GetControlRect(false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                            EditorGUI.HelpBox(rect, "Invalid subsystem type", MessageType.Error);
                            continue;
                        }

                        PlayerLoopSystem[] currentSubsystems = subsystem.subSystemList;
                        bool isExpanded = false;

                        // Display a foldout field if the current subsystem contains other subsystems
                        if (currentSubsystems != null && currentSubsystems.Length > 0)
                        {
                            using (var scope = new EditorGUI.ChangeCheckScope())
                            {
                                isExpanded = EditorGUILayout.Foldout(IsExpanded(subsystem.type), subsystem.type.FullName, true, EditorStyles.foldout.Bold());
                                if (scope.changed)
                                {
                                    if (isExpanded)
                                        _expandedPlayerLoopTypeNames.AddOnce(subsystem.type.AssemblyQualifiedName);
                                    else
                                        _expandedPlayerLoopTypeNames.Remove(subsystem.type.AssemblyQualifiedName);
                                }
                            }
                        }
                        // Otherwise, display a basic label field
                        else
                        {
                            EditorGUILayout.LabelField(subsystem.type.FullName);
                        }

                        // Display subsystems from the current one if applicable
                        if (isExpanded)
                            DisplaySubsystems(currentSubsystems, depth + 1);
                    }
                }
            }
        }

        #endregion


        #region Private API

        /// <summary>
        /// Checks if a given subsystem is expanded on GUI.
        /// </summary>
        /// <param name="subsystemType">The type of the subsystem to check.</param>
        /// <returns>Returns true if the given subsystem is expanded on GUI.</returns>
        private bool IsExpanded(Type subsystemType)
        {
            return _expandedPlayerLoopTypeNames.Contains(subsystemType.AssemblyQualifiedName);
        }

        /// <inheritdoc cref="EditorApplication.playModeStateChanged"/>
        private void HandlePlayModeStateChange(PlayModeStateChange change)
        {
            Repaint();
        }

        #endregion

    }

}
