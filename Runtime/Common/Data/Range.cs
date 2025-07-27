using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Represents a range between a minimum and a maximum.
    /// </summary>
    [System.Serializable]
    public struct Range
    {

        [SerializeField]
        [Tooltip("The minimum value of this range.")]
        private float _min;

        [SerializeField]
        [Tooltip("The maximum value of this range.")]
        private float _max;

        /// <summary>
        /// Represents a range between 0 and a given maximum.
        /// </summary>
        /// <inheritdoc cref="Range(float, float)"/>
        public Range(float max)
            : this (0, max) { }

        /// <inheritdoc cref="Range"/>
        /// <param name="min">The minimum value of this range.</param>
        /// <param name="min">The maximum value of this range.</param>
        public Range(float min, float max)
        {
            _min = min;
            _max = max;
        }

        /// <param name="other">The range to copy.</param>
        /// <inheritdoc cref="Range(float, float)"/>
        public Range(Range other)
            : this (other._min, other._max) { }

        /// <inheritdoc cref="_min"/>
        public float Min
        {
            get => Mathf.Min(_min, _max);
            set => _min = value;
        }

        /// <inheritdoc cref="_max"/>
        public float Max
        {
            get => Mathf.Max(_min, _max);
            set => _max = value;
        }

        /// <summary>
        /// Returns the delta between min and max, basically the result of (max - min).
        /// </summary>
        public float Delta => Max - Min;

        /// <summary>
        /// Gets a random value between min and max, both inclusive.
        /// </summary>
        public float Random => UnityEngine.Random.Range(Min, Max);

    }

}
