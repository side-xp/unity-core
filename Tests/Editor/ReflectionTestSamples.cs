using System;

using UnityEngine;

// These fields are exercised through reflection, so the compiler can't see them being used/assigned.
#pragma warning disable 169, 414, 649

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Base sample type used by the reflection tests, to exercise inheritance.
    /// </summary>
    internal class ReflectionSampleBase
    {
        public int BasePublicField;
        [SerializeField] private int _baseSerializedField;
        private int _basePrivateField;

        public int BasePublicProp { get; set; }
    }

    /// <summary>
    /// Sample type used by the reflection tests, covering the various access levels, members and an attribute.
    /// </summary>
    internal class ReflectionSample : ReflectionSampleBase
    {
        public int PublicField;
        [SerializeField] private int _serializedPrivateField;
        private int _plainPrivateField;
        protected int ProtectedField;

        [Obsolete] public int AttributedField;

        public int[] Numbers;
        public ReflectionSampleBase Nested;

        public int PublicProp { get; set; }
        private int PrivateProp { get; set; }
        public int ReadOnlyPublicProp { get; }
        public int MixedProp { get; private set; }

        public void Method(int a, string b) { }
        public int ReturnMethod() => 0;
    }

}

#pragma warning restore 169, 414, 649
