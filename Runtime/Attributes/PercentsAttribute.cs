namespace SideXP.Core
{

    /// <summary>
    /// Displays a value expected between 0 and 100 on GUI, and remaps it in this field as a value between 0 and 1.
    /// </summary>
    public class PercentsAttribute : RemapAttribute
    {

        /// <inheritdoc cref="PercentsAttribute"/>
        public PercentsAttribute()
            : base(0, 100, 0, 1)
        {
            Units = "%";
        }

    }

}