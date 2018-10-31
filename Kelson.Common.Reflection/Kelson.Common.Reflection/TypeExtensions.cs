using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Kelson.Common.Reflection
{
    public static class TypeExtensions
    {
        public class BoundPropertyInfo
        {
            private object Item;
            private PropertyInfo Property;

            public BoundPropertyInfo(object item, PropertyInfo property)
            {
                Item = item;
                Property = property;
            }

            public T Get<T>() => (T)Property.GetGetMethod().Invoke(Item, new object[0]);

            public void Set<T>(T value) => Property.GetSetMethod().Invoke(Item, new object[] { value });
        }

        public class AttributePropertyPair<TAttribute> where TAttribute : Attribute
        {
            public TAttribute Attribute { get; set; }
            public PropertyInfo Property { get; set; }

            public void On(object item) => new BoundPropertyInfo(item, Property);
        }

        public static IEnumerable<AttributePropertyPair<TAttribute>> GetAttributedProperties<TAttribute>(this Type type) where TAttribute : Attribute
        {
            return type.GetProperties()
                        .Select(p => (p, p.GetCustomAttribute<TAttribute>()))
                        .Where(pa => pa.Item2 != null)
                        .Select(pa => new AttributePropertyPair<TAttribute>
                        {
                            Property = pa.Item1,
                            Attribute = pa.Item2,
                        });

        }

        public static IEnumerable<T> EnumValues<T>(this Type t)
        {
            if (typeof(T) != t)
                throw new ArgumentException("Runtime and compile time types do not match");
            foreach (var value in t.GetEnumValues())
                yield return (T)value;
        }

        public static bool IsConvertableFrom<TBase>(this Type t)
            => t.IsConvertableFrom(typeof(TBase));

        public static bool IsConvertableFrom(this Type t, Type tbase)
        {
            if (t == tbase)
                return true;
            else if (tbase.IsSealed)
                return false;
            if (tbase.IsInterface)
                return t.Implements(tbase);
            else
                return t.Extends(tbase);

        }

        private static bool Extends(this Type t, Type tbase)
        {
            while (t.BaseType != null)
            {
                if (t.BaseType == tbase)
                    return true;
                else
                    t = t.BaseType;
            }
            return false;
        }

        public static bool Implements(this Type t, Type tinterface)
        {
            while (t.BaseType != null)
            {
                if (t.GetInterfaces().Contains(tinterface))
                    return true;
                else
                    t = t.BaseType;
            }
            return false;
        }

        public static bool TryGetAttribute<TAttribute>(this Type t, out TAttribute attribute) where TAttribute : Attribute
        {
            attribute = t.GetCustomAttribute<TAttribute>();
            return attribute != default;
        }
    }
}
