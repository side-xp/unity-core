using UnityEngine;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// A file-matched <see cref="ScriptableObject"/> used by the editor tests (e.g. to resolve its script path via
    /// <see cref="MonoScript"/>, and to exercise reset-to-defaults). The class name matches the file name on purpose so
    /// Unity associates a resolvable script asset with it.
    /// </summary>
    public class SampleScriptableObject : ScriptableObject
    {
        public int number = 3;
        public string text = "default";
    }

}
