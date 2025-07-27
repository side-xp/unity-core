using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Draws a progress bar next to a numeric field in the inspector.
    /// </summary>
    public class ProgressBarAttribute : PropertyAttribute
    {

        #region Fields

        public const FColor DefaultColor = FColor.Cyan | FColor.Alpha100;

        /// <summary>
        /// The minimum value of the progress bar.
        /// </summary>
        public float Min { get; private set; } = 0f;

        /// <summary>
        /// The maximum value of the progress bar.
        /// </summary>
        public float Max { get; private set; } = 100f;

        /// <summary>
        /// The color of the progress bar.
        /// </summary>
        public Color Color { get; set; } = Color.blue;

        /// <summary>
        /// The prefix to write before the value on the progress bar.
        /// </summary>
        public string Prefix { get; set; } = string.Empty;

        /// <summary>
        /// The suffix to write after the value on the progress bar.
        /// </summary>
        public string Suffix { get; set; } = string.Empty;

        /// <summary>
        /// If enabled, the field's value will be clamped between min and max.
        /// </summary>
        public bool Clamp { get; set; } = false;

        /// <summary>
        /// If enabled, the field is completely hidden by the progress bar.
        /// </summary>
        public bool Wide { get; set; } = false;

        /// <summary>
        /// Should the field be readonly?
        /// </summary>
        public bool Readonly { get; set; } = false;

        #endregion


        #region Lifecycle

        /// <summary>
        /// Draws a progress bar from 0 to 100 next to a numeric field in the inspector.
        /// </summary>
        /// <inheritdoc cref="ProgressBarAttribute(float, float, FColor)"/>
        public ProgressBarAttribute(FColor color = DefaultColor)
            : this(0, 100, color) { }

        /// <summary>
        /// Draws a progress bar from 0 to <paramref name="max"/> next to a numeric field in the inspector.
        /// </summary>
        /// <inheritdoc cref="ProgressBarAttribute(float, float, FColor)"/>
        public ProgressBarAttribute(float max, FColor color = DefaultColor)
            : this(0, max, color) { }

        /// <summary>
        /// Draws a progress bar from <paramref name="min"/> to <paramref name="max"/> next to a numeric field in the inspector.
        /// </summary>
        /// <param name="min"><inheritdoc cref="Min" path="/summary"/></param>
        /// <param name="max"><inheritdoc cref="Max" path="/summary"/></param>
        /// <param name="color"><inheritdoc cref="Color" path="/summary"/></param>
        public ProgressBarAttribute(float min, float max, FColor color = DefaultColor)
        {
            Min = min;
            Max = max;
            Color = color.ToColor();
        }

        #endregion


        #region Public API

        /// <summary>
        /// Gets the label of the progress bar, using the defined prefix and suffix.
        /// </summary>
        /// <param name="value">The current value of the field.</param>
        /// <returns>Returns the computed label.</returns>
        public string GetLabel(float value)
        {
            return $"{Prefix}{value}{Suffix}";
        }

        #endregion

    }

}