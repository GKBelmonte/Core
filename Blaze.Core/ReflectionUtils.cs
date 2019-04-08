using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Blaze.Core
{
    public static class ReflectionUtils
    {
        public static bool IsStruct(this Type self)
        {
            return self.IsValueType && !self.IsPrimitive;
        }

        public static T CreateInstance<T>()
        {
            return (T)CreateInstance(typeof(T));
        }

        public static object CreateInstance(this Type self)
        {
            ConstructorInfo[] ctors = self.GetConstructors(
                BindingFlags.Public 
                | BindingFlags.NonPublic
                | BindingFlags.Instance);
            ConstructorInfo defaultCtor = ctors.FirstOrDefault(ctor => ctor.GetParameters().Length == 0);
            if (defaultCtor != null)
                return defaultCtor.Invoke(new object[0]);
            object obj = FormatterServices.GetUninitializedObject(self);
            return obj;
        }

        public static bool UnsafeSetProperty(object owner, string propertyName, object propertyValue)
        {
            if (owner == null)
                throw new ArgumentNullException($"{nameof(owner)} cannot be null");

            Type type = owner.GetType();
            PropertyInfo pi = type.GetProperty(
                propertyName, 
                BindingFlags.Public 
                | BindingFlags.NonPublic 
                | BindingFlags.Instance);

            if (pi == null)
                throw new InvalidOperationException($"Property '{propertyName}' does not exist in type {type.Name}");

            if (pi.CanWrite)
            {
                pi.SetValue(owner, propertyValue);
                return true;
            }

            FieldInfo backingField = type
                .GetRuntimeFields()
                .Where(a => Regex.IsMatch(a.Name, $"\\A<{nameof(propertyName)}>k__BackingField\\Z"))
                .FirstOrDefault();
            if (backingField == null)
                return false;
            backingField.SetValue(owner, propertyValue);
            return true;
        }
    }
}
