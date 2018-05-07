﻿using System;
using System.Collections.Generic;

namespace NoData.Graph.Interfaces
{
    /// <summary>
    /// A class representation in a graph context.
    /// </summary>
    public interface IVertex<TValue> : IMergable<IVertex<TValue>>, ICloneable
        where TValue : IMergable<TValue>
    {
        TValue Value { get; }
        string ToString();
    }
}
