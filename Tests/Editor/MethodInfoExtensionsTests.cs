using System.Reflection;

using NUnit.Framework;

using SideXP.Core.Reflection;

namespace SideXP.Core.Tests
{

    public class MethodInfoExtensionsTests
    {

        private static MethodInfo Method(string name) => typeof(ReflectionSample).GetMethod(name);

        [Test]
        public void GetSignature_UsesDeclaringType_NotRuntimeInfoType()
        {
            string signature = Method("Method").GetSignature();

            // Regression guard: this used to read functionInfo.GetType() (RuntimeMethodInfo) instead of DeclaringType.
            StringAssert.Contains("SideXP.Core.Tests.ReflectionSample.Method", signature);
            StringAssert.DoesNotContain("RuntimeMethodInfo", signature);
        }

        [Test]
        public void GetSignature_IncludesReturnTypeAndParameters()
        {
            string signature = Method("Method").GetSignature();
            StringAssert.StartsWith("System.Void ", signature);
            StringAssert.Contains("System.Int32 a", signature);
            StringAssert.Contains("System.String b", signature);
        }

        [Test]
        public void GetSignature_NoParameters_HasEmptyParens()
        {
            string signature = Method("ReturnMethod").GetSignature();
            StringAssert.StartsWith("System.Int32 ", signature);
            StringAssert.Contains("ReturnMethod()", signature);
        }

    }

}
