using System.Reflection;

namespace SideXP.Core
{

    /// <summary>
    /// Extension functions for <see cref="MethodInfo"/> values.
    /// </summary>
    public static class MethodInfoExtensions
    {

        /// <summary>
        /// Gets the complete signature of the given function info.
        /// </summary>
        /// <param name="functionInfo">The informations about the function you want to get the signature.</param>
        /// <returns>Returns the complete signature of the function.</returns>
        public static string GetSignature(this MethodInfo functionInfo)
        {
            string[] paramsSignatures = functionInfo.GetParameters().Map(pi => $"{pi.ParameterType} {pi.Name}");
            return $"{functionInfo.ReturnType} {functionInfo.GetType().FullName}.{functionInfo.Name}({string.Join(", ", paramsSignatures)})";
        }

    }

}