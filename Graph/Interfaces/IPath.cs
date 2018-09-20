﻿using System;
using System.Collections.Generic;
using System.Linq;
using CodeTools;

namespace Graph.Interfaces
{
    [Immutable]
    public interface IPath<out TEdge, out TVertex, out TEdgeValue, out TVertexValue> : IEnumerable<TEdge>
        where TVertex : IVertex<TVertexValue>
        where TEdge : IEdge<TEdgeValue, TVertex, TVertexValue>
    {
        IEnumerable<TEdge> Edges { get; }
        void Traverse(Action<TEdge> edge);

        // IPath<TEdge, TVertex, TEdgeValue, TVertexValue> AppendWith(Func<TEdge> startWithFunc);
    }

    public static class PathExtensions
    {
        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> values, T value)
        {
            yield return value;
            foreach (T item in values)
            {
                yield return item;
            }
        }

        public static IPath<TEdge, TVertex, TEdgeValue, TVertexValue> PrependEdge<TEdge, TVertex, TEdgeValue, TVertexValue>(
            this IPath<TEdge, TVertex, TEdgeValue, TVertexValue> path,
            TEdge edgeToPrepend)
        where TVertex : IVertex<TVertexValue>
        where TEdge : IEdge<TEdgeValue, TVertex, TVertexValue>
        {
            return new Path<TEdge, TVertex, TEdgeValue, TVertexValue>(path.Edges.Prepend(edgeToPrepend));
        }
    }
}