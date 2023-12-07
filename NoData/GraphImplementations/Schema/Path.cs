using Immutability;

namespace NoData.GraphImplementations.Schema
{
    [Immutable]
    public class Path : Graph.Path<Edge, Vertex, Property, ClassInfo>
    {
        public Path(IEnumerable<Edge> edges) : base(edges) { }

        public Path PrependEdge(Edge start)
        {
            var result = new List<Edge>();
            result.Add(start);
            result.AddRange(Edges);
            return new Path(result);
        }
    }
}
