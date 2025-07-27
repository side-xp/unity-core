using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace SideXP.Core.Reflection
{

    /// <summary>
    /// Miscelaneous functions for working with C# reflection.
    /// </summary>
    public static class ReflectionUtility
    {

        /// <summary>
        /// Targets elements declared on the instance, public and non-public.
        /// </summary>
        public const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>
        /// Targets elements declared static, public and non-public.
        /// </summary>
        public const BindingFlags StaticFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>
        /// The name of the assemblies that are included in Unity C# subset, but are not part of the project.
        /// </summary>
        public static readonly string[] NonProjectAssemblies =
        {
            "mscorlib",
            "UnityEngine",
            "UnityEditor",
            "Unity.",
            "System",
            "Mono.",
            "netstandard",
            "Microsoft"
        };

        #region Types

        /// <summary>
        /// Gets all assemblies in the current app domain.
        /// </summary>
        /// <param name="excludedAssembliesPrefixes">The prefixes of the assembly names to exclude from this query.</param>
        /// <returns>Returns the found assemblies.</returns>
        public static Assembly[] GetAllAssemblies(IList<string> excludedAssembliesPrefixes = null)
        {
            using (var scope = new ListPoolScope<Assembly>())
            {
                // For each assembly in the current domain
                foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
                {
                    string assemblyName = a.GetName().Name;

                    // If assmeblies are filtered
                    if (excludedAssembliesPrefixes != null)
                    {
                        bool isExcluded = false;
                        // For each excluded assembly prefix
                        foreach (string assemblyPrefix in excludedAssembliesPrefixes)
                        {
                            // Mark the current assembly as excluded if the prefix matches
                            if (assemblyName.StartsWith(assemblyPrefix))
                            {
                                isExcluded = true;
                                break;
                            }
                        }

                        // Skip if the current assembly is excluded
                        if (isExcluded)
                        {
                            continue;
                        }
                    }

                    scope.List.Add(a);
                }

                return scope.List.ToArray();
            }
        }

        /// <summary>
        /// Get all assemblies related to the current Unity project.
        /// </summary>
        /// <inheritdoc cref="GetAllAssemblies(IList{string})"/>
        public static Assembly[] GetProjectAssemblies()
        {
            return GetAllAssemblies(NonProjectAssemblies);
        }

        /// <summary>
        /// Gets a type in the project by its full name (namespace and type name).
        /// </summary>
        /// <param name="className">The name of the class to find.</param>
        /// <param name="namespaceName">The name of the namespace that should contain the type.</param>
        /// <param name="type">Outputs the found type.</param>
        /// <returns>Returns true if a type has been found.</returns>
        public static bool FindType(string className, string namespaceName, out Type type)
        {
            Assembly[] assemblies = GetProjectAssemblies();
            type = null;
            string typeFullName = !string.IsNullOrEmpty(namespaceName)
                ? $"{namespaceName}.{className}"
                : className;

            // For each assembly in the project
            foreach (Assembly assembly in assemblies)
            {
                // Check if the current assembly contains the expected type.
                type = assembly.GetType(typeFullName);
                if (type != null)
                    break;
            }

            return type != null;
        }

        #endregion


        #region Attributes

        /// <summary>
        /// Tries to get an attribute of a given type defined on a given member.
        /// </summary>
        /// <param name="member">The member from which to get the attribute.</param>
        /// <param name="attributeType">The type of the attribute to get. Assumes it inherits from <see cref="Attribute"/>, and the member
        /// only uses it once.</param>
        /// <param name="attribute">Outputs the found attribute.</param>
        /// <param name="inherit">If enabled, the ancestors of the given member are also checked.</param>
        /// <returns>Returns true if the custom attribute has been found.</returns>
        public static bool TryGetAttribute(MemberInfo member, Type attributeType, out Attribute attribute, bool inherit = true)
        {
            try
            {
                attribute = member.GetCustomAttribute(attributeType, inherit);
            }
            catch (AmbiguousMatchException)
            {
                attribute = TryGetAttributes(member, attributeType, out Attribute[] attributes, inherit)
                    ? attributes[0]
                    : null;
            }

            return attribute != null;
        }

        /// <typeparam name="TAttribute"><inheritdoc cref="TryGetAttribute(MemberInfo, Type, out Attribute, bool)" path="/param[@name='attributeType']"/></typeparam>
        /// <inheritdoc cref="TryGetAttribute(MemberInfo, Type, out Attribute, bool)"/>
        public static bool TryGetAttribute<TAttribute>(MemberInfo member, out TAttribute attribute, bool inherit = true)
            where TAttribute : Attribute
        {
            if (!TryGetAttribute(member, typeof(TAttribute), out Attribute tmpAttribute, inherit))
            {
                attribute = null;
                return false;
            }

            attribute = tmpAttribute as TAttribute;
            return attribute != null;
        }

        /// <summary>
        /// Tries to get the attributes of a given type defined on a given member.
        /// </summary>
        /// <param name="member">The member from which to get the attributes.</param>
        /// <param name="attributeType">The type of the attributes to get.</param>
        /// <param name="attributes">Outputs the found attributes.</param>
        /// <param name="inherit">If enabled, the ancestors of the given member are also checked.</param>
        /// <returns>Returns true if at least one attribute of the given type has been found.</returns>
        public static bool TryGetAttributes(MemberInfo member, Type attributeType, out Attribute[] attributes, bool inherit = true)
        {
            IEnumerable<Attribute> attributesList = CustomAttributeExtensions.GetCustomAttributes(member, attributeType, inherit);
            attributes = attributesList.Map(i => i);
            return attributes.Length > 0;
        }

        /// <typeparam name="TAttribute"><inheritdoc cref="TryGetAttributes(MemberInfo, Type, out Attribute[], bool)" path="/param[@name='attributeType']"/></typeparam>
        /// <inheritdoc cref="TryGetAttributes(MemberInfo, Type, out Attribute[], bool)"/>
        public static bool TryGetAttributes<TAttribute>(MemberInfo member, out TAttribute[] attributes, bool inherit = true)
            where TAttribute : Attribute
        {
            IEnumerable<TAttribute> attributesList = CustomAttributeExtensions.GetCustomAttributes<TAttribute>(member, inherit);
            attributes = attributesList.Map(i => i);
            return attributes.Length > 0;
        }

        /// <summary>
        /// Checks if a given member has a given attribute.
        /// </summary>
        /// <param name="member">The member from which to check the attribute.</param>
        /// <param name="attributeType">The type of the attribute to check.</param>
        /// <param name="inherit">If enabled, the ancestors of the given member are also checked.</param>
        /// <returns>Returns true if the given memebr has an attribute of the given type.</returns>
        public static bool HasAttribute(MemberInfo member, Type attributeType, bool inherit = true)
        {
            return TryGetAttribute(member, attributeType, out _, inherit);
        }

        /// <typeparam name="TAttribute"><inheritdoc cref="HasAttribute(MemberInfo, Type, bool)" path="/param[@name='attributeType']"/></typeparam>
        /// <inheritdoc cref="HasAttribute(MemberInfo, Type, bool)"/>
        public static bool HasAttribute<TAttribute>(MemberInfo member, bool inherit = true)
            where TAttribute : Attribute
        {
            return HasAttribute(member, typeof(TAttribute), inherit);
        }

        #endregion


        #region Members

        /// <summary>
        /// Checks if a given member (field or property) is exposed (public or private serialized).
        /// </summary>
        /// <param name="member">The member to check.</param>
        /// <returns>Returns true if the given member is exposed.</returns>
        public static bool IsExposed(MemberInfo member)
        {
            return HasAttribute<SerializeField>(member, true) || IsPublic(member);
        }

        /// <summary>
        /// Checks if a given member (field or property) is public.
        /// </summary>
        /// <remarks>A property is considered public if one of its accessors is marked as public.</remarks>
        /// <param name="member">The member to check.</param>
        /// <returns>Returns true if the given member is public.</returns>
        public static bool IsPublic(MemberInfo member)
        {
            if (member is FieldInfo fieldInfo)
            {
                return fieldInfo.IsPublic;
            }
            else if (member is PropertyInfo propertyInfo)
            {
                MethodInfo accessor = propertyInfo.GetGetMethod(false);
                if (accessor != null && accessor.IsPublic)
                    return true;

                accessor = propertyInfo.GetSetMethod(false);
                if (accessor != null && accessor.IsPublic)
                    return true;
            }
            else if (member is FieldOrPropertyInfo fieldOrProperty)
            {
                return fieldOrProperty.IsField
                    ? IsPublic(fieldOrProperty.FieldInfo)
                    : IsPublic(fieldOrProperty.PropertyInfo);
            }

            return false;
        }

        /// <summary>
        /// Checks if a given member (field or property) is private.
        /// </summary>
        /// <remarks>A property is considered private if its accessors are both marked as private.</remarks>
        /// <param name="member">The member to check.</param>
        /// <returns>Returns true if the given member is private.</returns>
        public static bool IsPrivate(MemberInfo member)
        {
            if (member is FieldInfo fieldInfo)
            {
                return fieldInfo.IsPrivate;
            }
            else if (member is PropertyInfo propertyInfo)
            {
                MethodInfo accessor = propertyInfo.GetGetMethod(true);
                if (accessor == null || !accessor.IsPrivate)
                    return false;

                accessor = propertyInfo.GetSetMethod(true);
                if (accessor == null || !accessor.IsPrivate)
                    return false;
            }
            else if (member is FieldOrPropertyInfo fieldOrProperty)
            {
                return fieldOrProperty.IsField
                    ? IsPublic(fieldOrProperty.FieldInfo)
                    : IsPublic(fieldOrProperty.PropertyInfo);
            }

            return false;
        }

        /// <summary>
        /// Gets all the public or private serialized fields of a given type.
        /// </summary>
        /// <param name="type">The type of which to get the exposed fields.</param>
        /// <param name="inherit">If enabled, also get the fields from the given type's ancestors.</param>
        /// <returns>Returns the found public or private serialized fields of the given type.</returns>
        public static FieldInfo[] GetExposedFields(Type type, bool inherit = false)
        {
            using (var scope = new ListPoolScope<FieldInfo>())
            {
                foreach (FieldInfo field in type.GetFields(InstanceFlags))
                {
                    // Skip if the field is not exposed
                    if (!field.IsPublic && field.GetCustomAttribute<SerializeField>() == null)
                        continue;

                    scope.List.Add(field);
                }

                if (inherit && type.BaseType != null)
                    scope.List.InsertRange(0, GetExposedFields(type.BaseType, true));

                return scope.List.ToArray();
            }
        }

        /// <summary>
        /// Gets informations about a named field or property from a given type.
        /// </summary>
        /// <param name="type">The type from which to get the member info.</param>
        /// <param name="name">The name of the member to get.</param>
        /// <param name="inherited">If enabled, this function will try to get the named field or property from the given type, and from any
        /// of its base types.</param>
        /// <param name="bindingFlags">The binding flags used for querying the member.</param>
        /// <returns>Returns the informations about the found field or property.</returns>
        public static FieldOrPropertyInfo GetFieldOrProperty(Type type, string name, bool inherited = false, BindingFlags bindingFlags = InstanceFlags)
        {
            // Try get the named field
            foreach (FieldInfo field in type.GetFields(bindingFlags))
            {
                if (field.Name == name)
                    return new FieldOrPropertyInfo(field);
            }

            // Try get the named property
            foreach (PropertyInfo property in type.GetProperties(bindingFlags))
            {
                if (property.Name == name)
                    return new FieldOrPropertyInfo(property);
            }

            return inherited && type.BaseType != null
                ? GetFieldOrProperty(type.BaseType, name, inherited, bindingFlags)
                : null;
        }

        /// <summary>
        /// Tries to get informations about a named field or property from a given type.
        /// </summary>
        /// <param name="info">Outputs the informations about the found field or property.</param>
        /// <returns>Returns true if the named field or property has been found.</returns>
        /// <inheritdoc cref="GetFieldOrProperty(Type, string, bool, BindingFlags)"/>
        public static bool GetFieldOrProperty(Type type, string name, out FieldOrPropertyInfo info, bool inherited = false, BindingFlags bindingFlags = InstanceFlags)
        {
            info = GetFieldOrProperty(type, name, inherited, bindingFlags);
            return info != null;
        }

        /// <param name="target">The object from which to get the member info.</param>
        /// <inheritdoc cref="GetFieldOrProperty(Type, string, bool, BindingFlags)"/>
        public static FieldOrPropertyInfo GetFieldOrProperty(object target, string name, bool inherited = false, BindingFlags bindingFlags = InstanceFlags)
        {
            return GetFieldOrProperty(target.GetType(), name, inherited, bindingFlags);
        }

        /// <inheritdoc cref="GetFieldOrProperty(object, string, bool, BindingFlags)"/>
        /// <inheritdoc cref="GetFieldOrProperty(Type, string, out FieldOrPropertyInfo, bool, BindingFlags)"/>
        public static bool GetFieldOrProperty(object target, string name, out FieldOrPropertyInfo info, bool inherited = false, BindingFlags bindingFlags = InstanceFlags)
        {
            info = GetFieldOrProperty(target, name, inherited, bindingFlags);
            return info != null;
        }

        /// <typeparam name="T"><inheritdoc cref="GetFieldOrProperty(Type, string, bool, BindingFlags)" path="/param[@name='type']"/></typeparam>
        /// <inheritdoc cref="GetFieldOrProperty(Type, string, bool, BindingFlags)"/>
        public static FieldOrPropertyInfo GetFieldOrProperty<T>(string name, bool inherited = false, BindingFlags bindingFlags = InstanceFlags)
        {
            return GetFieldOrProperty(typeof(T), name, inherited, bindingFlags);
        }

        /// <inheritdoc cref="GetFieldOrProperty{T}(string, bool, BindingFlags)"/>
        /// <inheritdoc cref="GetFieldOrProperty(Type, string, out FieldOrPropertyInfo, bool, BindingFlags)"/>
        public static bool GetFieldOrProperty<T>(string name, out FieldOrPropertyInfo info, bool inherited = false, BindingFlags bindingFlags = InstanceFlags)
        {
            info = GetFieldOrProperty<T>(name, inherited, bindingFlags);
            return info != null;
        }

        /// <summary>
        /// Gets informations about all the fields and properties from a given type.
        /// </summary>
        /// <param name="type">The type from which to get the informations.</param>
        /// <param name="inherited">If enabled, this function will query all the fields and properties from the given type and all its base
        /// types.</param>
        /// <param name="bindingFlags">The binding flags used for querying the member.</param>
        /// <returns>Returns the informations ahout the found fields and properties;</returns>
        public static FieldOrPropertyInfo[] GetFieldsAndProperties(Type type, bool inherited = false, BindingFlags bindingFlags = BindingFlags.Instance)
        {
            using (var scope = new ListPoolScope<FieldOrPropertyInfo>())
            {
                foreach (FieldInfo field in type.GetFields(bindingFlags))
                    scope.List.Add(new FieldOrPropertyInfo(field));

                foreach (PropertyInfo property in type.GetProperties(bindingFlags))
                    scope.List.Add(new FieldOrPropertyInfo(property));

                if (inherited && type.BaseType != null)
                    scope.List.InsertRange(0, GetFieldsAndProperties(type.BaseType, inherited, bindingFlags));

                return scope.List.ToArray();
            }
        }

        /// <param name="target">The object from which to get the informations.</param>
        /// <inheritdoc cref="GetFieldsAndProperties(Type, bool, BindingFlags)"/>
        public static FieldOrPropertyInfo[] GetFieldsAndProperties(object target, bool inherited = false, BindingFlags bindingFlags = BindingFlags.Instance)
        {
            return GetFieldsAndProperties(target.GetType(), inherited, bindingFlags);
        }

        /// <typeparam name="T"><inheritdoc cref="GetFieldsAndProperties(Type, bool, BindingFlags)" path="/param[@name='type']"/></typeparam>
        /// <inheritdoc cref="GetFieldsAndProperties(Type, bool, BindingFlags)"/>
        public static FieldOrPropertyInfo[] GetFieldsAndProperties<T>(bool inherited = false, BindingFlags bindingFlags = BindingFlags.Instance)
        {
            return GetFieldsAndProperties(typeof(T), inherited, bindingFlags);
        }

        /// <summary>
        /// Gets the field or property info from property path.
        /// </summary>
        /// <param name="type">The type from which you want to parse the path.</param>
        /// <param name="path">The property path (as you can get it from <see cref="UnityEditor.SerializedProperty.propertyPath"/>).</param>
        /// <returns>Returns the found field or property informations.</returns>
        /// <inheritdoc cref="GetFieldOrProperty(Type, string, bool, BindingFlags)"/>
        public static FieldOrPropertyInfo GetFieldOrPropertyFromPath(Type type, string path, bool inherited = false, BindingFlags bindingFlags = InstanceFlags)
        {
            // Deal specific array representation from a Unity property path, as you can get it from SerializedProperty
            path = path.Replace(".Array.data[", "[");
            // Split the property path
            string[] propertyPathSplit = path.Split('.');

            string part = propertyPathSplit[0];
            FieldOrPropertyInfo partProperty;

            // Parse array part if applicable
            if (part.Contains("["))
            {
                string elementName = part.Substring(0, part.IndexOf("["));
                partProperty = GetFieldOrProperty(type, elementName, inherited, bindingFlags);
            }
            else
            {
                partProperty = GetFieldOrProperty(type, part, inherited, bindingFlags);
            }

            // Stop if the current part is the last one in the path (and so, is the one to find)
            if (propertyPathSplit.Length == 1)
            {
                return partProperty;
            }
            // Else, navigate to the next part. If the current part property's type is an array, navigate from its element type
            else
            {
                Type partPropertyType = partProperty.Type.IsArray
                    ? partProperty.Type.GetElementType()
                    : partProperty.Type;

                return GetFieldOrPropertyFromPath(partPropertyType, path.Substring(part.Length + 1), inherited);
            }
        }

        /// <param name="info">Outputs the found field or property informations.</param>
        /// <inheritdoc cref="GetFieldOrPropertyFromPath(Type, string, bool, BindingFlags)"/>
        public static bool GetFieldOrPropertyFromPath(Type type, string path, out FieldOrPropertyInfo info, bool inherited = false, BindingFlags bindingFlags = InstanceFlags)
        {
            info = GetFieldOrPropertyFromPath(type, path, inherited, bindingFlags);
            return info != null;
        }

        /// <summary>
        /// Gets a reference or value of an object given a property path (like myObject.myContainer[3].myProperty).<br/>
        /// This function is inspired by the SpacePuppy Unity Framework:
        /// https://github.com/lordofduct/spacepuppy-unity-framework-4.0/blob/master/Framework/com.spacepuppy.core/Editor/src/EditorHelper.cs
        /// </summary>
        /// <param name="source">The object in which you want to get the nested object.</param>
        /// <param name="propertyPath">The property path for navigating to the expected object (like myObject.myContainer[3].myProperty).</param>
        /// <returns>Returns the found nested object, or null if the path didn't lead to a valid object.</returns>
        public static object GetNestedObject(object source, string propertyPath)
        {
            // Deal specific array representation from a Unity property path, as you can get it from SerializedProperty
            propertyPath = propertyPath.Replace(".Array.data[", "[");
            // Split the property path
            string[] propertyPathSplit = propertyPath.Split('.');
            // For each part in the property path
            foreach (string part in propertyPathSplit)
            {
                // If the part targets an item in an array
                if (part.Contains("["))
                {
                    // Extract the name of the property that contains the array
                    string elementName = part.Substring(0, part.IndexOf("["));
                    // Extract the index value
                    int index = Convert.ToInt32(part.Substring(part.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    // Get the item at the expected position in the array
                    source = GetSubObject(source, elementName, index);
                }
                // Else, if the part targets an object
                else
                {
                    // Get that part's target
                    source = GetSubObject(source, part);
                }
            }
            return source;
        }

        /// <inheritdoc cref="GetNestedObject(object, string)"/>
        /// <typeparam name="T">The type of the expected target object.</typeparam>
        public static T GetNestedObject<T>(object source, string propertyPath)
        {
            return (T)GetNestedObject(source, propertyPath);
        }

        #endregion


        #region Private API

        /// <summary>
        /// Gets a container from the given source object by just using its field or property name.
        /// </summary>
        /// <param name="source">The source object from which you want to get the sub object.</param>
        /// <param name="fieldOrPropertyName">The name of the field or property for the container you want to get.</param>
        /// <returns>Returns the found sub-object.</returns>
        private static object GetSubObject(object source, string fieldOrPropertyName)
        {
            if (source == null)
                return null;

            Type type = source.GetType();
            while (type != null)
            {
                FieldOrPropertyInfo fieldOrProperty = GetFieldOrProperty(source.GetType(), fieldOrPropertyName);
                if (fieldOrProperty != null)
                    return fieldOrProperty.GetValue(source);
                type = type.BaseType;
            }
            return null;
        }

        /// <summary>
        /// Gets a container from the given source object by just using its field or property name, and the index of its source array.
        /// </summary>
        /// <param name="index">The index of the property for the container you want to find.</param>
        /// <inheritdoc cref="GetSubObject(object, string)"/>
        private static object GetSubObject(object source, string fieldOrPropertyName, int index)
        {
            IEnumerable enumerable = GetNestedObject(source, fieldOrPropertyName) as IEnumerable;
            if (enumerable == null)
                return null;

            // Iterate through the container to find the expected index
            IEnumerator enm = enumerable.GetEnumerator();
            for (int i = 0; i <= index; i++)
            {
                if (!enm.MoveNext())
                    return null;
            }
            return enm.Current;
        }

        #endregion

    }

}
