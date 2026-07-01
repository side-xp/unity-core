using NUnit.Framework;

using UnityEngine;

namespace SideXP.Core.Tests
{

    public class GradientExtensionsTests
    {

        private const float Tolerance = 1e-3f;

        private static Gradient MakeGradient()
        {
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.red, 0f), new GradientColorKey(Color.blue, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 1f) });
            return gradient;
        }

        private static void AssertColorApprox(Color expected, Color actual)
        {
            Assert.AreEqual(expected.r, actual.r, Tolerance, "r channel");
            Assert.AreEqual(expected.g, actual.g, Tolerance, "g channel");
            Assert.AreEqual(expected.b, actual.b, Tolerance, "b channel");
            Assert.AreEqual(expected.a, actual.a, Tolerance, "a channel");
        }

        #region Clone

        [Test]
        public void Clone_ReturnsIndependentInstanceWithSameKeysAndMode()
        {
            Gradient original = MakeGradient();
            original.mode = GradientMode.Fixed;

            Gradient clone = original.Clone();

            Assert.AreNotSame(original, clone);
            Assert.AreEqual(GradientMode.Fixed, clone.mode);
            Assert.AreEqual(original.colorKeys.Length, clone.colorKeys.Length);
            // Red at t=0, where the gradient's alpha key is 0 -> (1,0,0,0).
            AssertColorApprox(new Color(1f, 0f, 0f, 0f), clone.Evaluate(0f));
            // Blue at t=1, where alpha is 1 -> (0,0,1,1).
            Assert.AreEqual(1f, clone.Evaluate(1f).b, Tolerance);
        }

        [Test]
        public void Clone_MutatingCloneDoesNotAffectOriginal()
        {
            Gradient original = MakeGradient();
            Gradient clone = original.Clone();

            // Replace the clone's keys entirely; the original must be unaffected.
            clone.SetKeys(
                new[] { new GradientColorKey(Color.green, 0f), new GradientColorKey(Color.green, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });

            // Red at t=0 with alpha 0 -> (1,0,0,0); blue at t=1.
            AssertColorApprox(new Color(1f, 0f, 0f, 0f), original.Evaluate(0f));
            Assert.AreEqual(1f, original.Evaluate(1f).b, Tolerance);
        }

        #endregion


        #region Reverse

        // Regression: Reverse used to assign into gradient.colorKeys[i] (a copy returned by Unity's getter),
        // so the reversed keys were discarded and the result held default (black, time 0) keys. It now builds
        // and assigns whole arrays, so times are genuinely flipped (1 - time).
        [Test]
        public void Reverse_FlipsColorKeyTimes()
        {
            Gradient reversed = MakeGradient().Reverse();

            // Red was at t=0 (now t=1), blue was at t=1 (now t=0).
            Assert.AreEqual(1f, reversed.Evaluate(0f).b, Tolerance); // blue end
            Assert.AreEqual(1f, reversed.Evaluate(1f).r, Tolerance); // red end
        }

        [Test]
        public void Reverse_FlipsAlphaKeyTimes()
        {
            Gradient reversed = MakeGradient().Reverse();

            // Alpha 0 was at t=0 (now t=1), alpha 1 was at t=1 (now t=0).
            Assert.AreEqual(1f, reversed.Evaluate(0f).a, Tolerance);
            Assert.AreEqual(0f, reversed.Evaluate(1f).a, Tolerance);
        }

        [Test]
        public void Reverse_DoesNotMutateOriginal()
        {
            Gradient original = MakeGradient();
            original.Reverse();

            // The original still has red at t=0 with alpha 0.
            AssertColorApprox(new Color(1f, 0f, 0f, 0f), original.Evaluate(0f));
        }

        [Test]
        public void Reverse_PreservesModeAndKeyCount()
        {
            Gradient original = MakeGradient();
            original.mode = GradientMode.Fixed;

            Gradient reversed = original.Reverse();

            Assert.AreEqual(GradientMode.Fixed, reversed.mode);
            Assert.AreEqual(original.colorKeys.Length, reversed.colorKeys.Length);
            Assert.AreEqual(original.alphaKeys.Length, reversed.alphaKeys.Length);
        }

        #endregion

    }

}
