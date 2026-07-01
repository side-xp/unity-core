using NUnit.Framework;

using UnityEngine;

namespace SideXP.Core.Tests
{

    // These extensions are thin forwarders to AnimationCurveUtility (covered separately); the tests confirm
    // each one delegates and returns the expected value for a representative curve.
    public class AnimationCurveExtensionsTests
    {

        private const float Tolerance = 1e-4f;

        // Keys: (t=0, v=1), (t=1, v=5), (t=2, v=3). min value 1, max value 5, duration 2.
        private static AnimationCurve MakeCurve()
        {
            return new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(1f, 5f),
                new Keyframe(2f, 3f));
        }

        [Test]
        public void DefaultKeyframe_IsZeroTimeZeroValue()
        {
            Assert.AreEqual(0f, AnimationCurveExtensions.DefaultKeyframe.time, Tolerance);
            Assert.AreEqual(0f, AnimationCurveExtensions.DefaultKeyframe.value, Tolerance);
        }

        [Test]
        public void GetDuration_ReturnsTimeSpan()
        {
            Assert.AreEqual(2f, MakeCurve().GetDuration(), Tolerance);
        }

        [Test]
        public void GetRange_ReturnsValueSpan()
        {
            Assert.AreEqual(4f, MakeCurve().GetRange(), Tolerance);
        }

        [Test]
        public void GetMinTime_And_GetMaxTime()
        {
            AnimationCurve curve = MakeCurve();
            Assert.AreEqual(0f, curve.GetMinTime(), Tolerance);
            Assert.AreEqual(2f, curve.GetMaxTime(), Tolerance);
        }

        [Test]
        public void GetMinValue_And_GetMaxValue()
        {
            AnimationCurve curve = MakeCurve();
            Assert.AreEqual(1f, curve.GetMinValue(), Tolerance);
            Assert.AreEqual(5f, curve.GetMaxValue(), Tolerance);
        }

        [Test]
        public void GetFirstKeyframe_And_GetLastKeyframe()
        {
            AnimationCurve curve = MakeCurve();
            Assert.AreEqual(0f, curve.GetFirstKeyframe().time, Tolerance);
            Assert.AreEqual(2f, curve.GetLastKeyframe().time, Tolerance);
        }

        [Test]
        public void GetMinKeyframe_And_GetMaxKeyframe_MatchUtility()
        {
            AnimationCurve curve = MakeCurve();
            Assert.AreEqual(AnimationCurveUtility.GetMinKeyframe(curve).value, curve.GetMinKeyframe().value, Tolerance);
            Assert.AreEqual(AnimationCurveUtility.GetMaxKeyframe(curve).value, curve.GetMaxKeyframe().value, Tolerance);
        }

        [Test]
        public void Loop_SetsLoopWrapModesAndReturnsSameCurve()
        {
            AnimationCurve curve = MakeCurve();
            AnimationCurve result = curve.Loop();

            Assert.AreSame(curve, result);
            Assert.AreEqual(WrapMode.Loop, result.preWrapMode);
            Assert.AreEqual(WrapMode.Loop, result.postWrapMode);
        }

        [Test]
        public void PingPong_SetsPingPongWrapModesAndReturnsSameCurve()
        {
            AnimationCurve curve = MakeCurve();
            AnimationCurve result = curve.PingPong();

            Assert.AreSame(curve, result);
            Assert.AreEqual(WrapMode.PingPong, result.preWrapMode);
            Assert.AreEqual(WrapMode.PingPong, result.postWrapMode);
        }

    }

}
