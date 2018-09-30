using System.Collections.Generic;

namespace NoData.Tests.GraphQueryable.SharedTypes
{
    public class Path : Graph.Path<Edge, Vertex, string, string>
    {
        public Path(IEnumerable<Edge> edges) : base(edges)
        {
        }
    }
}