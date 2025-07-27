namespace SideXP.Core
{

    /// <summary>
    /// Represents a value that may be used or not given a boolean.
    /// </summary>
    public interface IOptionalValue
    {

        /// <summary>
        /// Is this value enabled?
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Gets the value itself.
        /// </summary>
        object Value { get; set; }

    }

    /// <typeparam name="T">The type of this value.</typeparam>
    /// <inheritdoc cref="IOptionalValue"/>
    public interface IOptionalValue<T> : IOptionalValue
    {

        /// <inheritdoc cref="IOptionalValue.Value"/>
        new T Value { get; set; }

    }

}