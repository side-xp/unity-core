using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Represents a range between a minimum and a maximum integers.
    /// </summary>
    [System.Serializable]
    public struct RangeInt
    {

        [SerializeField]
        [Tooltip("The minimum value of this range.")]
        private int _min;

        [SerializeField]
        [Tooltip("The maximum value of this range.")]
        private int _max;

        /// <summary>
        /// Represents a range between 0 and a given maximum.
        /// </summary>
        /// <inheritdoc cref="RangeInt(int, int)"/>
        public RangeInt(int max)
            : this (0, max) { }

        /// <inheritdoc cref="RangeInt"/>
        /// <param name="min">The minimum value of this range.</param>
        /// <param name="min">The maximum value of this range.</param>
        public RangeInt(int min, int max)
        {
            _min = min;
            _max = max;
        }

        /// <param name="other">The range to copy.</param>
        /// <inheritdoc cref="Range(int, int)"/>
        public RangeInt(RangeInt other)
            : this (other._min, other._max) { }

        /// <inheritdoc cref="_min"/>
        public int Min
        {
            get => Mathf.Min(_min, _max);
            set => _min = value;
        }

        /// <inheritdoc cref="_max"/>
        public int Max
        {
            get => Mathf.Max(_min, _max);
            set => _max = value;
        }

        /// <summary>
        /// Returns the delta between min and max, basically the result of (max - min).
        /// </summary>
        public int Delta => Max - Min;

        /// <summary>
        /// Gets a random value between min and max, both inclusive.
        /// </summary>
        public int Random => _min != _max ? UnityEngine.Random.Range(Min, Max + 1) : 0;

    }

}
