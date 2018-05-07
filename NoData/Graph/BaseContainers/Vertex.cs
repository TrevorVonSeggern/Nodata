using System.Collections.Generic;
using System.Linq;

namespace NoData.Graph.Base
{
    using Interfaces;
    using System;

    /// <summary>
    /// A class representation in a graph context.
    /// </summary>
    public class Vertex<TValue> : IVertex<TValue>
        where TValue : IMergable<TValue>
    {
        public virtual TValue Value { get; protected set; }

        public Vertex(TValue value)
        {
            Value = value;
        }

        
        public void Merge(IVertex<TValue> other)
        {
            if (!Value.Equals(other.Value))
                throw new ArgumentException("Can't merge vertex of a different type.");
            Value.Merge(other.Value);
        }

        public virtual object Clone() => MemberwiseClone();
        public override string ToString() => Value.ToString();

    }
}
