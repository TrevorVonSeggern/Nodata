using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Immutability;

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

        [Pure]
        IEnumerable<TVertex> VerticesConnectedTo(TVertex vertex);
        [Pure]
        IEnumerable<TVertex> VerticesConnectedFrom(TVertex vertex);
        [Pure]
        IEnumerable<TEdge> OutgoingEdges(TVertex vertex);
        [Pure]
        IEnumerable<TEdge> IncomingEdges(TVertex vertex);
    }
}
