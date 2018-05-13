using System.Collections.Generic;

namespace NoData.Graph
{
    public class Path : Base.Path<Edge, Vertex, EdgeMetaData, ClassInfo>
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
