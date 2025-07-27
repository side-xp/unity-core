using System.Collections.Generic;

namespace SideXP.Core
{

    /// <summary>
    /// Qualifies a class as being a container for a collection. This is useful to allow custom property drawers override the GUI of an
    /// entire array instead of every items in it.
    /// </summary>
    /// <remarks>By convention, a collection marked that implements this interface must have a serialized field named "_elements", which you
    /// can get to create custom GUI.</remarks>
    public interface ICollectionWrapper { }

    /// <typeparam name="T">The type of the elements in the collection.</typeparam>
    /// <inheritdoc cref="ICollectionWrapper"/>
    public interface ICollectionWrapper<T> : IEnumerable<T> { }

}