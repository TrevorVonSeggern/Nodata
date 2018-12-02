using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Immutability;

namespace Graph.Interfaces
{
    [Immutable]
    public interface ITree<TTree, TVertex, TEdge, TVertexValue, TEdgeValue>
        where TVertex : IVertex<TVertexValue>
        where TEdge : IEdge<TEdgeValue, TVertex, TVertexValue>
        where TTree : ITree<TTree, TVertex, TEdge, TVertexValue, TEdgeValue>
    {
        TVertex Root { get; }
        IEnumerable<ITuple<TEdge, TTree>> Children { get; }
        [Pure]
        void TraverseDepthFirstSearch(Action<TEdge> callback);
        [Pure]
        void TraverseDepthFirstSearch(Action<TVertex> callback);
        [Pure]
        void TraverseDepthFirstSearch(Action<TVertex, IEnumerable<TEdge>> callback);

        [Pure]
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
            where TTree : ITree<TTree, TVertex, TEdge, TVertexValue, TEdgeValue>
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
