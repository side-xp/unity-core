namespace SideXP.Core
{

    /// <summary>
    /// Defines how a reference to an object should be queried.
    /// </summary>
    /// <remarks>This is used by <see cref="SideXP.Core.EditorOnly.ObjectUtility"/> and <see cref="RuntimeObjectUtility"/>.</remarks>
    [System.Flags]
    public enum FFindObjectStrategy
    {

        None = 0,

        /// <summary>
        /// Find the reference in this <see cref="UnityEngine.GameObject"/>.
        /// </summary>
        Self = 1 << 0,

        /// <summary>
        /// Find the reference in the direct parent of this <see cref="UnityEngine.GameObject"/>.
        /// </summary>
        Parent = 1 << 1,

        /// <summary>
        /// Find the reference in the direct children of this <see cref="UnityEngine.GameObject"/>.
        /// </summary>
        Children = 1 << 2,

        /// <summary>
        /// Find the reference in the scene.
        /// </summary>
        Scene = 1 << 3,

        /// <summary>
        /// If this option is enabled with <see cref="Parent"/>, the reference will be queried from the direct parent to the
        /// top-level one.<br/>
        /// if this option is enabled with <see cref="Children"/>, the reference will be queried recursively among this
        /// <see cref="UnityEngine.GameObject"/>'s children.
        /// </summary>
        Recursive = 1 << 4,

        // Presets

        Anywhere = Self | Parent | Children | Scene,
        WholeHierarchy = Self | Parent | Children | Recursive,
        DirectHierarchy = Self | Parent | Children,
        SelfOrParent = Self | Parent,
        SelfOrChildren = Self | Children,

    }

}