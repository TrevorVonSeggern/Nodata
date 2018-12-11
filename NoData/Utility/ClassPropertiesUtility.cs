using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using QuickCache;
using Immutability;
using Graph;
using IBaseList = System.Collections.IEnumerable;

namespace NoData.Utility
{
    public interface IClassCache : ICacheForever<int, ClassInfoUtility>
    {
        ClassInfoUtility GetOrAdd(Type keyType, Func<ClassInfoUtility> addItemFactory);
        ClassInfoUtility GetOrAdd(Type keyType);
        Type GetTypeFromId(int typeId);
        ClassInfoUtility ClassFromTypeId(int typeId);
    }

    public class ClassCache : DictionaryCache<int, ClassInfoUtility>, IClassCache
    {
        public ClassInfoUtility GetOrAdd(Type keyType, Func<ClassInfoUtility> addItemFactory)
        {
            return base.GetOrAdd(keyType.GetHashCode(), addItemFactory);
        }
        public ClassInfoUtility GetOrAdd(Type keyType) => GetOrAdd(keyType, () => new ClassInfoUtility(keyType));

        public Type GetTypeFromId(int typeId) => Get(typeId).Type;

        public ClassInfoUtility ClassFromTypeId(int typeId) => base.Get(typeId);
    }

    [Immutable]
    public class ClassInfoUtility
    {
        static Type GetNullableType(Type type)
        {
            if (Nullable.GetUnderlyingType(type) != null)
                return type;
            if (type?.IsPrimitive == false)
                return type;
            type = typeof(Nullable<>).MakeGenericType(type);
            return type;
        }

        public static IReadOnlyList<Type> PrimitiveTypeWhiteList => new[] {
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
        }.Select(x => new Type[] { x, GetNullableType(x) }.Distinct()).SelectMany(x => x).ToList().AsReadOnly();

        public ClassInfoUtility(Type t)
        {
            this.Type = t;
            IEnumerable<string> GetNames(IEnumerable<PropertyInfo> infoList) => infoList.Select(p => p.Name);
            Properties = t.GetProperties().Where(x => x.CanRead).ToList().AsReadOnly();
            PropertyAndType = Properties.ToDictionary(p => p.Name, p => p.PropertyType);

            NonExpandableProperties = Properties.Where(p => PrimitiveTypeWhiteList.Contains(p.PropertyType)).ToList().AsReadOnly();
            var nonExpandProperties = NonExpandableProperties;

            ExpandableProperties = Properties.Where(p => !nonExpandProperties.Contains(p)).ToList().AsReadOnly();
            //(!x.PropertyType.Assembly.GetName().Name.StartsWith("System.") ||
            //(x.PropertyType.IsNestedPublic || x.PropertyType.IsInterface))
            //);
            Collections = ExpandableProperties.Where(x => typeof(IBaseList).IsAssignableFrom(x.PropertyType)).ToList().AsReadOnly();

            var collections = Collections;

            NavigationProperties = ExpandableProperties.Where(p => !collections.Contains(p)).ToList().AsReadOnly();

            PropertyNames = GetNames(Properties).ToList().AsReadOnly();
            ExpandablePropertyNames = GetNames(ExpandableProperties).ToList().AsReadOnly();
            NonExpandablePropertyNames = GetNames(NonExpandableProperties).ToList().AsReadOnly();
            CollectionNames = GetNames(Collections).ToList().AsReadOnly();
            NavigationPropertyNames = GetNames(NavigationProperties).ToList().AsReadOnly();

            AccessProperties = Properties.Select(p => ITuple.Create<string, Func<object, object>>(p.Name, p.GetValue))
                .ToDictionary(x => x.Item1, x => x.Item2);
        }

        public Type Type { get; }
        public IReadOnlyDictionary<string, Func<object, object>> AccessProperties { get; }
        public IReadOnlyList<PropertyInfo> Properties { get; }
        public IReadOnlyDictionary<string, Type> PropertyAndType { get; }
        public IReadOnlyList<PropertyInfo> ExpandableProperties { get; }
        public IReadOnlyList<PropertyInfo> NonExpandableProperties { get; }
        public IReadOnlyList<PropertyInfo> Collections { get; }
        public IReadOnlyList<PropertyInfo> NavigationProperties { get; }

        public IReadOnlyList<string> PropertyNames { get; }
        public IReadOnlyList<string> ExpandablePropertyNames { get; }
        public IReadOnlyList<string> NonExpandablePropertyNames { get; }
        public IReadOnlyList<string> CollectionNames { get; }
        public IReadOnlyList<string> NavigationPropertyNames { get; }
    }


    public static class ClassPropertiesUtility<TDto>
    {
        private static ClassInfoUtility Info { get; } = new ClassInfoUtility(typeof(TDto));

        public static IReadOnlyList<PropertyInfo> GetProperties => Info.Properties;
        public static IReadOnlyDictionary<string, Type> GetPropertiesAndType => Info.PropertyAndType;
        public static IReadOnlyList<PropertyInfo> GetExpandableProperties => Info.ExpandableProperties;
        public static IReadOnlyList<PropertyInfo> GetNonExpandableProperties => Info.NonExpandableProperties;
        public static IReadOnlyList<PropertyInfo> GetCollections => Info.Collections;
        public static IReadOnlyList<PropertyInfo> GetNavigationProperties => Info.NavigationProperties;

        public static IReadOnlyList<string> GetPropertyNames => Info.PropertyNames;
        public static IReadOnlyList<string> GetExpandablePropertyNames => Info.ExpandablePropertyNames;
        public static IReadOnlyList<string> GetNonExpandablePropertyNames => Info.NonExpandablePropertyNames;
        public static IReadOnlyList<string> GetCollectionNames => Info.CollectionNames;
        public static IReadOnlyList<string> GetNavigationPropertyNames => Info.NavigationPropertyNames;
    }
}
