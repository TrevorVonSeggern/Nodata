using System.Collections.Generic;

namespace NoData.Graph.Interfaces
{
    public interface IGraph<TVertex, TEdge, TVertexValue, TEdgeValue>
        where TVertex : IVertex<TVertexValue>
        where TVertexValue : IMergable<TVertexValue>
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
