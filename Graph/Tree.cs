﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using Immutability;
using Graph.Interfaces;

namespace Graph
{

    [Immutable]
    public class Tree<TTree, TVertex, TEdge, TVertexValue, TEdgeValue> : ITree<TTree, TVertex, TEdge, TVertexValue, TEdgeValue>
        where TVertex : class, IVertex<TVertexValue>
        where TEdge : class, IEdge<TEdgeValue, TVertex, TVertexValue>
        where TTree : class, ITree<TTree, TVertex, TEdge, TVertexValue, TEdgeValue>
    {
        public TVertex Root { get; }

        public IEnumerable<ITuple<TEdge, TTree>> Children { get; } = new List<ITuple<TEdge, TTree>>();

        public Tree(TVertex root, IEnumerable<ITuple<TEdge, TTree>> children = null)
        {
            Root = root;
            Children = children ?? new List<ITuple<TEdge, TTree>>();
            if (Children.Any(c => !root.Equals(c.Item1.From)))
                throw new ArgumentException("Children edges must all be from the root.");
        }

        [Pure]
        public virtual void TraverseDepthFirstSearch(Action<TEdge> callback)
        {
            if (Children == null)
                return;
            foreach (var tuple in Children)
            {
                callback(tuple.Item1);
                tuple.Item2?.TraverseDepthFirstSearch(callback);
            }
        }

        [Pure]
        public virtual void TraverseDepthFirstSearch(Action<TVertex> callback)
        {
            callback(Root);
            if (Children == null)
                return;
            foreach (var tuple in Children)
            {
                tuple.Item2?.TraverseDepthFirstSearch(callback);
            }
        }

        [Pure]
        public virtual void TraverseDepthFirstSearch(Action<TVertex, IEnumerable<TEdge>> callback)
        {
            if (Children == null)
            {
                callback(Root, new TEdge[] { });
                return;
            }
            callback(Root, Children.Select(c => c.Item1));
            foreach (var tuple in Children)
            {
                tuple.Item2.TraverseDepthFirstSearch(callback);
            }
        }

        [Pure]
        public IEnumerable<P> EnumerateAllPaths<P>(Func<IEnumerable<TEdge>, P> ctorFunc)
            where P : IPath<TEdge, TVertex, TEdgeValue, TVertexValue>
        {
            if (!Children.Any())
                yield break;
            foreach (var childPath in Children.Select(x => ITuple.Create(x.Item1, x.Item2.EnumerateAllPaths())))
            {
                var cPaths = childPath.Item2;
                var toPrepend = new List<IPath<TEdge, TVertex, TEdgeValue, TVertexValue>>();
                foreach (var cpath in cPaths)
                {
                    yield return ctorFunc((cpath.Prepend(childPath.Item1)));
                }
                yield return ctorFunc(new[] { childPath.Item1 });
            }
        }
        [Pure]
        public IEnumerable<IPath<TEdge, TVertex, TEdgeValue, TVertexValue>> EnumerateAllPaths() => EnumerateAllPaths(edges => new Path<TEdge, TVertex, TEdgeValue, TVertexValue>(edges));

        public Tree(IEnumerable<IEnumerable<TEdge>> expandPaths, Func<IEnumerable<IEnumerable<TEdge>>, TTree> childCtor, Func<TVertex, TTree> childCtor2)
        {
            Root = expandPaths.FirstOrDefault(x => x.Any())?.First()?.From;
            var children = new List<ITuple<TEdge, TTree>>();

            if (Root is null) return;

            if (expandPaths is null)
                expandPaths = new List<List<TEdge>>();

            expandPaths = expandPaths.Where(p => p != null && p.Any() == true);

            if (!expandPaths.Any())
                return;

            // Validate each path starts with root.
            if (expandPaths.Any(p => p.First().From != Root))
                throw new ArgumentException("Paths don't all begin at the same vertex");

            // each path that has the the same root.
            foreach (var path in expandPaths.GroupBy(x => x.First()))
            {
                var childPaths = path.Select(p => p.Skip(1)).Where(p => p.Any());
                if (childPaths.Any())
                    children.Add(ITuple.Create(path.Key, childCtor(childPaths)));
                else
                    children.Add(ITuple.Create(path.Key, childCtor2(path.Key.To)));
            }
            Children = children;
        }


        [Pure]
        public virtual ISubGraph<TVertex, TEdge, TVertexValue, TEdgeValue> Flatten()
        {
            var edges = new List<TEdge>();
            var vertices = new List<TVertex>();
            vertices.Add(Root);
            TraverseDepthFirstSearch(e =>
            {
                if (!edges.Contains(e))
                    edges.Add(e);
                if (!vertices.Contains(e.To))
                    vertices.Add(e.To);
            });
            return new SubGraph<TVertex, TEdge, TVertexValue, TEdgeValue>(vertices, edges);
        }
    }

    // outside of class because It has to implement IMergable<TVertex>. I don't want the tree class to enforce that generic constraint.
    public static class TreeExtension
    {
        public static Graph<TVertex, TEdge, TVertexValue, TEdgeValue> Flatten<TTree, TVertex, TEdge, TVertexValue, TEdgeValue>(this TTree selectionTree)
                where TVertex : class, IVertex<TVertexValue>, IMergable<TVertex>
                where TEdge : class, IEdge<TEdgeValue, TVertex, TVertexValue>
                where TTree : class, ITree<TTree, TVertex, TEdge, TVertexValue, TEdgeValue>
        {
            return Interfaces.TreeExtension.Flatten<
                    Graph<TVertex, TEdge, TVertexValue, TEdgeValue>,
                    TTree,
                    TVertex,
                    TEdge,
                    TVertexValue,
                    TEdgeValue
                    >(selectionTree, (vList, eList) => new Graph<TVertex, TEdge, TVertexValue, TEdgeValue>(vList, eList));
        }
    }

}
