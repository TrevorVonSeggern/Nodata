using NoData.Graph.Base;
using System;
using System.Collections.Generic;

namespace NoData.Graph.Interfaces
{
    public interface ITree<TVertex, TEdge, TVertexValue, TEdgeValue>
        where TVertex : IVertex<TVertexValue>
        where TVertexValue : IMergable<TVertexValue>
    {
        TVertex Root { get; }
        IEnumerable<ITuple<TEdge, ITree<TVertex, TEdge, TVertexValue, TEdgeValue>>> Children { get; }
        void Traverse(Action<TEdge> callback);
        void Traverse(Action<TVertex> callback);
        void Traverse(Action<TVertex, IEnumerable<TEdge>> callback);
    }
}
