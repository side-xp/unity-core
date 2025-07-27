using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Represents an asst with a unique identifier.
    /// </summary>
    [HelpURL(Constants.BaseHelpUrl)]
    public class UniqueSO : ScriptableObject
    {

        [UniqueId]
        [SerializeField, Readonly]
        [Tooltip("The unique identifier of this asset.")]
        private string _uniqueId = string.Empty;

        /// <summary>
        /// The unique identifier of this asset.
        /// </summary>
        public string UniqueId => _uniqueId;

    }

}
