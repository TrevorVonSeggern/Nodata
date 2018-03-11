using System;
using System.Collections.Generic;

namespace NoData.Graph.Interfaces
{
    /// <summary>
    /// A class representation in a graph context.
    /// </summary>
    public interface IVertex : ICloneable
    {
        object Value { get; }

        IEnumerable<IVertex> ConnectedTo(IGraph g);
        IEnumerable<IVertex> ConnectedFrom(IGraph g);
    }
}
