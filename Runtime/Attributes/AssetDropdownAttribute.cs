using System;

using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Draws a reference to an asset as a dropdown, which lists a <i>None</i> option and every asset of the field's type found in the
    /// project, including the assets of its derived types and sub-assets.
    /// </summary>
    /// <remarks>
    /// Use it for fields that reference a small, known set of "definition" assets (categories, types, tiers), where picking from a
    /// list is faster than drag-and-drop or the object picker.
    /// <para>
    /// The options are found when the dropdown opens, so new assets show up right away. Sub-assets are listed in a submenu named after
    /// their main asset, and a <c>/</c> in the name of a sub-asset creates a submenu too.
    /// </para>
    /// <para>
    /// The field must reference a type of asset that is not a component: a <see cref="ScriptableObject"/> type, or a type such as
    /// <see cref="Material"/>, <see cref="Sprite"/> or <see cref="GameObject"/> (prefabs and models). On arrays and lists, each element
    /// is a dropdown.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// [SerializeField]
    /// [AssetDropdown]
    /// private ItemCategorySO _category = null;
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class AssetDropdownAttribute : PropertyAttribute { }

}
