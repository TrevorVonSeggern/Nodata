using System.Collections.Generic;

namespace NoData.GraphImplementations.Schema
{
    public class PathToProperty : Path
    {
        public Property Property { get; }
        public PathToProperty(IEnumerable<Edge> edges, Property property) : base(edges)
        {
            Property = property;
        }
    }
}