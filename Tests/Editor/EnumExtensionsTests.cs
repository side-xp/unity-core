using System;

using NUnit.Framework;

namespace SideXP.Core.Tests
{

    public class EnumExtensionsTests
    {

        private enum Sample
        {
            A = 0,
            B = 1,
            C = 2,
        }

        [Flags]
        private enum Flags
        {
            None = 0,
            X = 1 << 0,
            Y = 1 << 1,
            Z = 1 << 2,
        }

        #region IsValid

        [Test]
        public void IsValid_DefinedValue_ReturnsTrue()
        {
            Assert.IsTrue(Sample.B.IsValid());
        }

        [Test]
        public void IsValid_UndefinedValue_ReturnsFalse()
        {
            Assert.IsFalse(((Sample)99).IsValid());
        }

        // IsValid matches a single declared item, so an ad-hoc flag combination that isn't itself
        // declared is reported as invalid.
        [Test]
        public void IsValid_UndeclaredFlagCombination_ReturnsFalse()
        {
            Assert.IsFalse((Flags.X | Flags.Y).IsValid());
        }

        #endregion


        #region Invert

        [Test]
        public void Invert_None_EnablesAllKnownFlags()
        {
            Flags value = Flags.None;
            value.Invert();
            Assert.IsTrue(value.HasFlag(Flags.X));
            Assert.IsTrue(value.HasFlag(Flags.Y));
            Assert.IsTrue(value.HasFlag(Flags.Z));
        }

        [Test]
        public void Invert_SingleFlag_DisablesItAndEnablesOthers()
        {
            Flags value = Flags.X;
            value.Invert();
            Assert.IsFalse(value.HasFlag(Flags.X));
            Assert.IsTrue(value.HasFlag(Flags.Y));
            Assert.IsTrue(value.HasFlag(Flags.Z));
        }

        [Test]
        public void Invert_Twice_RestoresOriginal()
        {
            Flags value = Flags.X | Flags.Z;
            value.Invert();
            value.Invert();
            Assert.AreEqual(Flags.X | Flags.Z, value);
        }

        #endregion


        #region AddFlag

        [Test]
        public void AddFlag_AddsBit()
        {
            Flags value = Flags.X;
            value.AddFlag(Flags.Y);
            Assert.AreEqual(Flags.X | Flags.Y, value);
        }

        [Test]
        public void AddFlag_ExistingFlag_IsIdempotent()
        {
            Flags value = Flags.X | Flags.Y;
            value.AddFlag(Flags.X);
            Assert.AreEqual(Flags.X | Flags.Y, value);
        }

        #endregion


        #region RemoveFlag

        [Test]
        public void RemoveFlag_RemovesBit()
        {
            Flags value = Flags.X | Flags.Y;
            value.RemoveFlag(Flags.X);
            Assert.AreEqual(Flags.Y, value);
        }

        [Test]
        public void RemoveFlag_AbsentFlag_IsNoOp()
        {
            Flags value = Flags.Y;
            value.RemoveFlag(Flags.X);
            Assert.AreEqual(Flags.Y, value);
        }

        #endregion

    }

}
