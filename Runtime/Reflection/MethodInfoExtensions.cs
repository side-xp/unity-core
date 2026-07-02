using System;
using System.Reflection;

namespace SideXP.Core.Reflection
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
            return $"{functionInfo.ReturnType} {functionInfo.DeclaringType.FullName}.{functionInfo.Name}({string.Join(", ", paramsSignatures)})";
        }

    }

}

namespace SideXP.Core
{

    /// <summary>
    /// Obsolete forwarder kept for backward compatibility after <see cref="Reflection.MethodInfoExtensions"/> was moved to
    /// the <c>SideXP.Core.Reflection</c> namespace. The methods here are intentionally not extension methods, so they never
    /// clash with the real ones when both namespaces are imported.
    /// </summary>
    [Obsolete("Moved to SideXP.Core.Reflection.MethodInfoExtensions. Add a 'using SideXP.Core.Reflection;' and use it as an extension method instead. This forwarder will be removed in a future major version.")]
    public static class MethodInfoExtensions
    {

        /// <inheritdoc cref="Reflection.MethodInfoExtensions.GetSignature(MethodInfo)"/>
        public static string GetSignature(MethodInfo functionInfo)
        {
            return Reflection.MethodInfoExtensions.GetSignature(functionInfo);
        }

    }

}
