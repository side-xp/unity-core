using System.Text;

using UnityEngine;
using UnityEngine.LowLevel;

namespace SideXP.Core
{

    /// <summary>
    /// Miscellaneous utility functions and extensions for working with <see cref="UnityEngine.LowLevel.PlayerLoop"/> system.
    /// </summary>
    /// <seealso href="https://www.youtube.com/watch?v=ilvmOQtl57c"/>
    public static class PlayerLoopUtility
    {

        #region Public API

        /// <summary>
        /// Inserts a given Player Loop subsystem as child of a given system type.
        /// </summary>
        /// <remarks>This overload will search for the expected system type from the current Player Loop. Also, it will apply changes
        /// automatically by calling <see cref="PlayerLoop.SetPlayerLoop(PlayerLoopSystem)"/> after the system is inserted successfully, and
        /// if <paramref name="skipApply"/> is disabled.</remarks>
        /// <typeparam name="T">The type of the system to which the given system will be added as a child.</typeparam>
        /// <param name="system">The subsystem to add as child of the given system type.</param>
        /// <param name="index">The index at which to insert the subsystem in the list. If negative or out of bounds, add the subsystem at
        /// the end of the list.</param>
        /// <param name="skipApply">By default, this function will apply the changes by calling
        /// <see cref="PlayerLoop.SetPlayerLoop(PlayerLoopSystem)"/> after the system is inserted successfully. If enabled, you must call
        /// that function by yourself to apply the changes.</param>
        /// <returns>Returns true if the subsystem has been inserted successfully.</returns>
        public static bool InsertSubsystem<T>(in PlayerLoopSystem system, int index = -1, bool skipApply = false)
        {
            PlayerLoopSystem currentLoop = PlayerLoop.GetCurrentPlayerLoop();
            if (!InsertSystemInto<T>(ref currentLoop, in system, index))
                return false;

            if (!skipApply)
                PlayerLoop.SetPlayerLoop(currentLoop);

            return true;
        }

        /// <param name="from">The system from which to search for the expected subsystem type.</param>
        /// <remarks>This overload won't apply the Player loop changes: you must call
        /// <see cref="PlayerLoop.SetPlayerLoop(PlayerLoopSystem)"/> to do so.</remarks>
        /// <inheritdoc cref="InsertSubsystem{T}(in PlayerLoopSystem, int, bool)"/>
        public static bool InsertSystemInto<T>(ref PlayerLoopSystem from, in PlayerLoopSystem system, int index = -1)
        {
            // Try to insert the system deeper in the subystems hierarchy if the expected type doesn't match
            if (from.type != typeof(T))
                return TryInsertSystem(ref from, system, index);

            using (var subsystemsList = new ListPoolScope<PlayerLoopSystem>())
            {
                if (from.subSystemList != null)
                    subsystemsList.AddRange(from.subSystemList);

                // Fix index
                if (index < 0 || index > subsystemsList.Count)
                    index = subsystemsList.Count;

                subsystemsList.Insert(index, system);
                from.subSystemList = subsystemsList.ToArray();
            }
            return true;

            // Try to insert a given system inside a subsystem of a given type from a given parent one
            bool TryInsertSystem(ref PlayerLoopSystem parent, in PlayerLoopSystem system, int index)
            {
                if (parent.subSystemList == null)
                    return false;

                for (int i = 0; i < parent.subSystemList.Length; i++)
                {
                    if (!InsertSystemInto<T>(ref parent.subSystemList[i], in system, index))
                        continue;

                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Removes all subsystems of a given system type from the current Player Loop.
        /// </summary>
        /// <remarks>This overload will search for the expected system type from the current Player Loop. Also, it will apply changes
        /// automatically by calling <see cref="PlayerLoop.SetPlayerLoop(PlayerLoopSystem)"/> if a system has been removed successfully, and
        /// if <paramref name="skipApply"/> is disabled.</remarks>
        /// <typeparam name="T">The type of the system to remove.</typeparam>
        /// <param name="skipApply">By default, this function will apply the changes by calling
        /// <see cref="PlayerLoop.SetPlayerLoop(PlayerLoopSystem)"/> if a system has been removed successfully. If enabled, you must call
        /// that function by yourself to apply the changes.</param>
        /// <returns>Returns true if a subsystem has been removed successfully.</returns>
        public static bool RemoveSubsystem<T>(bool skipApply = false)
        {
            PlayerLoopSystem currentLoop = PlayerLoop.GetCurrentPlayerLoop();
            if (!RemovePlayerLoop<T>(ref currentLoop))
                return false;

            if (!skipApply)
                PlayerLoop.SetPlayerLoop(currentLoop);

            return true;
        }

        /// <param name="from">The system from which to search for the expected subsystem type.</param>
        /// <remarks>This overload won't apply the Player loop changes: you must call
        /// <see cref="PlayerLoop.SetPlayerLoop(PlayerLoopSystem)"/> to do so.</remarks>
        /// <inheritdoc cref="RemoveSubsystem{T}(bool)"/>
        public static bool RemovePlayerLoop<T>(ref PlayerLoopSystem from)
        {
            if (from.subSystemList == null)
                return false;

            using (var subsystemsList = new ListPoolScope<PlayerLoopSystem>())
            {
                subsystemsList.AddRange(from.subSystemList);
                bool didRemoveSubsystem = false;

                for (int i = subsystemsList.Count - 1; i >= 0; i--)
                {
                    if (subsystemsList[i].type == typeof(T))
                    {
                        subsystemsList.RemoveAt(i);
                        didRemoveSubsystem = true;
                        continue;
                    }
                }

                if (didRemoveSubsystem)
                    from.subSystemList = subsystemsList.ToArray();

                // Try remove recursively
                for (int i = 0; i < from.subSystemList.Length; i++)
                {
                    if (RemovePlayerLoop<T>(ref from.subSystemList[i]))
                        didRemoveSubsystem = true;
                }

                return didRemoveSubsystem;
            }
        }

        /// <summary>
        /// Logs a message in console that displays the full subsystems hierarchy of the current Player Loop.
        /// </summary>
        /// <inheritdoc cref="PrintPlayerLoop(in PlayerLoopSystem)"/>
        public static void PrintPlayerLoop()
        {
            PrintPlayerLoop(PlayerLoop.GetCurrentPlayerLoop());
        }

        /// <summary>
        /// Logs a message in console that displays the full subsystems hierarchy from a given system.
        /// </summary>
        /// <param name="system">The system from which to display the subsystems hierarchy.</param>
        public static void PrintPlayerLoop(in PlayerLoopSystem system)
        {
            StringBuilder messageBuilder = new StringBuilder();
            messageBuilder.AppendLine("Unity Player Loop:");

            PrintPlayerLoop(in system, messageBuilder, 0);
            Debug.Log(messageBuilder.ToString());

            void PrintPlayerLoop(in PlayerLoopSystem system, StringBuilder messageBuilder, int depth)
            {
                if (system.type != null)
                    messageBuilder.AppendLine("\t".Repeat(depth) + system.type.FullName);

                if (system.subSystemList == null)
                    return;

                foreach (PlayerLoopSystem subsystem in system.subSystemList)
                    PrintPlayerLoop(in subsystem, messageBuilder, depth + 1);
            }
        }

        #endregion

    }

}
