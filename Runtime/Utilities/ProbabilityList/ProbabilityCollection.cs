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
        /// <remarks>An item with a probability of zero or less is never picked. If the collection is empty or every item has a
        /// non-positive probability, this returns false.</remarks>
        /// <param name="data">Outputs the picked object.</param>
        /// <returns>Returns true if a valid object has been picked from the list successfully.</returns>
        public virtual bool Get(out object data)
        {
            // Sum the weights. A negative or zero probability means the item can't be picked.
            float total = 0f;
            foreach (IProbabilityItem item in Items)
                total += Mathf.Max(0f, item.Probability);

            // No item can be picked (empty collection, or all probabilities are zero or negative).
            if (total <= 0f)
            {
                data = default;
                return false;
            }

            float random = Random.Range(0f, total);
            float cursor = 0f;
            IProbabilityItem lastPickable = null;
            foreach (IProbabilityItem item in Items)
            {
                float weight = Mathf.Max(0f, item.Probability);
                if (weight <= 0f)
                    continue;

                lastPickable = item;
                cursor += weight;
                if (random <= cursor)
                {
                    data = item.Data;
                    return true;
                }
            }

            // Fallback for floating-point rounding (random landing just past the last cursor).
            data = lastPickable.Data;
            return true;
        }

        /// <summary>
        /// Gets the probability percentage for a given object in this collection to be picked.
        /// </summary>
        /// <remarks>A probability of zero or less is reported as a 0% chance, consistently with <see cref="Get(out object)"/>.</remarks>
        /// <param name="data">The object from this collection.</param>
        /// <returns>Returns the probability percentage for the given object to be picked.</returns>
        public virtual float GetProbabilityPercents(object data)
        {
            IProbabilityItem targetItem = null;

            // Sum the weights, consistently with Get() (negative or zero probabilities don't count).
            float total = 0f;
            foreach (IProbabilityItem item in Items)
            {
                total += Mathf.Max(0f, item.Probability);
                if (item.Data != null && item.Data.Equals(data))
                    targetItem = item;
            }

            // Cancel if the given value is not in the list, or if no item can be picked.
            if (targetItem == null || total <= 0f)
                return 0f;

            return Mathf.Max(0f, targetItem.Probability) / total * 100f;
        }

        #endregion

    }

}