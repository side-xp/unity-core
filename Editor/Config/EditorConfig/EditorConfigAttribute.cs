using System;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Identifies an object as representing settings for editor features, and defines where its data is stored.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class EditorConfigAttribute : Attribute
    {

        /// <summary>
        /// The scope of the settings contained in the object.
        /// </summary>
        public EEditorConfigScope Scope { get; set; } = EEditorConfigScope.Project;

        /// <summary>
        /// The name of the file (without extension) that stores the settings, or the <see cref="UnityEditor.EditorPrefs"/> key if the
        /// scope is <see cref="EEditorConfigScope.Editor"/>.
        /// </summary>
        /// <remarks>
        /// If not defined, the full name of the type is used. Defining it explicitly lets you rename or move the type without losing the
        /// stored settings.
        /// </remarks>
        public string Key { get; set; } = null;

        /// <inheritdoc cref="EditorConfigAttribute"/>
        /// <param name="scope"><inheritdoc cref="Scope" path="/summary"/></param>
        public EditorConfigAttribute(EEditorConfigScope scope)
        {
            Scope = scope;
        }

    }

}
