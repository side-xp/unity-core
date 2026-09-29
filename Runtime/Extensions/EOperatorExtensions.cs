using System;

using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Extension functions for <see cref="EOperator"/> values.
    /// </summary>
    public static class EOperatorExtensions
    {

        /// <summary>
        /// Applies an operator to all the given values, from left to right.
        /// </summary>
        /// <param name="op">The operator to apply.</param>
        /// <param name="values">The values to compute. The first one is used as the initial left-hand operand.</param>
        /// <returns>
        /// Returns the result of the operator applied to all the given values, or 0 if no values are given. If only one value is given,
        /// returns that value.
        /// </returns>
        /// <example>
        /// <code>
        /// int result = EOperator.Subtract.Apply(10, 3, 2);
        /// // result is 5, computed as ((10 - 3) - 2)
        ///
        /// result = EOperator.Divide.Apply(100, 0, 5);
        /// // result is 20: the division by 0 is ignored, then 100 is divided by 5
        /// </code>
        /// </example>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the operator is not a valid <see cref="EOperator"/> value.</exception>
        public static int Apply(this EOperator op, params int[] values)
        {
            if (values == null || values.Length <= 0)
                return 0;

            int output = values[0];
            for (int i = 1; i < values.Length; i++)
                output = Apply(op, output, values[i]);
            return output;
        }

        /// <summary>
        /// Applies an operator to the given values.
        /// </summary>
        /// <param name="op">The operator to apply.</param>
        /// <param name="a">The left-hand operand.</param>
        /// <param name="b">The right-hand operand.</param>
        /// <returns>
        /// Returns the result of the operation. If the operation is ignored (such as a division by 0, see <see cref="EOperator"/> values
        /// for details), returns <paramref name="a"/> unchanged.
        /// </returns>
        /// <example>
        /// <code>
        /// int result = EOperator.Add.Apply(7, 2);
        /// // result is 9
        ///
        /// result = EOperator.Divide.Apply(7, 2);
        /// // result is 3, as integer divisions are truncated
        ///
        /// result = EOperator.Modulo.Apply(7, 0);
        /// // result is 7, as the modulo by 0 is ignored
        /// </code>
        /// </example>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the operator is not a valid <see cref="EOperator"/> value.</exception>
        public static int Apply(this EOperator op, int a, int b)
        {
            switch (op)
            {
                case EOperator.Add:
                    return a + b;

                case EOperator.Subtract:
                    return a - b;

                case EOperator.Multiply:
                    return a * b;

                case EOperator.Divide:
                    return b != 0 ? a / b : a;

                case EOperator.Modulo:
                    return b != 0 ? a % b : a;

                case EOperator.Set:
                    return b;

                case EOperator.Power:
                    return (a == 0) && (b < 0) ? a : (int)System.Math.Pow(a, b);

                default:
                    throw new ArgumentOutOfRangeException(nameof(op), op, $"Unsupported operator: {op}");
            }
        }

        /// <inheritdoc cref="Apply(EOperator, int[])"/>
        /// <example>
        /// <code>
        /// float result = EOperator.Multiply.Apply(10f, 1.5f, 2f);
        /// // result is 30, computed as ((10 * 1.5) * 2)
        ///
        /// result = EOperator.Divide.Apply(100f, 0f, 4f);
        /// // result is 25: the division by 0 is ignored, then 100 is divided by 4
        /// </code>
        /// </example>
        public static float Apply(this EOperator op, params float[] values)
        {
            if (values == null || values.Length <= 0)
                return 0;

            float output = values[0];
            for (int i = 1; i < values.Length; i++)
                output = Apply(op, output, values[i]);
            return output;
        }

        /// <inheritdoc cref="Apply(EOperator, int, int)"/>
        /// <example>
        /// <code>
        /// float result = EOperator.Divide.Apply(7f, 2f);
        /// // result is 3.5
        ///
        /// result = EOperator.Power.Apply(2f, -1f);
        /// // result is 0.5
        ///
        /// result = EOperator.Divide.Apply(7f, 0f);
        /// // result is 7, as the division by 0 is ignored (instead of returning infinity)
        /// </code>
        /// </example>
        public static float Apply(this EOperator op, float a, float b)
        {
            switch (op)
            {
                case EOperator.Add:
                    return a + b;

                case EOperator.Subtract:
                    return a - b;

                case EOperator.Multiply:
                    return a * b;

                case EOperator.Divide:
                    return b != 0 ? a / b : a;

                case EOperator.Modulo:
                    return b != 0 ? a % b : a;

                case EOperator.Set:
                    return b;

                case EOperator.Power:
                    return (a == 0) && (b < 0) ? a : Mathf.Pow(a, b);

                default:
                    throw new ArgumentOutOfRangeException(nameof(op), op, $"Unsupported operator: {op}");
            }
        }

    }

}
