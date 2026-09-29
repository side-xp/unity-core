using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Represents an operation to apply to a value, made of an operator and an operand (such as "+10" or "x1.5"). This is meant to
    /// compute cumulated effects, such as bonuses and penalties applied to the stats of a character in an RPG.<br/>
    /// Use <see cref="ValueModifierUtility.ApplyAll(float, ValueModifier[])"/> to apply several modifiers to a base value.
    /// </summary>
    /// <remarks>
    /// The value to modify is always the left-hand operand, and the value of the modifier is the right-hand one. So a modifier with
    /// the <see cref="EOperator.Subtract"/> operator and a value of 10 means "minus 10", and a modifier with the
    /// <see cref="EOperator.Divide"/> operator and a value of 2 means "divided by 2".<br/>
    /// The default modifier (<see cref="EOperator.Add"/> with a value of 0) leaves the value unchanged.
    /// </remarks>
    /// <example>
    /// <code>
    /// using UnityEngine;
    /// using SideXP.Core;
    ///
    /// public class Character : MonoBehaviour
    /// {
    ///     [SerializeField]
    ///     private float _baseSpeed = 5f;
    ///
    ///     [SerializeField]
    ///     private ValueModifier _sprintModifier = new ValueModifier(EOperator.Multiply, 1.5f);
    ///
    ///     public float SprintSpeed => _sprintModifier.ApplyTo(_baseSpeed);
    ///     // SprintSpeed is 7.5
    /// }
    /// </code>
    /// </example>
    [System.Serializable]
    public struct ValueModifier
    {

        [SerializeField]
        [Tooltip("The operator used to apply this modifier to a value.")]
        private EOperator _operator;

        [SerializeField]
        [Tooltip("The value of this modifier, used as the right-hand operand when applied to a value.")]
        private float _value;

        /// <inheritdoc cref="ValueModifier"/>
        /// <param name="op">The operator used to apply this modifier to a value.</param>
        /// <param name="value">The value of this modifier, used as the right-hand operand when applied to a value.</param>
        public ValueModifier(EOperator op, float value)
        {
            _operator = op;
            _value = value;
        }

        /// <inheritdoc cref="_operator"/>
        public EOperator Operator
        {
            readonly get => _operator;
            set => _operator = value;
        }

        /// <inheritdoc cref="_value"/>
        public float Value
        {
            readonly get => _value;
            set => _value = value;
        }

        /// <summary>
        /// Applies this modifier to a given value.
        /// </summary>
        /// <param name="input">The value to modify, used as the left-hand operand.</param>
        /// <returns>
        /// Returns the result of this modifier applied to the given value. If the operation is ignored (such as a division by 0, see
        /// <see cref="EOperator"/> values for details), returns <paramref name="input"/> unchanged.
        /// </returns>
        /// <example>
        /// <code>
        /// ValueModifier modifier = new ValueModifier(EOperator.Subtract, 10);
        /// float result = modifier.ApplyTo(100);
        /// // result is 90
        /// </code>
        /// </example>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if the operator of this modifier is not a valid <see cref="EOperator"/> value.
        /// </exception>
        public readonly float ApplyTo(float input)
        {
            return _operator.Apply(input, _value);
        }

    }

}
