using System;
using System.Collections.Generic;

namespace NoData.Graph.Interfaces
{
    public interface IEdge : ICloneable
    {
        IVertex From { get; }
        IVertex To { get; }
        bool HasMany { get; }
        string Name { get; }

        bool IsFullyConnected(IGraph g);
        bool IsFullyConnected(IEnumerable<IVertex> vertices);
    }
}
