using System.CodeDom;
using System.Reflection;

using NUnit.Framework;

using SideXP.Core.EditorOnly;

namespace SideXP.Core.Tests
{

    public class CodeParameterDeclarationExpressionCollectionExtensionsTests
    {

        private class PlainSample { }

        private static void ParamSample(int normal, ref int byRef) { byRef = 0; }

        private static ParameterInfo[] GetParamSampleParameters()
        {
            return typeof(CodeParameterDeclarationExpressionCollectionExtensionsTests)
                .GetMethod(nameof(ParamSample), BindingFlags.Static | BindingFlags.NonPublic)
                .GetParameters();
        }

        #region Add(Type, name)

        [Test]
        public void Add_Type_Primitive_AddsParameterAndReturnsIndex()
        {
            var collection = new CodeParameterDeclarationExpressionCollection();
            var imports = new CodeNamespace();

            int index = collection.Add(typeof(int), "count", imports);

            Assert.AreEqual(0, index);
            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual("count", collection[0].Name);
            Assert.AreEqual(0, imports.Imports.Count, "a primitive parameter needs no import");
        }

        [Test]
        public void Add_Type_CustomType_AddsImport()
        {
            var collection = new CodeParameterDeclarationExpressionCollection();
            var imports = new CodeNamespace();

            collection.Add(typeof(PlainSample), "sample", imports);

            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual(nameof(PlainSample), collection[0].Type.BaseType);
            Assert.AreEqual(1, imports.Imports.Count);
            Assert.AreEqual(typeof(PlainSample).Namespace, ((CodeNamespaceImport)imports.Imports[0]).Namespace);
        }

        [Test]
        public void Add_Type_MultipleParameters_ReturnsIncrementingIndices()
        {
            var collection = new CodeParameterDeclarationExpressionCollection();
            var imports = new CodeNamespace();

            Assert.AreEqual(0, collection.Add(typeof(int), "a", imports));
            Assert.AreEqual(1, collection.Add(typeof(string), "b", imports));
            Assert.AreEqual(2, collection.Count);
        }

        #endregion


        #region Add(ParameterInfo)

        [Test]
        public void Add_ParameterInfo_AddsParameterAndReturnsIndex()
        {
            var collection = new CodeParameterDeclarationExpressionCollection();
            var imports = new CodeNamespace();
            ParameterInfo[] parameters = GetParamSampleParameters();

            int index = collection.Add(parameters[0], imports);

            Assert.AreEqual(0, index);
            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual("normal", collection[0].Name);
        }

        [Test]
        public void Add_ParameterInfo_PreservesDirection()
        {
            var collection = new CodeParameterDeclarationExpressionCollection();
            var imports = new CodeNamespace();
            ParameterInfo[] parameters = GetParamSampleParameters();

            collection.Add(parameters[1], imports);

            Assert.AreEqual("byRef", collection[0].Name);
            Assert.AreEqual(FieldDirection.Ref, collection[0].Direction);
        }

        #endregion

    }

}
