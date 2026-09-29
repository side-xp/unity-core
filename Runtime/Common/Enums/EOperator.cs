namespace SideXP.Core
{

    /// <summary>
    /// Represents an arithmetic operator, applied to a left-hand operand (a) and a right-hand operand (b).<br/>
    /// Use the <see cref="EOperatorExtensions"/> class to compute the result of an operation.
    /// </summary>
    public enum EOperator
    {
        /// <summary>
        /// Computes (a + b).
        /// </summary>
        Add,

        /// <summary>
        /// Computes (a - b).
        /// </summary>
        Subtract,

        /// <summary>
        /// Computes (a * b).
        /// </summary>
        Multiply,

        /// <summary>
        /// Computes (a / b). The operation is ignored if b is 0, so the result is a.
        /// </summary>
        Divide,

        /// <summary>
        /// Computes (a % b). The operation is ignored if b is 0, so the result is a.
        /// </summary>
        Modulo,

        /// <summary>
        /// Replaces a by b, so the result is b.
        /// </summary>
        Set,

        /// <summary>
        /// Computes a raised to the power of b. The operation is ignored if a is 0 and b is negative (which would imply a division by
        /// 0), so the result is a.
        /// </summary>
        Power,
    }

}
