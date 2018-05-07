using System;
using System.Collections.Generic;

namespace NoData.Graph.Interfaces
{
    /// <summary>
    /// A class representation in a graph context.
    /// </summary>
    public interface IMergable<T>
    {
        void Merge(T other);
    }
}
