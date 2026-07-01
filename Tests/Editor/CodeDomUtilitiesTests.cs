using System;
using System.CodeDom;
using System.Reflection;

using NUnit.Framework;

using SideXP.Core.EditorOnly;

namespace SideXP.Core.Tests
{

    public class CodeDomUtilitiesTests
    {

        #region Samples

        // A plain type in this test namespace (SideXP.Core.Tests), used to check name/import handling.
        private class PlainSample { }

        // An abstract type exposing one abstract method and one abstract property, plus a non-abstract member that must be ignored.
        private abstract class AbstractSample
        {
            public abstract int AbstractMethod(string arg);
            public abstract int AbstractProp { get; set; }
            public virtual void VirtualMethod() { }
        }

        // An interface exposing one method and one (get-only) property.
        private interface ISample
        {
            int InterfaceMethod(int x);
            string InterfaceProp { get; }
        }

        // Provides parameters of every direction so CreateParameter(ParameterInfo) can be exercised.
        private static void ParamSample(int normal, ref int byRef, out int byOut, in int byIn) { byOut = 0; }

        private static ParameterInfo[] GetParamSampleParameters()
        {
            return typeof(CodeDomUtilitiesTests)
                .GetMethod(nameof(ParamSample), BindingFlags.Static | BindingFlags.NonPublic)
                .GetParameters();
        }

        // Finds a generated member by name, or null.
        private static T FindMember<T>(CodeTypeDeclaration type, string name) where T : CodeTypeMember
        {
            foreach (CodeTypeMember member in type.Members)
            {
                if (member is T typed && member.Name == name)
                    return typed;
            }
            return null;
        }

        #endregion


        #region GetTypeReference

        [Test]
        public void GetTypeReference_String_UsesKeywordAndNoImport()
        {
            var imports = new CodeNamespace();
            CodeTypeReference typeRef = CodeDomUtilities.GetTypeReference(typeof(string), imports);
            Assert.AreEqual("string", typeRef.BaseType);
            Assert.AreEqual(0, imports.Imports.Count, "string must not trigger a 'System' import");
        }

        [Test]
        public void GetTypeReference_Object_UsesKeywordAndNoImport()
        {
            var imports = new CodeNamespace();
            CodeTypeReference typeRef = CodeDomUtilities.GetTypeReference(typeof(object), imports);
            Assert.AreEqual("object", typeRef.BaseType);
            Assert.AreEqual(0, imports.Imports.Count);
        }

        [Test]
        public void GetTypeReference_Primitive_AddsNoImport()
        {
            var imports = new CodeNamespace();
            CodeDomUtilities.GetTypeReference(typeof(int), imports);
            Assert.AreEqual(0, imports.Imports.Count, "primitives are rendered as keywords, no import needed");
        }

        [Test]
        public void GetTypeReference_CustomType_UsesShortNameAndAddsImport()
        {
            var imports = new CodeNamespace();
            CodeTypeReference typeRef = CodeDomUtilities.GetTypeReference(typeof(PlainSample), imports);
            Assert.AreEqual(nameof(PlainSample), typeRef.BaseType);
            Assert.AreEqual(1, imports.Imports.Count);
            Assert.AreEqual(typeof(PlainSample).Namespace, ((CodeNamespaceImport)imports.Imports[0]).Namespace);
        }

        [Test]
        public void GetTypeReference_NullDomNamespace_DoesNotThrow()
        {
            // Regression: ContainsImport used to dereference domNamespace.Name unconditionally (NRE when omitted).
            var imports = new CodeNamespace();
            CodeTypeReference typeRef = null;
            Assert.DoesNotThrow(() => typeRef = CodeDomUtilities.GetTypeReference(typeof(PlainSample), imports));
            Assert.AreEqual(nameof(PlainSample), typeRef.BaseType);
            Assert.AreEqual(1, imports.Imports.Count);
        }

        [Test]
        public void GetTypeReference_AttributeType_StripsAttributeSuffix()
        {
            var imports = new CodeNamespace();
            CodeTypeReference typeRef = CodeDomUtilities.GetTypeReference(typeof(ObsoleteAttribute), imports);
            Assert.AreEqual("Obsolete", typeRef.BaseType);
        }

        [Test]
        public void GetTypeReference_AttributeBaseType_KeepsName()
        {
            // The "-Attribute" suffix must not be stripped from System.Attribute itself.
            var imports = new CodeNamespace();
            CodeTypeReference typeRef = CodeDomUtilities.GetTypeReference(typeof(Attribute), imports);
            Assert.AreEqual(nameof(Attribute), typeRef.BaseType);
        }

        [Test]
        public void GetTypeReference_FullyQualified_UsesFullNameAndSkipsImport()
        {
            // Regression: the import step used to run even when fullyQualified was set (|| instead of &&).
            var imports = new CodeNamespace();
            CodeTypeReference typeRef = CodeDomUtilities.GetTypeReference(typeof(PlainSample), imports, fullyQualified: true);
            Assert.AreEqual(typeof(PlainSample).FullName, typeRef.BaseType);
            Assert.AreEqual(0, imports.Imports.Count, "a fully-qualified reference must not add an import");
        }

        [Test]
        public void GetTypeReference_SkipImport_ProcessesNameButAddsNoImport()
        {
            var imports = new CodeNamespace();
            CodeTypeReference typeRef = CodeDomUtilities.GetTypeReference(typeof(PlainSample), imports, skipImport: true);
            Assert.AreEqual(nameof(PlainSample), typeRef.BaseType, "the short name is still produced");
            Assert.AreEqual(0, imports.Imports.Count);
        }

        [Test]
        public void GetTypeReference_SameTypeTwice_ImportsOnlyOnce()
        {
            var imports = new CodeNamespace();
            CodeDomUtilities.GetTypeReference(typeof(PlainSample), imports);
            CodeDomUtilities.GetTypeReference(typeof(PlainSample), imports);
            Assert.AreEqual(1, imports.Imports.Count, "an already-imported namespace must not be added again");
        }

        #endregion


        #region ContainsImport

        [Test]
        public void ContainsImport_EmptyNamespace_ReturnsTrue()
        {
            Assert.IsTrue(CodeDomUtilities.ContainsImport("", new CodeNamespace()));
            Assert.IsTrue(CodeDomUtilities.ContainsImport("   ", new CodeNamespace()));
        }

        [Test]
        public void ContainsImport_NullDomNamespace_MissingImport_ReturnsFalse()
        {
            // Regression: used to NRE on the null domNamespace default.
            var imports = new CodeNamespace();
            bool result = true;
            Assert.DoesNotThrow(() => result = CodeDomUtilities.ContainsImport("Some.Namespace", imports));
            Assert.IsFalse(result);
        }

        [Test]
        public void ContainsImport_AlreadyImported_ReturnsTrue()
        {
            var imports = new CodeNamespace();
            imports.Imports.Add(new CodeNamespaceImport("Some.Namespace"));
            Assert.IsTrue(CodeDomUtilities.ContainsImport("Some.Namespace", imports));
        }

        [Test]
        public void ContainsImport_DomNamespaceIsWithinNamespace_ReturnsTrue()
        {
            var imports = new CodeNamespace();
            var dom = new CodeNamespace("SideXP.Core.Tests");
            // The generated script's own namespace already "sees" the outer namespace.
            Assert.IsTrue(CodeDomUtilities.ContainsImport("SideXP.Core", imports, dom));
        }

        [Test]
        public void ContainsImport_TypeOverload_ChecksTypeNamespace()
        {
            var imports = new CodeNamespace();
            imports.Imports.Add(new CodeNamespaceImport(typeof(PlainSample).Namespace));
            Assert.IsTrue(CodeDomUtilities.ContainsImport(typeof(PlainSample), imports));
        }

        #endregion


        #region CreateParameter

        [Test]
        public void CreateParameter_Primitive_SetsTypeAndName()
        {
            var imports = new CodeNamespace();
            CodeParameterDeclarationExpression param = CodeDomUtilities.CreateParameter(typeof(int), "count", imports);
            Assert.AreEqual("count", param.Name);
            Assert.AreEqual(0, imports.Imports.Count);
        }

        [Test]
        public void CreateParameter_CustomType_AddsImport()
        {
            var imports = new CodeNamespace();
            CodeParameterDeclarationExpression param = CodeDomUtilities.CreateParameter(typeof(PlainSample), "sample", imports);
            Assert.AreEqual("sample", param.Name);
            Assert.AreEqual(nameof(PlainSample), param.Type.BaseType);
            Assert.AreEqual(1, imports.Imports.Count);
        }

        [Test]
        public void CreateParameter_ParameterInfo_MapsDirections()
        {
            var imports = new CodeNamespace();
            ParameterInfo[] parameters = GetParamSampleParameters();

            CodeParameterDeclarationExpression normal = CodeDomUtilities.CreateParameter(parameters[0], imports);
            CodeParameterDeclarationExpression byRef = CodeDomUtilities.CreateParameter(parameters[1], imports);
            CodeParameterDeclarationExpression byOut = CodeDomUtilities.CreateParameter(parameters[2], imports);
            CodeParameterDeclarationExpression byIn = CodeDomUtilities.CreateParameter(parameters[3], imports);

            Assert.AreEqual("normal", normal.Name);
            Assert.AreEqual(FieldDirection.In, normal.Direction);

            Assert.AreEqual("byRef", byRef.Name);
            Assert.AreEqual(FieldDirection.Ref, byRef.Direction);

            Assert.AreEqual("byOut", byOut.Name);
            Assert.AreEqual(FieldDirection.Out, byOut.Direction);

            Assert.AreEqual("byIn", byIn.Name);
            Assert.AreEqual(FieldDirection.In, byIn.Direction);
        }

        #endregion


        #region InheritFrom

        [Test]
        public void InheritFrom_NullParent_ReturnsFalse()
        {
            var cls = new CodeTypeDeclaration("Generated");
            var imports = new CodeNamespace();
            Assert.IsFalse(CodeDomUtilities.InheritFrom(cls, null, imports));
            Assert.AreEqual(0, cls.BaseTypes.Count);
        }

        [Test]
        public void InheritFrom_ConcreteParent_AddsBaseTypeAndNoOverrides()
        {
            var cls = new CodeTypeDeclaration("Generated");
            var imports = new CodeNamespace();
            Assert.IsTrue(CodeDomUtilities.InheritFrom(cls, typeof(PlainSample), imports));
            Assert.AreEqual(1, cls.BaseTypes.Count);
            Assert.AreEqual(0, cls.Members.Count, "a non-abstract parent needs no overrides");
        }

        [Test]
        public void InheritFrom_AbstractParent_OverridesMembersWithoutDuplicatingAccessors()
        {
            var cls = new CodeTypeDeclaration("Generated");
            var imports = new CodeNamespace();
            Assert.IsTrue(CodeDomUtilities.InheritFrom(cls, typeof(AbstractSample), imports));

            Assert.AreEqual(1, cls.BaseTypes.Count);

            // The property is emitted once, and its get_/set_ accessors are NOT emitted as separate methods.
            Assert.IsNotNull(FindMember<CodeMemberProperty>(cls, nameof(AbstractSample.AbstractProp)));
            Assert.IsNotNull(FindMember<CodeMemberMethod>(cls, nameof(AbstractSample.AbstractMethod)));
            Assert.IsNull(FindMember<CodeMemberMethod>(cls, "get_" + nameof(AbstractSample.AbstractProp)));
            Assert.IsNull(FindMember<CodeMemberMethod>(cls, "set_" + nameof(AbstractSample.AbstractProp)));

            // The non-abstract virtual member must not be overridden.
            Assert.IsNull(FindMember<CodeMemberMethod>(cls, nameof(AbstractSample.VirtualMethod)));
        }

        [Test]
        public void InheritFrom_AbstractParent_NoOverride_AddsBaseTypeOnly()
        {
            var cls = new CodeTypeDeclaration("Generated");
            var imports = new CodeNamespace();
            Assert.IsTrue(CodeDomUtilities.InheritFrom(cls, typeof(AbstractSample), imports, noOverride: true));
            Assert.AreEqual(1, cls.BaseTypes.Count);
            Assert.AreEqual(0, cls.Members.Count);
        }

        #endregion


        #region OverrideAbstractProperties

        [Test]
        public void OverrideAbstractProperties_AbstractParent_EmitsOverrideProperty()
        {
            var cls = new CodeTypeDeclaration("Generated");
            var imports = new CodeNamespace();
            Assert.IsTrue(CodeDomUtilities.OverrideAbstractProperties(cls, typeof(AbstractSample), imports));

            CodeMemberProperty prop = FindMember<CodeMemberProperty>(cls, nameof(AbstractSample.AbstractProp));
            Assert.IsNotNull(prop);
            Assert.IsTrue(prop.HasGet);
            Assert.IsTrue(prop.HasSet);
            Assert.AreEqual(MemberAttributes.Override, prop.Attributes & MemberAttributes.ScopeMask);
            Assert.AreEqual(MemberAttributes.Public, prop.Attributes & MemberAttributes.AccessMask);
        }

        [Test]
        public void OverrideAbstractProperties_Interface_EmitsPublicFinalProperty()
        {
            var cls = new CodeTypeDeclaration("Generated");
            var imports = new CodeNamespace();
            Assert.IsTrue(CodeDomUtilities.OverrideAbstractProperties(cls, typeof(ISample), imports));

            CodeMemberProperty prop = FindMember<CodeMemberProperty>(cls, nameof(ISample.InterfaceProp));
            Assert.IsNotNull(prop);
            Assert.IsTrue(prop.HasGet);
            Assert.IsFalse(prop.HasSet, "the interface property is get-only");
            Assert.AreEqual(MemberAttributes.Final, prop.Attributes & MemberAttributes.ScopeMask);
            Assert.AreEqual(MemberAttributes.Public, prop.Attributes & MemberAttributes.AccessMask);
        }

        [Test]
        public void OverrideAbstractProperties_ConcreteParent_EmitsNothing()
        {
            var cls = new CodeTypeDeclaration("Generated");
            var imports = new CodeNamespace();
            Assert.IsFalse(CodeDomUtilities.OverrideAbstractProperties(cls, typeof(PlainSample), imports));
            Assert.AreEqual(0, cls.Members.Count);
        }

        #endregion


        #region OverrideAbstractMethods

        [Test]
        public void OverrideAbstractMethods_AbstractParent_ReturnsTrueAndEmitsOverrideMethod()
        {
            // Regression: the return value used to stay false even when methods were emitted,
            // and the "override" scope flag used to be masked away.
            var cls = new CodeTypeDeclaration("Generated");
            var imports = new CodeNamespace();
            Assert.IsTrue(CodeDomUtilities.OverrideAbstractMethods(cls, typeof(AbstractSample), imports));

            CodeMemberMethod method = FindMember<CodeMemberMethod>(cls, nameof(AbstractSample.AbstractMethod));
            Assert.IsNotNull(method);
            Assert.AreEqual(MemberAttributes.Override, method.Attributes & MemberAttributes.ScopeMask);
            Assert.AreEqual(MemberAttributes.Public, method.Attributes & MemberAttributes.AccessMask);

            // Body throws NotImplementedException.
            Assert.AreEqual(1, method.Statements.Count);
            Assert.IsInstanceOf<CodeThrowExceptionStatement>(method.Statements[0]);

            // One parameter, matching the source signature.
            Assert.AreEqual(1, method.Parameters.Count);
            Assert.AreEqual("arg", method.Parameters[0].Name);
        }

        [Test]
        public void OverrideAbstractMethods_SkipsPropertyAccessors()
        {
            // Regression: property get_/set_ accessors (IsSpecialName) must not be emitted as standalone methods.
            var cls = new CodeTypeDeclaration("Generated");
            var imports = new CodeNamespace();

            // AbstractSample's only non-accessor abstract method is AbstractMethod.
            CodeDomUtilities.OverrideAbstractMethods(cls, typeof(AbstractSample), imports);

            foreach (CodeTypeMember member in cls.Members)
                Assert.IsFalse(member.Name.StartsWith("get_") || member.Name.StartsWith("set_"), $"unexpected accessor member: {member.Name}");
        }

        [Test]
        public void OverrideAbstractMethods_Interface_EmitsPublicFinalMethod()
        {
            var cls = new CodeTypeDeclaration("Generated");
            var imports = new CodeNamespace();
            Assert.IsTrue(CodeDomUtilities.OverrideAbstractMethods(cls, typeof(ISample), imports));

            CodeMemberMethod method = FindMember<CodeMemberMethod>(cls, nameof(ISample.InterfaceMethod));
            Assert.IsNotNull(method);
            Assert.AreEqual(MemberAttributes.Final, method.Attributes & MemberAttributes.ScopeMask);
            Assert.AreEqual(MemberAttributes.Public, method.Attributes & MemberAttributes.AccessMask);
        }

        [Test]
        public void OverrideAbstractMethods_ConcreteParent_ReturnsFalse()
        {
            var cls = new CodeTypeDeclaration("Generated");
            var imports = new CodeNamespace();
            Assert.IsFalse(CodeDomUtilities.OverrideAbstractMethods(cls, typeof(PlainSample), imports));
            Assert.AreEqual(0, cls.Members.Count);
        }

        #endregion

    }

}
