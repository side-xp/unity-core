using System;
using System.CodeDom;
using System.Reflection;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Extensions for <see cref="CodeParameterDeclarationExpressionCollection"/> instances.
    /// </summary>
    public static class CodeParameterDeclarationExpressionCollectionExtensions
    {

        /// <summary>
        /// Adds a parameter declaration to this collection, importing the given parameter type if it's not yet.
        /// </summary>
        /// <param name="collection">The collection to which the parameter declaration is added.</param>
        /// <returns>The index at which the new element was inserted.</returns>
        /// <inheritdoc cref="CodeDomUtilities.CreateParameter(Type, string, CodeNamespace, CodeNamespace)"/>
        public static int Add(this CodeParameterDeclarationExpressionCollection collection, Type type, string name, CodeNamespace importsNamespace, CodeNamespace domNamespace = null)
        {
            CodeParameterDeclarationExpression declaration = CodeDomUtilities.CreateParameter(type, name, importsNamespace, domNamespace);
            return collection.Add(declaration);
        }

        /// <inheritdoc cref="Add(CodeParameterDeclarationExpressionCollection, Type, string, CodeNamespace, CodeNamespace)"/>
        /// <inheritdoc cref="CodeDomUtilities.CreateParameter(ParameterInfo, CodeNamespace, CodeNamespace)"/>
        public static int Add(this CodeParameterDeclarationExpressionCollection collection, ParameterInfo param, CodeNamespace importsNamespace, CodeNamespace domNamespace = null)
        {
            CodeParameterDeclarationExpression declaration = CodeDomUtilities.CreateParameter(param, importsNamespace, domNamespace);
            return collection.Add(declaration);
        }

    }

}
