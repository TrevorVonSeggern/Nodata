using System;
using System.Collections.Generic;

namespace NoData.Graph.Interfaces
{
    public interface IPath
    {
        IEnumerable<IEdge> Edge { get; }
        void Traverse(Action<IEdge> edge);
    }
}
