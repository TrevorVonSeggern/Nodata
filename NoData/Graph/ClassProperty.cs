using System;
using System.Reflection;

namespace NoData.Graph
{
    public class ClassProperty
    {
        public Type Type { get; }
        public string Name { get; }
        public bool IsCollection { get; }
        public bool IsNavigationProperty { get; }

        public ClassProperty(PropertyInfo prop)
        {
            Type = prop.PropertyType;
            Name = prop.Name;
        }
    }
}