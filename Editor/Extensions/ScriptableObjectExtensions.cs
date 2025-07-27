using UnityEngine;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Extension functions for <see cref="ScriptableObject"/> instances.
    /// </summary>
    public static class ScriptableObjectExtensions
    {

        /// <inheritdoc cref="ScriptableObjectUtility.GetScriptPath(ScriptableObject)"/>
        public static string GetScriptPath(this ScriptableObject obj)
        {
            return ScriptableObjectUtility.GetScriptPath(obj);
        }

        /// <inheritdoc cref="ScriptableObjectUtility.ResetToDefaults(ScriptableObject)"/>
        public static void ResetToDefaults(this ScriptableObject obj)
        {
            ScriptableObjectUtility.ResetToDefaults(obj);
        }

    }

}