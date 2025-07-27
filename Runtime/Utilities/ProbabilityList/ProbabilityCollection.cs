using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Base class for implementing lists with items bound to a probability value, and able to pick an item based on that probability value.
    /// </summary>
    [System.Serializable]
    public abstract class ProbabilityCollection
    {

        #region Public API

        /// <summary>
        /// The items in this collection.
        /// </summary>
        public abstract IProbabilityItem[] Items { get; }

        /// <summary>
        /// Get an item in this collection at a given index.
        /// </summary>
        /// <param name="index">The index of the item to get.</param>
        public abstract IProbabilityItem this[int index] { get; }

        /// <returns>Returns the picked object</returns>
        /// <inheritdoc cref="Get(out object)"/>
        public virtual object Get()
        {
            return Get(out object data) ? data : null;
        }

        /// <summary>
        /// Gets an object from this collection picked randomly based on the probability values.
        /// </summary>
        /// <param name="data">Outputs the picked object.</param>
        /// <returns>Returns true if a valid object has been picked from the list successfully.</returns>
        public virtual bool Get(out object data)
        {
            // Calculate the random range
            float min = 0f;
            float total = 0;
            foreach (IProbabilityItem item in Items)
            {
                min = Mathf.Min(min, item.Probability);
                total += Mathf.Abs(item.Probability);
            }

            float random = Random.Range(min, min + total);
            // Return first (or default) item if the random output is exactly the minimum value
            if (random == min)
            {
                data = Items.Length > 0 ? Items[0].Data : default;
                return Items.Length > 0;
            }

            float cursor = min;
            // For each item in this collection
            for (int i = 0; i < Items.Length; i++)
            {
                // If the random value is in the range from current cursor to item's probability, return it
                if (random > cursor && random <= (cursor + Items[i].Probability))
                {
                    data = Items[i].Data;
                    return true;
                }

                // Move cursor to next item
                cursor += Mathf.Abs(Items[i].Probability);
            }

            data = default;
            return false;
        }

        /// <summary>
        /// Gets the probability percentage for a given object in this collection to be picked.
        /// </summary>
        /// <param name="data">The object from this collection.</param>
        /// <returns>Returns the probability percentage for the given object to be picked.</returns>
        public virtual float GetProbabilityPercents(object data)
        {
            IProbabilityItem targetItem = null;

            // Calculate the probability range
            float total = 0f;
            foreach (IProbabilityItem item in Items)
            {
                total += Mathf.Abs(item.Probability);
                if (item.Data != null && item.Data.Equals(data))
                    targetItem = item;
            }

            // Cancel if the given value is not in the list
            if (targetItem == null)
                return 0f;

            return targetItem.Probability.Ratio(0, total) * 100;
        }

        #endregion

    }

}