using System.Collections.Generic;

namespace NoData.GraphImplementations.Schema
{
    public class Path : Graph.Path<Edge, Vertex, EdgeMetaData, ClassInfo>
    {
        public Path(IEnumerable<Edge> edges) : base(edges) { }


        public virtual Path PrependEdge(Edge start)
        {
            var result = new List<Edge>();
            result.Add(start);
            result.AddRange(Edges);
            return new Path(result);
        }
    }
}
