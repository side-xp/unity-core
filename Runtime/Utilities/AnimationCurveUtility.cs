using UnityEngine;

namespace SideXP.Core
{

    /// <summary>
    /// Miscellanous functions for working with <see cref="AnimationCurve"/> instances.
    /// </summary>
    public static class AnimationCurveUtility
    {

        #region Fields

        /// <summary>
        /// Default <see cref="Keyframe"/> value.
        /// </summary>
        public static readonly Keyframe DefaultKeyframe = new Keyframe(0f, 0f);

        #endregion


        #region Presets

        /// <summary>
        /// Creates an <see cref="AnimationCurve"/> with linear easing.
        /// </summary>
        public static AnimationCurve Linear
        {
            get
            {
                Keyframe[] keyframes =
                {
                    new Keyframe(0, 0, 1, 1),
                    new Keyframe(1, 1, 1, 1),
                };

                return new AnimationCurve(keyframes);
            }
        }

        /// <summary>
        /// Creates an <see cref="AnimationCurve"/> with in easing.
        /// </summary>
        public static AnimationCurve EaseIn
        {
            get
            {
                Keyframe[] keyframes =
                {
                    new Keyframe(0, 0, 0, 0),
                    new Keyframe(1, 1, 2, 2),
                };

                return new AnimationCurve(keyframes);
            }
        }

        /// <summary>
        /// Creates an <see cref="AnimationCurve"/> with out easing.
        /// </summary>
        public static AnimationCurve EaseOut
        {
            get
            {
                Keyframe[] keyframes =
                {
                    new Keyframe(0, 0, 2, 2),
                    new Keyframe(1, 1, 0, 0),
                };

                return new AnimationCurve(keyframes);
            }
        }

        /// <summary>
        /// Creates an <see cref="AnimationCurve"/> with in-out easing.
        /// </summary>
        public static AnimationCurve EaseInOut
        {
            get
            {
                Keyframe[] keyframes =
                {
                    new Keyframe(0, 0, 0, 0),
                    new Keyframe(1, 1, 0, 0),
                };

                return new AnimationCurve(keyframes);
            }
        }

        #endregion


        #region Public API

        /// <summary>
        /// Gets the first keyframe on X axis of this <see cref="AnimationCurve"/>.
        /// </summary>
        /// <param name="curve">The curve to process.</param>
        /// <returns>Returns the found keyframe, or the default one if there's no keyframe on the curve.</returns>
        public static Keyframe GetFirstKeyframe(AnimationCurve curve)
        {
            int count = curve.keys.Length;
            if (count > 0)
            {
                int minTimeIndex = 0;
                for (int i = 1; i < count; i++)
                {
                    if (curve.keys[i].time < curve.keys[minTimeIndex].time)
                    {
                        minTimeIndex = i;
                    }
                }
                return curve.keys[minTimeIndex];
            }
            return DefaultKeyframe;
        }

        /// <summary>
        /// Gets the last keyframe on X axis of this <see cref="AnimationCurve"/>.
        /// </summary>
        /// <inheritdoc cref="GetFirstKeyframe(AnimationCurve)"/>
        public static Keyframe GetLastKeyframe(AnimationCurve curve)
        {
            int count = curve.keys.Length;
            if (count > 0)
            {
                int maxTimeIndex = 0;
                for (int i = 1; i < count; i++)
                {
                    if (curve.keys[i].time > curve.keys[maxTimeIndex].time)
                    {
                        maxTimeIndex = i;
                    }
                }
                return curve.keys[maxTimeIndex];
            }
            return DefaultKeyframe;
        }

        /// <summary>
        /// Gets the lowest keyframe on Y axis of this <see cref="AnimationCurve"/>.
        /// </summary>
        /// <inheritdoc cref="GetFirstKeyframe(AnimationCurve)"/>
        public static Keyframe GetMinKeyframe(AnimationCurve curve)
        {
            int count = curve.keys.Length;
            if (count > 0)
            {
                int minValueIndex = 0;
                for (int i = 1; i < count; i++)
                {
                    if (curve.keys[i].value < curve.keys[minValueIndex].value)
                    {
                        minValueIndex = i;
                    }
                }
                return curve.keys[minValueIndex];
            }
            return DefaultKeyframe;
        }

        /// <summary>
        /// Gets the highest keyframe on Y axis of this <see cref="AnimationCurve"/>.
        /// </summary>
        /// <inheritdoc cref="GetFirstKeyframe(AnimationCurve)"/>
        public static Keyframe GetMaxKeyframe(AnimationCurve curve)
        {
            int count = curve.keys.Length;
            if (count > 0)
            {
                int maxValueIndex = 0;
                for (int i = 1; i < count; i++)
                {
                    if (curve.keys[i].value > curve.keys[maxValueIndex].value)
                    {
                        maxValueIndex = i;
                    }
                }
                return curve.keys[maxValueIndex];
            }
            return DefaultKeyframe;
        }

        /// <summary>
        /// Gets the minimum time of this <see cref="AnimationCurve"/>.
        /// </summary>
        /// <returns>Returns the found minimum time, or default keyframe's if there's no keyframe on the curve.</returns>
        /// <inheritdoc cref="GetFirstKeyframe(AnimationCurve)"/>
        public static float GetMinTime(AnimationCurve curve)
        {
            return curve.GetFirstKeyframe().time;
        }

        /// <summary>
        /// Gets the maximum time of this <see cref="AnimationCurve"/>.
        /// </summary>
        /// <returns>Returns the found maximum time, or default keyframe's if there's no keyframe on the curve.</returns>
        /// <inheritdoc cref="GetFirstKeyframe(AnimationCurve)"/>
        public static float GetMaxTime(AnimationCurve curve)
        {
            return curve.GetLastKeyframe().time;
        }

        /// <summary>
        /// Gets the minimum value of this <see cref="AnimationCurve"/>.
        /// </summary>
        /// <returns>Returns the found minimum value, or default keyframe's if there's no keyframe on the curve.</returns>
        /// <inheritdoc cref="GetFirstKeyframe(AnimationCurve)"/>
        public static float GetMinValue(AnimationCurve curve)
        {
            return curve.GetMinKeyframe().value;
        }

        /// <summary>
        /// Gets the maximum value of this <see cref="AnimationCurve"/>.
        /// </summary>
        /// <returns>Returns the found maximum value, or default keyframe's if there's no keyframe on the curve.</returns>
        /// <inheritdoc cref="GetFirstKeyframe(AnimationCurve)"/>
        public static float GetMaxValue(AnimationCurve curve)
        {
            return curve.GetMaxKeyframe().value;
        }

        /// <summary>
        /// Calculates the duration of an <see cref="AnimationCurve"/>, using its first and last keyframes on X axis.
        /// </summary>
        /// <returns>Returns the calculated duration of the <see cref="AnimationCurve"/>. Note that it will return 0 if the <see cref="AnimationCurve"/> counts less than 2 keyframes.</returns>
        /// <inheritdoc cref="GetFirstKeyframe(AnimationCurve)"/>
        public static float GetDuration(AnimationCurve curve)
        {
            float minTime = GetFirstKeyframe(curve).time;
            float maxTime = GetLastKeyframe(curve).time;
            return maxTime - minTime;
        }

        /// <summary>
        /// Calculates the value range of an <see cref="AnimationCurve"/>, using its lowest and highest keyframes on Y axis.
        /// </summary>
        /// <returns>Returns the calculated value range of the <see cref="AnimationCurve"/>. Note that it will return 0 if the <see cref="AnimationCurve"/> counts less than 2 keyframes.</returns>
        /// <inheritdoc cref="GetFirstKeyframe(AnimationCurve)"/>
        public static float GetRange(AnimationCurve curve)
        {
            float minRange = GetMinKeyframe(curve).value;
            float maxRange = GetMaxKeyframe(curve).value;
            return maxRange - minRange;
        }

        /// <summary>
        /// Makes this <see cref="AnimationCurve"/> repeat (loop) before its first frame, and after its last frame.
        /// </summary>
        /// <returns>Returns the updated input curve.</returns>
        /// <inheritdoc cref="GetFirstKeyframe(AnimationCurve)"/>
        public static AnimationCurve Loop(AnimationCurve curve)
        {
            curve.preWrapMode = WrapMode.Loop;
            curve.postWrapMode = WrapMode.Loop;
            return curve;
        }

        /// <summary>
        /// Makes this <see cref="AnimationCurve"/> ping-pong before its first frame, and after its last frame.
        /// </summary>
        /// <returns>Returns the updated input curve.</returns>
        /// <inheritdoc cref="GetFirstKeyframe(AnimationCurve)"/>
        public static AnimationCurve PingPong(AnimationCurve curve)
        {
            curve.preWrapMode = WrapMode.PingPong;
            curve.postWrapMode = WrapMode.PingPong;
            return curve;
        }

        #endregion

    }

}