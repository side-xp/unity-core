namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// User preferences for the Core package features.
    /// </summary>
    /// <remarks>
    /// These preferences are stored in <see cref="UnityEditor.EditorPrefs"/>, so they're shared by all the projects (and all the Unity
    /// versions) on the user's machine.
    /// </remarks>
    [EditorConfig(EEditorConfigScope.Editor, Key = "SideXP.Core.EditorOnly.CorePreferences")]
    public class CorePreferences : EditorConfig<CorePreferences>
    {

    }

}
