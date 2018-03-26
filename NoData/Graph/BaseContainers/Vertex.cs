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
        public Vertex(TValue value)
        {
            this.value = value;
        }

        private readonly TValue value;
        public TValue Value { get { return value; } }
        
        public void Merge(IVertex<TValue> other)
        {
            if (!Value.Equals(other.Value))
                throw new ArgumentException("Can't merge vertex of a different type.");
            Value.Merge(other.Value);
        }

        public object Clone() => MemberwiseClone();
        public override string ToString() => value.ToString();

    }
}
