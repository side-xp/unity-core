
namespace SideXP.Core
{

    /// <summary>
    /// Qualifies a class as being an item in a <see cref="ProbabilityCollection"/>.
    /// </summary>
    public interface IProbabilityItem
    {

        /// <summary>
        /// The data bound to the probability value.
        /// </summary>
        object Data { get; }

        /// <summary>
        /// The probability value of this item.
        /// </summary>
        float Probability { get; }

        /// <summary>
        /// Gets the name or identifier of the item.
        /// </summary>
        string Label { get; }

    }

    /// <inheritdoc cref="IProbabilityItem"/>
    /// <typeparam name="T">The type of the data bound to the probability value.</typeparam>
    public interface IProbabilityItem<T> : IProbabilityItem
    {

        /// <inheritdoc cref="IProbabilityItem.Data"/>
        new T Data { get; }

    }

}