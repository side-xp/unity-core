using System;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Identifies an object as representing settings for editor features.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class EditorConfigAttribute : Attribute
    {

        /// <summary>
        /// The scope of the settings contained in the object.
        /// </summary>
        public EEditorConfigScope Scope { get; set; } = EEditorConfigScope.Project;

        /// <inheritdoc cref="EditorConfigAttribute"/>
        /// <param name="scope"><inheritdoc cref="Scope" path="/summary"/></param>
        public EditorConfigAttribute(EEditorConfigScope scope)
        {
            Scope = scope;
        }

    }

}
