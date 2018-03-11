using System;
using System.Collections.Generic;
using System.Linq;

namespace NoData.Graph
{
    public class ItemInfo
    {
        public ItemInfo(Type type)
        {
            PropertyNames = Utility.ClassInfoCache.GetOrAdd(type).NonExpandablePropertyNames.ToList();
            Type = type;
        }

        public Type Type;
        public IList<string> PropertyNames;
    }

    /// <summary>
    /// A class representation in a graph context.
    /// </summary>
    public class Vertex : Base.Vertex
    {
        public Vertex(ItemInfo value) : base(value)
        {
        }
        public Vertex(Type type) : base(new ItemInfo(type))
        {
        }

        public new ItemInfo Value => base.Value as ItemInfo;
    }
}
