using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Extension functions for <see cref="AnimationCurve"/> instances.
    /// </summary>
    public static class AnimationCurveExtensions
    {

        /// <summary>
        /// Default <see cref="Keyframe"/> value.
        /// </summary>
        public static readonly Keyframe DefaultKeyframe = new Keyframe(0f, 0f);

        /// <inheritdoc cref="AnimationCurveUtility.GetDuration(AnimationCurve)"/>
        public static float GetDuration(this AnimationCurve curve)
        {
            return AnimationCurveUtility.GetDuration(curve);
        }

        /// <inheritdoc cref="AnimationCurveUtility.GetRange(AnimationCurve)"/>
        public static float GetRange(this AnimationCurve curve)
        {
            return AnimationCurveUtility.GetRange(curve);
        }

        /// <inheritdoc cref="AnimationCurveUtility.GetFirstKeyframe(AnimationCurve)"/>
        public static Keyframe GetFirstKeyframe(this AnimationCurve curve)
        {
            return AnimationCurveUtility.GetFirstKeyframe(curve);
        }

        /// <inheritdoc cref="AnimationCurveUtility.GetLastKeyframe(AnimationCurve)"/>
        public static Keyframe GetLastKeyframe(this AnimationCurve curve)
        {
            return AnimationCurveUtility.GetLastKeyframe(curve);
        }

        /// <inheritdoc cref="AnimationCurveUtility.GetMinKeyframe(AnimationCurve)"/>
        public static Keyframe GetMinKeyframe(this AnimationCurve curve)
        {
            return AnimationCurveUtility.GetMinKeyframe(curve);
        }

        /// <inheritdoc cref="AnimationCurveUtility.GetMaxKeyframe(AnimationCurve)"/>
        public static Keyframe GetMaxKeyframe(this AnimationCurve curve)
        {
            return AnimationCurveUtility.GetMaxKeyframe(curve);
        }

        /// <inheritdoc cref="AnimationCurveUtility.GetMinTime(AnimationCurve)"/>
        public static float GetMinTime(this AnimationCurve curve)
        {
            return AnimationCurveUtility.GetMinTime(curve);
        }

        /// <inheritdoc cref="AnimationCurveUtility.GetMaxTime(AnimationCurve)"/>
        public static float GetMaxTime(this AnimationCurve curve)
        {
            return AnimationCurveUtility.GetMaxTime(curve);
        }

        /// <inheritdoc cref="AnimationCurveUtility.GetMinValue(AnimationCurve)"/>
        public static float GetMinValue(this AnimationCurve curve)
        {
            return AnimationCurveUtility.GetMinValue(curve);
        }

        /// <inheritdoc cref="AnimationCurveUtility.GetMaxValue(AnimationCurve)"/>
        public static float GetMaxValue(this AnimationCurve curve)
        {
            return AnimationCurveUtility.GetMaxValue(curve);
        }

        /// <inheritdoc cref="AnimationCurveUtility.Loop(AnimationCurve)"/>
        public static AnimationCurve Loop(this AnimationCurve curve)
        {
            return AnimationCurveUtility.Loop(curve);
        }

        /// <inheritdoc cref="AnimationCurveUtility.PingPong(AnimationCurve)"/>
        public static AnimationCurve PingPong(this AnimationCurve curve)
        {
            return AnimationCurveUtility.PingPong(curve);
        }

    }

}