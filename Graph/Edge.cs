using System;
using System.Collections.Generic;
using System.Linq;
using CodeTools;
using Graph.Interfaces;

namespace Graph
{
    [Immutable]
    public class Edge<TEdgeValue, TVertex, TVertexValue> : IEdge<TEdgeValue, TVertex, TVertexValue>
        where TVertex : class, IVertex<TVertexValue>
    {
        public TVertex From { get; }
        public TVertex To { get; }
        public TEdgeValue Value { get; }

        public Edge(TVertex from, TVertex to, TEdgeValue value = default(TEdgeValue))
        {
            From = from ?? throw new ArgumentNullException(nameof(from));
            To = to ?? throw new ArgumentNullException(nameof(to));
            Value = value;
        }

        public override bool Equals(object obj)
        {
            if (obj is Edge<TEdgeValue, TVertex, TVertexValue>)
            {
                var other = obj as Edge<TEdgeValue, TVertex, TVertexValue>;
                return From.Equals(other.From) && To.Equals(other.To) && Value.Equals(other.Value) /*&& HasMany == other.HasMany*/;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (this.Value?.GetHashCode() ?? 0 + this.From.GetHashCode() + this.To.GetHashCode()) % int.MaxValue;
        }

        public object Clone() => MemberwiseClone();

        public override string ToString() => $"{From.ToString()}->{To.ToString()}";

    }
}
