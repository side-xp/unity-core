using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Makes a property readonly, so it's visible but not editable in the inspector (using custom property drawer).
    /// </summary>
    public class ReadonlyAttribute : PropertyAttribute { }

}