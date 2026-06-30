using System;
using System.Reflection;

using NUnit.Framework;

using SideXP.Core.Reflection;

namespace SideXP.Core.Tests
{

    public class MemberInfoExtensionsTests
    {

        private const BindingFlags Flags = ReflectionUtility.InstanceFlags;

        private static FieldInfo Field(string name) => typeof(ReflectionSample).GetField(name, Flags);

        [Test]
        public void IsExposed_IsPublic_IsPrivate_MatchUtility()
        {
            FieldInfo pub = Field("PublicField");
            FieldInfo priv = Field("_plainPrivateField");

            Assert.IsTrue(pub.IsExposed());
            Assert.IsTrue(pub.IsPublic());
            Assert.IsFalse(pub.IsPrivate());

            Assert.IsTrue(priv.IsPrivate());
            Assert.IsFalse(priv.IsPublic());
        }

        [Test]
        public void TryGetAttribute_Extension_Works()
        {
            Assert.IsTrue(Field("AttributedField").TryGetAttribute(out ObsoleteAttribute attr));
            Assert.IsNotNull(attr);
        }

        [Test]
        public void TryGetAttributes_Extension_Works()
        {
            Assert.IsTrue(Field("AttributedField").TryGetAttributes(out ObsoleteAttribute[] attrs));
            Assert.AreEqual(1, attrs.Length);
        }

        [Test]
        public void ObsoleteForwarder_StillWorks()
        {
            // The renamed-typo shim should keep compiling and behaving for explicit calls.
#pragma warning disable 618
            Assert.IsTrue(MemberInfoExtentions.IsPublic(Field("PublicField")));
#pragma warning restore 618
        }

    }

}
