using System;
using System.Collections.Generic;

using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Represents a list of enum values bound to a probabiility value, from which those items can be picked randomly by taking account of
    /// their probability.
    /// </summary>
    /// <typeparam name="T">The type of the enumerations of which values are bound to probability values.</typeparam>
    [System.Serializable]
    [ProbabilityCollectionOptions(AllowAddOrRemove = false, Reorderable = false)]
    public class ProbabilityEnum<T> : ProbabilityCollection, ISerializationCallbackReceiver
        where T : System.Enum
    {

        #region Subclasses

        /// <summary>
        /// Groups an item of the enum and its probability value.
        /// </summary>
        [System.Serializable]
        public class ProbabilityItem : IProbabilityItem<T>
        {

            [SerializeField]
            [Tooltip("The value of the represented enum item.")]
            private int _enumValue = 0;

            [SerializeField, Range(0, 1)]
            [Tooltip("The probability value assigned to the item.")]
            private float _probability = MaxProbability;

            /// <inheritdoc cref="ProbabilityItem"/>
            /// <param name="enumValue"><inheritdoc cref="_enumValue" path="/summary"/></param>
            /// <param name="probability"><inheritdoc cref="_probability" path="/summary"/></param>
            internal ProbabilityItem(int enumValue, float probability = MaxProbability)
            {
                _enumValue = enumValue;
                _probability = Mathf.Clamp(probability, MinProbability, MaxProbability);
            }

            /// <inheritdoc cref="_enumValue"/>
            public int EnumValue => _enumValue;

            /// <inheritdoc cref="IProbabilityItem{T}.Data"/>
            public T Data => (T)(object)_enumValue;

            /// <inheritdoc cref="IProbabilityItem.Probability"/>
            public float Probability => _probability;

            /// <inheritdoc cref="IProbabilityItem.Label"/>
            public string Label => Data.ToString();

            /// <inheritdoc cref="IProbabilityItem.Data"/>
            object IProbabilityItem.Data => Data;

        }

        #endregion


        #region Fields

        public const float MinProbability = 0f;
        public const float MaxProbability = 100f;

        [SerializeField]
        [Tooltip("The items in this probability collection.")]
        private ProbabilityItem[] _items = { };

        private bool _reloaded = false;

        #endregion


        #region Lifecycle

        /// <inheritdoc cref="ISerializationCallbackReceiver.OnAfterDeserialize"/>
        public void OnAfterDeserialize()
        {
            Reload(true);
        }

        /// <inheritdoc cref="ISerializationCallbackReceiver.OnBeforeSerialize"/>
        public void OnBeforeSerialize() { }

        #endregion


        #region Public API

        /// <inheritdoc cref="ProbabilityCollection.Items"/>
        public override IProbabilityItem[] Items => _items;

        /// <inheritdoc cref="ProbabilityCollection[int]"/>
        public override IProbabilityItem this[int index]
        {
            get
            {
                Reload();
                return _items[index];
            }
        }

        /// <summary>
        /// Gets an item in this collection by its enum value.
        /// </summary>
        /// <param name="enumValue">The value of the item to get.</param>
        public IProbabilityItem this[T enumValue]
        {
            get
            {
                Reload();
                foreach (ProbabilityItem item in _items)
                {
                    if (item.EnumValue == (int)(object)enumValue)
                        return item;
                }
                return null;
            }
        }

        /// <inheritdoc cref="ProbabilityCollection.Get(out object)"/>
        public bool Get(out T data)
        {
            Reload();
            bool success = Get(out object dataObj);
            data = success ? (T)dataObj : default;
            return success;
        }

        /// <inheritdoc cref="ProbabilityCollection.GetProbabilityPercents(object)"/>
        public float GetProbabilityPercents(T data)
        {
            return GetProbabilityPercents((object)data);
        }

        #endregion


        #region Editor Only

        /// <summary>
        /// Reload the items list, based on the actual available values of the enum.
        /// </summary>
        /// <param name="force">If enabled, forces the items to reload, even if it has been done before.</param>
        private void Reload(bool force = false)
        {
            if (_reloaded && !force)
                return;

            List<ProbabilityItem> itemsList = new List<ProbabilityItem>();

            // Filter the existing items, using only those related to a valid enum value
            foreach (ProbabilityItem item in _items)
            {
                if (Enum.IsDefined(typeof(T), item.EnumValue))
                    itemsList.Add(new ProbabilityItem(item.EnumValue, item.Probability));
            }

            // Add items for missing enum values
            foreach (int enumValue in Enum.GetValues(typeof(T)))
            {
                if (itemsList.Exists(i => i.EnumValue == enumValue))
                    continue;

                itemsList.Add(new ProbabilityItem(enumValue));
            }

            itemsList.Sort((a, b) => a.EnumValue.CompareTo(b.EnumValue));
            _items = itemsList.ToArray();
            _reloaded = true;
        }

        #endregion

    }

}