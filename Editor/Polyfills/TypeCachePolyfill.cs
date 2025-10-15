#if !UNITY_2020_OR_NEWER

using SideXP.Core.Reflection;

using System;
using System.Reflection;

namespace SideXP.Core.EditorOnly
{

    public static class TypeCachePolyfill
    {

        public static FieldInfo[] GetFieldsWithAttribute<T>()
        {
            return GetFieldsWithAttribute(typeof(T));
        }

        public static FieldInfo[] GetFieldsWithAttribute(Type attrType)
        {
            using (var fields = new ListPoolScope<FieldInfo>())
            {
                foreach (Assembly assembly in ReflectionUtility.GetProjectAssemblies())
                {
                    foreach (Type type in assembly.GetTypes())
                    {
                        foreach (FieldInfo field in type.GetFields(ReflectionUtility.StaticFlags | ReflectionUtility.InstanceFlags))
                        {
                            if (field.TryGetAttribute(attrType, out _, true))
                                fields.Add(field);
                        }
                    }
                }
                return fields.ToArray();
            }
        }

    }

}

#endif