using System;
using System.Collections.Generic;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Informations about an editor icon available in the index.
    /// </summary>
    public readonly struct IconInfo
    {

        #region Fields

        /// <summary>
        /// The name of the icon, as used to get it (for example <c>home</c>).
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The category of the icon (for example <c>Buildings</c>).
        /// </summary>
        public string Category { get; }

        /// <summary>
        /// The tags of the icon, which describe it with other words than its name.
        /// </summary>
        public IReadOnlyList<string> Tags { get; }

        /// <summary>
        /// Whether the icon exists in the <see cref="EIconStyle.Filled"/> style. Every icon exists in the
        /// <see cref="EIconStyle.Outline"/> style.
        /// </summary>
        public bool HasFilled { get; }

        #endregion


        #region Lifecycle

        /// <summary>
        /// Creates the informations about an icon from its index entry.
        /// </summary>
        /// <param name="entry">The index entry of the icon.</param>
        internal IconInfo(IconIndexEntry entry)
        {
            Name = entry.name;
            Category = entry.category ?? string.Empty;
            // Wrapped, so callers can't change the index through the returned list
            Tags = Array.AsReadOnly(entry.tags ?? Array.Empty<string>());
            HasFilled = entry.hasFilled;
        }

        #endregion

    }

}
