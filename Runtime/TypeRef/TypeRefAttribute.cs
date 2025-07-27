using System;

using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Marks a serialized string property as containing the <see cref="Type.AssemblyQualifiedName"/> of a <see cref="Type"/>.
    /// </summary>
    /// <remarks>
    /// This is used to ensure that the serialized type name is correctly synced with the actual type declared in the script that declares
    /// the named type.<br/>
    /// To be clear, this is not a magic attribute that makes types serializable, but a utility to make it happen. The internal logic makes
    /// sure that even if the "real" type's name changes, the value of this property automatically changes too to match the new type name.
    /// </remarks>
    public class TypeRefAttribute : PropertyAttribute
    {

        /// <summary>
        /// If enabled, the field will be disabled in the inspector.
        /// </summary>
        public bool Readonly { get; set; } = false;

    }

}