namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Qualifies an object as representing editor settings.
    /// </summary>
    public interface IEditorConfig
    {

        /// <summary>
        /// Called after the settings has been loaded from <see cref="EditorConfigUtility"/>.
        /// </summary>
        void PostLoad();

    }

}
