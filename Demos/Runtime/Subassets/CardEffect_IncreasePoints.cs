using UnityEngine;

namespace SideXP.Core.Demos
{

    /// <summary>
    /// Represents an effect that increase the player's score by a defined amount.<br/>
    /// Used to illustrate Subassets usage.
    /// </summary>
    [SubassetLabel("Increase points by [amount]")]
    public class CardEffect_IncreasePoints : CardEffectBase
    {

        public int amount;

        public override bool Apply()
        {
            Debug.Log($"Add {amount} points to player");
            return true;
        }

    }

}