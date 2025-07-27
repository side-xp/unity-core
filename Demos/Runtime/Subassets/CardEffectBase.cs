using UnityEngine;

namespace SideXP.Core.Demos
{

    /// <summary>
    /// Base class that represents an effect for a playable card.<br/>
    /// Used to illustrate Subassets usage.
    /// </summary>
    public abstract class CardEffectBase : ScriptableObject
    {

        public abstract bool Apply();

    }

}