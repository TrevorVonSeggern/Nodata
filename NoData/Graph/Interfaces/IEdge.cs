using System;
using System.Collections.Generic;

namespace NoData.Graph.Interfaces
{
    public interface IEdge : ICloneable
    {
        IVertex From { get; }
        IVertex To { get; }
        object Value { get; }

        bool IsFullyConnected(IGraph g);
        bool IsFullyConnected(IEnumerable<IVertex> vertices);

        string ToString();
    }
}
