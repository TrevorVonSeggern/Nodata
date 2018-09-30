using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using CodeTools;

namespace Graph.Interfaces
{
    /// <summary>
    /// A class representation in a graph context.
    /// </summary>
    [Immutable]
    public interface IVertex<out TValue>
    {
        TValue Value { get; }
        [Pure]
        string ToString();
    }
}
