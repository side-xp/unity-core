namespace SideXP.Core
{

    /// <summary>
    /// Extension functions for boolean values.
    /// </summary>
    public static class BoolExtensions
    {

        public const string TrueLabel = "true";
        public const string FalseLabel = "false";

        /// <summary>
        /// Converts this boolean into a label.
        /// </summary>
        /// <param name="value">The boolean value to convert.</param>
        /// <param name="labelIfTrue">The output label if the boolean is true.</param>
        /// <param name="labelIfFalse">The output label if the boolean is false.</param>
        /// <returns>Returns the appropriate label.</returns>
        public static string ToLabel(this bool value, string labelIfTrue = TrueLabel, string labelIfFalse = FalseLabel)
        {
            return value ? labelIfTrue : labelIfFalse;
        }

    }

}