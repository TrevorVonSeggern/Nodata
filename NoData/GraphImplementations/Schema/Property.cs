using System;
using System.Reflection;

namespace NoData.GraphImplementations.Schema
{
    public class Property
    {
        public Type Type { get; }
        public string Name { get; }
        public bool IsCollection { get; }
        public bool IsNavigationProperty { get; }

        public Property(PropertyInfo prop)
        {
            Type = prop.PropertyType;
            Name = prop.Name;
        }
    }
}