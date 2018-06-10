using System.Collections.Generic;

namespace NoData.Graph
{
    public class PathToProperty : Path
    {
        public ClassProperty Property { get; }
        public PathToProperty(IEnumerable<Edge> edges, ClassProperty property) : base(edges)
        {
            Property = property;
        }
    }
}