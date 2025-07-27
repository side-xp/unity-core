using System;

namespace SideXP.Core
{

    /// <summary>
    /// Extension functions for enum values.
    /// </summary>
    public static class EnumExtensions
    {

        /// <summary>
        /// Checks if the given enumeration value matches an item.
        /// </summary>
        /// <typeparam name="TEnum">The enumeration type.</typeparam>
        /// <param name="enumValue">The enumeration value to check.</param>
        /// <returns>Returns true if the given enumeration value matches an item.</returns>
        public static bool IsValid<TEnum>(this TEnum enumValue)
            where TEnum : Enum
        {
            int enumValueInt = Convert.ToInt32(enumValue);
            foreach (int i in Enum.GetValues(typeof(TEnum)))
            {
                if (enumValueInt == i)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Invert the given enumeration value: enabled flags become disabled flags, and vice-versa.
        /// </summary>
        /// <typeparam name="TEnum">The enumeration type.</typeparam>
        /// <param name="enumValue">The enumeration value to invert.</param>
        public static void Invert<TEnum>(this ref TEnum enumValue)
            where TEnum : struct, Enum
        {
            int enumValueInt = ~Convert.ToInt32(enumValue);
            enumValue = (TEnum)(object)enumValueInt;
        }

        /// <summary>
        /// Adds the given flag to the enum value.
        /// </summary>
        /// <typeparam name="TEnum">The enumeration type.</typeparam>
        /// <param name="enumValue">The enumeration value to which you want to add the flag.</param>
        /// <param name="flag">The flag to add.</param>
        public static void AddFlag<TEnum>(this ref TEnum enumValue, TEnum flag)
            where TEnum : struct, Enum
        {
            int enumValueInt = Convert.ToInt32(enumValue);
            int flagValueInt = Convert.ToInt32(flag);

            enumValueInt |= flagValueInt;

            enumValue = (TEnum)(object)enumValueInt;
        }

        /// <summary>
        /// Removes the given flag from the enum value.
        /// </summary>
        /// <typeparam name="TEnum">The enumeration type.</typeparam>
        /// <param name="enumValue">The enumeration value from which you want to remove the flag.</param>
        /// <param name="flag">The flag to remove.</param>
        public static void RemoveFlag<TEnum>(this ref TEnum enumValue, TEnum flag)
            where TEnum : struct, Enum
        {
            int enumValueInt = Convert.ToInt32(enumValue);
            int flagValueInt = Convert.ToInt32(flag);

            enumValueInt &= ~flagValueInt;

            enumValue = (TEnum)(object)enumValueInt;
        }

    }

}