﻿using System;
using System.Collections.Generic;
using System.Linq;
using CodeTools;

namespace Graph.Interfaces
{
    [Immutable]
    public interface ITree<TVertex, TEdge, TVertexValue, TEdgeValue>
        where TVertex : IVertex<TVertexValue>
        where TEdge : IEdge<TEdgeValue, TVertex, TVertexValue>
    {
        TVertex Root { get; }
        IEnumerable<ITuple<TEdge, ITree<TVertex, TEdge, TVertexValue, TEdgeValue>>> Children { get; }
        void TraverseDepthFirstSearch(Action<TEdge> callback);
        void TraverseDepthFirstSearch(Action<TVertex> callback);
        void TraverseDepthFirstSearch(Action<TVertex, IEnumerable<TEdge>> callback);

        IEnumerable<IPath<TEdge, TVertex, TEdgeValue, TVertexValue>> EnumerateAllPaths();
    }

    public static class TreeExtension
    {
        // These Generics are stupid long, I know. It's supposed to be as extensible as possible. Less generic hell is provided at the class level.
        public static TGraph Flatten<TGraph, TTree, TVertex, TEdge, TVertexValue, TEdgeValue>(
                this TTree selectionTree,
                Func<IEnumerable<TVertex>, IEnumerable<TEdge>, TGraph> graphCtorFunc)
            where TVertex : IVertex<TVertexValue>, IMergable<TVertex>
            where TEdge : IEdge<TEdgeValue, TVertex, TVertexValue>
            where TTree : ITree<TVertex, TEdge, TVertexValue, TEdgeValue>
        {
            var edges = new List<TEdge>();
            selectionTree.TraverseDepthFirstSearch(edges.Add);
            var vertices = new Dictionary<TVertexValue, TVertex>();

            void MergeVertexes(IEnumerable<TVertex> selection)
            {
                // selection = selection.Where(x => x != null);
                foreach (var v in selection)
                {
                    if (vertices.ContainsKey(v.Value))
                        vertices[v.Value].Merge(v);
                    else
                        vertices.Add(v.Value, v);
                }
            }

            MergeVertexes(new[] { selectionTree.Root });
            MergeVertexes(edges.Select(e => e.From));
            MergeVertexes(edges.Select(e => e.To));
            var vList = vertices.Values.ToList();
            var eList = edges.Where(e => vList.Contains(e.From) && vList.Contains(e.To));
            return graphCtorFunc(vList, eList);
        }
    }
}