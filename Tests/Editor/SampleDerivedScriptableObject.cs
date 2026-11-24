namespace SideXP.Core.Tests
{

    /// <summary>
    /// A file-matched type derived from <see cref="SampleScriptableObject"/>, used by the editor tests that search the assets of a type
    /// and of its derived types. The class name matches the file name on purpose, so its assets can be saved and loaded.
    /// </summary>
    public class SampleDerivedScriptableObject : SampleScriptableObject { }

}
