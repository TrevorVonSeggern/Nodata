using System.Collections.Generic;
using CodeTools;

namespace Graph.Interfaces
{
    [Immutable]
    public interface ISubGraph<TVertex, TEdge, TVertexValue, TEdgeValue>
        where TVertex : IVertex<TVertexValue>
        where TEdge : IEdge<TEdgeValue, TVertex, TVertexValue>
    {
        IEnumerable<TVertex> Vertices { get; }
        IEnumerable<TEdge> Edges { get; }
        TVertex VertexOfValue(TVertexValue value);

        IEnumerable<TVertex> VerticesConnectedTo(TVertex vertex);
        IEnumerable<TVertex> VerticesConnectedFrom(TVertex vertex);
        IEnumerable<TEdge> OutgoingEdges(TVertex vertex);
        IEnumerable<TEdge> IncomingEdges(TVertex vertex);
    }
}
