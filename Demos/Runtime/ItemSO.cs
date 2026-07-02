using UnityEngine;

namespace SideXP.Core.Demos
{

    /// <summary>
    /// Generic asset used in editor features demos.
    /// </summary>
    [HelpURL(Constants.BaseHelpUrl + "/api/SideXP.Core.Demos/" + nameof(ItemSO))]
    [CreateAssetMenu(fileName = "NewItem", menuName = Constants.CreateAssetMenuDemos + "/Item")]
    public class ItemSO : ScriptableObject
    {

        public string DisplayName;

        [TextArea(3, 6)]
        public string Description;

        [Min(0)]
        public int Price;

#if UNITY_EDITOR
        // Illustrates the usage of the OpenAsset callback. See ItemSOEditorWindow script from editor demos to learn more about handlers.
        [UnityEditor.Callbacks.OnOpenAsset]
#if UNITY_6000_5_OR_NEWER
        private static bool OpenAsset(EntityId entityId, int line)
        {
            return RuntimeObjectUtility.TryOpenAsset(entityId, line);
        }
#else
        private static bool OpenAsset(int instanceId, int line)
        {
            return RuntimeObjectUtility.TryOpenAsset(instanceId, line);
        }
#endif
#endif

    }

}
