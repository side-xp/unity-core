using System;

using UnityEditor;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Miscellaneous extension functions for <see cref="MonoScript"/> and assets.
    /// </summary>
    public static class MonoScriptExtensions
    {

        /// <inheritdoc cref="ScriptUtility.GetScriptType(MonoScript, out Type)"/>
        public static bool GetScriptType(this MonoScript script, out Type type)
        {
            return ScriptUtility.GetScriptType(script, out type);
        }

    }

}