using UnityEngine;

namespace SideXP.Core.Demos
{

    /// <summary>
    /// Represents an effect that add a defined effect to all cards related to a defined asset.<br/>
    /// Used to illustrate Subassets usage.
    /// </summary>
    [SubassetLabel("Add [effect] to all [card] instances")]
    public class CardEffect_AddEffectToCards : CardEffectBase
    {

        [Subasset]
        public CardEffectBase effectToAdd;
        public CardSO card;

        public override bool Apply()
        {
            Debug.Log($"Add the effect {effectToAdd.name} to all cards instanced from {card.name}");
            return true;
        }

    }

}