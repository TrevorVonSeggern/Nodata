using System.Collections.Generic;
using CodeTools;

namespace NoData.GraphImplementations.Schema
{
    [Immutable]
    public class PathToProperty : Path
    {
        public Property Property { get; }
        public PathToProperty(IEnumerable<Edge> edges, Property property) : base(edges)
        {
            Property = property;
        }
    }
}