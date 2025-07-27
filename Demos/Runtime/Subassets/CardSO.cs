using UnityEngine;

namespace SideXP.Core.Demos
{

    /// <summary>
    /// Illustrates usage of the Subassets features.
    /// </summary>
    [HelpURL(Constants.BaseHelpUrl)]
    [CreateAssetMenu(fileName = "NewCard", menuName = Constants.CreateAssetMenuDemos + "/Card")]
    public class CardSO : ScriptableObject
    {

        [TextArea(3, 6)]
        public string description;

        [Space]

        // To store a single subasset, you must use the [Subasset] attribute
        [Subasset]
        public CardEffectBase EffectOnDraw = null;

        [Space]

        // To store a list of subassets, just use the SubassetsList<T> type
        public SubassetsList<CardEffectBase> EffectsOnPlay = new SubassetsList<CardEffectBase>();

        [Space]

        [Subasset]
        public CardEffectBase EffectOnDiscard = null;

    }

}