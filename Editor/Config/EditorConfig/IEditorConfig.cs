using System;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Qualifies an object as representing editor settings.
    /// </summary>
    [Obsolete("Inherit from EditorConfig<T> instead, and override its OnLoaded() function in place of PostLoad().")]
    public interface IEditorConfig
    {

        /// <summary>
        /// Called after the settings has been loaded from <see cref="EditorConfigUtility"/>.
        /// </summary>
        void PostLoad();

    }

}
