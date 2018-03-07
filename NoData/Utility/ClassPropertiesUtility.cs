using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using IBaseList = System.Collections.IEnumerable;

namespace NoData.Internal.Utility
{
    public static class ClassPropertiesUtility<TDto>
    {
        public static IEnumerable<string> GetProperties()
            => typeof(TDto).GetProperties().Select(x => x.Name);

        private static IDictionary<string, Type> GetPropertyAndTypeFromList(IEnumerable<PropertyInfo> properties)
        {
            var result = new Dictionary<string, Type>();
            foreach (var prop in properties.Where(x => x.CanRead))
            {
                result.Add(prop.Name, prop.PropertyType);
            }
            return result;
        }

        public static readonly IDictionary<string, Type> GetPropertiesAndType = GetPropertyAndTypeFromList(typeof(TDto).GetProperties());

        public static readonly IEnumerable<PropertyInfo> GetExpandableProperties =
                typeof(TDto).GetProperties().Where(x => x.CanRead && (x.PropertyType.IsNestedPublic || x.PropertyType.IsInterface));

        public static readonly IEnumerable<PropertyInfo> GetNonExpandableProperties =
                typeof(TDto).GetProperties().Where(x => x.CanRead && !x.PropertyType.IsNestedPublic && !x.PropertyType.IsInterface);

        public static readonly IEnumerable<string> GetExpandablePropertyNames = GetExpandableProperties.Select(x => x.Name);
        public static readonly IEnumerable<string> GetNonExpandablePropertyNames = GetNonExpandableProperties.Select(x => x.Name);

        public static readonly IEnumerable<PropertyInfo> GetCollections =
            GetExpandableProperties.Where(x => typeof(IBaseList).IsAssignableFrom(x.PropertyType));

        public static readonly IEnumerable<PropertyInfo> GetNavigationProperties =
            GetExpandableProperties.Where(x => !typeof(IBaseList).IsAssignableFrom(x.PropertyType));

        public static readonly IEnumerable<string> GetNavigationPropertyNames = GetNavigationProperties.Select(x => x.Name);
    }
}
