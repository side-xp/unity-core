namespace SideXP.Core
{

    /// <summary>
    /// Defines a color using flags, so you can blend simple color values.<br/>
    /// Use the <see cref="FColorExtensions"/> class to resolve the values.
    /// </summary>
    [System.Flags]
    public enum FColor
    {
        // RGB components
        Red = 1 << 0,
        Green = 1 << 2,
        Blue = 1 << 4,
        // Half RGB components
        Maroon = 1 << 1,
        Lime = 1 << 3,
        Navy = 1 << 5,

        // Alpha
        Alpha100 = 1 << 6,
        Alpha87 = 1 << 7,
        Alpha75 = 1 << 8,
        Alpha50 = 1 << 9,
        Alpha25 = 1 << 10,
        Alpha12 = 1 << 11,
        Alpha0 = 1 << 12,

        // Tints
        Clear = 0,
        Black = Alpha100,
        Grey = Maroon | Green | Navy,
        White = Red | Green | Blue,

        // Other colors
        Yellow = Red | Green,
        Orange = Red | Lime,
        Olive = Maroon | Green,
        Purple = Maroon | Navy,
        Magenta = Red | Blue,
        Teal = Green | Navy,
        Cyan = Green | Blue,
        Azure = Lime | Blue,

        // Aliases
        Fuchsia = Magenta,
        Aqua = Cyan,
    }

}