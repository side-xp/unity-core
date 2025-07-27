using System;
using System.Reflection;

namespace SideXP.Core.Reflection
{

    /// <summary>
    /// Groups informations about a field or a property queried using C# reflection.
    /// </summary>
    public class FieldOrPropertyInfo : MemberInfo
    {

        #region Fields

        /// <summary>
        /// Informations about the field. Null if the element is a property (see <see cref="Property"/>).
        /// </summary>
        private FieldInfo _fieldInfo;

        /// <summary>
        /// Informations about the property. Null if the element is a field (see <see cref="Field"/>).
        /// </summary>
        private PropertyInfo _propertyInfo;

        #endregion


        #region Lifecycle

        /// <inheritdoc cref="FieldOrPropertyInfo"/>
        /// <param name="memberInfo">The field or property to represent.</param>
        public FieldOrPropertyInfo(MemberInfo memberInfo)
        {
            if (memberInfo is FieldInfo fieldInfo)
            {
                _fieldInfo = fieldInfo;
            }
            else if (memberInfo is PropertyInfo propertyInfo)
            {
                _propertyInfo = propertyInfo;
            }
            else if (memberInfo is FieldOrPropertyInfo fieldOrPropertyInfo)
            {
                if (fieldOrPropertyInfo._fieldInfo != null)
                    _fieldInfo = fieldOrPropertyInfo._fieldInfo;
                else
                    _propertyInfo = fieldOrPropertyInfo._propertyInfo;
            }    
        }

        #endregion


        #region Public API

        /// <inheritdoc cref="_fieldInfo"/>
        public FieldInfo FieldInfo => _fieldInfo;

        /// <inheritdoc cref="_propertyInfo"/>
        public PropertyInfo PropertyInfo => _propertyInfo;

        /// <inheritdoc cref="MemberInfo.Name"/>
        public override string Name => _fieldInfo != null ? _fieldInfo.Name : _propertyInfo.Name;

        /// <inheritdoc cref="MemberInfo.DeclaringType"/>
        public override Type DeclaringType => _fieldInfo != null ? _fieldInfo.DeclaringType : _propertyInfo.DeclaringType;

        /// <inheritdoc cref="MemberInfo.MemberType"/>
        public override MemberTypes MemberType => _fieldInfo != null ? MemberTypes.Field : MemberTypes.Property;

        /// <inheritdoc cref="MemberInfo.ReflectedType"/>
        public override Type ReflectedType => _fieldInfo != null ? _fieldInfo.ReflectedType : _propertyInfo.ReflectedType;

        /// <summary>
        /// Gets the type of the represented field or property.
        /// </summary>
        public Type Type => _fieldInfo != null ? _fieldInfo.FieldType : _propertyInfo.PropertyType;

        /// <summary>
        /// Checks if this object represents a field.
        /// </summary>
        public bool IsField => _fieldInfo != null;

        /// <summary>
        /// Checks if this object represents a property.
        /// </summary>
        public bool IsProperty => _propertyInfo != null;

        /// <summary>
        /// Checks if a valid member has been assigned to this object.
        /// </summary>
        public bool IsValid => _fieldInfo != null || _propertyInfo != null;

        /// <summary>
        /// Checks if the value of the element can be read.
        /// </summary>
        public bool CanRead => _fieldInfo != null || _propertyInfo.CanRead;

        /// <summary>
        /// Checks if the value of the element can be set.
        /// </summary>
        public bool CanWrite => _fieldInfo != null || _propertyInfo.CanWrite;

        /// <inheritdoc cref="ReflectionUtility.IsPublic(MemberInfo)"/>
        public bool IsPublic => ReflectionUtility.IsPublic(_fieldInfo != null ? _fieldInfo : _propertyInfo);

        /// <inheritdoc cref="ReflectionUtility.IsPrivate(MemberInfo)"/>
        public bool IsPrivate => ReflectionUtility.IsPublic(_fieldInfo != null ? _fieldInfo : _propertyInfo);

        /// <inheritdoc cref="MemberInfo.GetCustomAttributes(bool)"/>
        public override object[] GetCustomAttributes(bool inherit)
        {
            return _fieldInfo != null
                ? _fieldInfo.GetCustomAttributes(inherit)
                : _propertyInfo.GetCustomAttributes(inherit);
        }

        /// <inheritdoc cref="MemberInfo.GetCustomAttributes(Type, bool)"/>
        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return _fieldInfo != null
                ? _fieldInfo.GetCustomAttributes(attributeType, inherit)
                : _propertyInfo.GetCustomAttributes(attributeType, inherit);
        }

        /// <inheritdoc cref="MemberInfo.IsDefined(Type, bool)"/>
        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return _fieldInfo != null
                ? _fieldInfo.IsDefined(attributeType, inherit)
                : _propertyInfo.IsDefined(attributeType, inherit);
        }

        /// <summary>
        /// Gets the value of this field or property.
        /// </summary>
        /// <param name="target">The object that owns the field or property to read.</param>
        /// <returns>Returns the value of the field or property.</returns>
        public object GetValue(object target)
        {
            return _fieldInfo != null
                ? _fieldInfo.GetValue(target)
                : _propertyInfo.GetValue(target);
        }

        /// <typeparam name="T">The expected type of the value.</typeparam>
        /// <inheritdoc cref="GetValue(object)"/>
        public T GetValue<T>(object target)
        {
            if (!Type.Is<T>())
                return default;

            return (T)GetValue(target);
        }

        /// <summary>
        /// Sets the value of this field or property.
        /// </summary>
        /// <param name="target">The object that owns the field or property to set.</param>
        /// <param name="value">The value you want to set for the field or property.</param>
        public void SetValue(object target, object value)
        {
            if (_fieldInfo != null)
                _fieldInfo.SetValue(target, value);
            else
                _propertyInfo.SetValue(target, value);
        }

        #endregion

    }

}
