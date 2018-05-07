using System;
using System.Collections.Generic;

namespace NoData.Graph.Interfaces
{
    public interface IPath <TEdge, TVertex, TEdgeValue, TVertexValue>
        where TVertex : IVertex<TVertexValue>
        where TEdge : IEdge<TEdgeValue, TVertex, TVertexValue>
        where TVertexValue : IMergable<TVertexValue>
    {
        IEnumerable<TEdge> Edges { get; }
        void Traverse(Action<TEdge> edge);
    }
}
