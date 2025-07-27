using System;
using System.Reflection;

namespace SideXP.Core.Reflection
{

    /// <summary>
    /// Extension functions for <see cref="MemberInfo"/> instances.
    /// </summary>
    public static class MemberInfoExtentions
    {

        /// <inheritdoc cref="ReflectionUtility.IsExposed(MemberInfo)"/>
        public static bool IsExposed(this MemberInfo member)
        {
            return ReflectionUtility.IsExposed(member);
        }

        /// <inheritdoc cref="ReflectionUtility.IsPublic(MemberInfo)"/>
        public static bool IsPublic(this MemberInfo member)
        {
            return ReflectionUtility.IsPublic(member);
        }

        /// <inheritdoc cref="ReflectionUtility.IsPrivate(MemberInfo)"/>
        public static bool IsPrivate(this MemberInfo member)
        {
            return ReflectionUtility.IsPrivate(member);
        }

        /// <inheritdoc cref="ReflectionUtility.TryGetAttribute(MemberInfo, Type, out Attribute, bool)"/>
        public static bool TryGetAttribute(this MemberInfo member, Type attributeType, out Attribute attribute, bool inherit = true)
        {
            return ReflectionUtility.TryGetAttribute(member, attributeType, out attribute, inherit);
        }

        /// <inheritdoc cref="ReflectionUtility.TryGetAttribute{TAttribute}(MemberInfo, out TAttribute, bool)"/>
        public static bool TryGetAttribute<TAttribute>(this MemberInfo member, out TAttribute attribute, bool inherit = true)
            where TAttribute : Attribute
        {
            return ReflectionUtility.TryGetAttribute(member, out attribute, inherit);
        }

        /// <inheritdoc cref="ReflectionUtility.TryGetAttributes(MemberInfo, Type, out Attribute[], bool)"/>
        public static bool TryGetAttributes(this MemberInfo member, Type attributeType, out Attribute[] attributes, bool inherit = true)
        {
            return ReflectionUtility.TryGetAttributes(member, attributeType, out attributes, inherit);
        }
        
        /// <inheritdoc cref="ReflectionUtility.TryGetAttributes{TAttribute}(MemberInfo, out TAttribute[], bool)"/>
        public static bool TryGetAttributes<TAttribute>(this MemberInfo member, out TAttribute[] attributes, bool inherit = true)
            where TAttribute : Attribute
        {
            return ReflectionUtility.TryGetAttributes(member, out attributes, inherit);
        }

    }

}
