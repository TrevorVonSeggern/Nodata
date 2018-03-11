using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using IBaseList = System.Collections.IEnumerable;

namespace NoData.Utility
{
    public static class ClassInfoCache
    {
        private static IDictionary<Type, ClassInfoUtility> cache = new ConcurrentDictionary<Type, ClassInfoUtility>();
        
        public static ClassInfoUtility GetOrAdd(Type type)
        {
            if (cache.TryGetValue(type, out var result))
                return result;
            result = new ClassInfoUtility(type);
            cache.Add(type, result);
            return result;
        }
    }

    public struct ClassInfoUtility
    {
        internal ClassInfoUtility(Type t)
        {
            IEnumerable<string> GetNames(IEnumerable<PropertyInfo> infoList) => infoList.Select(p => p.Name);
            Properties = t.GetProperties();
            PropertyAndType = Properties.ToDictionary(p => p.Name, p => p.PropertyType);
            ExpandableProperties =      Properties.Where(x => x.CanRead && (x.PropertyType.IsNestedPublic || x.PropertyType.IsInterface));
            NonExpandableProperties =    Properties.Where(x => x.CanRead && !x.PropertyType.IsNestedPublic && !x.PropertyType.IsInterface);
            Collections = ExpandableProperties.Where(x => typeof(IBaseList).IsAssignableFrom(x.PropertyType));
            NavigationProperties = ExpandableProperties.Where(x => !typeof(IBaseList).IsAssignableFrom(x.PropertyType));

            PropertyNames = GetNames(Properties);
            ExpandablePropertyNames = GetNames(ExpandableProperties);
            NonExpandablePropertyNames = GetNames(NonExpandableProperties);
            CollectionNames = GetNames(Collections);
            NavigationPropertyNames = GetNames(NavigationProperties);
        }

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
