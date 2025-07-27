using System;

namespace SideXP.Core
{

    /// <summary>
    /// Defines options for a probability collection and how it behaves in the inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ProbabilityCollectionOptionsAttribute : Attribute
    {

        /// <summary>
        /// If enabled, the add and remove button are enabled in the inspector, allowing user to add or remove items in the collection.
        /// </summary>
        public bool AllowAddOrRemove { get; set; } = true;

        /// <summary>
        /// If enabled, allow the user to reorder the items in the collection from the inspector.
        /// </summary>
        public bool Reorderable { get; set; } = true;

    }

}