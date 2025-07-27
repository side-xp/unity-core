using System;

namespace SideXP.Core
{

    /// <summary>
    /// Extension functions for <see cref="Type"/> instances.
    /// </summary>
    public static class TypeExtensions
    {

        /// <summary>
        /// Checks if a given type inherits from a given parent type. Note that if the given type is the same as the expected parent type,
        /// this function will return false, as it's not true that the given type inherits from itself.
        /// </summary>
        /// <returns>Returns true if the given type inherits from the parent type.</returns>
        /// <inheritdoc cref="Is(Type, Type)"/>
        public static bool Inherits(this Type type, Type parent)
        {
            return type != parent && parent.IsAssignableFrom(type);
        }

        /// <typeparam name="TParent"><inheritdoc cref="Inherits(Type, Type)" path="/param[@name='type']"/></typeparam>
        /// <inheritdoc cref="Inherits(Type, Type)"/>
        public static bool Inherits<TParent>(this Type type)
        {
            return type != typeof(TParent) && typeof(TParent).IsAssignableFrom(type);
        }

        /// <summary>
        /// Checks if a given type is or inherits from a given parent type. 
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="parent">The expected parent type.</param>
        /// <returns>Returns true if the given type is or inherits from the parent type.</returns>
        public static bool Is(this Type type, Type parent)
        {
            return parent.IsAssignableFrom(type);
        }

        /// <typeparam name="TParent"><inheritdoc cref="Is(Type, Type)" path="/param[@name='type']"/></typeparam>
        /// <inheritdoc cref="Is(Type, Type)"/>
        public static bool Is<TParent>(this Type type)
        {
            return typeof(TParent).IsAssignableFrom(type);
        }

    }

}
