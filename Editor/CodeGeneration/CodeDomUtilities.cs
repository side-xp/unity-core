using System;
using System.CodeDom;
using System.Reflection;

using SideXP.Core.Reflection;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Miscellaneous utility functions for working with code generation.
    /// </summary>
    public static class CodeDomUtilities
    {

        /// <summary>
        /// Creates a type reference, importing that type if it's not yet.
        /// </summary>
        /// <remarks>This function will also remove the "-Attribute" suffix for types that inherit from <see cref="Attribute"/>.</remarks>
        /// <param name="type">The type to reference.</param>
        /// <param name="importsNamespace">The namespace used to store the import statements. A new import statement will be added to it if
        /// the given type is not already imported.</param>
        /// <param name="fullyQualified">By default, the type will be referenced using its name, not its full name (including the
        /// namespace). If checked, the type will be referenced using its full name, skipping the import step (since the namespace will be
        /// explicit).</param>
        /// <param name="skipImport">By default, if the given type is not imported, a new import statement is added to the given imports
        /// namespace. If checked or <paramref name="fullyQualified"/> is enabled, this default behavior is skipped (but the output type
        /// name will be processed as expected).</param>
        /// <param name="domNamespace">The namespace used to store the actual classes to generate. This is used by this function to check if
        /// the named namespace is already included in that namespace.</param>
        /// <returns>Returns the created type reference.</returns>
        public static CodeTypeReference GetTypeReference(Type type, CodeNamespace importsNamespace, CodeNamespace domNamespace = null, bool fullyQualified = false, bool skipImport = false)
        {
            Type elementType = type.IsArray ? type.GetElementType() : type;

            // If the given type is string, we use a custom behavior: since strings are not primitives, the CodeDom will read it as
            // "System.String" and use it instead of "string".
            if (type == typeof(string) || elementType == typeof(string))
                return new CodeTypeReference(type) { BaseType = "string" };
            // Same for "object"
            else if (type == typeof(object) || elementType == typeof(object))
                return new CodeTypeReference(type) { BaseType = "object" };
            // And if the type is a "real" primitive or void, no need to process the type name
            else if (type.IsPrimitive || elementType.IsPrimitive || type == typeof(void))
                return new CodeTypeReference(type);

            string baseTypeName = fullyQualified ? type.FullName : type.Name;

            // Remove the "-Attribute" suffix if applicable
            if (baseTypeName.EndsWith(nameof(Attribute)) && type != typeof(Attribute))
                baseTypeName = baseTypeName.Slice(0, -nameof(Attribute).Length);

            var typeRef = new CodeTypeReference(type) { BaseType = baseTypeName };

            // Import the referenced type if applicable
            if (!skipImport || !fullyQualified)
            {
                if (!ContainsImport(type.Namespace, importsNamespace, domNamespace))
                    importsNamespace.Imports.Add(new CodeNamespaceImport(type.Namespace));
            }

            return typeRef;
        }

        /// <summary>
        /// Checks the namespace of a given type is imported in the script being generated.
        /// </summary>
        /// <inheritdoc cref="ContainsImport(string, CodeNamespace, CodeNamespace)"/>
        public static bool ContainsImport(Type type, CodeNamespace importsNamespace, CodeNamespace domNamespace = null)
        {
            return ContainsImport(type.Namespace, importsNamespace, domNamespace);
        }

        /// <summary>
        /// Checks if a given namespace is imported in the script being generated.
        /// </summary>
        /// <param name="namespaceStr">The namespace to check.</param>
        /// <param name="importsNamespace">The namespace used to store the import statements.</param>
        /// <param name="domNamespace">The namespace used to store the actual classes to generate. This is used by this function to check if
        /// the named namespace is already included in that namespace.</param>
        /// <returns>Returns true if the named namespace is included in import statements.</returns>
        public static bool ContainsImport(string namespaceStr, CodeNamespace importsNamespace, CodeNamespace domNamespace = null)
        {
            if (string.IsNullOrWhiteSpace(namespaceStr))
                return true;

            // Stop if the script being generated is part of the expected namespace
            if (!string.IsNullOrWhiteSpace(domNamespace.Name) && domNamespace.Name.StartsWith(namespaceStr))
                return true;

            // Foreach existing import for the script to generate
            foreach (object import in importsNamespace.Imports)
            {
#if UNITY_2023_1_OR_NEWER
                if (import is not CodeNamespaceImport namespaceImport)
                    continue;
#else
                if (!(import is CodeNamespaceImport namespaceImport))
                    continue;
#endif

                // Check if the base type's namespace is already imported
                if (namespaceStr == namespaceImport.Namespace)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Creates a parameter declaration, importing that type if it's not yet.
        /// </summary>
        /// <param name="type">The type of the parameter.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>Returns the created parameter declaration.</returns>
        /// <inheritdoc cref="GetTypeReference(Type, CodeNamespace, CodeNamespace, bool, bool)"/>
        public static CodeParameterDeclarationExpression CreateParameter(Type type, string name, CodeNamespace importsNamespace, CodeNamespace domNamespace = null)
        {
            return type.IsPrimitive || type == typeof(string) || type == typeof(object)
                ? new CodeParameterDeclarationExpression(type, name)
                : new CodeParameterDeclarationExpression(GetTypeReference(type, importsNamespace, domNamespace: domNamespace), name);
        }

        /// <summary>
        /// Creates a parameter declaration, importing that type if it's not yet, and using the appropriate keyword depending on the
        /// parameter direction.
        /// </summary>
        /// <param name="param">Informations about the parameter.</param>
        /// <inheritdoc cref="CreateParameter(Type, string, CodeNamespace, CodeNamespace)"/>
        public static CodeParameterDeclarationExpression CreateParameter(ParameterInfo param, CodeNamespace importsNamespace, CodeNamespace domNamespace = null)
        {
            CodeParameterDeclarationExpression declaration = null;
            if (param.ParameterType.IsByRef)
            {
                declaration = CreateParameter(param.ParameterType.GetElementType(), param.Name, importsNamespace, domNamespace);

                // Handle parameter direction
                if (param.IsIn)
                    declaration.Direction = FieldDirection.In;
                else if (param.IsOut)
                    declaration.Direction = FieldDirection.Out;
                else
                    declaration.Direction = FieldDirection.Ref;
            }
            else
            {
                declaration = CreateParameter(param.ParameterType, param.Name, importsNamespace, domNamespace);
            }

            return declaration;
        }

        /// <summary>
        /// Appends a given base type to the a given class.
        /// </summary>
        /// <param name="inheritorClass">The class that will inherit from the given parent type.</param>
        /// <param name="parent">The type from which the given inheritor class should inherit.</param>
        /// <param name="noOverride">By default, the system will try to implement all the abstract functions and properties of the given
        /// base type, if it is abstract itself. If checked, this step is skipped, but may cause the script to not compile.</param>
        /// <returns>Returns true if the base type has been added successfully.</returns>
        /// <inheritdoc cref="GetTypeReference(Type, CodeNamespace, CodeNamespace, bool, bool)"/>
        public static bool InheritFrom(CodeTypeDeclaration inheritorClass, Type parent, CodeNamespace importsNamespace, CodeNamespace domNamespace = null, bool noOverride = false)
        {
            if (parent == null)
                return false;

            inheritorClass.BaseTypes.Add(GetTypeReference(parent, importsNamespace, domNamespace));

            if (!parent.IsAbstract || noOverride)
                return true;

            OverrideAbstractProperties(inheritorClass, parent, importsNamespace, domNamespace);
            OverrideAbstractMethods(inheritorClass, parent, importsNamespace, domNamespace);
            return true;
        }

        /// <summary>
        /// Overrides all the abstract properties declared in a given parent type.
        /// </summary>
        /// <param name="inheritorClass">The class that will contain the overrides.</param>
        /// <param name="parent">The type from which to override the properties.</param>
        /// <returns>Returns true if at least one property has been overriden successfully.</returns>
        /// <inheritdoc cref="GetTypeReference(Type, CodeNamespace, CodeNamespace, bool, bool)"/>
        public static bool OverrideAbstractProperties(CodeTypeDeclaration inheritorClass, Type parent, CodeNamespace importsNamespace, CodeNamespace domNamespace = null)
        {
            bool success = false;

            // For each property of the base type
            foreach (PropertyInfo propertyInfo in parent.GetProperties(ReflectionUtility.InstanceFlags))
            {
                MethodInfo getMethod = propertyInfo.GetGetMethod(true);
                MethodInfo setMethod = propertyInfo.GetSetMethod(true);

                // Skip if the property is not abstract
                if (getMethod != null && !getMethod.IsAbstract
                    || (setMethod != null && !setMethod.IsAbstract))
                {
                    continue;
                }

                // Create property member
                var prop = new CodeMemberProperty
                {
                    Name = propertyInfo.Name,
                    Type = GetTypeReference(propertyInfo.PropertyType, importsNamespace, domNamespace: domNamespace),
                    HasGet = getMethod != null,
                    HasSet = setMethod != null,
                };

                // If the parent class is an interface, just use the "public" modifier, since there's no use for "override"
                if (parent.IsInterface)
                {
                    prop.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                }
                // Else, use the "override" attribute, and copy the access modifiers of the original property
                else
                {
                    prop.Attributes = MemberAttributes.Override;

                    MethodAttributes getMethodAtttributes = getMethod != null ? getMethod.Attributes : 0;
                    MethodAttributes setMethodAtttributes = setMethod != null ? setMethod.Attributes : 0;
                    if (getMethodAtttributes.HasFlag(MethodAttributes.Public) || setMethodAtttributes.HasFlag(MethodAttributes.Public))
                        prop.Attributes |= MemberAttributes.Public;
                    else if (getMethodAtttributes.HasFlag(MethodAttributes.Family) || setMethodAtttributes.HasFlag(MethodAttributes.Family))
                        prop.Attributes |= MemberAttributes.Family;
                    else if (getMethodAtttributes.HasFlag(MethodAttributes.FamANDAssem) || setMethodAtttributes.HasFlag(MethodAttributes.FamANDAssem))
                        prop.Attributes |= MemberAttributes.FamilyAndAssembly;
                    else if (getMethodAtttributes.HasFlag(MethodAttributes.FamORAssem) || setMethodAtttributes.HasFlag(MethodAttributes.FamORAssem))
                        prop.Attributes |= MemberAttributes.FamilyOrAssembly;
                }

                // Override property
                inheritorClass.Members.Add(prop);
                success = true;
            }

            return success;
        }

        /// <summary>
        /// Overrides all the abstract methods declared in a given parent type.
        /// </summary>
        /// <param name="inheritorClass">The class that will contain the overrides.</param>
        /// <param name="parent">The type from which to override the methods.</param>
        /// <returns>Returns true if at least one method has been overriden successfully.</returns>
        /// <inheritdoc cref="GetTypeReference(Type, CodeNamespace, CodeNamespace, bool, bool)"/>
        public static bool OverrideAbstractMethods(CodeTypeDeclaration inheritorClass, Type parent, CodeNamespace importsNamespace, CodeNamespace domNamespace = null)
        {
            bool success = false;

            // For each method of the base type
            foreach (MethodInfo methodInfo in parent.GetMethods(ReflectionUtility.InstanceFlags))
            {
                // Skip if the method is not abstract
                if (!methodInfo.IsAbstract)
                    continue;

                // Create method member
                var method = new CodeMemberMethod
                {
                    Name = methodInfo.Name,
                    ReturnType = GetTypeReference(methodInfo.ReturnType, importsNamespace, domNamespace: domNamespace),
                };

                // If the parent class is an interface, just use the "public" modifier, since there's no use for "override"
                if (parent.IsInterface)
                {
                    method.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                }
                // Else, use the "override" attribute, and copy the access modifiers of the original property
                else
                {
                    method.Attributes = MemberAttributes.Override;

                    method.Attributes &= MemberAttributes.AccessMask;
                    if (methodInfo.Attributes.HasFlag(MethodAttributes.Public) || parent.IsInterface)
                        method.Attributes |= MemberAttributes.Public;
                    else if (methodInfo.Attributes.HasFlag(MethodAttributes.Family))
                        method.Attributes |= MemberAttributes.Family;
                    else if (methodInfo.Attributes.HasFlag(MethodAttributes.FamANDAssem))
                        method.Attributes |= MemberAttributes.FamilyAndAssembly;
                    else if (methodInfo.Attributes.HasFlag(MethodAttributes.FamORAssem))
                        method.Attributes |= MemberAttributes.FamilyOrAssembly;
                }

                // Add generic parameters
                foreach (Type genericTypeParam in methodInfo.GetGenericArguments())
                    method.TypeParameters.Add(genericTypeParam.Name);

                // Add parameters
                foreach (ParameterInfo param in methodInfo.GetParameters())
                    method.Parameters.Add(param, importsNamespace, domNamespace);

                // Add NotImplementedException as method implementation
                method.Statements.Add(new CodeThrowExceptionStatement(new CodeObjectCreateExpression(typeof(NotImplementedException))));

                inheritorClass.Members.Add(method);
            }

            return success;
        }

    }

}
