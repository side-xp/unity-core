using System.Collections.Generic;

using UnityEditor;
using UnityEditor.Build;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Utility class for managing scripting define symbols through code.
    /// </summary>
    public static class DefinesUtility
    {

        public const string DEFINES_SEPARATOR = ";";

        /// <summary>
        /// Gets the scripting define symbols from Player Settings for the current target platform.
        /// </summary>
        /// <inheritdoc cref="GetDefines(BuildTargetGroup)"/>
        public static string[] GetDefines()
        {
            return GetDefines(EditorUserBuildSettings.selectedBuildTargetGroup);
        }

        /// <summary>
        /// Gets the scripting define symbols from Player Settings for the given target platform.
        /// </summary>
        /// <param name="platform">The platform of which you want to get the scripting define symbols.</param>
        /// <returns>Returns the defines from Player Settings.</returns>
        public static string[] GetDefines(BuildTargetGroup platform)
        {
#if UNITY_6000_OR_NEWER
            NamedBuildTarget buildTarget = NamedBuildTarget.FromBuildTargetGroup(platform);
            PlayerSettings.GetScriptingDefineSymbols(buildTarget, out string[] defines);
#else
            string definesStr = PlayerSettings.GetScriptingDefineSymbolsForGroup(platform);
            string[] defines = definesStr.Split(DEFINES_SEPARATOR);
            for (int i = 0; i < defines.Length; i++)
                defines[i] = defines[i].Trim();
#endif
            return defines;
        }

        /// <summary>
        /// Checks if the named scripting define symbol is set for the current target platform.
        /// </summary>
        /// <inheritdoc cref="IsDefined(string, BuildTargetGroup)"/>
        public static bool IsDefined(string define)
        {
            return IsDefined(define, EditorUserBuildSettings.selectedBuildTargetGroup);
        }

        /// <summary>
        /// Checks if the named scripting define symbol is set for the given target platform.
        /// </summary>
        /// <param name="define">The define you want to check.</param>
        /// <param name="platform">The platform for which you want to check the define.</param>
        /// <returns>Returns true if the define is set.</returns>
        public static bool IsDefined(string define, BuildTargetGroup platform)
        {
            List<string> defines = new List<string>(GetDefines(platform));
            return defines.Contains(define);
        }

        /// <summary>
        /// Adds the named scripting define symbol for the current target platform.
        /// </summary>
        /// <inheritdoc cref="AddDefine(string, BuildTargetGroup)"/>
        public static bool AddDefine(string define)
        {
            return AddDefine(define, EditorUserBuildSettings.selectedBuildTargetGroup);
        }

        /// <summary>
        /// Adds the named scripting define symbol for the given target platform.
        /// </summary>
        /// <param name="define">The define you want to add.</param>
        /// <param name="platform">The platform to which the define is added.</param>
        /// <returns>Returns true if the define has been added successfully, or false if it was already registered.</returns>
        public static bool AddDefine(string define, BuildTargetGroup platform)
        {
            List<string> defines = new List<string>(GetDefines(platform));
            if (defines.Contains(define))
                return false;

            defines.Add(define);
#if UNITY_6000_OR_NEWER
            NamedBuildTarget buildTarget = NamedBuildTarget.FromBuildTargetGroup(platform);
            PlayerSettings.SetScriptingDefineSymbols(buildTarget, defines.ToArray());
#elif UNITY_2020_2_OR_NEWER
            PlayerSettings.SetScriptingDefineSymbolsForGroup(platform, defines.ToArray());
#else
            PlayerSettings.SetScriptingDefineSymbolsForGroup(platform, string.Join(DEFINES_SEPARATOR, defines));
#endif
            return true;
        }

        /// <summary>
        /// Removes the named scripting define symbol for the current target platform.
        /// </summary>
        /// <inheritdoc cref="RemoveDefine(string, BuildTargetGroup)"/>
        public static bool RemoveDefine(string define)
        {
            return RemoveDefine(define, EditorUserBuildSettings.selectedBuildTargetGroup);
        }

        /// <summary>
        /// Removes the named scripting define symbol for the given target platform.
        /// </summary>
        /// <param name="define">The define you want to remove.</param>
        /// <param name="platform">The platform from which the define is removed.</param>
        /// <returns>Returns true if the define has been removed successfully, or false if it wasn't used.</returns>
        public static bool RemoveDefine(string define, BuildTargetGroup platform)
        {
            List<string> defines = new List<string>(GetDefines(platform));
            if (!defines.Contains(define))
                return false;

            defines.Remove(define);
#if UNITY_6000_OR_NEWER
            NamedBuildTarget buildTarget = NamedBuildTarget.FromBuildTargetGroup(platform);
            PlayerSettings.SetScriptingDefineSymbols(buildTarget, defines.ToArray());
#elif UNITY_2020_2_OR_NEWER
            PlayerSettings.SetScriptingDefineSymbolsForGroup(platform, defines.ToArray());
#else
            PlayerSettings.SetScriptingDefineSymbolsForGroup(platform, string.Join(DEFINES_SEPARATOR, defines));
#endif
            return true;
        }

    }

}