using System;
using System.CodeDom;
using System.Reflection;

namespace SideXP.Core.EditorOnly
{

    /// <inheritdoc cref="CodeDomUtility"/>
    [System.Obsolete("Use " + nameof(CodeDomUtility) + " instead. This class will be removed in a future update.")]
    public static class CodeDomUtilities
    {

        /// <inheritdoc cref="CodeDomUtility.GetTypeReference(Type, CodeNamespace, CodeNamespace, bool, bool)"/>
        public static CodeTypeReference GetTypeReference(Type type, CodeNamespace importsNamespace, CodeNamespace domNamespace = null, bool fullyQualified = false, bool skipImport = false)
            => CodeDomUtility.GetTypeReference(type, importsNamespace, domNamespace, fullyQualified, skipImport);

        /// <inheritdoc cref="CodeDomUtility.ContainsImport(Type, CodeNamespace, CodeNamespace)"/>
        public static bool ContainsImport(Type type, CodeNamespace importsNamespace, CodeNamespace domNamespace = null)
            => CodeDomUtility.ContainsImport(type, importsNamespace, domNamespace);

        /// <inheritdoc cref="CodeDomUtility.ContainsImport(string, CodeNamespace, CodeNamespace)"/>
        public static bool ContainsImport(string namespaceStr, CodeNamespace importsNamespace, CodeNamespace domNamespace = null)
            => CodeDomUtility.ContainsImport(namespaceStr, importsNamespace, domNamespace);

        /// <inheritdoc cref="CodeDomUtility.CreateParameter(Type, string, CodeNamespace, CodeNamespace)"/>
        public static CodeParameterDeclarationExpression CreateParameter(Type type, string name, CodeNamespace importsNamespace, CodeNamespace domNamespace = null)
            => CodeDomUtility.CreateParameter(type, name, importsNamespace, domNamespace);

        /// <inheritdoc cref="CodeDomUtility.CreateParameter(ParameterInfo, CodeNamespace, CodeNamespace)"/>
        public static CodeParameterDeclarationExpression CreateParameter(ParameterInfo param, CodeNamespace importsNamespace, CodeNamespace domNamespace = null)
            => CodeDomUtility.CreateParameter(param, importsNamespace, domNamespace);

        /// <inheritdoc cref="CodeDomUtility.InheritFrom(CodeTypeDeclaration, Type, CodeNamespace, CodeNamespace, bool)"/>
        public static bool InheritFrom(CodeTypeDeclaration inheritorClass, Type parent, CodeNamespace importsNamespace, CodeNamespace domNamespace = null, bool noOverride = false)
            => CodeDomUtility.InheritFrom(inheritorClass, parent, importsNamespace, domNamespace, noOverride);

        /// <inheritdoc cref="CodeDomUtility.OverrideAbstractProperties(CodeTypeDeclaration, Type, CodeNamespace, CodeNamespace)"/>
        public static bool OverrideAbstractProperties(CodeTypeDeclaration inheritorClass, Type parent, CodeNamespace importsNamespace, CodeNamespace domNamespace = null)
            => CodeDomUtility.OverrideAbstractProperties(inheritorClass, parent, importsNamespace, domNamespace);

        /// <inheritdoc cref="CodeDomUtility.OverrideAbstractMethods(CodeTypeDeclaration, Type, CodeNamespace, CodeNamespace)"/>
        public static bool OverrideAbstractMethods(CodeTypeDeclaration inheritorClass, Type parent, CodeNamespace importsNamespace, CodeNamespace domNamespace = null)
            => CodeDomUtility.OverrideAbstractMethods(inheritorClass, parent, importsNamespace, domNamespace);

    }

}
