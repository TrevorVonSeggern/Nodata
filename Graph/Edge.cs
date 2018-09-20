﻿using System;
using System.Collections.Generic;
using System.Linq;
using CodeTools;
using Graph.Interfaces;

namespace Graph
{

    public class Edge<TEdgeValue, TVertex, TVertexValue> : IEdge<TEdgeValue, TVertex, TVertexValue>
        where TVertex : class, IVertex<TVertexValue>
        where TVertexValue : IMergable<TVertexValue>
        where TEdgeValue : ICloneable
    {
        public virtual TVertex From { get; protected set; }
        public virtual TVertex To { get; protected set; }
        public virtual TEdgeValue Value { get; protected set; }

        public Edge(TVertex from, TVertex to, TEdgeValue value = default(TEdgeValue))
        {
            From = from ?? throw new ArgumentNullException(nameof(from));
            To = to ?? throw new ArgumentNullException(nameof(to));
            Value = value;
        }

        // public IEdge<TEdgeValue, TVertex, TVertexValue> CloneWithNewReferences(TVertex from, TVertex to)
        // {
        //     if (!From.Value.Equals(from.Value) || !To.Value.Equals(to.Value))
        //         throw new ArgumentException("Cloned vertex references don't have the same value.");
        //     return new Edge<TEdgeValue, TVertex, TVertexValue>(from, to, (TEdgeValue)Value.Clone());
        // }

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