using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Displays a value expected between "from" minimum and maximum on GUI, and remaps it in this field as a value between "to" minimum and
    /// maximum.
    /// </summary>
    public class RemapAttribute : PropertyAttribute
    {

        #region Fields

        /// <summary>
        /// The minium value displayed on GUI.
        /// </summary>
        public float FromMin { get; set; } = 0f;

        /// <summary>
        /// The maximum value displayed on GUI.
        /// </summary>
        public float FromMax { get; set; } = 100f;

        /// <summary>
        /// The minimum field value.
        /// </summary>
        public float ToMin { get; set; } = 0f;

        /// <summary>
        /// The maximum field value.
        /// </summary>
        public float ToMax { get; set; } = 1f;

        /// <summary>
        /// The unit to display on GUI.
        /// </summary>
        public string Units { get; set; } = null;

        /// <summary>
        /// If enabled, the value is clamped between its allowed bounds.
        /// </summary>
        public bool Clamped { get; set; } = false;

        #endregion


        #region Lifecycle

        /// <summary>
        /// Displays a value expected between 0 and 100 on GUI, and remaps it in this field as a value between 0 and 1.
        /// </summary>
        public RemapAttribute()
            : this(0, 100, 0, 1) { }

        /// <summary>
        /// Displays a value expected between 0 and <paramref name="fromMax"/> on GUI, and remaps it in this field as a value between 0 and
        /// <paramref name="toMax"/>.
        /// </summary>
        /// <inheritdoc cref="RemapAttribute(float, float, float, float)"/>
        public RemapAttribute(float fromMax, float toMax)
            : this(0, fromMax, 0, toMax) { }

        /// <summary>
        /// Displays a value expected between <paramref name="fromMin"/> and <paramref name="fromMax"/> on GUI, and remaps it in this field
        /// as a value between <paramref name="toMin"/> and <paramref name="toMax"/>.
        /// </summary>
        /// <param name="fromMin">The minium value displayed on GUI.</param>
        /// <param name="fromMax">The maximum value displayed on GUI.</param>
        /// <param name="toMin">The minimum field value.</param>
        /// <param name="toMax">The maximum field value.</param>
        public RemapAttribute(float fromMin, float fromMax, float toMin, float toMax)
        {
            FromMin = fromMin;
            FromMax = fromMax;
            ToMin = toMin;
            ToMax = toMax;
        }

        #endregion

    }

}