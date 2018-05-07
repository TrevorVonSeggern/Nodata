using System.Collections.Generic;

namespace NoData.Graph
{
    public class Path : Base.Path<Edge, Vertex, EdgeMetaData, ClassInfo>
    {
        public Path(IEnumerable<Edge> edges) : base(edges) { }
    }
}
