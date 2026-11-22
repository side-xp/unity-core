namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// A change to apply to the stored files of an icon after the index has been refreshed.
    /// </summary>
    /// <remarks>
    /// Made by <see cref="IconIndexDiff.Compare(IconIndex, IconIndex)"/>, and applied by <see cref="IconIndexDiff.Apply"/>.
    /// </remarks>
    internal readonly struct IconIndexChange
    {

        #region Fields

        /// <summary>
        /// The type of the change.
        /// </summary>
        public EIconIndexChange Type { get; }

        /// <summary>
        /// The name of the icon.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The category of the icon when the change is applied, as written in the index.
        /// </summary>
        public string Category { get; }

        /// <summary>
        /// The category to move the icon to, for <see cref="EIconIndexChange.MoveFolder"/>. Null for other changes.
        /// </summary>
        public string TargetCategory { get; }

        /// <summary>
        /// The style of the files to delete, for <see cref="EIconIndexChange.DeleteStyleFiles"/>.
        /// </summary>
        public EIconStyle Style { get; }

        #endregion


        #region Lifecycle

        /// <summary>
        /// Creates a change.
        /// </summary>
        /// <param name="type"><inheritdoc cref="Type" path="/summary"/></param>
        /// <param name="name"><inheritdoc cref="Name" path="/summary"/></param>
        /// <param name="category"><inheritdoc cref="Category" path="/summary"/></param>
        /// <param name="targetCategory"><inheritdoc cref="TargetCategory" path="/summary"/></param>
        /// <param name="style"><inheritdoc cref="Style" path="/summary"/></param>
        private IconIndexChange(EIconIndexChange type, string name, string category, string targetCategory, EIconStyle style)
        {
            Type = type;
            Name = name;
            Category = category;
            TargetCategory = targetCategory;
            Style = style;
        }

        /// <summary>
        /// Creates a change that moves the folder of an icon to another category folder.
        /// </summary>
        /// <param name="name">The name of the icon.</param>
        /// <param name="category">The previous category of the icon.</param>
        /// <param name="targetCategory">The new category of the icon.</param>
        /// <returns>Returns the created change.</returns>
        internal static IconIndexChange MoveFolder(string name, string category, string targetCategory)
        {
            return new IconIndexChange(EIconIndexChange.MoveFolder, name, category, targetCategory, EIconStyle.Outline);
        }

        /// <summary>
        /// Creates a change that deletes the folder of an icon.
        /// </summary>
        /// <param name="name">The name of the icon.</param>
        /// <param name="category">The category of the icon.</param>
        /// <returns>Returns the created change.</returns>
        internal static IconIndexChange DeleteFolder(string name, string category)
        {
            return new IconIndexChange(EIconIndexChange.DeleteFolder, name, category, null, EIconStyle.Outline);
        }

        /// <summary>
        /// Creates a change that deletes the files of an icon for a given style.
        /// </summary>
        /// <param name="name">The name of the icon.</param>
        /// <param name="category">The category of the icon.</param>
        /// <param name="style">The style of the files to delete.</param>
        /// <returns>Returns the created change.</returns>
        internal static IconIndexChange DeleteStyleFiles(string name, string category, EIconStyle style)
        {
            return new IconIndexChange(EIconIndexChange.DeleteStyleFiles, name, category, null, style);
        }

        #endregion


        #region Public API

        /// <inheritdoc/>
        public override string ToString()
        {
            switch (Type)
            {
                case EIconIndexChange.MoveFolder:
                    return $"Move \"{Name}\" from \"{Category}\" to \"{TargetCategory}\"";

                case EIconIndexChange.DeleteStyleFiles:
                    return $"Delete the {IconStorage.GetStyleName(Style)} files of \"{Name}\" in \"{Category}\"";

                default:
                    return $"Delete \"{Name}\" in \"{Category}\"";
            }
        }

        #endregion

    }

}
