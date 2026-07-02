using System;
using System.Reflection;

using NUnit.Framework;

using SideXP.Core.Reflection;

namespace SideXP.Core.Tests
{

    public class FieldOrPropertyInfoTests
    {

        private const BindingFlags Flags = ReflectionUtility.InstanceFlags;

        private static FieldInfo Field(string name) => typeof(ReflectionSample).GetField(name, Flags);
        private static PropertyInfo Property(string name) => typeof(ReflectionSample).GetProperty(name, Flags);

        #region Construction

        [Test]
        public void Constructor_Field_DescribesField()
        {
            var info = new FieldOrPropertyInfo(Field("PublicField"));
            Assert.IsTrue(info.IsField);
            Assert.IsFalse(info.IsProperty);
            Assert.IsTrue(info.IsValid);
            Assert.AreEqual("PublicField", info.Name);
            Assert.AreEqual(typeof(int), info.Type);
            Assert.AreEqual(typeof(ReflectionSample), info.DeclaringType);
            Assert.AreEqual(MemberTypes.Field, info.MemberType);
        }

        [Test]
        public void Constructor_Property_DescribesProperty()
        {
            var info = new FieldOrPropertyInfo(Property("PublicProp"));
            Assert.IsTrue(info.IsProperty);
            Assert.IsFalse(info.IsField);
            Assert.AreEqual("PublicProp", info.Name);
            Assert.AreEqual(typeof(int), info.Type);
            Assert.AreEqual(MemberTypes.Property, info.MemberType);
        }

        [Test]
        public void Constructor_Copy_CopiesUnderlyingMember()
        {
            var original = new FieldOrPropertyInfo(Field("PublicField"));
            var copy = new FieldOrPropertyInfo(original);
            Assert.IsTrue(copy.IsField);
            Assert.AreEqual("PublicField", copy.Name);
        }

        [Test]
        public void Constructor_InvalidMember_Throws()
        {
            MethodInfo method = typeof(ReflectionSample).GetMethod("Method");
            Assert.Throws<ArgumentException>(() => new FieldOrPropertyInfo(method));
        }

        [Test]
        public void Constructor_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new FieldOrPropertyInfo(null));
        }

        #endregion


        #region Read / write capability

        [Test]
        public void CanReadWrite_Field_BothTrue()
        {
            var info = new FieldOrPropertyInfo(Field("PublicField"));
            Assert.IsTrue(info.CanRead);
            Assert.IsTrue(info.CanWrite);
        }

        [Test]
        public void CanReadWrite_ReadOnlyProperty()
        {
            var info = new FieldOrPropertyInfo(Property("ReadOnlyPublicProp"));
            Assert.IsTrue(info.CanRead);
            Assert.IsFalse(info.CanWrite);
        }

        #endregion


        #region Get / set value

        [Test]
        public void GetSetValue_Field()
        {
            var obj = new ReflectionSample();
            var info = new FieldOrPropertyInfo(Field("PublicField"));
            info.SetValue(obj, 99);
            Assert.AreEqual(99, info.GetValue(obj));
            Assert.AreEqual(99, obj.PublicField);
        }

        [Test]
        public void GetSetValue_Property()
        {
            var obj = new ReflectionSample();
            var info = new FieldOrPropertyInfo(Property("PublicProp"));
            info.SetValue(obj, 123);
            Assert.AreEqual(123, info.GetValue(obj));
            Assert.AreEqual(123, obj.PublicProp);
        }

        [Test]
        public void GetValueGeneric_MatchingType_ReturnsValue()
        {
            var obj = new ReflectionSample { PublicField = 8 };
            var info = new FieldOrPropertyInfo(Field("PublicField"));
            Assert.AreEqual(8, info.GetValue<int>(obj));
        }

        [Test]
        public void GetValueGeneric_IncompatibleType_ReturnsDefault()
        {
            var obj = new ReflectionSample { PublicField = 8 };
            var info = new FieldOrPropertyInfo(Field("PublicField"));
            Assert.IsNull(info.GetValue<string>(obj));
        }

        #endregion


        #region Access level & attributes

        [Test]
        public void IsPublic_IsPrivate_DelegateToReflectionUtility()
        {
            var pub = new FieldOrPropertyInfo(Field("PublicField"));
            var priv = new FieldOrPropertyInfo(Field("_plainPrivateField"));
            Assert.IsTrue(pub.IsPublic);
            Assert.IsFalse(pub.IsPrivate);
            Assert.IsTrue(priv.IsPrivate);
            Assert.IsFalse(priv.IsPublic);
        }

        [Test]
        public void Attributes_ForwardToUnderlyingMember()
        {
            var info = new FieldOrPropertyInfo(Field("AttributedField"));
            Assert.IsTrue(info.IsDefined(typeof(ObsoleteAttribute), true));
            Assert.IsTrue(Array.Exists(info.GetCustomAttributes(true), a => a is ObsoleteAttribute));
            Assert.AreEqual(1, info.GetCustomAttributes(typeof(ObsoleteAttribute), true).Length);
        }

        #endregion

    }

}
