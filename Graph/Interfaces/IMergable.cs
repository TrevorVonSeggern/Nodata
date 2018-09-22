using System;
using System.Collections.Generic;

namespace Graph.Interfaces
{
    /// <summary>
    /// A class representation in a graph context.
    /// </summary>
    public interface IMergable<in T>
    {
        void Merge(T other);
    }
}
