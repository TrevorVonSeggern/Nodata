using NoData.Graph.Base;
using System;
using System.Collections.Generic;

namespace NoData.Graph.Interfaces
{
    public interface ITree
    {
        IVertex Root { get; }
        IEnumerable<ITuple<IEdge, ITree>> Children { get; }
        void Traverse(Action<IEdge> callback);
        void Traverse(Action<IVertex> callback);
        void Traverse(Action<IVertex, IEnumerable<IEdge>> callback);
        IGraph Flatten();
    }
}
