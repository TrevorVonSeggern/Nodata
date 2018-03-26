using System;
using System.Collections.Generic;

namespace NoData.Graph.Interfaces
{
    public interface IEdge<TEdgeValue, TVertex, TVertexValue> : ICloneable
        where TVertex : IVertex<TVertexValue>
        where TVertexValue : IMergable<TVertexValue>
    {
        TVertex From { get; }
        TVertex To { get; }
        TEdgeValue Value { get; }

        IEdge<TEdgeValue, TVertex, TVertexValue> CloneWithNewReferences(TVertex from, TVertex to);

        string ToString();
    }
}
