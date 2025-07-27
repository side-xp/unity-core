using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Represents a value that may be used or not given a boolean.
    /// </summary>
    [System.Serializable]
    public class OptionalValue<T> : IOptionalValue<T>
    {

        [SerializeField]
        [Tooltip("Is this value enabled?")]
        private bool _enabled = false;

        [SerializeField]
        [Tooltip("The value to use if enabled.")]
        private T _value = default;

        /// <inheritdoc cref="OptionalValue{T}.OptionalValue(T, bool)"/>
        public OptionalValue()
            : this(default, false) { }

        /// <inheritdoc cref="OptionalValue{T}"/>
        /// <param name="value"><inheritdoc cref="IOptionalValue{T}.Value"/></param>
        /// <param name="enabled"><inheritdoc cref="IOptionalValue.Enabled"/></param>
        public OptionalValue(T value, bool enabled = false)
        {
            _value = value;
            _enabled = enabled;
        }

        /// <inheritdoc cref="IOptionalValue{T}.Value"/>
        public virtual T Value
        {
            get => _value;
            set => _value = value;
        }

        /// <inheritdoc cref="IOptionalValue.Value"/>
        object IOptionalValue.Value
        {
            get => Value;
            set => Value = (T)value;
        }

        /// <inheritdoc cref="IOptionalValue.Enabled"/>
        public virtual bool Enabled
        {
            get => _enabled;
            set => _enabled = value;
        }

        /// <summary>
        /// Converts this optional value into its expected value type.
        /// </summary>
        /// <param name="value">The optional value to convert.</param>
        public static implicit operator T(OptionalValue<T> value)
        {
            return value.Value;
        }

        /// <summary>
        /// Converts a value into an optional value instance.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        public static implicit operator OptionalValue<T>(T value)
        {
            return new OptionalValue<T>(value);
        }

    }

}