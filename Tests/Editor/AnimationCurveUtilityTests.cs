using System;

using NUnit.Framework;

using UnityEngine;

namespace SideXP.Core.Tests
{

    public class AnimationCurveUtilityTests
    {

        private const float Tolerance = 1e-3f;

        // A curve whose values are deliberately not ordered by time, so min/max-by-value differs from first/last-by-time.
        // times:  0   1    2
        // values: 5  -3   10
        private static AnimationCurve MakeSampleCurve()
        {
            return new AnimationCurve(
                new Keyframe(0f, 5f),
                new Keyframe(1f, -3f),
                new Keyframe(2f, 10f));
        }

        #region Presets

        [Test]
        public void Presets_StartAtZeroEndAtOne()
        {
            foreach (AnimationCurve curve in new[]
            {
                AnimationCurveUtility.Linear,
                AnimationCurveUtility.EaseIn,
                AnimationCurveUtility.EaseOut,
                AnimationCurveUtility.EaseInOut,
            })
            {
                Assert.AreEqual(2, curve.keys.Length);
                Assert.AreEqual(0f, curve.Evaluate(0f), Tolerance);
                Assert.AreEqual(1f, curve.Evaluate(1f), Tolerance);
            }
        }

        [Test]
        public void Linear_MidpointIsHalf()
        {
            Assert.AreEqual(0.5f, AnimationCurveUtility.Linear.Evaluate(0.5f), Tolerance);
        }

        [Test]
        public void EaseInOut_IsSymmetricAtMidpoint()
        {
            Assert.AreEqual(0.5f, AnimationCurveUtility.EaseInOut.Evaluate(0.5f), Tolerance);
        }

        [Test]
        public void EaseIn_IsSlowAtStart()
        {
            // Slow start => below the linear midpoint.
            Assert.Less(AnimationCurveUtility.EaseIn.Evaluate(0.5f), 0.5f);
        }

        [Test]
        public void EaseOut_IsFastAtStart()
        {
            // Fast start => above the linear midpoint.
            Assert.Greater(AnimationCurveUtility.EaseOut.Evaluate(0.5f), 0.5f);
        }

        [Test]
        public void Presets_ReturnNewInstanceEachAccess()
        {
            Assert.AreNotSame(AnimationCurveUtility.Linear, AnimationCurveUtility.Linear);
        }

        #endregion


        #region Keyframe accessors

        [Test]
        public void GetFirstKeyframe_ReturnsEarliestByTime()
        {
            Assert.AreEqual(0f, AnimationCurveUtility.GetFirstKeyframe(MakeSampleCurve()).time, Tolerance);
        }

        [Test]
        public void GetLastKeyframe_ReturnsLatestByTime()
        {
            Assert.AreEqual(2f, AnimationCurveUtility.GetLastKeyframe(MakeSampleCurve()).time, Tolerance);
        }

        [Test]
        public void GetMinKeyframe_ReturnsLowestByValue()
        {
            Keyframe key = AnimationCurveUtility.GetMinKeyframe(MakeSampleCurve());
            Assert.AreEqual(-3f, key.value, Tolerance);
            Assert.AreEqual(1f, key.time, Tolerance);
        }

        [Test]
        public void GetMaxKeyframe_ReturnsHighestByValue()
        {
            Keyframe key = AnimationCurveUtility.GetMaxKeyframe(MakeSampleCurve());
            Assert.AreEqual(10f, key.value, Tolerance);
            Assert.AreEqual(2f, key.time, Tolerance);
        }

        [Test]
        public void Accessors_EmptyCurve_ReturnDefaultKeyframe()
        {
            var empty = new AnimationCurve();
            Assert.AreEqual(AnimationCurveUtility.DefaultKeyframe.time, AnimationCurveUtility.GetFirstKeyframe(empty).time, Tolerance);
            Assert.AreEqual(AnimationCurveUtility.DefaultKeyframe.value, AnimationCurveUtility.GetMinKeyframe(empty).value, Tolerance);
        }

        #endregion


        #region Derived getters

        [Test]
        public void MinMaxTimeAndValue()
        {
            var curve = MakeSampleCurve();
            Assert.AreEqual(0f, AnimationCurveUtility.GetMinTime(curve), Tolerance);
            Assert.AreEqual(2f, AnimationCurveUtility.GetMaxTime(curve), Tolerance);
            Assert.AreEqual(-3f, AnimationCurveUtility.GetMinValue(curve), Tolerance);
            Assert.AreEqual(10f, AnimationCurveUtility.GetMaxValue(curve), Tolerance);
        }

        [Test]
        public void GetDuration_IsTimeSpan()
        {
            Assert.AreEqual(2f, AnimationCurveUtility.GetDuration(MakeSampleCurve()), Tolerance);
        }

        [Test]
        public void GetRange_IsValueSpan()
        {
            Assert.AreEqual(13f, AnimationCurveUtility.GetRange(MakeSampleCurve()), Tolerance);
        }

        [Test]
        public void GetDurationAndRange_SingleKeyframe_AreZero()
        {
            var curve = new AnimationCurve(new Keyframe(3f, 7f));
            Assert.AreEqual(0f, AnimationCurveUtility.GetDuration(curve), Tolerance);
            Assert.AreEqual(0f, AnimationCurveUtility.GetRange(curve), Tolerance);
        }

        #endregion


        #region Loop / PingPong

        [Test]
        public void Loop_SetsBothWrapModesAndReturnsSameCurve()
        {
            var curve = AnimationCurveUtility.Linear;
            var result = AnimationCurveUtility.Loop(curve);
            Assert.AreSame(curve, result);
            Assert.AreEqual(WrapMode.Loop, curve.preWrapMode);
            Assert.AreEqual(WrapMode.Loop, curve.postWrapMode);
        }

        [Test]
        public void PingPong_SetsBothWrapModesAndReturnsSameCurve()
        {
            var curve = AnimationCurveUtility.Linear;
            var result = AnimationCurveUtility.PingPong(curve);
            Assert.AreSame(curve, result);
            Assert.AreEqual(WrapMode.PingPong, curve.preWrapMode);
            Assert.AreEqual(WrapMode.PingPong, curve.postWrapMode);
        }

        #endregion


        #region Null guards

        [Test]
        public void CoreMethods_NullCurve_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => AnimationCurveUtility.GetFirstKeyframe(null));
            Assert.Throws<ArgumentNullException>(() => AnimationCurveUtility.GetLastKeyframe(null));
            Assert.Throws<ArgumentNullException>(() => AnimationCurveUtility.GetMinKeyframe(null));
            Assert.Throws<ArgumentNullException>(() => AnimationCurveUtility.GetMaxKeyframe(null));
            Assert.Throws<ArgumentNullException>(() => AnimationCurveUtility.Loop(null));
            Assert.Throws<ArgumentNullException>(() => AnimationCurveUtility.PingPong(null));
        }

        [Test]
        public void DerivedMethods_NullCurve_Throw()
        {
            // These funnel through the guarded core methods.
            Assert.Throws<ArgumentNullException>(() => AnimationCurveUtility.GetDuration(null));
            Assert.Throws<ArgumentNullException>(() => AnimationCurveUtility.GetMinTime(null));
        }

        #endregion

    }

}
