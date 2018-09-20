using System;
using System.Collections.Generic;
using CodeTools;

namespace Graph.Interfaces
{
    [Immutable]
    public interface IEdge<out TEdgeValue, out TVertex, out TVertexValue> : ICloneable
        where TVertex : IVertex<TVertexValue>
    {
        TVertex From { get; }
        TVertex To { get; }
        TEdgeValue Value { get; }

        string ToString();
    }
}
