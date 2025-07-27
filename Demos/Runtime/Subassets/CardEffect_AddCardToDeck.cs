using UnityEngine;

namespace SideXP.Core.Demos
{

    /// <summary>
    /// Represents an effect that add a specific card to the player's deck.<br/>
    /// Used to illustrate Subassets usage.
    /// </summary>
    [SubassetLabel("Add [card] to deck")]
    public class CardEffect_AddCardToDeck : CardEffectBase
    {

        public CardSO card;

        public override bool Apply()
        {
            Debug.Log($"Add card {card.name} to deck");
            return true;
        }

    }

}