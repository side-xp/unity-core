using System;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Former name of <see cref="CoreProjectSettings"/>, kept so existing code using <c>CoreEditorConfig.I</c> still compiles.
    /// </summary>
    [Obsolete("Use " + nameof(CoreProjectSettings) + " instead.")]
    public static class CoreEditorConfig
    {

        /// <inheritdoc cref="EditorConfig{T}.Instance"/>
        public static CoreProjectSettings Instance => CoreProjectSettings.Instance;

        /// <inheritdoc cref="EditorConfig{T}.Instance"/>
        public static CoreProjectSettings I => Instance;

    }

}
