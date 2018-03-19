using System.Collections.Generic;

namespace NoData.Graph.Interfaces
{
    public interface IGraph
    {
        IEnumerable<IVertex> Vertices { get; }
        IEnumerable<IEdge> Edges { get; }
        IVertex VertexOfValue(object value);
    }
}
