using System;
using System.Linq;
using System.Reflection;

using NUnit.Framework;

using SideXP.Core.Reflection;

namespace SideXP.Core.Tests
{

    public class ReflectionUtilityTests
    {

        private const BindingFlags Flags = ReflectionUtility.InstanceFlags;

        private static FieldInfo Field(string name) => typeof(ReflectionSample).GetField(name, Flags);
        private static PropertyInfo Property(string name) => typeof(ReflectionSample).GetProperty(name, Flags);

        #region Assemblies & types

        [Test]
        public void GetAllAssemblies_ReturnsAssemblies()
        {
            Assert.Greater(ReflectionUtility.GetAllAssemblies().Length, 0);
        }

        [Test]
        public void GetAllAssemblies_ExcludesByPrefix()
        {
            Assembly[] filtered = ReflectionUtility.GetAllAssemblies(new[] { "UnityEngine" });
            Assert.IsFalse(filtered.Any(a => a.GetName().Name.StartsWith("UnityEngine")));
        }

        [Test]
        public void GetProjectAssemblies_ExcludesEngineAndBclAssemblies_ButIncludesCore()
        {
            Assembly[] project = ReflectionUtility.GetProjectAssemblies();
            foreach (string prefix in ReflectionUtility.NonProjectAssemblies)
                Assert.IsFalse(project.Any(a => a.GetName().Name.StartsWith(prefix)), $"prefix '{prefix}' should be excluded");

            Assert.Contains(typeof(ReflectionUtility).Assembly, project, "the Core assembly should be a project assembly");
        }

        [Test]
        public void FindType_ExistingProjectType_IsFound()
        {
            Assert.IsTrue(ReflectionUtility.FindType(nameof(ReflectionUtility), "SideXP.Core.Reflection", out Type type));
            Assert.AreEqual(typeof(ReflectionUtility), type);
        }

        [Test]
        public void FindType_UnknownType_ReturnsFalse()
        {
            Assert.IsFalse(ReflectionUtility.FindType("NopeNotAType", "SideXP.Core", out _));
        }

        [Test]
        public void FindType_BclType_NotFound_BecauseSystemIsExcluded()
        {
            Assert.IsFalse(ReflectionUtility.FindType("String", "System", out _));
        }

        #endregion


        #region Attributes

        [Test]
        public void TryGetAttribute_PresentAttribute_ReturnsIt()
        {
            Assert.IsTrue(ReflectionUtility.TryGetAttribute(Field("AttributedField"), typeof(ObsoleteAttribute), out Attribute attr));
            Assert.IsInstanceOf<ObsoleteAttribute>(attr);
        }

        [Test]
        public void TryGetAttribute_Generic_PresentAttribute_ReturnsIt()
        {
            Assert.IsTrue(ReflectionUtility.TryGetAttribute(Field("AttributedField"), out ObsoleteAttribute attr));
            Assert.IsNotNull(attr);
        }

        [Test]
        public void TryGetAttribute_AbsentAttribute_ReturnsFalse()
        {
            Assert.IsFalse(ReflectionUtility.TryGetAttribute(Field("PublicField"), out ObsoleteAttribute _));
        }

        [Test]
        public void TryGetAttributes_ReturnsAllMatching()
        {
            Assert.IsTrue(ReflectionUtility.TryGetAttributes(Field("AttributedField"), out ObsoleteAttribute[] attrs));
            Assert.AreEqual(1, attrs.Length);
        }

        [Test]
        public void HasAttribute_ReflectsPresence()
        {
            Assert.IsTrue(ReflectionUtility.HasAttribute<ObsoleteAttribute>(Field("AttributedField")));
            Assert.IsFalse(ReflectionUtility.HasAttribute<ObsoleteAttribute>(Field("PublicField")));
        }

        #endregion


        #region Access level

        [Test]
        public void IsPublic_Fields()
        {
            Assert.IsTrue(ReflectionUtility.IsPublic(Field("PublicField")));
            Assert.IsFalse(ReflectionUtility.IsPublic(Field("_plainPrivateField")));
        }

        [Test]
        public void IsPrivate_Fields()
        {
            Assert.IsTrue(ReflectionUtility.IsPrivate(Field("_plainPrivateField")));
            Assert.IsFalse(ReflectionUtility.IsPrivate(Field("PublicField")));
        }

        [Test]
        public void IsPublic_Properties()
        {
            Assert.IsTrue(ReflectionUtility.IsPublic(Property("PublicProp")));
            Assert.IsTrue(ReflectionUtility.IsPublic(Property("ReadOnlyPublicProp")));
            Assert.IsTrue(ReflectionUtility.IsPublic(Property("MixedProp")), "a public getter makes the property public");
            Assert.IsFalse(ReflectionUtility.IsPublic(Property("PrivateProp")));
        }

        [Test]
        public void IsPrivate_Properties()
        {
            // Regression guard: IsPrivate used to return false for every property.
            Assert.IsTrue(ReflectionUtility.IsPrivate(Property("PrivateProp")));
            Assert.IsFalse(ReflectionUtility.IsPrivate(Property("PublicProp")));
            Assert.IsFalse(ReflectionUtility.IsPrivate(Property("MixedProp")), "a public accessor means it is not private");
        }

        [Test]
        public void IsExposed_PublicOrSerialized()
        {
            Assert.IsTrue(ReflectionUtility.IsExposed(Field("PublicField")));
            Assert.IsTrue(ReflectionUtility.IsExposed(Field("_serializedPrivateField")), "private [SerializeField] is exposed");
            Assert.IsFalse(ReflectionUtility.IsExposed(Field("_plainPrivateField")));
            Assert.IsFalse(ReflectionUtility.IsExposed(Field("ProtectedField")));
        }

        [Test]
        public void IsPrivate_FieldOrPropertyInfo_DelegatesCorrectly()
        {
            // Regression guard: the FieldOrPropertyInfo branch used to call IsPublic by mistake.
            var privateField = new FieldOrPropertyInfo(Field("_plainPrivateField"));
            var publicField = new FieldOrPropertyInfo(Field("PublicField"));
            Assert.IsTrue(ReflectionUtility.IsPrivate(privateField));
            Assert.IsFalse(ReflectionUtility.IsPrivate(publicField));
        }

        #endregion


        #region Exposed fields

        [Test]
        public void GetExposedFields_DeclaredOnly_IncludesPublicAndSerialized_ExcludesHiddenAndInherited()
        {
            string[] names = ReflectionUtility.GetExposedFields(typeof(ReflectionSample)).Select(f => f.Name).ToArray();

            // Public + private [SerializeField] are exposed.
            CollectionAssert.Contains(names, "PublicField");
            CollectionAssert.Contains(names, "_serializedPrivateField");
            // Non-exposed and inherited members are not.
            CollectionAssert.DoesNotContain(names, "_plainPrivateField");
            CollectionAssert.DoesNotContain(names, "ProtectedField");
            CollectionAssert.DoesNotContain(names, "BasePublicField");
            Assert.IsFalse(names.Any(n => n.Contains("k__BackingField")), "auto-property backing fields are not exposed");
        }

        [Test]
        public void GetExposedFields_Inherited_IncludesBaseWithoutDuplicates()
        {
            string[] names = ReflectionUtility.GetExposedFields(typeof(ReflectionSample), inherit: true).Select(f => f.Name).ToArray();

            CollectionAssert.Contains(names, "PublicField");
            CollectionAssert.Contains(names, "BasePublicField");
            CollectionAssert.Contains(names, "_baseSerializedField");
            // Regression guard: the inherited public field must appear exactly once (no double-counting).
            Assert.AreEqual(1, names.Count(n => n == "BasePublicField"));
        }

        #endregion


        #region Fields and properties

        [Test]
        public void GetFieldsAndProperties_DefaultFlags_ReturnsMembers()
        {
            // Regression guard: the default BindingFlags.Instance used to return nothing.
            FieldOrPropertyInfo[] members = ReflectionUtility.GetFieldsAndProperties(typeof(ReflectionSample));
            string[] names = members.Select(m => m.Name).ToArray();

            Assert.Greater(members.Length, 0);
            CollectionAssert.Contains(names, "PublicField");
            CollectionAssert.Contains(names, "PublicProp");
            CollectionAssert.DoesNotContain(names, "BasePublicField", "inherited members are excluded when inherited:false");
        }

        [Test]
        public void GetFieldsAndProperties_Inherited_IncludesBaseWithoutDuplicates()
        {
            FieldOrPropertyInfo[] members = ReflectionUtility.GetFieldsAndProperties(typeof(ReflectionSample), inherited: true);
            string[] names = members.Select(m => m.Name).ToArray();

            CollectionAssert.Contains(names, "BasePublicField");
            CollectionAssert.Contains(names, "BasePublicProp");
            Assert.AreEqual(1, names.Count(n => n == "BasePublicField"));
            Assert.AreEqual(1, names.Count(n => n == "BasePublicProp"));
        }

        #endregion


        #region GetFieldOrProperty

        [Test]
        public void GetFieldOrProperty_Field()
        {
            FieldOrPropertyInfo info = ReflectionUtility.GetFieldOrProperty(typeof(ReflectionSample), "PublicField");
            Assert.IsNotNull(info);
            Assert.IsTrue(info.IsField);
            Assert.AreEqual(typeof(int), info.Type);
        }

        [Test]
        public void GetFieldOrProperty_Property()
        {
            FieldOrPropertyInfo info = ReflectionUtility.GetFieldOrProperty(typeof(ReflectionSample), "PublicProp");
            Assert.IsNotNull(info);
            Assert.IsTrue(info.IsProperty);
        }

        [Test]
        public void GetFieldOrProperty_Unknown_ReturnsNull()
        {
            Assert.IsNull(ReflectionUtility.GetFieldOrProperty(typeof(ReflectionSample), "Nope"));
        }

        [Test]
        public void GetFieldOrProperty_InheritedPrivateField_RequiresInheritedFlag()
        {
            Assert.IsNull(ReflectionUtility.GetFieldOrProperty(typeof(ReflectionSample), "_baseSerializedField", inherited: false));
            Assert.IsNotNull(ReflectionUtility.GetFieldOrProperty(typeof(ReflectionSample), "_baseSerializedField", inherited: true));
        }

        [Test]
        public void GetFieldOrProperty_OutOverload_AndGenericAndObject()
        {
            Assert.IsTrue(ReflectionUtility.GetFieldOrProperty(typeof(ReflectionSample), "PublicField", out _));
            Assert.IsNotNull(ReflectionUtility.GetFieldOrProperty<ReflectionSample>("PublicField"));
            Assert.IsNotNull(ReflectionUtility.GetFieldOrProperty(new ReflectionSample(), "PublicField"));
        }

        #endregion


        #region Property-path navigation

        [Test]
        public void GetFieldOrPropertyFromPath_Simple()
        {
            FieldOrPropertyInfo info = ReflectionUtility.GetFieldOrPropertyFromPath(typeof(ReflectionSample), "PublicField");
            Assert.AreEqual("PublicField", info.Name);
        }

        [Test]
        public void GetFieldOrPropertyFromPath_Nested()
        {
            FieldOrPropertyInfo info = ReflectionUtility.GetFieldOrPropertyFromPath(typeof(ReflectionSample), "Nested.BasePublicField");
            Assert.AreEqual("BasePublicField", info.Name);
            Assert.AreEqual(typeof(int), info.Type);
        }

        [Test]
        public void GetNestedObject_SimpleField()
        {
            var obj = new ReflectionSample { PublicField = 7 };
            Assert.AreEqual(7, ReflectionUtility.GetNestedObject(obj, "PublicField"));
        }

        [Test]
        public void GetNestedObject_NestedField()
        {
            var obj = new ReflectionSample { Nested = new ReflectionSampleBase { BasePublicField = 42 } };
            Assert.AreEqual(42, ReflectionUtility.GetNestedObject(obj, "Nested.BasePublicField"));
        }

        [Test]
        public void GetNestedObject_ArrayElement_BothPathStyles()
        {
            var obj = new ReflectionSample { Numbers = new[] { 10, 20, 30 } };
            Assert.AreEqual(20, ReflectionUtility.GetNestedObject(obj, "Numbers[1]"));
            // Unity serializes array element paths as "Field.Array.data[i]".
            Assert.AreEqual(30, ReflectionUtility.GetNestedObject(obj, "Numbers.Array.data[2]"));
        }

        [Test]
        public void GetNestedObject_Generic()
        {
            var obj = new ReflectionSample { PublicField = 5 };
            Assert.AreEqual(5, ReflectionUtility.GetNestedObject<int>(obj, "PublicField"));
        }

        [Test]
        public void GetNestedObject_UnknownPath_ReturnsNull()
        {
            Assert.IsNull(ReflectionUtility.GetNestedObject(new ReflectionSample(), "DoesNotExist"));
        }

        #endregion

    }

}
