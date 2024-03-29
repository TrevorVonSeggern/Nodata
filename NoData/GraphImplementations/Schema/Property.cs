using System.Collections;
using System.Reflection;
using Immutability;

namespace NoData.GraphImplementations.Schema
{
    [Immutable]
    public class Property
    {
        public string Name { get; }

        public bool IsCollection { get; }
        public bool IsNavigationProperty { get; }
        public bool IsPrimitive => !IsCollection && !IsNavigationProperty;

        public Property(PropertyInfo prop)
        {
            Name = prop.Name;


            if (Utility.ClassInfoUtility.PrimitiveTypeWhiteList.Contains(prop.PropertyType))
            {
                IsCollection = false;
                IsNavigationProperty = false;
            }
            else if (typeof(IEnumerable).IsAssignableFrom(prop.PropertyType) || typeof(IList).IsAssignableFrom(prop.PropertyType) || typeof(ICollection).IsAssignableFrom(prop.PropertyType))
            {
                IsCollection = true;
                IsNavigationProperty = false;
            }
            else
            {
                IsCollection = false;
                IsNavigationProperty = true;
            }
        }
    }
}
