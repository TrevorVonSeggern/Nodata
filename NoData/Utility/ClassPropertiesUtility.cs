using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Cache;
using CodeTools;
using Graph;
using IBaseList = System.Collections.IEnumerable;

namespace NoData.Utility
{
    public static class ClassInfoCache
    {
        private static ICache<ClassInfoUtility> cache = new DictionaryCache<ClassInfoUtility>();

        public static ClassInfoUtility GetOrAdd(Type type)
        {
            return cache.GetOrAdd(type.FullName, () => new ClassInfoUtility(type));
        }
    }

    [Immutable]
    public class ClassInfoUtility
    {
        private static Type GetNullableType(Type type)
        {
            if (Nullable.GetUnderlyingType(type) != null)
                return type;
            if (type?.IsPrimitive == false)
                return type;
            type = typeof(Nullable<>).MakeGenericType(type);
            return type;
        }

        private IEnumerable<Type> PrimitiveTypeWhiteList => new[] {
            typeof(bool),
            typeof(byte),
            typeof(Int16),
            typeof(Int32),
            typeof(Int64),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(DateTime),
            typeof(DateTimeKind),
            typeof(DateTimeOffset),
            typeof(string),
        }.Select(x => new Type[] { x, GetNullableType(x) }.Distinct()).SelectMany(x => x);

        internal ClassInfoUtility(Type t)
        {
            IEnumerable<string> GetNames(IEnumerable<PropertyInfo> infoList) => infoList.Select(p => p.Name);
            Properties = t.GetProperties().Where(x => x.CanRead);
            PropertyAndType = Properties.ToDictionary(p => p.Name, p => p.PropertyType);

            NonExpandableProperties = Properties.Where(p => PrimitiveTypeWhiteList.Contains(p.PropertyType));
            var nonExpandProperties = NonExpandableProperties;

            ExpandableProperties = Properties.Where(p => !nonExpandProperties.Contains(p));
            //(!x.PropertyType.Assembly.GetName().Name.StartsWith("System.") ||
            //(x.PropertyType.IsNestedPublic || x.PropertyType.IsInterface))
            //);
            Collections = ExpandableProperties.Where(x => typeof(IBaseList).IsAssignableFrom(x.PropertyType));

            var collections = Collections;

            NavigationProperties = ExpandableProperties.Where(p => !collections.Contains(p));

            PropertyNames = GetNames(Properties);
            ExpandablePropertyNames = GetNames(ExpandableProperties);
            NonExpandablePropertyNames = GetNames(NonExpandableProperties);
            CollectionNames = GetNames(Collections);
            NavigationPropertyNames = GetNames(NavigationProperties);

            AccessProperties = Properties.Select(p => ITuple.Create<string, Func<object, object>>(p.Name, p.GetValue))
                .ToDictionary(x => x.Item1, x => x.Item2);
        }

        public readonly IDictionary<string, Func<object, object>> AccessProperties;
        public readonly IEnumerable<PropertyInfo> Properties;
        public readonly IDictionary<string, Type> PropertyAndType;
        public readonly IEnumerable<PropertyInfo> ExpandableProperties;
        public readonly IEnumerable<PropertyInfo> NonExpandableProperties;
        public readonly IEnumerable<PropertyInfo> Collections;
        public readonly IEnumerable<PropertyInfo> NavigationProperties;

        public readonly IEnumerable<string> PropertyNames;
        public readonly IEnumerable<string> ExpandablePropertyNames;
        public readonly IEnumerable<string> NonExpandablePropertyNames;
        public readonly IEnumerable<string> CollectionNames;
        public readonly IEnumerable<string> NavigationPropertyNames;
    }

    public static class ClassPropertiesUtility<TDto>
    {
        private static readonly ClassInfoUtility Info = ClassInfoCache.GetOrAdd(typeof(TDto));

        public static IEnumerable<PropertyInfo> GetProperties => Info.Properties;
        public static IDictionary<string, Type> GetPropertiesAndType => Info.PropertyAndType;
        public static IEnumerable<PropertyInfo> GetExpandableProperties => Info.ExpandableProperties;
        public static IEnumerable<PropertyInfo> GetNonExpandableProperties => Info.NonExpandableProperties;
        public static IEnumerable<PropertyInfo> GetCollections => Info.Collections;
        public static IEnumerable<PropertyInfo> GetNavigationProperties => Info.NavigationProperties;

        public static IEnumerable<string> GetPropertyNames => Info.PropertyNames;
        public static IEnumerable<string> GetExpandablePropertyNames => Info.ExpandablePropertyNames;
        public static IEnumerable<string> GetNonExpandablePropertyNames => Info.NonExpandablePropertyNames;
        public static IEnumerable<string> GetCollectionNames => Info.CollectionNames;
        public static IEnumerable<string> GetNavigationPropertyNames => Info.NavigationPropertyNames;
    }
}
