using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Draws an animation curve field with custom settings (using custom property drawer).
    /// </summary>
    public class AnimCurveAttribute : PropertyAttribute
    {

        #region Fields

        /// <summary>
        /// The minimum time value (along the X axis).
        /// </summary>
        public float MinTime { get; private set; } = 0f;

        /// <summary>
        /// The maximum time value (along the X axis).
        /// </summary>
        public float MaxTime { get; private set; } = 1f;

        /// <summary>
        /// The minimum curve value (along the Y axis).
        /// </summary>
        public float MinValue { get; private set; } = 0f;

        /// <summary>
        /// The maximum curve value (along the Y axis).
        /// </summary>
        public float MaxValue { get; private set; } = 1f;

        /// <summary>
        /// The color of the curve in the inspector.
        /// </summary>
        public Color CurveColor { get; set; } = Color.green;

        #endregion


        #region Lifecycle

        /// <summary>
        /// Clamps the animation curve between 0 and 1 on both axes.
        /// </summary>
        /// <inheritdoc cref="AnimCurveAttribute(float, float, float, float, FColor)"/>
        public AnimCurveAttribute(FColor color = FColor.Green)
            : this(0, 1, 0, 1, color) { }

        /// <summary>
        /// Clamps the animation curve between 0 and the given maximums for both time and value.
        /// </summary>
        /// <inheritdoc cref="AnimCurveAttribute(float, float, float, float, FColor)"/>
        public AnimCurveAttribute(float maxTime, float maxValue, FColor color = FColor.Green)
            : this(0, maxTime, 0, maxValue)
        {
            MaxTime = maxTime;
            MaxValue = maxValue;
            CurveColor = color.ToColor(true);
        }

        /// <summary>
        /// Clamps the animation curve between the given values, for both time and value.
        /// </summary>
        /// <param name="minTime">The minimum value along the X axis.</param>
        /// <param name="maxTime">The maximum value along the X axis.</param>
        /// <param name="minValue">The minimum value along the Y axis.</param>
        /// <param name="maxValue">The maximum value along the Y axis.</param>
        /// <param name="color">The color of the curve in the inspector.</param>
        public AnimCurveAttribute(float minTime, float maxTime, float minValue, float maxValue, FColor color = FColor.Green)
        {
            MinTime = minTime;
            MaxTime = maxTime;
            MinValue = minValue;
            MaxValue = maxValue;
            CurveColor = color.ToColor(true);
        }

        #endregion


        #region Public API

        /// <summary>
        /// Creates a <see cref="Rect"/> representing the curve's ranges:<br/>
        /// - x is min time<br/>
        /// - y is min value<br/>
        /// - width is (abs(min time) + abs(max time)<br/>
        /// - height is (abs(min value) + abs(max value)<br/>
        /// </summary>
        public Rect Ranges
        {
            get
            {
                return new Rect
                (
                    Mathf.Min(MinTime, MaxTime),
                    Mathf.Min(MinValue, MaxValue),
                    Mathf.Abs(Mathf.Min(MinTime, MaxTime)) + Mathf.Abs(Mathf.Max(MinTime, MaxTime)),
                    Mathf.Abs(Mathf.Min(MinValue, MaxValue)) + Mathf.Abs(Mathf.Max(MinValue, MaxValue))
                );
            }
        }

        #endregion

    }

}