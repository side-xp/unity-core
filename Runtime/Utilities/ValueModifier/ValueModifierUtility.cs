using System.Collections.Generic;

namespace SideXP.Core
{

    /// <summary>
    /// Utility functions for working with <see cref="ValueModifier"/> values.
    /// </summary>
    public static class ValueModifierUtility
    {

        /// <summary>
        /// Applies all the given modifiers to a base value, in order.
        /// </summary>
        /// <remarks>
        /// Each modifier is applied to the result of the previous one, so the order of the modifiers matters: with a base value of 100,
        /// "+10" then "x2" gives 220, while "x2" then "+10" gives 210.
        /// </remarks>
        /// <param name="baseValue">The initial value to modify.</param>
        /// <param name="modifiers">The modifiers to apply, in order.</param>
        /// <returns>Returns the result of all the modifiers applied, or the base value if no modifiers are given.</returns>
        /// <example>
        /// <code>
        /// float damage = ValueModifierUtility.ApplyAll(
        ///     100,
        ///     new ValueModifier(EOperator.Add, 10),       // Weapon bonus: 110
        ///     new ValueModifier(EOperator.Multiply, 2),   // Critical hit: 220
        ///     new ValueModifier(EOperator.Subtract, 20)   // Target's armor: 200
        /// );
        /// // damage is 200
        /// </code>
        /// </example>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if the operator of a modifier is not a valid <see cref="EOperator"/> value.
        /// </exception>
        public static float ApplyAll(float baseValue, params ValueModifier[] modifiers)
        {
            if (modifiers == null)
                return baseValue;

            float output = baseValue;
            for (int i = 0; i < modifiers.Length; i++)
                output = modifiers[i].ApplyTo(output);
            return output;
        }

        /// <inheritdoc cref="ApplyAll(float, ValueModifier[])"/>
        /// <example>
        /// <code>
        /// List&lt;ValueModifier&gt; speedModifiers = new List&lt;ValueModifier&gt;();
        /// speedModifiers.Add(new ValueModifier(EOperator.Multiply, 1.5f));    // Haste potion
        /// speedModifiers.Add(new ValueModifier(EOperator.Set, 0));            // Frozen
        ///
        /// float speed = ValueModifierUtility.ApplyAll(5, speedModifiers);
        /// // speed is 0, as the "Set" modifier discards the previous result
        /// </code>
        /// </example>
        public static float ApplyAll(float baseValue, IEnumerable<ValueModifier> modifiers)
        {
            if (modifiers == null)
                return baseValue;

            float output = baseValue;
            foreach (ValueModifier modifier in modifiers)
                output = modifier.ApplyTo(output);
            return output;
        }

    }

}
